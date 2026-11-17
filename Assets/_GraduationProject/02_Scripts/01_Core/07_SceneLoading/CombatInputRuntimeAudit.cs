using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// Opt-in loading smoke runner. Has no bootstrap of its own.
/// Test setup teleports only the player; attacks then use ordinary Input System,
/// animation events and overlap-box collision. Never changes stats, HP or AI state.
/// Caller must already redirect saves to the isolated smoke-test save file.
/// </summary>
public static class CombatInputRuntimeAudit
{
    public static IEnumerator Run(PlayerController player, Action<object> record)
    {
        string[] args = SceneLoadingSmokeTest.GetArguments();
        if (!args.Contains("--soom-loading-smoke") || !args.Contains("--soom-combat-input-audit"))
            throw new InvalidOperationException("Combat input audit requires both explicit smoke-test flags.");
        RequireLivingPlayer(player);
        if (player.InputReader.CurrentInputMode != InputReaderSO.InputMode.Gameplay)
            throw new InvalidOperationException("Combat input audit requires naturally enabled Gameplay input.");
        if (player.Combat.CounterStacks != 0)
            throw new InvalidOperationException("Run input audit before feedback/parry tests: expected zero stacks.");

        Enemy target = UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None)
            .Where(e => e && e.gameObject.activeInHierarchy &&
                e.name.IndexOf("Monster_a", StringComparison.OrdinalIgnoreCase) >= 0)
            .FirstOrDefault(e => e.GetComponentInParent<GroupAiZone>() is GroupAiZone z &&
                z.name.Replace("_", "").Replace(" ", "").ToLowerInvariant().EndsWith("zone1"));
        if (!target) throw new InvalidOperationException("No authored active Zone 1 Monster A found.");

        // Awake references/health are deferred until distance activation. Only
        // approach the authored enemy; never call CombatEnter/Initialize manually.
        PositionPlayer(player, target, record, "initial-grounded-approach");
        double activationStart = Time.realtimeSinceStartupAsDouble;
        while (target && (target._initializer == null || !target._initializer.IsInitialized("Final") ||
            !target._aiController || !target._aiController.isActiveAndEnabled || !target.EnemyHealth))
        {
            RequireLivingPlayer(player);
            if (Time.realtimeSinceStartupAsDouble - activationStart > 12d)
                throw new TimeoutException("Zone 1 Monster A did not activate naturally after approach.");
            yield return null;
        }
        if (!target || target.EnemyHealth.IsDead)
            throw new InvalidOperationException("Zone 1 target is unavailable or already dead.");

        EnemyHealth enemyHealth = target.EnemyHealth;
        PlayerHealth playerHealth = player.Health;
        PlayerEvents events = player.Events;
        InputReaderSO reader = player.InputReader;
        PlayerCombat combat = player.Combat;
        string targetName = target.name;
        EnemyStat enemyStat = target.enemyStat;
        string authoredPlayerBefore = JsonUtility.ToJson(player.Data);
        string authoredEnemyBefore = JsonUtility.ToJson(enemyStat);
        int initialEnemyHp = enemyHealth.CurrentHealth, initialPlayerHp = playerHealth.CurrentHealth;
        int lastEnemyHp = initialEnemyHp;
        int inputCount = 0, heavyInputCount = 0, attackStarted = 0, attackPerformed = 0, attackFinished = 0;
        int targetHits = 0, targetHpLoss = 0, playerHpLoss = 0, targetDamagedPlayer = 0;
        bool nonNormalHit = false;
        var observedEvents = new List<object>();
        double started = Time.realtimeSinceStartupAsDouble;
        Gamepad pad = null;

