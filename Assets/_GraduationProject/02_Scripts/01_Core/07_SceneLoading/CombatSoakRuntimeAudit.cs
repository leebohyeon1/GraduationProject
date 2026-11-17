using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BehaviorTree;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Explicit native QA only, called after the smoke runner redirects all saves.
/// Sustained real AI/projectile observation against a protected QA target.
/// This is NOT a validation of actual damage received by the player.
/// </summary>
public static class CombatSoakRuntimeAudit
{
    private const string Scope = "Actual AI attacks/projectiles against an invulnerable QA player; NOT player damage validation. Player-only grounded setup teleport; no AI/FSM/stat/SO overrides.";
    private static readonly FieldInfo PoolInstancesField = typeof(ProjectilePoolManager).GetField("InstanceKeys", BindingFlags.Static | BindingFlags.NonPublic);

    public static IEnumerator Run(PlayerController player, int seconds, Action<object> record)
    {
        string[] args = SceneLoadingSmokeTest.GetArguments();
        if (!args.Contains("--soom-loading-smoke") || !args.Contains("--soom-combat-soak-seconds"))
            throw new InvalidOperationException("Combat soak requires --soom-loading-smoke and --soom-combat-soak-seconds.");
        RequirePlayer(player);
        if (player.InputReader.CurrentInputMode != InputReaderSO.InputMode.Gameplay)
            throw new InvalidOperationException("Combat soak requires naturally enabled Gameplay input.");
        int duration = Mathf.Clamp(seconds, 30, 600);
        Enemy[] sceneEnemies = UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Enemy anchor = sceneEnemies.FirstOrDefault(e => e && e.gameObject.activeInHierarchy &&
            e.name.IndexOf("Monster_c", StringComparison.OrdinalIgnoreCase) >= 0 &&
            e.GetComponentInParent<GroupAiZone>() is GroupAiZone zone &&
            zone.name.Replace("_", "").Replace(" ", "").ToLowerInvariant().EndsWith("zone5"));
        if (!anchor) throw new InvalidOperationException("No authored Zone 5 Monster C found for combat soak.");

        var events = player.Events;
        int initialHealth = player.Health.CurrentHealth;
        string sourcePlayerData = JsonUtility.ToJson(player.Data);
        var sourceEnemyData = new Dictionary<EnemyStat, string>();
        foreach (var enemy in sceneEnemies)
            if (enemy && enemy.enemyStat && !sourceEnemyData.ContainsKey(enemy.enemyStat))
                sourceEnemyData.Add(enemy.enemyStat, JsonUtility.ToJson(enemy.enemyStat));

        var incoming = new Dictionary<string, DamageTally>();
        var attackEntries = new Dictionary<string, int>();
        var hitWindows = new Dictionary<string, int>();
        var windowListeners = new Dictionary<Enemy_AnimationEventHandler, Action>();
        var previousPoolActive = new Dictionary<int, bool>();
        var projectileActivations = new Dictionary<string, int>();
        int interceptedContacts = 0, positiveDamageContacts = 0;
        long preventedDamage = 0;
        double started = 0d;
        int startNodeCount = 0, stableNodeMin = int.MaxValue, stableNodeMax = 0;
        int peakActiveProjectiles = 0, peakTrackedPoolObjects = 0;
        bool completed = false;

        // Installed AFTER ordinary components, and BEFORE teleporting into range.
        // These five values match the project's Invincible/RevivalProtection path.
        // The contextual DamageData is a value copy; the attack configuration and
        // player health/stat fields are never assigned by this QA helper.
        void Protect(ref PlayerDamageContext context)
        {
            DamageData before = context.Data;
            Enemy source = before.SourceEnemy;
            if (!source && before.AttackerTransform) source = before.AttackerTransform.GetComponentInParent<Enemy>();
            string sourceName = source ? source.name + "#" + source.GetInstanceID() : "unknown";
            string key = sourceName + "/" + before.AttackType;
            if (!incoming.TryGetValue(key, out var tally)) incoming.Add(key, tally = new DamageTally());
            tally.contacts++;
            tally.damageBeforeQaGuard += Math.Max(0, before.DamageAmount);
            tally.lastFrame = Time.frameCount;
            interceptedContacts++;
            if (before.DamageAmount > 0) positiveDamageContacts++;
            preventedDamage += Math.Max(0, before.DamageAmount);

            context.Data.DamageAmount = 0;
            context.Data.StiffnessAmount = 0;
            context.Data.KnockbackDuration = 0f;
            context.Data.KnockbackForce = 0f;
            context.HasSuperArmor = true;
        }

        void ObserveLog(string condition, string stack, LogType type)
        {
            // BaseAttackNode emits this only after its real execution gates pass,
            // animation is requested, and the actual attack state is entered.
            if (condition.StartsWith("[Attack Node Enter] ", StringComparison.Ordinal)) Increment(attackEntries, condition);
        }

        bool SourceDataUnchanged()
        {
            if (!player || sourcePlayerData != JsonUtility.ToJson(player.Data)) return false;
            foreach (var pair in sourceEnemyData)
                if (!pair.Key || pair.Value != JsonUtility.ToJson(pair.Key)) return false;
            return true;
        }

        void SubscribeAvailableEnemies()
        {
            foreach (var enemy in sceneEnemies)
            {
                if (!enemy || enemy._initializer == null || !enemy._initializer.IsInitialized("Final")) continue;
                var handler = enemy.animHandler;
                if (!handler || windowListeners.ContainsKey(handler)) continue;
                string identity = enemy.name + "#" + enemy.GetInstanceID();
                Action onWindow = () => Increment(hitWindows, identity);
                handler.HitWindowOpened += onWindow;
                windowListeners.Add(handler, onWindow);
            }
        }

        void ObservePoolActivations()
        {
            // No per-frame FindObjects or allocations. This is a read-only view
            // of the existing small pool map; it never creates or releases a shot.
            var map = PoolInstancesField?.GetValue(null) as Dictionary<GameObject, long>;
            if (map == null) return;
            int active = 0;
            foreach (var pair in map)
            {
                GameObject instance = pair.Key;
                if (!instance) continue;
                bool isActive = instance.activeInHierarchy;
                if (isActive) active++;
                int id = instance.GetInstanceID();
                if (isActive && (!previousPoolActive.TryGetValue(id, out bool wasActive) || !wasActive))
                {
                    string family = instance.GetComponent<HomingProjectile>() ? "HomingProjectile" :
                        instance.GetComponent<ParabolicMineProjectile>() ? "ParabolicMineProjectile" :
                        instance.GetComponent<EnemyProjectile>() ? "EnemyProjectile" : "other";
                    Increment(projectileActivations, family);
                }
                previousPoolActive[id] = isActive;
            }
            peakActiveProjectiles = Mathf.Max(peakActiveProjectiles, active);
            peakTrackedPoolObjects = Mathf.Max(peakTrackedPoolObjects, map.Count);
        }

        void Sample(string stage)
        {
            int nodes = Resources.FindObjectsOfTypeAll<Node>().Length;
            double elapsed = started <= 0d ? 0d : Time.realtimeSinceStartupAsDouble - started;
            if (elapsed >= 60d) { stableNodeMin = Mathf.Min(stableNodeMin, nodes); stableNodeMax = Mathf.Max(stableNodeMax, nodes); }
            int activeAi = 0;
            foreach (var enemy in sceneEnemies)
                if (enemy && enemy._aiController && enemy._aiController.isActiveAndEnabled) activeAi++;

            int simpleShots = UnityEngine.Object.FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None).Length;
            int homingShots = UnityEngine.Object.FindObjectsByType<HomingProjectile>(FindObjectsSortMode.None).Length;
            int mineShots = UnityEngine.Object.FindObjectsByType<ParabolicMineProjectile>(FindObjectsSortMode.None).Length;
            var map = PoolInstancesField?.GetValue(null) as Dictionary<GameObject, long>;
            int tracked = map?.Count ?? 0, activePooled = 0, destroyedWrappers = 0;
            if (map != null)
                foreach (var pair in map)
                    if (!pair.Key) destroyedWrappers++;
                    else if (pair.Key.activeInHierarchy) activePooled++;
            long allocated = Profiler.GetTotalAllocatedMemoryLong();
            long managed = Profiler.GetMonoUsedSizeLong();
            bool unchanged = SourceDataUnchanged();
            object anchorDiagnostics = null;
            if (anchor)
            {
                var board = anchor._aiController?._aiBrain?.blackboard;
                var path = anchor.aIPath;
                var zone = anchor.GetComponentInParent<GroupAiZone>();
                var controller = anchor.GetComponent<CharacterController>();
                Vector3 anchorPosition = anchor.transform.position;
                Vector3 toPlayer = player.transform.position - anchorPosition;
                bool groundFound = Physics.Raycast(anchorPosition + Vector3.up * 2f, Vector3.down,
                    out RaycastHit ground, 12f, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore);
                var nearest = AstarPath.active != null
                    ? AstarPath.active.GetNearest(anchorPosition, Pathfinding.NNConstraint.Default)
                    : default(Pathfinding.NNInfo);
                anchorDiagnostics = new
                {
                    identity = anchor.name + "#" + anchor.GetInstanceID(),
                    position = anchorPosition.ToString("F3"),
                    forward = anchor.transform.forward.ToString("F3"),
                    active = anchor.gameObject.activeInHierarchy,
                    aiEnabled = anchor._aiController && anchor._aiController.isActiveAndEnabled,
                    currentState = anchor.CurrentState.ToString(),
                    stateLocked = anchor._stateController?.IsStateLocked,
                    bridgeIsAttacking = anchor._animationBridge?.IsAttacking,
                    actionFinished = anchor.animHandler ? (bool?)anchor.animHandler.IsActionFinished : null,
                    hitWindowOpen = anchor.animHandler ? (bool?)anchor.animHandler.IsHitWindowOpen : null,
                    combat = anchor._aiController?._aiBrain?._isCombat,
                    hasAttackToken = board != null && board.HasKey("HasAttackToken") ? (bool?)board.GetValue<bool>("HasAttackToken") : null,
                    blackboardIsAttacking = board != null && board.HasKey("IsAttacking") ? (bool?)board.GetValue<bool>("IsAttacking") : null,
                    blackboardDistance = board != null && board.HasKey("DistanceToTarget") ? (float?)board.GetValue<float>("DistanceToTarget") : null,
                    pressurePosition = board != null && board.HasKey("PressurePos") ? board.GetValue<Vector3>("PressurePos").ToString("F3") : null,
                    distanceToPlayer = toPlayer.magnitude,
                    horizontalDistanceToPlayer = new Vector2(toPlayer.x, toPlayer.z).magnitude,
                    playerHeightOffset = toPlayer.y,
                    zone = zone ? zone.name : null,
                    anchorInZone = zone ? (bool?)zone.IsInZone(anchorPosition) : null,
                    playerInZone = zone ? (bool?)zone.IsInZone(player.transform.position) : null,
                    controllerGrounded = controller && controller.enabled ? (bool?)controller.isGrounded : null,
                    groundFound,
                    groundObject = groundFound ? ground.collider.name : null,
                    groundPosition = groundFound ? ground.point.ToString("F3") : null,
                    rootAboveGround = groundFound ? (float?)(anchorPosition.y - ground.point.y) : null,
                    controllerBottomAboveGround = groundFound && controller && controller.enabled ? (float?)(controller.bounds.min.y - ground.point.y) : null,
                    nearestNodeWalkable = nearest.node != null ? (bool?)nearest.node.Walkable : null,
                    nearestNodePosition = nearest.node != null ? nearest.position.ToString("F3") : null,
                    nearestNodeDistance = nearest.node != null ? (float?)Vector3.Distance(anchorPosition, nearest.position) : null,
                    aiPath = path == null ? null : new
                    {
                        path.enabled, path.canMove, path.isStopped, path.maxSpeed,
                        path.hasPath, path.pathPending, path.reachedEndOfPath,
                        remainingDistance = float.IsInfinity(path.remainingDistance) || float.IsNaN(path.remainingDistance) ? (float?)null : path.remainingDistance,
                        destination = path.destination.ToString("F3"),
                        velocity = path.velocity.ToString("F3")
                    }
                };
            }
            record?.Invoke(new
            {
                stage, scope = Scope, scene = player.gameObject.scene.name, durationSeconds = duration,
                elapsedSeconds = elapsed, frame = Time.frameCount, timeScale = Time.timeScale,
                playerHealth = player.Health.CurrentHealth, initialPlayerHealth = initialHealth,
                playerPosition = player.transform.position.ToString("F3"), activeAiControllerCount = activeAi,
                anchorDiagnostics,
                nodeCount = nodes, startNodeCount, nodeDelta = nodes - startNodeCount,
                stableWindowNodeMin = stableNodeMin == int.MaxValue ? (int?)null : stableNodeMin,
                stableWindowNodeMax = stableNodeMin == int.MaxValue ? (int?)null : stableNodeMax,
                activeEnemyProjectiles = simpleShots, activeHomingProjectiles = homingShots, activeMineProjectiles = mineShots,
                activeAllProjectileComponents = simpleShots + homingShots + mineShots,
                poolTrackingAvailable = map != null, trackedPoolObjects = tracked, activePooledObjects = activePooled,
                inactivePooledObjects = tracked - activePooled - destroyedWrappers, destroyedWrappers,
                peakActiveProjectiles, peakTrackedPoolObjects,
                observedPoolActivationLowerBounds = new Dictionary<string, int>(projectileActivations),
                activationCountLimit = "Frame sampling may miss objects activated and released within one frame; counts are lower bounds.",
                interceptedContacts, positiveDamageContacts, preventedDamage,
                incomingAfterGameplayModifiersBeforeQaGuard = CopyTallies(incoming),
                actualBaseAttackEntriesByEnemyAndPattern = new Dictionary<string, int>(attackEntries),
                animationHitWindowsByEnemy = new Dictionary<string, int>(hitWindows),
                sourceDataUnchanged = unchanged,
                unityAllocatedBytes = allocated, unityReservedBytes = Profiler.GetTotalReservedMemoryLong(),
                managedUsedBytes = managed, nativeEstimateBytes = Math.Max(0L, allocated - managed)
            });
            if (!unchanged) throw new InvalidOperationException("Authored player/enemy data changed during combat soak.");
            if (activeAi == 0) throw new InvalidOperationException("Combat soak has no active AI.");
        }

