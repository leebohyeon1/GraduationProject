using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;

// Explicitly invoked in the isolated smoke session; never starts itself.
// Runs production FSM/overlap/damage/counter handlers in one frame. No asset writes.
public static class HeavyCounterResponseRuntimeAudit
{
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    public static IEnumerator Run(PlayerController player, Action<object> record)
    {
        bool passed;
        object result = RunChecks(player, true, record, out passed);
        record?.Invoke(result);
        if (!passed) throw new InvalidOperationException("Charged-counter response regression; see heavy-counter-response audit record.");
        yield break;
    }

    static object RunChecks(PlayerController player, bool includeSuccessfulCounter, Action<object> record, out bool passedResult)
    {
        RequireIsolatedSave();

        var enemy = FindEnemy();
        if (!player || player.FSM == null || !player.Health || player.Health.IsDead || !enemy)
            throw new InvalidOperationException("Load a fresh isolated scene, complete a real F intro, and provide a living initialized F/player.");
        var data = player.Combat.HeavyCounterAttackConfig;
        int baseDamage = (int)data.Damage.Value, baseStiffness = (int)data.Stiffness.Value;
        int totalDamage = (int)(baseDamage * (1f + player.Data.CounterDamageMultiply[1]));
        var ordinaryHeavy = player.Combat.HeavyAttackConfigList.FirstOrDefault();
        if (ordinaryHeavy == null) throw new InvalidOperationException("Authored ordinary heavy-attack configuration missing.");
        int ordinaryHeavyDamage = (int)ordinaryHeavy.Damage.Value;
        record?.Invoke(new { stage = "heavy-counter-response-inputs", order = "motion-before-counter", enemy = enemy.name,
            enemyHp = enemy.EnemyHealth.CurrentHealth, baseDamage, baseStiffness,
            counterMultiplier = player.Data.CounterDamageMultiply[1], totalDamage, ordinaryHeavyDamage,
            requiredSurvivingDamage = totalDamage + ordinaryHeavyDamage });
        if (enemy.EnemyHealth.CurrentHealth <= totalDamage + ordinaryHeavyDamage || baseStiffness <= 0)
            throw new InvalidOperationException("F must survive authored counter damage; this fixture does not assign HP/stats.");
        var impact = player.GetComponentInChildren<CombatImpactFeedback>(true);
        if (!impact) throw new InvalidOperationException("Player impact feedback is missing.");
        var heavy = (MMF_Player)typeof(CombatImpactFeedback).GetField("_heavyHit", Private).GetValue(impact);
        var bindings = (List<Enemy_AnimationEventHandler.FeedbackPlayer>)typeof(Enemy_AnimationEventHandler)
            .GetField("_feedbacks", Private).GetValue(enemy.animHandler);
        var strongDamage = bindings.Where(b => b.name == "Damage_FB" && b.HitType >= AttackType.Strong_1 && b.feedback)
            .Select(b => b.feedback).Distinct().ToArray();
        var normalDamage = bindings.Where(b => b.name == "Damage_FB" && b.HitType <= AttackType.Normal_3 && b.feedback)
            .Select(b => b.feedback).Distinct().ToArray();
        var stunFeedback = bindings.Where(b => b.name == "Stun_FB" && b.feedback).Select(b => b.feedback).Distinct().ToArray();
        if (strongDamage.Length == 0 || normalDamage.Length == 0 || stunFeedback.Length == 0)
            throw new InvalidOperationException("This actual F needs separate normal/heavy damage and stun feedback bindings for the audit.");
        string source = JsonUtility.ToJson(player.Data), enemySource = JsonUtility.ToJson(enemy.enemyStat);
        var controller = player.GetComponent<CharacterController>();
        bool controllerEnabled = controller && controller.enabled;
        Vector3 position = player.transform.position; Quaternion rotation = player.transform.rotation;
        var hits = new List<DamageData>(); int successes = 0; AttackType successType = AttackType.Normal_0;
        void Hit(IDamageable target, DamageData damage) { if (ReferenceEquals(target, enemy.EnemyHealth)) hits.Add(damage); }
        void Succeeded(Transform target, AttackType type) { if (target == enemy.transform) { successes++; successType = type; } }
        player.Combat.AttackEvent += Hit; player.Events.CounterSucceeded += Succeeded;
        var cases = new List<object>(); bool passed = false;
        try
        {
            player.FSM.ChangeState<PlayerIdleState>();
            if (controller) controller.enabled = false;
            Vector3 direction = enemy.transform.forward; direction.y = 0f; direction.Normalize();
            player.transform.SetPositionAndRotation(enemy.transform.position - direction * 2f, Quaternion.LookRotation(direction));
            Physics.SyncTransforms();
            // Fill by the production normal-hit accumulation API, never field/stat assignment.
            int fill = Mathf.Max(0, enemy.StiffnessSystem.StiffnessThreshold - baseStiffness - enemy.StiffnessSystem.CurrentStiffness);
            if (fill > 0) enemy.StiffnessSystem.AddStiffness(fill, AttackType.Normal_0);
            int hpBefore = enemy.EnemyHealth.CurrentHealth, gaugeBefore = enemy.StiffnessSystem.CurrentStiffness;
            int heavyBefore = heavy.PlayCount, strongBefore = Count(strongDamage), normalBefore = Count(normalDamage), stunBefore = Count(stunFeedback);
            player.Combat.SetCharge(true);
            player.FSM.ChangeState<PlayerHeavyCounterState>();
            player.Events.TriggerAttackPerformed();
            int hpAfter = enemy.EnemyHealth.CurrentHealth, gaugeAfter = enemy.StiffnessSystem.CurrentStiffness;
            bool failurePass = hpBefore - hpAfter == baseDamage && hits.Count == 1 && successes == 0 &&
                enemy.ParrySystem.CurrentStun == StunType.None && gaugeAfter == gaugeBefore + baseStiffness &&
                heavy.PlayCount == heavyBefore && Count(strongDamage) == strongBefore && Count(stunFeedback) == stunBefore && Count(normalDamage) > normalBefore && Suppressed(hits[0]);
            cases.Add(new { name = "unconfirmed-motion-at-stun-threshold", pass = failurePass, hpBefore, hpAfter,
                baseDamage, gaugeBefore, gaugeAfter, threshold = enemy.StiffnessSystem.StiffnessThreshold,
                currentStun = enemy.ParrySystem.CurrentStun.ToString(), successes, hitCount = hits.Count,
                attackType = hits.Count > 0 ? hits[0].AttackType.ToString() : "none",
                heavyFeedbackDelta = heavy.PlayCount - heavyBefore, strongDamageFeedbackDelta = Count(strongDamage) - strongBefore,
                normalDamageFeedbackDelta = Count(normalDamage) - normalBefore, stunFeedbackDelta = Count(stunFeedback) - stunBefore });
            passed = failurePass;
            if (includeSuccessfulCounter && failurePass)
            {
                int playerHp = player.Health.CurrentHealth; hpBefore = enemy.EnemyHealth.CurrentHealth;
                stunBefore = Count(stunFeedback); strongBefore = Count(strongDamage);
                player.Events.TriggerCounterWindowStarted();
                player.Health.TakeDamage(new DamageData(enemy.transform, AttackType.Strong_1, 1, 0));
                int expectedExtra = totalDamage - baseDamage;
                bool successPass = successes == 1 && successType == AttackType.Strong_Counter &&
                    player.Health.CurrentHealth == playerHp && hpBefore - enemy.EnemyHealth.CurrentHealth == expectedExtra &&
                    enemy.ParrySystem.CurrentStun == StunType.Full && enemy.StiffnessSystem.CurrentStiffness == 0 &&
                    Count(stunFeedback) > stunBefore && Count(strongDamage) > strongBefore;
                cases.Add(new { name = "incoming-hit-confirmed-counter-after-motion", pass = successPass, successes,
                    successType = successType.ToString(), playerHpBefore = playerHp, playerHpAfter = player.Health.CurrentHealth,
                    hpBefore, hpAfter = enemy.EnemyHealth.CurrentHealth, expectedExtra,
                    currentStun = enemy.ParrySystem.CurrentStun.ToString(), gaugeAfter = enemy.StiffnessSystem.CurrentStiffness,
                    stunFeedbackDelta = Count(stunFeedback) - stunBefore, strongDamageFeedbackDelta = Count(strongDamage) - strongBefore });
                passed &= successPass;
                if (successPass)
                {
                    enemy.ParrySystem.ClearStun();
                    player.FSM.ChangeState<PlayerIdleState>();
                    hpBefore = enemy.EnemyHealth.CurrentHealth; heavyBefore = heavy.PlayCount; strongBefore = Count(strongDamage);
                    int hitsBefore = hits.Count;
                    player.Combat.ExecuteAttack(ordinaryHeavy);
                    bool ordinaryPass = hpBefore - enemy.EnemyHealth.CurrentHealth == ordinaryHeavyDamage && hits.Count == hitsBefore + 1 &&
                        heavy.PlayCount > heavyBefore && Count(strongDamage) > strongBefore && !Suppressed(hits.Last());
                    cases.Add(new { name = "ordinary-heavy-attack-response-unchanged", pass = ordinaryPass,
                        ordinaryHeavyDamage, hpBefore, hpAfter = enemy.EnemyHealth.CurrentHealth,
                        heavyFeedbackDelta = heavy.PlayCount - heavyBefore, strongDamageFeedbackDelta = Count(strongDamage) - strongBefore });
                    passed &= ordinaryPass;
                }
            }
            passed &= RunObjectChecks(player, cases);
        }
        finally
        {
            player.Combat.AttackEvent -= Hit; player.Events.CounterSucceeded -= Succeeded;
            player.FSM.ChangeState<PlayerIdleState>();
            if (controller) controller.enabled = false;
            player.transform.SetPositionAndRotation(position, rotation);
            if (controller) controller.enabled = controllerEnabled;
            Physics.SyncTransforms();
        }
        bool sourcesUnchanged = source == JsonUtility.ToJson(player.Data) && enemySource == JsonUtility.ToJson(enemy.enemyStat);
        passedResult = passed && sourcesUnchanged;
        return new { stage = "heavy-counter-response", pass = passedResult, enemy = enemy.name, includeSuccessfulCounter, sourcesUnchanged, cases,
            scope = "Production heavy-counter FSM + overlap, then actual incoming PlayerHealth.TakeDamage. QA player positioned briefly, live enemy gauge primed via normal hit API. Not natural button timing. QA enemy damage/stun intentionally remain; no save or asset changes." };
    }