        Action onNormal = () => { inputCount++; observedEvents.Add(new { kind = "normal-input", frame = Time.frameCount }); };
        Action onHeavy = () => { heavyInputCount++; observedEvents.Add(new { kind = "unexpected-heavy-input", frame = Time.frameCount }); };
        Action onStart = () => { attackStarted++; observedEvents.Add(new { kind = "animation-attack-start", frame = Time.frameCount }); };
        Action onPerform = () => { attackPerformed++; observedEvents.Add(new { kind = "animation-attack-perform", frame = Time.frameCount }); };
        Action onFinish = () => { attackFinished++; observedEvents.Add(new { kind = "animation-attack-finish", frame = Time.frameCount }); };
        Action<int, int> onEnemyHp = (previous, current) =>
        {
            lastEnemyHp = current;
            targetHpLoss += Mathf.Max(0, previous - current);
            observedEvents.Add(new { kind = "enemy-health", frame = Time.frameCount, previous, current });
        };
        Action<int, int> onPlayerHp = (previous, current) =>
        {
            playerHpLoss += Mathf.Max(0, previous - current);
            observedEvents.Add(new { kind = "player-health", frame = Time.frameCount, previous, current });
        };
        Action<DamageData> onPlayerDamage = damage =>
        {
            Enemy source = damage.SourceEnemy;
            if (!source && damage.AttackerTransform) source = damage.AttackerTransform.GetComponentInParent<Enemy>();
            if (source == target) targetDamagedPlayer++;
            observedEvents.Add(new { kind = "player-damaged", frame = Time.frameCount,
                source = source ? source.name : "unknown", amount = damage.DamageAmount, type = damage.AttackType.ToString() });
        };
        Action<IDamageable, DamageData> onHit = (damageable, damage) =>
        {
            if (!ReferenceEquals(damageable, enemyHealth)) return;
            targetHits++;
            bool normal = (int)damage.AttackType >= (int)AttackType.Normal_0 && (int)damage.AttackType <= (int)AttackType.Normal_3;
            nonNormalHit |= !normal;
            observedEvents.Add(new { kind = "collision-attack-event", frame = Time.frameCount,
                amount = damage.DamageAmount, type = damage.AttackType.ToString(), hp = damageable.CurrentHealth,
                state = player.FSM?.CurrentState?.GetType().Name,
                animatorStateHash = player.Animator.GetCurrentAnimatorStateInfo(0).fullPathHash });
        };