        events.BeforeDamaged += Protect;
        Application.logMessageReceived += ObserveLog;
        try
        {
            MoveToZoneFive(player, anchor, record);
            double activationStarted = Time.realtimeSinceStartupAsDouble;
            while (anchor && (anchor._initializer == null || !anchor._initializer.IsInitialized("Final") ||
                !anchor._aiController || !anchor._aiController.isActiveAndEnabled || !anchor.EnemyHealth))
            {
                RequirePlayer(player);
                if (Time.realtimeSinceStartupAsDouble - activationStarted > 15d)
                    throw new TimeoutException("Zone 5 Monster C did not activate naturally.");
                yield return null;
            }
            if (!anchor || anchor.EnemyHealth.IsDead) throw new InvalidOperationException("Zone 5 Monster C is unavailable/dead.");
            SubscribeAvailableEnemies();
            startNodeCount = Resources.FindObjectsOfTypeAll<Node>().Length;
            started = Time.realtimeSinceStartupAsDouble;
            double nextSample = 30d, nextSubscriptions = 1d;
            double zeroScaleStarted = -1d;
            Sample("combat-soak-start");
            while (Time.realtimeSinceStartupAsDouble - started < duration)
            {
                RequirePlayer(player);
                if (Time.timeScale <= 0f)
                {
                    if (zeroScaleStarted < 0d) zeroScaleStarted = Time.realtimeSinceStartupAsDouble;
                    if (Time.realtimeSinceStartupAsDouble - zeroScaleStarted > 2d)
                        throw new InvalidOperationException("Combat soak stalled at zero time scale for more than two seconds.");
                }
                else zeroScaleStarted = -1d;
                if (player.InputReader.CurrentInputMode != InputReaderSO.InputMode.Gameplay)
                    throw new InvalidOperationException("Gameplay input left its normal mode during combat soak.");
                ObservePoolActivations();
                double elapsed = Time.realtimeSinceStartupAsDouble - started;
                if (elapsed >= nextSubscriptions)
                {
                    SubscribeAvailableEnemies();
                    nextSubscriptions = Math.Floor(elapsed) + 1d;
                }
                if (elapsed >= nextSample)
                {
                    Sample("combat-soak-sample");
                    nextSample = (Math.Floor(elapsed / 30d) + 1d) * 30d;
                }
                yield return null;
            }
            Sample("combat-soak-final");
            completed = true;
            bool combatObserved = attackEntries.Count > 0 && positiveDamageContacts > 0;
            bool projectilesObserved = projectileActivations.Count > 0;
            bool hpUnchanged = player.Health.CurrentHealth == initialHealth;
            bool coveragePass = combatObserved && projectilesObserved && hpUnchanged;
            record?.Invoke(new { stage = "combat-soak-completed", completed, coveragePass, scope = Scope,
                passScope = "Sustained combat/projectile execution coverage only. Node/memory slopes need separate evaluation; this is not proof of receiving damage or absence of all leaks.",
                durationSeconds = duration, elapsedSeconds = Time.realtimeSinceStartupAsDouble - started,
                combatObserved, projectilesObserved, protectedPlayerHpUnchanged = hpUnchanged,
                initialPlayerHealth = initialHealth, finalPlayerHealth = player.Health.CurrentHealth });
            if (!coveragePass) throw new InvalidOperationException("Combat soak completed without required attack/projectile/protected-health coverage.");
        }
        finally
        {
            events.BeforeDamaged -= Protect;
            Application.logMessageReceived -= ObserveLog;
            foreach (var pair in windowListeners) if (pair.Key) pair.Key.HitWindowOpened -= pair.Value;
            // Even aborts release QA protection. Do not restore HP or move the
            // player here; the outer isolated smoke runner owns scene cleanup.
            record?.Invoke(new { stage = "combat-soak-protection-removed", completed, scope = Scope });
        }
    }

    private static void RequirePlayer(PlayerController player)
    {
        if (!player || !player.enabled || !player.Health || player.Health.IsDead ||
            !player.InputReader || player.RuntimeData == null)
            throw new InvalidOperationException("Combat soak lost its initialized, living QA player.");
    }

    private static void MoveToZoneFive(PlayerController player, Enemy anchor, Action<object> record)
    {
        var zone = anchor.GetComponentInParent<GroupAiZone>();
        Vector3 forward = anchor.transform.forward; forward.y = 0f;
        if (forward.sqrMagnitude < .01f) forward = Vector3.forward;
        forward.Normalize();
        Vector3? point = null;
        foreach (float radius in new[] { 7f, 6f, 8f })
        {
            for (int index = 0; index < 36; index++)
            {
                Vector3 candidate = anchor.transform.position + Quaternion.AngleAxis(index * 10f, Vector3.up) * forward * radius;
                if (!Physics.Raycast(candidate + Vector3.up * 8f, Vector3.down, out RaycastHit hit, 20f,
                    LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore) || hit.normal.y < .75f) continue;
                candidate = hit.point + Vector3.up * .12f;
                if (!zone || !zone.IsInZone(candidate) || Mathf.Abs(candidate.y - anchor.transform.position.y) > 2f) continue;
                if (Physics.CheckCapsule(candidate + Vector3.up * .4f, candidate + Vector3.up * 1.2f, .4f,
                    LayerMask.GetMask("Wall", "Default"), QueryTriggerInteraction.Ignore)) continue;
                // Stop short of the enemy itself: some authored ragdoll child
                // colliders use Default and must not be mistaken for scenery.
                Vector3 sightEnd = anchor.transform.position + Vector3.up +
                    (candidate - anchor.transform.position).normalized * 1.2f;
                if (Physics.Linecast(candidate + Vector3.up, sightEnd,
                    LayerMask.GetMask("Wall", "Default"), QueryTriggerInteraction.Ignore)) continue;
                point = candidate; break;
            }
            if (point.HasValue) break;
        }
        if (!point.HasValue) throw new InvalidOperationException("No safe visible Ground/painted Zone 5 point 6-8m from Monster C.");
        var controller = player.GetComponent<CharacterController>();
        bool wasEnabled = controller && controller.enabled;
        Vector3 oldPosition = player.transform.position;
        try
        {
            if (controller) controller.enabled = false;
            player.transform.position = point.Value;
            Vector3 toward = anchor.transform.position - point.Value; toward.y = 0f;
            player.transform.rotation = Quaternion.LookRotation(toward.normalized, Vector3.up);
            Physics.SyncTransforms();
        }
        finally { if (controller) controller.enabled = wasEnabled; }
        record?.Invoke(new { stage = "combat-soak-teleport-setup", scope = Scope, anchor = anchor.name,
            zone = zone.name, from = oldPosition.ToString("F3"), to = point.Value.ToString("F3"),
            anchorPosition = anchor.transform.position.ToString("F3"), distance = Vector3.Distance(point.Value, anchor.transform.position) });
    }

    private static void Increment(Dictionary<string, int> counters, string key)
    {
        counters.TryGetValue(key, out int value); counters[key] = value + 1;
    }

    [Serializable]
    private sealed class DamageTally
    {
        public int contacts;
        public long damageBeforeQaGuard;
        public int lastFrame;
    }

    private static Dictionary<string, object> CopyTallies(Dictionary<string, DamageTally> source)
    {
        var result = new Dictionary<string, object>();
        foreach (var pair in source)
            result.Add(pair.Key, new { pair.Value.contacts, pair.Value.damageBeforeQaGuard, pair.Value.lastFrame });
        return result;
    }
}
