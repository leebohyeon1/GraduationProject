using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>Opt-in isolated-save guard regression. Calls the real player hit-processing
/// pipeline with an authored normal attack and an actual scene enemy collider.
/// This is explicit gameplay API injection, not natural input/animation validation.</summary>
public static class GuardStiffnessRuntimeAudit
{
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    const string Scope = "Isolated QA: actual scene enemy + authored normal attack through PlayerCombat.ProcessHitEnemiesInternal. Temporary activation of existing inactive ancestors, real proximity intro/interaction, player reposition and runtime Shield.IsActive fixture; no input/attack-animation claim, enemy HP/gauge/stat assignments, or user-save writes.";
    static readonly MethodInfo ProcessHits = typeof(PlayerCombat).GetMethod("ProcessHitEnemiesInternal", Private);
    static bool _part1Observed;
    static bool _allPassed;
    static int _caseCount;
    static bool _counterAllPassed;
    static int _counterCaseCount;

    public static IEnumerator Run(PlayerController player, Action<object> record)
    {
        RequireIsolatedSave();
        var loader = SceneLoadingManager.Instance;
        if (!player || !player.Health || player.Health.IsDead || !loader || loader.CurrentActiveChunk == null || ProcessHits == null)
            throw new InvalidOperationException("Guard audit needs a living initialized player and the real hit-processing API.");
        string scene = loader.CurrentActiveChunk.name;
        bool gordian = scene == "Part2-C";
        if (scene != "Part1" && !gordian) throw new InvalidOperationException("Guard audit is for Part1 and Part2-C only.");
        if (!gordian) { _part1Observed = false; _allPassed = true; _caseCount = 0; _counterAllPassed = true; _counterCaseCount = 0; }

        var enemies = UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var enemy = enemies.Where(e => e && e.gameObject.scene.IsValid() && e.gameObject.scene.isLoaded && e.GetComponent<EnemyShield>() &&
                (gordian ? string.Equals(e.name, "Gordian", StringComparison.OrdinalIgnoreCase) :
                    !string.Equals(e.name, "Gordian", StringComparison.OrdinalIgnoreCase) && !e.GetComponent<EnemyInteract>()))
            .OrderByDescending(e => e.gameObject.activeInHierarchy && e._initializer && e._initializer.IsInitialized("Final"))
            .ThenBy(e => Vector3.Distance(player.transform.position, e.transform.position)).FirstOrDefault();
        if (!enemy) throw new InvalidOperationException("Actual " + (gordian ? "Gordian" : "ordinary shield E") + " is unavailable in the loaded scene.");
        var shield = enemy.GetComponent<EnemyShield>();
        bool oldShield = shield.IsActive;
        var activated = new List<GameObject>();
        for (Transform item = enemy.transform; item; item = item.parent)
            if (!item.gameObject.activeSelf) activated.Add(item.gameObject);
        var controller = player.GetComponent<CharacterController>();
        bool oldController = controller && controller.enabled;
        Vector3 oldPosition = player.transform.position; Quaternion oldRotation = player.transform.rotation;
        var events = player.Events;
        int protectedHits = 0;
        void Protect(ref PlayerDamageContext context)
        {
            protectedHits++;
            context.Data.DamageAmount = 0; context.Data.StiffnessAmount = 0;
            context.Data.KnockbackForce = 0; context.Data.KnockbackDuration = 0;
            context.HasSuperArmor = true;
        }
        events.BeforeDamaged += Protect;
        try
        {
            record?.Invoke(new { stage = "guard-stiffness-activation", scene, enemy = enemy.name,
                activated = activated.Select(item => new { path = PathOf(item.transform), wasActive = false }).ToArray(), scope = Scope });
            for (int i = activated.Count - 1; i >= 0; i--) activated[i].SetActive(true);
            var initializer = enemy.GetComponent<EnemyInitializer>();
            if (!initializer) throw new InvalidOperationException("Shield enemy initializer is missing.");
            initializer.Initialize();
            double deadline = Time.realtimeSinceStartupAsDouble + 25d;
            while (!initializer.IsInitialized("Final"))
            {
                if (Time.realtimeSinceStartupAsDouble > deadline) throw new TimeoutException("Shield enemy Final initialization timed out.");
                yield return null;
            }
            MovePlayer(player, controller, enemy, true);
            if (controller) controller.enabled = oldController;
            if (enemy.Interact && !enemy.Interact._isInteracted) enemy.Interact.Interact();
            var intro = enemy.GetComponent<EnemySpawnIntro>();
            while ((intro && !intro.Completed) ||
                   (enemy.Interact && !enemy._aiController._aiBrain.blackboard.GetValueOrDefault<bool>(EnemyBlackboardKeys.InteractNoneHIt, false)))
            {
                if (Time.realtimeSinceStartupAsDouble > deadline) throw new TimeoutException("Shield enemy actual intro/interaction immunity did not finish.");
                yield return null;
            }
            if (enemy.EnemyHealth.IsDead || enemy.ParrySystem.CurrentStun != StunType.None)
                throw new InvalidOperationException("Guard audit requires a living, currently unstunned enemy.");
            var config = player.Combat.NormalAttackConfigList.FirstOrDefault(a => a != null && a.AttackType < AttackType.Normal_Counter);
            var collider = enemy.GetComponents<Collider>().FirstOrDefault(c => c && !c.isTrigger);
            if (config == null || !collider || !ReferenceEquals(collider.GetComponent<EnemyHealth>(), enemy.EnemyHealth))
                throw new InvalidOperationException("Authored normal attack / real damage collider missing.");
            int rawDamage = (int)config.Damage.Value, rawStiffness = (int)config.Stiffness.Value;
            if (rawDamage <= 0 || rawStiffness <= 0 || enemy.EnemyHealth.CurrentHealth <= rawDamage * 3)
                throw new InvalidOperationException("Guard fixture needs positive authored damage/gauge and enough unmodified enemy HP.");
            string playerSource = JsonUtility.ToJson(player.Data), enemySource = JsonUtility.ToJson(enemy.enemyStat);
            int playerHealth = player.Health.CurrentHealth;
            var cases = new List<object>(); bool passed = true;
            record?.Invoke(new { stage = "guard-stiffness-ready", scene, enemy = enemy.name, enemyPath = PathOf(enemy.transform),
                shield.DamageReduction, shield.BlockAngle, rawDamage, rawStiffness, attackType = config.AttackType.ToString(),
                enemyHealth = enemy.EnemyHealth.CurrentHealth, gauge = enemy.StiffnessSystem.CurrentStiffness, scope = Scope });

            // Deliberately synchronous: the AI cannot rotate/re-enable the shield between
            // pose setup, the block snapshot and the production damage/gauge call.
            foreach (string name in new[] { "front-active", "back-active", "front-disabled" })
            {
                bool front = name != "back-active";
                shield.IsActive = name != "front-disabled";
                MovePlayer(player, controller, enemy, front);
                bool blocked = shield.CheckBlock(player.transform.position);
                int hpBefore = enemy.EnemyHealth.CurrentHealth, gaugeBefore = enemy.StiffnessSystem.CurrentStiffness;
                int hitCount = 0; DamageData observed = default;
                void Hit(IDamageable target, DamageData data)
                {
                    if (ReferenceEquals(target, enemy.EnemyHealth)) { hitCount++; observed = data; }
                }
                player.Combat.AttackEvent += Hit;
                try { ProcessHits.Invoke(player.Combat, new object[] { config, new[] { collider }, null, false }); }
                finally { player.Combat.AttackEvent -= Hit; }
                float multiplier = blocked ? 1f - shield.DamageReduction : 1f;
                int expectedHpLoss = Mathf.RoundToInt(observed.DamageAmount * multiplier);
                int expectedGaugeGain = Mathf.RoundToInt(rawStiffness * multiplier);
                int hpLoss = hpBefore - enemy.EnemyHealth.CurrentHealth;
                int gaugeGain = enemy.StiffnessSystem.CurrentStiffness - gaugeBefore;
                bool pass = blocked == (name == "front-active") && hitCount == 1 && observed.DamageAmount > 0 &&
                    hpLoss == expectedHpLoss && gaugeGain == expectedGaugeGain && enemy.ParrySystem.CurrentStun == StunType.None;
                passed &= pass; _caseCount++;
                var result = new { name, pass, blocked, hitCount, authoredDamage = rawDamage, appliedRawDamage = observed.DamageAmount,
                    rawStiffness, multiplier, hpBefore, hpAfter = enemy.EnemyHealth.CurrentHealth, hpLoss, expectedHpLoss,
                    gaugeBefore, gaugeAfter = enemy.StiffnessSystem.CurrentStiffness, gaugeGain, expectedGaugeGain,
                    stun = enemy.ParrySystem.CurrentStun.ToString() };
                cases.Add(result);
                record?.Invoke(new { stage = "guard-stiffness-case", scene, enemy = enemy.name, result, scope = Scope });
            }
            // Run synchronously with the real counter handler, without the QA damage
            // protector masking a missed counter. No AI frame runs during this call.
            bool counterPassed;
            events.BeforeDamaged -= Protect;
            try { counterPassed = RunCounterCase(player, controller, enemy, shield, collider, scene, record); }
            finally { events.BeforeDamaged += Protect; }
            _counterAllPassed &= counterPassed;
            _counterCaseCount++;
            bool unchanged = playerSource == JsonUtility.ToJson(player.Data) && enemySource == JsonUtility.ToJson(enemy.enemyStat);
            passed &= unchanged && player.Health.CurrentHealth == playerHealth;
            _allPassed &= passed;
            if (!gordian) _part1Observed = true;
            record?.Invoke(new { stage = "guard-stiffness-scene", scene, enemy = enemy.name, pass = passed,
                sourceStatsUnchanged = unchanged, protectedPlayerHealthUnchanged = player.Health.CurrentHealth == playerHealth,
                protectedHits, cases, scope = Scope });
            if (gordian)
            {
                bool complete = _part1Observed && _allPassed && _caseCount == 6;
                bool counterComplete = _part1Observed && _counterAllPassed && _counterCaseCount == 2 && unchanged;
                record?.Invoke(new { stage = "guard-stiffness-complete", pass = complete, ordinaryEObserved = _part1Observed,
                    gordianObserved = true, cases = _caseCount, counterCases = _counterCaseCount, counterPass = counterComplete, scope = Scope });
                record?.Invoke(new { stage = "guard-counter-complete", pass = counterComplete, cases = _counterCaseCount,
                    ordinaryEObserved = _part1Observed, gordianObserved = true });
                if (!complete || !counterComplete) throw new InvalidOperationException("Guard HP/stiffness/counter regression: inspect both scene case records.");
            }
        }
        finally
        {
            events.BeforeDamaged -= Protect;
            if (shield) shield.IsActive = oldShield;
            if (player)
            {
                if (controller) controller.enabled = false;
                player.transform.SetPositionAndRotation(oldPosition, oldRotation);
                if (controller) controller.enabled = oldController;
                Physics.SyncTransforms();
            }
            foreach (var item in activated) if (item) item.SetActive(false);
        }
    }