    // Run on a separate authored F after the SideB landing audit, preserving the
    // real 200-HP budget instead of healing, cloning, or changing damage values.
    public static IEnumerator RunReverseOrder(PlayerController player, Action<object> record)
    {
        RequireIsolatedSave();
        var enemy = FindEnemy();
        if (!player || player.FSM == null || !player.Health || player.Health.IsDead || !enemy)
            throw new InvalidOperationException("Reverse-order audit needs a living initialized F/player after the real intro.");
        var data = player.Combat.HeavyCounterAttackConfig;
        int baseDamage = (int)data.Damage.Value;
        int totalDamage = (int)(baseDamage * (1f + player.Data.CounterDamageMultiply[1]));
        record?.Invoke(new { stage = "heavy-counter-response-inputs", order = "counter-before-motion", enemy = enemy.name,
            enemyHp = enemy.EnemyHealth.CurrentHealth, baseDamage, counterMultiplier = player.Data.CounterDamageMultiply[1], totalDamage,
            requiredSurvivingDamage = totalDamage });
        if (enemy.EnemyHealth.CurrentHealth <= totalDamage)
            throw new InvalidOperationException("Reverse-order target must survive the authored counter; no HP assignments are made.");
        var impact = player.GetComponentInChildren<CombatImpactFeedback>(true);
        if (!impact) throw new InvalidOperationException("Player impact feedback missing.");
        var heavy = (MMF_Player)typeof(CombatImpactFeedback).GetField("_heavyHit", Private).GetValue(impact);
        var bindings = (List<Enemy_AnimationEventHandler.FeedbackPlayer>)typeof(Enemy_AnimationEventHandler)
            .GetField("_feedbacks", Private).GetValue(enemy.animHandler);
        var strongDamage = bindings.Where(b => b.name == "Damage_FB" && b.HitType >= AttackType.Strong_1 && b.feedback).Select(b => b.feedback).Distinct().ToArray();
        var stunFeedback = bindings.Where(b => b.name == "Stun_FB" && b.feedback).Select(b => b.feedback).Distinct().ToArray();
        if (strongDamage.Length == 0 || stunFeedback.Length == 0) throw new InvalidOperationException("Actual F feedback bindings missing.");
        string source = JsonUtility.ToJson(player.Data), enemySource = JsonUtility.ToJson(enemy.enemyStat);
        var controller = player.GetComponent<CharacterController>(); bool controllerEnabled = controller && controller.enabled;
        Vector3 position = player.transform.position; Quaternion rotation = player.transform.rotation;
        var hits = new List<DamageData>(); int successes = 0; AttackType successType = AttackType.Normal_0;
        void Hit(IDamageable target, DamageData damage) { if (ReferenceEquals(target, enemy.EnemyHealth)) hits.Add(damage); }
        void Succeeded(Transform target, AttackType type) { if (target == enemy.transform) { successes++; successType = type; } }
        player.Combat.AttackEvent += Hit; player.Events.CounterSucceeded += Succeeded;
        bool passed = false;
        try
        {
            player.FSM.ChangeState<PlayerIdleState>();
            if (controller) controller.enabled = false;
            Vector3 direction = enemy.transform.forward; direction.y = 0f; direction.Normalize();
            player.transform.SetPositionAndRotation(enemy.transform.position - direction * 2f, Quaternion.LookRotation(direction));
            Physics.SyncTransforms();
            int fill = Mathf.Max(0, enemy.StiffnessSystem.StiffnessThreshold - enemy.StiffnessSystem.CurrentStiffness);
            if (fill > 0) enemy.StiffnessSystem.AddStiffness(fill, AttackType.Normal_0);
            player.FSM.ChangeState<PlayerHeavyCounterState>(); player.Combat.SetCharge(true);
            player.Events.TriggerCounterWindowStarted();
            int hpBefore = enemy.EnemyHealth.CurrentHealth, playerHp = player.Health.CurrentHealth;
            int stunBefore = Count(stunFeedback), strongBefore = Count(strongDamage);
            player.Health.TakeDamage(new DamageData(enemy.transform, AttackType.Strong_1, 1, 0));
            int confirmedHp = enemy.EnemyHealth.CurrentHealth, confirmedHits = hits.Count;
            int confirmedStun = Count(stunFeedback), confirmedStrong = Count(strongDamage), confirmedHeavy = heavy.PlayCount;
            bool stateActive = player.FSM.CurrentState is PlayerHeavyCounterState;
            bool confirmed = successes == 1 && successType == AttackType.Strong_Counter && hpBefore - confirmedHp == totalDamage &&
                player.Health.CurrentHealth == playerHp && enemy.ParrySystem.CurrentStun == StunType.Full &&
                confirmedStun > stunBefore && confirmedStrong > strongBefore && hits.Count == 1 && !Suppressed(hits[0]);
            player.Events.TriggerAttackPerformed();
            bool deduplicated = enemy.EnemyHealth.CurrentHealth == confirmedHp && hits.Count == confirmedHits &&
                Count(stunFeedback) == confirmedStun && Count(strongDamage) == confirmedStrong && heavy.PlayCount == confirmedHeavy;
            bool sourcesUnchanged = source == JsonUtility.ToJson(player.Data) && enemySource == JsonUtility.ToJson(enemy.enemyStat);
            passed = confirmed && stateActive && deduplicated && sourcesUnchanged;
            record?.Invoke(new { stage = "heavy-counter-reverse-order", name = "confirmed-counter-before-motion-no-duplicate", pass = passed,
                enemy = enemy.name, confirmed, stateActive, deduplicated, totalDamage, hpBefore, confirmedHp, hpAfterMotion = enemy.EnemyHealth.CurrentHealth,
                confirmedHits, hitsAfterMotion = hits.Count, currentStun = enemy.ParrySystem.CurrentStun.ToString(), sourcesUnchanged,
                scope = "Production incoming Health.TakeDamage then heavy-counter motion event/overlap. QA player positioned briefly; no natural button-timing claim." });
        }
        finally
        {
            player.Combat.AttackEvent -= Hit; player.Events.CounterSucceeded -= Succeeded;
            player.FSM.ChangeState<PlayerIdleState>();
            if (controller) controller.enabled = false;
            player.transform.SetPositionAndRotation(position, rotation);
            if (controller) controller.enabled = controllerEnabled;
            Physics.SyncTransforms();
        }
        if (!passed) throw new InvalidOperationException("Confirmed counter/motion deduplication regression.");
        yield break;
    }