        reader.NormalAttackEvent += onNormal;
        reader.HeavyAttackEvent += onHeavy;
        events.AttackStarted += onStart;
        events.AttackPerformed += onPerform;
        events.AttackFinished += onFinish;
        events.Damaged += onPlayerDamage;
        combat.AttackEvent += onHit;
        enemyHealth.OnHealthChanged += onEnemyHp;
        playerHealth.OnHealthChanged += onPlayerHp;
        try
        {
            pad = InputSystem.AddDevice<Gamepad>("SOOM_CombatInputAudit_Gamepad");
            // A real Move action primes the existing device detector. The first
            // short attack can perform on release (Hold,Press), so its magnitude
            // alone is not a reliable Gamepad detection signal. No private field
            // or InputDeviceDetector method is changed/called by the audit.
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(.3f, 0f) });
            double primeStart = Time.realtimeSinceStartupAsDouble;
            while (player.InputHandler.CurrentInputDevice != InputDeviceType.Gamepad)
            {
                if (Time.realtimeSinceStartupAsDouble - primeStart > 2d)
                    throw new TimeoutException("Virtual Gamepad did not activate the normal device detector.");
                yield return null;
            }
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return null;
            PositionPlayer(player, target, record, "after-real-stick-device-primer");

            // Observe a natural AI response before attacking. Lack of a hit in
            // this short window is reported separately, never fabricated.
            double observeStart = Time.realtimeSinceStartupAsDouble;
            while (Time.realtimeSinceStartupAsDouble - observeStart < 4d && playerHpLoss == 0)
            {
                RequireLivingPlayer(player);
                yield return null;
            }

            int attempts = 0;
            for (; attempts < 3 && target && !enemyHealth.IsDead; attempts++)
            {
                RequireLivingPlayer(player);
                double readyStart = Time.realtimeSinceStartupAsDouble;
                while (!(player.FSM?.CurrentState is PlayerIdleState))
                {
                    RequireLivingPlayer(player);
                    if (Time.realtimeSinceStartupAsDouble - readyStart > 5d)
                        throw new TimeoutException("Player did not return to Idle naturally before attack input.");
                    yield return null;
                }
                PositionPlayer(player, target, record, "attack-setup-" + (attempts + 1));
                int beforeFinished = attackFinished;
                InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.West));
                yield return new WaitForSecondsRealtime(.08f);
                InputSystem.QueueStateEvent(pad, new GamepadState());
                double attackStart = Time.realtimeSinceStartupAsDouble;
                while (attackFinished == beforeFinished && Time.realtimeSinceStartupAsDouble - attackStart < 3d)
                {
                    RequireLivingPlayer(player);
                    yield return null;
                }
                // Let normal hit-stop, enemy knockback and FSM transition finish.
                yield return new WaitForSecondsRealtime(.25f);
                foreach (object row in observedEvents) record?.Invoke(new { stage = "combat-input-event", detail = row });
                observedEvents.Clear();
                if (heavyInputCount > 0) break;
                if (targetHits > 0 && targetHpLoss > 0) { attempts++; break; }
            }

            bool authoredStatsUnchanged = authoredPlayerBefore == JsonUtility.ToJson(player.Data) &&
                authoredEnemyBefore == JsonUtility.ToJson(enemyStat);
            bool timeRestored = Mathf.Abs(Time.timeScale - 1f) < .01f;
            bool pass = inputCount > 0 && heavyInputCount == 0 && attackStarted > 0 && attackPerformed > 0 &&
                targetHits > 0 && targetHpLoss > 0 && !nonNormalHit && authoredStatsUnchanged && timeRestored;
            foreach (object row in observedEvents) record?.Invoke(new { stage = "combat-input-event", detail = row });
            observedEvents.Clear();
            record?.Invoke(new { stage = "combat-input-audit", pass, target = targetName,
                scene = player.gameObject.scene.name, elapsedSeconds = Time.realtimeSinceStartupAsDouble - started,
                setup = "Player-only grounded teleport; real virtual Gamepad Move primer and short buttonWest pulses (same action as LMB). No HP/stat/AI/FSM/animation overrides.",
                attempts, inputCount, heavyInputCount, attackStarted, attackPerformed, attackFinished,
                targetHits, targetHpLoss, playerHpLoss, targetDamagedPlayer,
                initialEnemyHp, finalEnemyHp = enemyHealth ? enemyHealth.CurrentHealth : lastEnemyHp, initialPlayerHp,
                finalPlayerHp = playerHealth.CurrentHealth, authoredStatsUnchanged, timeRestored, finalTimeScale = Time.timeScale,
                counterStacks = combat.CounterStacks,
                playerDamageObservation = playerHpLoss > 0 ? "observed through normal AI damage path" : "not observed in four-second pre-attack window" });
            if (!pass) throw new InvalidOperationException("Normal input/animation/collider damage audit failed; inspect combat-input events.");
        }
        finally
        {
            foreach (object row in observedEvents) record?.Invoke(new { stage = "combat-input-event", detail = row });
            observedEvents.Clear();
            if (reader) { reader.NormalAttackEvent -= onNormal; reader.HeavyAttackEvent -= onHeavy; }
            events.AttackStarted -= onStart;
            events.AttackPerformed -= onPerform;
            events.AttackFinished -= onFinish;
            events.Damaged -= onPlayerDamage;
            if (combat) combat.AttackEvent -= onHit;
            if (enemyHealth) enemyHealth.OnHealthChanged -= onEnemyHp;
            if (playerHealth) playerHealth.OnHealthChanged -= onPlayerHp;
            if (pad != null && pad.added) InputSystem.RemoveDevice(pad);
        }
    }

    private static void RequireLivingPlayer(PlayerController player)
    {
        if (!player || !player.enabled || !player.Health || player.Health.IsDead || !player.InputReader ||
            !player.Combat || player.RuntimeData == null)
            throw new InvalidOperationException("Combat input audit lost its initialized, living player.");
        // Ordinary impact feedback briefly sets timeScale to zero. Real-time
        // deadlines and the final restoration assertion distinguish it from a stall.
    }

    private static void PositionPlayer(PlayerController player, Enemy target, Action<object> record, string reason)
    {
        var zone = target.GetComponentInParent<GroupAiZone>();
        var controller = player.GetComponent<CharacterController>();
        Vector3 from = player.transform.position;
        Vector3 forward = target.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < .01f) forward = Vector3.forward;
        forward.Normalize();
        Vector3? destination = null;
        for (int index = 0; index < 12; index++)
        {
            Vector3 offset = Quaternion.AngleAxis(index * 30f, Vector3.up) * forward * 1.7f;
            Vector3 candidate = target.transform.position + offset;
            if (!Physics.Raycast(candidate + Vector3.up * 4f, Vector3.down, out RaycastHit hit, 10f,
                LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore) || hit.normal.y < .75f) continue;
            candidate = hit.point + Vector3.up * .12f;
            if (zone && !zone.IsInZone(candidate)) continue;
            if (Physics.CheckCapsule(candidate + Vector3.up * .4f, candidate + Vector3.up * 1.2f, .35f,
                LayerMask.GetMask("Wall", "Default"), QueryTriggerInteraction.Ignore)) continue;
            destination = candidate;
            break;
        }
        if (!destination.HasValue) throw new InvalidOperationException("No safe grounded melee setup point inside authored Zone 1.");
        bool wasEnabled = controller && controller.enabled;
        try
        {
            if (controller) controller.enabled = false;
            player.transform.position = destination.Value;
            Vector3 facing = target.transform.position - destination.Value;
            facing.y = 0f;
            player.transform.rotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
            Physics.SyncTransforms();
        }
        finally { if (controller) controller.enabled = wasEnabled; }
        record?.Invoke(new { stage = "combat-input-teleport-setup", reason,
            from = from.ToString("F3"), to = destination.Value.ToString("F3"),
            enemy = target.name, targetPosition = target.transform.position.ToString("F3"),
            separation = Vector3.Distance(destination.Value, target.transform.position) });
    }
}