    static bool RunCounterCase(PlayerController player, CharacterController controller, Enemy enemy,
        EnemyShield shield, Collider collider, string scene, Action<object> record)
    {
        const string counterScope = "Actual HeavyCounter FSM + counter window + incoming PlayerHealth.TakeDamage, followed by the real motion event/overlap. QA guard active/front pose; damage protector temporarily removed. No HP/gauge/stat assignments or natural-input claim.";
        var data = player.Combat.HeavyCounterAttackConfig;
        int rawDamage = (int)WithCounterModifier(data.Damage, player.Data.CounterDamageMultiply[1]);
        int rawStiffness = Mathf.RoundToInt(WithCounterModifier(data.Stiffness, player.Data.CounterStiffnessMultiply[1]));
        float multiplier = 1f - shield.DamageReduction;
        int expectedHpLoss = Mathf.RoundToInt(rawDamage * multiplier);
        int expectedStiffness = Mathf.RoundToInt(rawStiffness * multiplier);
        int hpBefore = enemy.EnemyHealth.CurrentHealth, gaugeBefore = enemy.StiffnessSystem.CurrentStiffness;
        int playerHpBefore = player.Health.CurrentHealth;
        int threshold = enemy.StiffnessSystem.StiffnessThreshold;
        StunType expectedStun = gaugeBefore + expectedStiffness >= threshold ? StunType.Full : StunType.Weak;
        int expectedGauge = expectedStun == StunType.Full ? 0 : gaugeBefore + expectedStiffness;
        record?.Invoke(new { stage = "guard-counter-inputs", scene, enemy = enemy.name, rawDamage, rawStiffness,
            multiplier, expectedHpLoss, expectedStiffness, hpBefore, gaugeBefore, threshold, expectedGauge,
            expectedStun = expectedStun.ToString(), scope = counterScope });
        if (hpBefore <= expectedHpLoss || playerHpBefore <= 1 || enemy.ParrySystem.CurrentStun != StunType.None)
            throw new InvalidOperationException("Guard counter needs an unstunned target/player that survive authored damage.");
        int hitCount = 0, successes = 0;
        DamageData observed = default; AttackType successType = AttackType.Normal_0;
        void Hit(IDamageable target, DamageData damage)
        { if (ReferenceEquals(target, enemy.EnemyHealth)) { hitCount++; observed = damage; } }
        void Succeeded(Transform target, AttackType type)
        { if (target == enemy.transform) { successes++; successType = type; } }
        player.Combat.AttackEvent += Hit;
        player.Events.CounterSucceeded += Succeeded;
        try
        {
            player.FSM.ChangeState<PlayerIdleState>();
            shield.IsActive = true;
            MovePlayer(player, controller, enemy, true);
            bool blocked = shield.CheckBlock(player.transform.position);
            player.FSM.ChangeState<PlayerHeavyCounterState>();
            player.Combat.SetCharge(true);
            player.Events.TriggerCounterWindowStarted();
            player.Health.TakeDamage(new DamageData(enemy.transform, AttackType.Strong_1, 1, 0));
            int confirmedHp = enemy.EnemyHealth.CurrentHealth, confirmedGauge = enemy.StiffnessSystem.CurrentStiffness;
            int confirmedHits = hitCount;
            StunType confirmedStun = enemy.ParrySystem.CurrentStun;
            bool confirmed = blocked && successes == 1 && successType == AttackType.Strong_Counter &&
                hitCount == 1 && observed.AttackType == AttackType.Strong_Counter && observed.DamageAmount == rawDamage &&
                !observed.SuppressHeavyHitFeedback && hpBefore - confirmedHp == expectedHpLoss &&
                confirmedGauge == expectedGauge && confirmedStun == expectedStun && player.Health.CurrentHealth == playerHpBefore;
            bool stateActive = player.FSM.CurrentState is PlayerHeavyCounterState;
            bool motionOverlapContainsTarget = Physics.OverlapBox(player.Combat.GetAttackCenter(data), data.AttackRadius / 2f,
                player.transform.rotation, player.Data.AttackLayerMask, QueryTriggerInteraction.Collide).Contains(collider);
            player.Events.TriggerAttackPerformed();
            bool deduplicated = enemy.EnemyHealth.CurrentHealth == confirmedHp && enemy.StiffnessSystem.CurrentStiffness == confirmedGauge &&
                enemy.ParrySystem.CurrentStun == confirmedStun && hitCount == confirmedHits && successes == 1;
            bool pass = confirmed && stateActive && motionOverlapContainsTarget && deduplicated;
            record?.Invoke(new { stage = "guard-counter-case", scene, enemy = enemy.name,
                name = "front-active-confirmed-counter-before-motion", pass, confirmed, blocked, stateActive, motionOverlapContainsTarget,
                deduplicated, successes, successType = successType.ToString(), rawDamage, rawStiffness, multiplier,
                expectedHpLoss, expectedStiffness, hpBefore, confirmedHp, hpAfterMotion = enemy.EnemyHealth.CurrentHealth,
                gaugeBefore, confirmedGauge, gaugeAfterMotion = enemy.StiffnessSystem.CurrentStiffness, expectedGauge,
                threshold, expectedStun = expectedStun.ToString(), confirmedStun = confirmedStun.ToString(),
                confirmedHits, hitsAfterMotion = hitCount, playerHpBefore, playerHpAfter = player.Health.CurrentHealth, scope = counterScope });
            return pass;
        }
        finally
        {
            player.Combat.AttackEvent -= Hit;
            player.Events.CounterSucceeded -= Succeeded;
            player.FSM.ChangeState<PlayerIdleState>();
        }
    }