    static Enemy FindEnemy() => UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
        .Where(e => e && e.name.IndexOf("Monster_f", StringComparison.OrdinalIgnoreCase) >= 0 &&
            e._initializer && e._initializer.IsInitialized("Final") && e.EnemyHealth && !e.EnemyHealth.IsDead &&
            e.ParrySystem && e.ParrySystem.CurrentStun == StunType.None && !e.Shield &&
            (!e.GetComponent<EnemySpawnIntro>() || e.GetComponent<EnemySpawnIntro>().Completed))
        .OrderByDescending(e => e.EnemyHealth.CurrentHealth).FirstOrDefault();

    static int Count(MMF_Player[] feedbacks) => feedbacks.Sum(f => f.PlayCount);

    static bool RunObjectChecks(PlayerController player, List<object> cases)
    {
        // Temporary production components in an empty, elevated QA pocket. These
        // checks accept the real overlap payload, not a handcrafted DamageData.
        var root = new GameObject("QA charged-counter objects");
        root.transform.position = player.transform.position + Vector3.up * 100f;
        var singleton = typeof(PuzzleGridManager).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        var oldGrid = PuzzleGridManager.Instance;
        ObjectTotem push = null;
        bool hardPass = false, pushPass = false;
        IDamageable observed = null; DamageData received = default; int receivedCount = 0;
        void Hit(IDamageable target, DamageData damage) { if (ReferenceEquals(target, observed)) { received = damage; receivedCount++; } }
        player.Combat.AttackEvent += Hit;
        try
        {
            var hardObject = new GameObject("QA hardened destructible");
            hardObject.SetActive(false); hardObject.transform.SetParent(root.transform, false);
            hardObject.layer = LayerMask.NameToLayer("HitObject");
            hardObject.AddComponent<BoxCollider>().size = Vector3.one * 2f;
            var hard = hardObject.AddComponent<TotemDestructibleObject>();
            Set(hard, "_destructibleType", TotemDestructibleType.Hardened);
            Set(hard, "_maxHealth", (int)player.Combat.HeavyCounterAttackConfig.Damage.Value);
            hardObject.SetActive(true);
            observed = hard;
            HitObject(player, hardObject.transform.position);
            hardPass = receivedCount == 1 && hard.IsDead && !Suppressed(received) &&
                received.AttackType == player.Combat.HeavyCounterAttackConfig.AttackType;
            cases.Add(new { name = "unconfirmed-motion-hard-object-break", pass = hardPass, receivedCount,
                isDead = hard.IsDead, attackType = received.AttackType.ToString(), received.DamageAmount, suppressed = Suppressed(received) });
            hardObject.SetActive(false);

            var gridObject = new GameObject("QA temporary grid"); gridObject.SetActive(false);
            gridObject.transform.SetParent(root.transform, false);
            var grid = gridObject.AddComponent<PuzzleGridManager>();
            Set(grid, "_gridSize", 3); Set(grid, "_cellSize", 2f); Set(grid, "_originPoint", gridObject.transform);
            singleton.SetValue(null, null); gridObject.SetActive(true);
            if (PuzzleGridManager.Instance != grid) throw new InvalidOperationException("Temporary puzzle grid did not initialize.");
            var pushObject = new GameObject("QA strong-only push object"); pushObject.SetActive(false);
            pushObject.transform.SetParent(root.transform, false); pushObject.layer = LayerMask.NameToLayer("HitObject");
            pushObject.AddComponent<BoxCollider>().size = Vector3.one * 2f;
            push = pushObject.AddComponent<ObjectTotem>();
            var start = new Vector2Int(1, 1); var destination = new Vector2Int(1, 2);
            Set(push, "_currentGridPos", start); Set(push, "_startGridPos", start);
            Set(push, "_receiveType", TotemReceiveType.Strong);
            Set(push, "feedback", new FeedbackPlayManager(Array.Empty<string>()));
            pushObject.transform.position = grid.GridToWorld(start);
            pushObject.SetActive(true); grid.RegisterTotem(push, start);
            observed = push; receivedCount = 0;
            HitObject(player, pushObject.transform.position);
            bool slideStarted = (TotemState)Get(push, "_state") == TotemState.Sliding;
            bool destinationReserved = grid.IsOccupied(destination) && !grid.IsOccupied(start);
            pushPass = receivedCount == 1 && slideStarted && destinationReserved && !Suppressed(received) &&
                received.AttackType == player.Combat.HeavyCounterAttackConfig.AttackType;
            cases.Add(new { name = "unconfirmed-motion-strong-only-object-slide-start", pass = pushPass, receivedCount,
                slideStarted, destinationReserved, attackType = received.AttackType.ToString(), suppressed = Suppressed(received),
                scope = "Actual production TakeDamage/SlideToPosition start and grid reservation, not full slide animation." });
        }
        finally
        {
            player.Combat.AttackEvent -= Hit;
            if (push) { push.StopAllCoroutines(); DOTween.Kill(push.transform); }
            singleton.SetValue(null, oldGrid);
            root.SetActive(false); UnityEngine.Object.Destroy(root);
        }
        return hardPass && pushPass;
    }

