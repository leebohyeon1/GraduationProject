using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Opt-in natural boss AI observation with a protected QA target.</summary>
public static class BossPatternRuntimeAudit
{
    private const string Scope = "Real boss AI against protected QA player; not received-damage validation. Player-only 4/8/14m range setup and one scripted nonlethal phase-threshold hit. No AI/FSM/enemy-transform/stat overrides.";
    private const float PhaseSeconds = 45f;
    // Read-only mirrors of the authored extra BT cooldown gates; never applied.
    private static readonly Dictionary<string, float> GateSeconds = new Dictionary<string, float>
    {
        { "Boss_S_Attack_1", 3f }, { "Boss_S_Attack_2", 3f },
        { "Boss_B_Attack_1", 6f }, { "Boss_B_Attack_2", 6f },
        { "Boss_J_Attack", 5f }, { "Boss_R_Attack", 7f }, { "Boss_Tail_Attack", 10f }
    };

    public static IEnumerator Run(PlayerController player, Enemy boss, Action<object> record)
    {
        if (!SceneLoadingSmokeTest.GetArguments().Contains("--soom-boss-pattern-audit"))
            throw new InvalidOperationException("Boss pattern observation requires its explicit QA flag.");
        if (!player || !player.Health || !boss || !boss.EnemyHealth || !boss._aiController)
            throw new InvalidOperationException("Boss/player components are unavailable for pattern observation.");
        var events = player.Events;
        var health = boss.EnemyHealth;
        var state = boss.GetComponent<EnemyStateController>();
        var brain = boss._aiController._aiBrain;
        var bb = brain.blackboard;
        if (!state || bb == null || boss._aiController.inGameenemyAttackDatas == null)
            throw new InvalidOperationException("Boss pattern runtime is not initialized.");
        var tracker = player.GetComponent<PlayerZoneTracker>();
        int initialPlayerHp = player.Health.CurrentHealth;
        int initialBossMaxHp = health.MaxHealth;
        string playerSource = JsonUtility.ToJson(player.Data), bossSource = JsonUtility.ToJson(boss.enemyStat);
        var patterns = new Dictionary<string, int>();
        var phaseCounts = new int[2];
        var contacts = new Dictionary<string, int>();
        var eventRows = new List<object>();
        int phaseIndex = 0, blockedContacts = 0;
        string phaseLabel = "phase1", lastPattern = null, lastEndPhase = null;
        bool transitioning = false, phase2AnimationSeen = false, phase2AnimationCompleted = false;
        double started = Time.realtimeSinceStartupAsDouble, lastEndReal = -1d;
        float lastEndScaled = -1f, maxIdleReal = 0f, maxIdleScaled = 0f;

        void Protect(ref PlayerDamageContext context)
        {
            string key = (context.Data.SourceEnemy ? context.Data.SourceEnemy.name : "unknown") + "/" + context.Data.AttackType;
            contacts.TryGetValue(key, out int old); contacts[key] = old + 1; blockedContacts++;
            context.Data.DamageAmount = 0;
            context.Data.StiffnessAmount = 0;
            context.Data.KnockbackForce = 0f;
            context.Data.KnockbackDuration = 0f;
            context.HasSuperArmor = true;
        }
        void OnState(EnemyStateController.EnemyState previous, EnemyStateController.EnemyState next)
        {
            bool wasAttack = previous == EnemyStateController.EnemyState.Attack || previous == EnemyStateController.EnemyState.Rush;
            bool isAttack = next == EnemyStateController.EnemyState.Attack || next == EnemyStateController.EnemyState.Rush;
            if (!wasAttack || isAttack) return;
            lastEndReal = Time.realtimeSinceStartupAsDouble; lastEndScaled = Time.time; lastEndPhase = phaseLabel;
            eventRows.Add(new { kind = "boss-attack-state-ended", phase = phaseLabel, pattern = lastPattern,
                realTime = lastEndReal, scaledTime = lastEndScaled, previous = previous.ToString(), next = next.ToString() });
        }
        void OnLog(string message, string trace, LogType type)
        {
            string prefix = "[Attack Node Enter] " + boss.name + " entered attack node for ";
            if (!message.StartsWith(prefix, StringComparison.Ordinal)) return;
            string pattern = message.Substring(prefix.Length);
            patterns.TryGetValue(pattern, out int count); patterns[pattern] = count + 1;
            if (!transitioning) phaseCounts[phaseIndex]++;
            double now = Time.realtimeSinceStartupAsDouble;
            if (lastEndReal >= 0d)
            {
                float realGap = (float)(now - lastEndReal), scaledGap = Time.time - lastEndScaled;
                bool includesPhaseTransition = transitioning || lastEndPhase != phaseLabel;
                if (!includesPhaseTransition) { maxIdleReal = Mathf.Max(maxIdleReal, realGap); maxIdleScaled = Mathf.Max(maxIdleScaled, scaledGap); }
                eventRows.Add(new { kind = "boss-pattern-idle-gap", fromPattern = lastPattern, toPattern = pattern,
                    fromPhase = lastEndPhase, toPhase = phaseLabel, realSeconds = realGap, scaledSeconds = scaledGap,
                    includesPhaseTransition, zoneId = tracker ? tracker.CurrentZoneId : -1,
                    distance = Vector3.Distance(player.transform.position, boss.transform.position), timeScale = Time.timeScale });
                lastEndReal = -1d;
            }
            lastPattern = pattern;
            eventRows.Add(new { kind = "boss-real-attack-start", phase = phaseLabel, pattern,
                realTime = now, scaledTime = Time.time, zoneId = tracker ? tracker.CurrentZoneId : -1 });
        }
        void Sample(string stage)
        {
            foreach (var row in eventRows) record?.Invoke(new { stage = "boss-pattern-event", detail = row });
            eventRows.Clear();
            var cooldowns = boss._aiController.inGameenemyAttackDatas.Where(d => d).Select(d =>
            {
                float gate = GateSeconds.TryGetValue(d.AttackName, out float configured) ? configured : d.Cooltime;
                gate = Mathf.Max(gate, d.Cooltime);
                return new { name = d.AttackName, sourceCooldown = d.Cooltime, effectiveCooldownGate = gate,
                    ready = brain.IsSkillReady(d.AttackName, gate), lastUseScaled = brain.GetLastSkillUseTime(d.AttackName) };
            }).ToArray();
            var animator = boss.animator;
            var anim = animator.GetCurrentAnimatorStateInfo(0);
            record?.Invoke(new
            {
                stage, scope = Scope, phase = phaseLabel, elapsedSeconds = Time.realtimeSinceStartupAsDouble - started,
                realTime = Time.realtimeSinceStartupAsDouble, scaledTime = Time.time, timeScale = Time.timeScale,
                zoneId = tracker ? tracker.CurrentZoneId : -1, distance = Vector3.Distance(player.transform.position, boss.transform.position),
                state = boss.CurrentState.ToString(), locked = state.IsStateLocked,
                bridgeAttacking = boss._animationBridge.IsAttacking, combat = brain._isCombat,
                attackToken = bb.GetValueOrDefault(EnemyBlackboardKeys.HasAttackToken, false),
                attackingFlag = bb.GetValueOrDefault("IsAttacking", false),
                walkingTimeBool = bb.GetValue<bool>("WalkingTime"), walkingTimeFloat = bb.GetValue<float>("WalkingTime"),
                requestedPhase = bb.GetValue<float>("RequestedPhase"), committedPhase = bb.GetValue<int>(EnemyBlackboardKeys.Phase),
                phase2 = bb.GetValue<bool>("Phase_2"), phase2AnimationSeen, phase2AnimationCompleted,
                animatorHash = anim.fullPathHash, normalizedTime = anim.normalizedTime, animatorSpeed = animator.speed,
                hitWindow = boss.animHandler.IsHitWindowOpen, actionFinished = boss.animHandler.IsActionFinished,
                cooldownReadyAttackNames = cooldowns.Where(c => c.ready).Select(c => c.name).ToArray(), cooldowns,
                cooldownReadinessLimit = "Cooldown readiness only; serialized zone/state/weighted-selection conditions are separate.",
                pendingIdleRealSeconds = lastEndReal < 0d ? 0d : Time.realtimeSinceStartupAsDouble - lastEndReal,
                pendingIdleScaledSeconds = lastEndReal < 0d ? 0f : Time.time - lastEndScaled,
                maxIdleReal, maxIdleScaled, phase1AttackCount = phaseCounts[0], phase2AttackCount = phaseCounts[1],
                actualPatterns = new Dictionary<string, int>(patterns), blockedContacts,
                qaInterceptedContacts = new Dictionary<string, int>(contacts), playerHp = player.Health.CurrentHealth,
                bossHp = health.CurrentHealth, bossMaxHp = health.MaxHealth,
                sourceStatsUnchanged = playerSource == JsonUtility.ToJson(player.Data) && bossSource == JsonUtility.ToJson(boss.enemyStat)
            });
        }

        events.BeforeDamaged += Protect;
        state.OnStateChanged += OnState;
        Application.logMessageReceived += OnLog;
        try
        {
            for (phaseIndex = 0; phaseIndex < 2; phaseIndex++)
            {
                phaseLabel = phaseIndex == 0 ? "phase1" : "phase2";
                if (phaseIndex == 1)
                {
                    transitioning = true; phaseLabel = "phase2-transition";
                    int targetHealth = Mathf.Max(1, Mathf.FloorToInt(health.MaxHealth * .49f));
                    int amount = Mathf.Max(0, health.CurrentHealth - targetHealth);
                    if (amount > 0)
                        player.Combat.Attack(health, new DamageData { AttackerTransform = player.transform,
                            AttackType = AttackType.Absoluteness, DamageAmount = amount, StiffnessAmount = 0 });
                    record?.Invoke(new { stage = "boss-pattern-scripted-phase-threshold-hit", scope = Scope, amount,
                        resultingHealth = health.CurrentHealth, maxHealth = health.MaxHealth,
                        method = "One scripted nonlethal PlayerCombat.Attack; not input/combat damage validation" });
                    if (health.IsDead || health.CurrentHealth > health.MaxHealth * .5f)
                        throw new InvalidOperationException("Scripted nonlethal hit did not reach the phase threshold safely.");
                    double transitionStart = Time.realtimeSinceStartupAsDouble, nextTransitionSample = 0d;
                    while (true)
                    {
                        var anim = boss.animator.GetCurrentAnimatorStateInfo(0);
                        bool inPhaseAnimation = anim.IsName("Phase_2") || anim.IsName("Base Layer.Phase_2");
                        phase2AnimationSeen |= inPhaseAnimation;
                        // The task commits Phase_2 only after its animation reached normalizedTime >= 1.
                        // It may transition away before this coroutine samples that final frame.
                        phase2AnimationCompleted |= inPhaseAnimation && anim.normalizedTime >= 1f ||
                            phase2AnimationSeen && !inPhaseAnimation && bb.GetValue<bool>("Phase_2");
                        if (phase2AnimationSeen && phase2AnimationCompleted && bb.GetValue<bool>("Phase_2") &&
                            bb.GetValue<int>(EnemyBlackboardKeys.Phase) == 2) break;
                        double elapsed = Time.realtimeSinceStartupAsDouble - transitionStart;
                        if (elapsed >= nextTransitionSample) { Sample("boss-pattern-phase-transition"); nextTransitionSample += 2d; }
                        if (elapsed > 30d) throw new TimeoutException("Boss did not complete its actual phase 2 animation/commit.");
                        yield return null;
                    }
                    transitioning = false; phaseLabel = "phase2";
                }
                double phaseStart = Time.realtimeSinceStartupAsDouble, nextSample = 0d;
                int rangeCase = 0;
                float[] distances = { 4f, 8f, 14f };
                while (Time.realtimeSinceStartupAsDouble - phaseStart < PhaseSeconds)
                {
                    if (!player || player.Health.IsDead || !boss || health.IsDead)
                        throw new InvalidOperationException("Boss pattern audit lost its living participants.");
                    double elapsed = Time.realtimeSinceStartupAsDouble - phaseStart;
                    if (rangeCase < distances.Length && elapsed >= rangeCase * 15d)
                    {
                        SetupPlayerRange(player, boss, distances[rangeCase], phaseLabel, record);
                        rangeCase++;
                    }
                    if (elapsed >= nextSample) { Sample("boss-pattern-sample"); nextSample += 5d; }
                    yield return null;
                }
                Sample("boss-pattern-phase-complete");
            }
            bool statsUnchanged = playerSource == JsonUtility.ToJson(player.Data) && bossSource == JsonUtility.ToJson(boss.enemyStat) && health.MaxHealth == initialBossMaxHp;
            bool pass = phaseCounts[0] > 0 && phaseCounts[1] > 0 && phase2AnimationSeen && phase2AnimationCompleted &&
                player.Health.CurrentHealth == initialPlayerHp && statsUnchanged;
            record?.Invoke(new { stage = "boss-pattern-completed", pass, scope = Scope, phaseSeconds = PhaseSeconds,
                phase1AttackCount = phaseCounts[0], phase2AttackCount = phaseCounts[1], maxIdleReal, maxIdleScaled,
                statsUnchanged, phase2AnimationSeen, phase2AnimationCompleted,
                passScope = "Observed natural attacks in both phases and real phase animation; inspect recorded idle gaps separately. No claim of normal player damage or balance validation." });
            if (!pass) throw new InvalidOperationException("Boss pattern observation coverage failed; samples retained.");
        }
        finally
        {
            events.BeforeDamaged -= Protect;
            if (state) state.OnStateChanged -= OnState;
            Application.logMessageReceived -= OnLog;
            record?.Invoke(new { stage = "boss-pattern-protection-removed", scope = Scope });
        }
    }