    static float WithCounterModifier(Stat original, float amount)
    {
        // A detached value calculation preserves any authored/current modifiers;
        // the live attack stat is changed only by its production counter handler.
        var expected = new Stat(() => original.BaseValue);
        foreach (var modifier in original.Modifiers) expected.AddModifier(modifier);
        expected.AddModifier(new StatModifier(amount, StatModifierType.PercentAdd, "Guard audit expectation"));
        return expected.Value;
    }

    static void MovePlayer(PlayerController player, CharacterController controller, Enemy enemy, bool front)
    {
        Vector3 forward = enemy.transform.forward; forward.y = 0f;
        if (forward.sqrMagnitude < .001f) forward = Vector3.forward;
        forward.Normalize();
        if (controller) controller.enabled = false;
        Vector3 position = enemy.transform.position + forward * (front ? 2f : -2f);
        player.transform.SetPositionAndRotation(position, Quaternion.LookRotation(front ? -forward : forward));
        Physics.SyncTransforms();
    }

    static string PathOf(Transform item) => item.parent ? PathOf(item.parent) + "/" + item.name : item.name;

    static void RequireIsolatedSave()
    {
        var args = SceneLoadingSmokeTest.GetArguments();
        if (!args.Contains("--soom-loading-smoke") || !args.Contains("--soom-guard-stiffness-audit") || !Application.isPlaying || !DataManager.Instance)
            throw new InvalidOperationException("Explicit guard-stiffness isolated Play QA flags required.");
        string save = (string)typeof(DataManager).GetField("_saveFileName", Private).GetValue(DataManager.Instance);
        int outputIndex = Array.IndexOf(args, "--soom-smoke-output");
        if (outputIndex < 0 || outputIndex + 1 >= args.Length || !Path.IsPathRooted(args[outputIndex + 1]))
            throw new InvalidOperationException("Guard audit requires an explicit absolute smoke report path.");
        string reportDirectory = Path.GetDirectoryName(Path.GetFullPath(args[outputIndex + 1]));
        string user = Path.GetFullPath(Application.persistentDataPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (string.IsNullOrEmpty(save) || !Path.IsPathRooted(save) || !save.EndsWith(".smoke-save.json", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(Path.GetDirectoryName(Path.GetFullPath(save)), reportDirectory, StringComparison.OrdinalIgnoreCase) ||
            Path.GetFullPath(save).StartsWith(user, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Guard audit refuses any non-isolated/user save path.");
    }
}