    static void HitObject(PlayerController player, Vector3 target)
    {
        player.FSM.ChangeState<PlayerIdleState>();
        player.transform.SetPositionAndRotation(target - Vector3.forward * 2f, Quaternion.identity);
        Physics.SyncTransforms();
        player.FSM.ChangeState<PlayerHeavyCounterState>();
        player.Combat.SetCharge(true);
        player.Events.TriggerAttackPerformed();
    }
    static FieldInfo Field(Type type, string name)
    {
        while (type != null) { var field = type.GetField(name, Private); if (field != null) return field; type = type.BaseType; }
        throw new MissingFieldException(name);
    }
    static object Get(object owner, string name) => Field(owner.GetType(), name).GetValue(owner);
    static void Set(object owner, string name, object value) => Field(owner.GetType(), name).SetValue(owner, value);
    static bool Suppressed(DamageData damage)
    {
        var field = typeof(DamageData).GetField("SuppressHeavyHitFeedback");
        return field != null && (bool)field.GetValue(damage);
    }
    static void RequireIsolatedSave()
    {
        if (!Application.isPlaying || !DataManager.Instance) throw new InvalidOperationException("Play Mode isolated QA only.");
        string save = (string)typeof(DataManager).GetField("_saveFileName", Private).GetValue(DataManager.Instance);
        string userPath = Path.GetFullPath(Application.persistentDataPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (string.IsNullOrEmpty(save) || !Path.IsPathRooted(save) || !save.EndsWith(".smoke-save.json", StringComparison.OrdinalIgnoreCase) ||
            Path.GetFullPath(save).StartsWith(userPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Refusing damage outside the root's isolated smoke save.");
    }
}