    private static void SetupPlayerRange(PlayerController player, Enemy boss, float distance, string phase, Action<object> record)
    {
        Vector3 radial = player.transform.position - boss.transform.position; radial.y = 0f;
        if (radial.sqrMagnitude < .1f) radial = boss.transform.forward;
        radial.Normalize();
        Vector3? point = null;
        for (int index = 0; index < 24; index++)
        {
            Vector3 near = boss.transform.position + Quaternion.AngleAxis(index * 15f, Vector3.up) * radial * distance;
            if (!Physics.Raycast(near + Vector3.up * 12f, Vector3.down, out RaycastHit hit, 26f,
                LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore) || hit.normal.y < .75f) continue;
            Vector3 candidate = hit.point + Vector3.up * .12f;
            if (Mathf.Abs(candidate.y - boss.transform.position.y) > 3f) continue;
            if (Physics.CheckCapsule(candidate + Vector3.up * .4f, candidate + Vector3.up * 1.2f, .4f,
                LayerMask.GetMask("Wall", "Default"), QueryTriggerInteraction.Ignore)) continue;
            point = candidate; break;
        }
        if (!point.HasValue)
        {
            record?.Invoke(new { stage = "boss-pattern-range-setup-skipped", phase, requestedDistance = distance,
                reason = "No safe grounded location; kept current player position", actualDistance = Vector3.Distance(player.transform.position, boss.transform.position) });
            return;
        }
        Vector3 old = player.transform.position;
        var controller = player.GetComponent<CharacterController>();
        bool wasEnabled = controller && controller.enabled;
        try
        {
            if (controller) controller.enabled = false;
            player.transform.position = point.Value;
            Vector3 facing = boss.transform.position - point.Value; facing.y = 0f;
            player.transform.rotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
            Physics.SyncTransforms();
        }
        finally { if (controller) controller.enabled = wasEnabled; }
        record?.Invoke(new { stage = "boss-pattern-range-setup", scope = Scope, phase, requestedDistance = distance,
            from = old.ToString("F3"), to = point.Value.ToString("F3"), actualDistance = Vector3.Distance(point.Value, boss.transform.position) });
    }
}
