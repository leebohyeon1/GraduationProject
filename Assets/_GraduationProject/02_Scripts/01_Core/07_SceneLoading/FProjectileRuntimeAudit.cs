using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BehaviorTree;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Profiling;

/// <summary>
/// Opt-in QA: the authored dungeon trigger and intro activate F, then actual AI
/// attacks a protected test player. This does not validate received HP damage.
/// No bootstrap: only SceneLoadingSmokeTest's isolated-save session calls Run.
/// </summary>
public static class FProjectileRuntimeAudit
{
    private const string Scope = "Actual F AI/mine projectiles against a protected QA player; NOT received-damage validation. Player-only entry/range setup teleports, no enemy/stat/AI/FSM overrides.";
    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    private static readonly FieldInfo AddedTags = typeof(GamePlayTagTrigger).GetField("_tagsToAdd", PrivateInstance);
    private static readonly FieldInfo PoolInstances = typeof(ProjectilePoolManager).GetField("InstanceKeys", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly FieldInfo MineEnemy = typeof(ParabolicMineProjectile).GetField("_enemy", PrivateInstance);

    public static IEnumerator Run(PlayerController player, int seconds, Action<object> record)
    {
        var args = SceneLoadingSmokeTest.GetArguments();
        if (!args.Contains("--soom-loading-smoke") || !args.Contains("--soom-f-projectile-audit"))
            throw new InvalidOperationException("F projectile audit requires both explicit smoke/F flags.");
        RequirePlayer(player);
        if (SceneLoadingManager.Instance.CurrentActiveChunk.name != "Part2-B")
            throw new InvalidOperationException("F audit requires the actual Part2-B portal load.");
        if (AddedTags == null || PoolInstances == null || MineEnemy == null)
            throw new MissingFieldException("F audit read-only observation fields changed.");
        if (GamePlayTagManager.Instance.HasTag("SubDungeon2Enter"))
            throw new InvalidOperationException("Fresh F audit requires its authored entrance trigger to add SubDungeon2Enter.");

        Enemy[] enemies = UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(e => e && e.name.IndexOf("Monster_f", StringComparison.OrdinalIgnoreCase) >= 0 &&
                e.GetComponentInParent<GroupAiZone>(true) is GroupAiZone z &&
                z.name.Replace("_", "").ToLowerInvariant().EndsWith("zone1")).ToArray();
        if (enemies.Length == 0) throw new InvalidOperationException("Part2-B authored F group was not found.");
        Enemy anchor = enemies.OrderBy(e => Vector3.Distance(player.transform.position, e.transform.position)).First();
        var zone = anchor.GetComponentInParent<GroupAiZone>(true);
        var director = zone.GetComponent<PlayableDirector>();
        var trigger = UnityEngine.Object.FindObjectsByType<GamePlayTagTrigger>(FindObjectsSortMode.None)
            .FirstOrDefault(t => t.gameObject.scene == zone.gameObject.scene &&
                AddedTags.GetValue(t) is List<GamePlayTagSO> tags && tags.Any(tag => tag && tag.ID == "SubDungeon2Enter"));
        if (!trigger || !director) throw new InvalidOperationException("Authored F entrance trigger/timeline is missing.");
        var entryCollider = trigger.GetComponent<BoxCollider>();
        if (!entryCollider || !entryCollider.enabled) throw new InvalidOperationException("F entrance BoxCollider is unavailable.");
        var events = player.Events;
        int initialHealth = player.Health.CurrentHealth;
        string playerSource = JsonUtility.ToJson(player.Data);
        var enemySources = enemies.Select(e => e.enemyStat).Distinct().ToDictionary(s => s, s => JsonUtility.ToJson(s));
        var attackPatterns = new Dictionary<string, int>();
        var stateEntries = new Dictionary<string, int>();
        var windows = new Dictionary<string, int>();
        var minesByEnemy = new Dictionary<string, int>();
        var contacts = new Dictionary<string, int>();
        var damageBeforeGuard = new Dictionary<string, long>();
        var oldPoolActive = new Dictionary<int, bool>();
        var mineComponents = new Dictionary<int, ParabolicMineProjectile>();
        var stateListeners = new Dictionary<EnemyStateController, Action<EnemyStateController.EnemyState, EnemyStateController.EnemyState>>();
        var hitListeners = new Dictionary<Enemy_AnimationEventHandler, Action>();
        int fAttackCount = 0, mineActivationCount = 0, interceptedContacts = 0, rangeCorrections = 0;
        int peakPoolCount = 0, peakActiveMines = 0, initialNodeCount = 0;
        double began = 0d;
        bool completed = false, introPlayingSeen = false;

        void Protect(ref PlayerDamageContext context)
        {
            Enemy source = context.Data.SourceEnemy;
            if (!source && context.Data.AttackerTransform) source = context.Data.AttackerTransform.GetComponentInParent<Enemy>();
            string key = (source ? Identity(source) : "unknown") + "/" + context.Data.AttackType;
            Increment(contacts, key);
            damageBeforeGuard.TryGetValue(key, out long total);
            damageBeforeGuard[key] = total + Math.Max(0, context.Data.DamageAmount);
            interceptedContacts++;
            context.Data.DamageAmount = 0;
            context.Data.StiffnessAmount = 0;
            context.Data.KnockbackForce = 0f;
            context.Data.KnockbackDuration = 0f;
            context.HasSuperArmor = true;
        }
        void OnLog(string message, string trace, LogType type)
        {
            if (!message.StartsWith("[Attack Node Enter] ", StringComparison.Ordinal) ||
                message.IndexOf("Monster_f", StringComparison.OrdinalIgnoreCase) < 0) return;
            fAttackCount++;
            Increment(attackPatterns, message);
        }
        bool SourceUnchanged()
        {
            if (playerSource != JsonUtility.ToJson(player.Data)) return false;
            foreach (var pair in enemySources) if (!pair.Key || JsonUtility.ToJson(pair.Key) != pair.Value) return false;
            return true;
        }
        void ObserveMines()
        {
            var map = (Dictionary<GameObject, long>)PoolInstances.GetValue(null);
            peakPoolCount = Mathf.Max(peakPoolCount, map.Count);
            int activeMines = 0;
            foreach (var pair in map)
            {
                var obj = pair.Key;
                if (!obj) continue;
                int id = obj.GetInstanceID();
                if (!mineComponents.TryGetValue(id, out var mine))
                    mineComponents[id] = mine = obj.GetComponent<ParabolicMineProjectile>();
                if (!mine) continue;
                bool active = obj.activeInHierarchy;
                if (active) activeMines++;
                if (active && (!oldPoolActive.TryGetValue(id, out bool oldActive) || !oldActive))
                {
                    Enemy owner = MineEnemy.GetValue(mine) as Enemy;
                    if (owner && enemies.Contains(owner))
                    {
                        mineActivationCount++;
                        Increment(minesByEnemy, Identity(owner));
                    }
                }
                oldPoolActive[id] = active;
            }
            peakActiveMines = Mathf.Max(peakActiveMines, activeMines);
        }
        object EnemySnapshot(Enemy enemy)
        {
            if (!enemy) return new { missing = true };
            var ai = enemy._aiController;
            var bb = ai?._aiBrain?.blackboard;
            var state = enemy.GetComponent<EnemyStateController>();
            var handler = enemy.animHandler;
            var intro = enemy.GetComponent<EnemySpawnIntro>();
            var animator = enemy.animator;
            return new
            {
                enemy = Identity(enemy), active = enemy.gameObject.activeInHierarchy,
                initialized = enemy._initializer != null && enemy._initializer.IsInitialized("Final"),
                aiEnabled = ai && ai.isActiveAndEnabled, actionable = ai && ai.IsActionable(),
                state = enemy.CurrentState.ToString(), locked = state && state.IsStateLocked,
                isCombat = ai?._aiBrain?._isCombat ?? false,
                bridgeAttacking = enemy._animationBridge != null && enemy._animationBridge.IsAttacking,
                distance = Vector3.Distance(enemy.transform.position, player.transform.position),
                blackboardDistance = bb?.GetValueOrDefault(EnemyBlackboardKeys.DistanceBetween, -1f),
                attackToken = bb?.GetValueOrDefault(EnemyBlackboardKeys.HasAttackToken, false),
                isAttackingFlag = bb?.GetValueOrDefault("IsAttacking", false),
                detectPlayer = bb?.GetValueOrDefault(EnemyBlackboardKeys.DetectPlayer, false),
                hasLineOfSight = bb?.GetValueOrDefault(EnemyBlackboardKeys.IsHasLOS, false),
                engage = bb?.GetValueOrDefault(EnemyBlackboardKeys.Engage, false),
                goHome = bb?.GetValueOrDefault("GoHome", false),
                introCompleted = !intro || intro.Completed,
                hitWindow = handler && handler.IsHitWindowOpen, actionFinished = handler && handler.IsActionFinished,
                actionSo = handler && handler.IsActionSO,
                animatorState = animator ? animator.GetCurrentAnimatorStateInfo(0).fullPathHash : 0,
                normalizedTime = animator ? animator.GetCurrentAnimatorStateInfo(0).normalizedTime : 0f,
                animatorSpeed = animator ? animator.speed : 0f,
                position = enemy.transform.position.ToString("F3"),
                health = enemy.EnemyHealth ? enemy.EnemyHealth.CurrentHealth : (int?)null,
                attacks = ai?.inGameenemyAttackDatas?.Where(d => d).Select(d => new
                    { name = d.AttackName, radius = d.damageRadius, cooldown = d.Cooltime, type = d.damageData.AttackType.ToString(), damage = d.damageData.DamageAmount }).ToArray()
            };
        }
        void Sample(string stage)
        {
            var map = (Dictionary<GameObject, long>)PoolInstances.GetValue(null);
            int activePool = 0, destroyed = 0;
            foreach (var pair in map) if (!pair.Key) destroyed++; else if (pair.Key.activeInHierarchy) activePool++;
            long allocated = Profiler.GetTotalAllocatedMemoryLong(), managed = Profiler.GetMonoUsedSizeLong();
            int nodes = Resources.FindObjectsOfTypeAll<Node>().Length;
            record?.Invoke(new
            {
                stage, scope = Scope, elapsedSeconds = began == 0d ? 0d : Time.realtimeSinceStartupAsDouble - began,
                durationSeconds = seconds, frame = Time.frameCount, playerHealth = player.Health.CurrentHealth,
                initialHealth, playerPosition = player.transform.position.ToString("F3"),
                fAttackCount, mineActivationCount, rangeCorrections, interceptedContacts,
                actualFPatternEntries = new Dictionary<string, int>(attackPatterns),
                attackStateEntriesByEnemy = new Dictionary<string, int>(stateEntries),
                hitWindowsByEnemy = new Dictionary<string, int>(windows),
                observedMineActivationLowerBoundsByEnemy = new Dictionary<string, int>(minesByEnemy),
                activationCountLimit = "Per-frame pool observation may miss shots spawned/released in one frame; counts are lower bounds.",
                contactsBeforeQaGuard = new Dictionary<string, int>(contacts),
                damageAfterGameplayModifiersBeforeQaGuard = new Dictionary<string, long>(damageBeforeGuard),
                enemies = enemies.Select(EnemySnapshot).ToArray(),
                nodeCount = nodes, initialNodeCount, nodeDelta = nodes - initialNodeCount,
                poolCount = map.Count, activePoolCount = activePool, destroyedPoolWrappers = destroyed,
                activeMineProjectiles = UnityEngine.Object.FindObjectsByType<ParabolicMineProjectile>(FindObjectsSortMode.None).Length,
                peakActiveMines, peakPoolCount, sourceDataUnchanged = SourceUnchanged(),
                unityAllocatedBytes = allocated, managedUsedBytes = managed,
                unityReservedBytes = Profiler.GetTotalReservedMemoryLong(), nativeEstimateBytes = Math.Max(0L, allocated - managed)
            });
        }

        events.BeforeDamaged += Protect;
        Application.logMessageReceived += OnLog;
        try
        {
            Vector3 entryPoint = FindGroundedTriggerPoint(entryCollider);
            TeleportPlayer(player, entryPoint, anchor.transform.position, "authored-trigger-entry", record);
            double entryStarted = Time.realtimeSinceStartupAsDouble;
            while (true)
            {
                RequirePlayer(player, true);
                introPlayingSeen |= director.state == PlayState.Playing;
                bool entered = GamePlayTagManager.Instance.HasTag("SubDungeon2Enter") && zone.gameObject.activeInHierarchy;
                if (entered && introPlayingSeen && director.state != PlayState.Playing) break;
                if (Time.realtimeSinceStartupAsDouble - entryStarted > 25d)
                {
                    Sample("f-projectile-entry-timeout");
                    throw new TimeoutException("Authored SubDungeon2Enter trigger/3.3-second F timeline did not finish naturally.");
                }
                yield return null;
            }
            while (enemies.Any(e => !e || e._initializer == null || !e._initializer.IsInitialized("Final") ||
                !e._aiController || !e._aiController.isActiveAndEnabled) ||
                player.InputReader.CurrentInputMode != InputReaderSO.InputMode.Gameplay)
            {
                RequirePlayer(player, true);
                if (Time.realtimeSinceStartupAsDouble - entryStarted > 35d)
                {
                    Sample("f-projectile-initialization-timeout");
                    throw new TimeoutException("F initialization/Gameplay did not finish after the authored introduction.");
                }
                yield return null;
            }
            record?.Invoke(new { stage = "f-projectile-authored-entry-complete", scope = Scope,
                tag = "SubDungeon2Enter", tagAddedByRealTrigger = true, introPlayingSeen,
                directorState = director.state.ToString(), timelineDuration = director.duration,
                elapsedSeconds = Time.realtimeSinceStartupAsDouble - entryStarted, enemyCount = enemies.Length });

            foreach (var enemy in enemies)
            {
                string identity = Identity(enemy);
                var state = enemy.GetComponent<EnemyStateController>();
                Action<EnemyStateController.EnemyState, EnemyStateController.EnemyState> onState = (previous, next) =>
                { if (next == EnemyStateController.EnemyState.Attack) Increment(stateEntries, identity); };
                state.OnStateChanged += onState; stateListeners.Add(state, onState);
                var handler = enemy.animHandler;
                Action onHit = () => Increment(windows, identity);
                handler.HitWindowOpened += onHit; hitListeners.Add(handler, onHit);
            }
            TeleportPlayer(player, FindRangePoint(anchor, enemies, zone), anchor.transform.position, "initial-6-to-8-metre-range", record);
            seconds = Mathf.Clamp(seconds, 30, 600);
            initialNodeCount = Resources.FindObjectsOfTypeAll<Node>().Length;
            began = Time.realtimeSinceStartupAsDouble;
            double nextSample = 30d;
            Sample("f-projectile-start");
            while (Time.realtimeSinceStartupAsDouble - began < seconds)
            {
                RequirePlayer(player);
                ObserveMines();
                double elapsed = Time.realtimeSinceStartupAsDouble - began;
                if (elapsed >= nextSample)
                {
                    Sample("f-projectile-sample");
                    if (!SourceUnchanged()) throw new InvalidOperationException("Authored source stats changed during F audit.");
                    float closest = enemies.Where(e => e && e.gameObject.activeInHierarchy).Min(e => Vector3.Distance(player.transform.position, e.transform.position));
                    if (closest < 5f || closest > 12f)
                    {
                        // Controlled ranged-attack exposure after normal jumps/
                        // chasing. This moves only the QA player and is logged;
                        // it is not natural traversal or a forced AI pattern.
                        TeleportPlayer(player, FindRangePoint(anchor, enemies, zone), anchor.transform.position,
                            "QA-range-correction-at-30-second-boundary", record);
                        rangeCorrections++;
                    }
                    nextSample = (Math.Floor(elapsed / 30d) + 1d) * 30d;
                }
                yield return null;
            }
            ObserveMines();
            Sample("f-projectile-final");
            completed = true;
            bool pass = fAttackCount >= 3 && mineActivationCount >= 3 && SourceUnchanged() && player.Health.CurrentHealth == initialHealth;
            record?.Invoke(new { stage = "f-projectile-completed", completed, pass, scope = Scope,
                passScope = "At least three real F attack entries and three observed F-owned mine activations; protected HP/source stats preserved. Memory/node leak judgment is separate.",
                fAttackCount, mineActivationCount, durationSeconds = seconds,
                elapsedSeconds = Time.realtimeSinceStartupAsDouble - began, rangeCorrections });
            if (!pass) throw new InvalidOperationException("F projectile repeated-firing coverage failed; all diagnostic samples retained.");
        }
        finally
        {
            events.BeforeDamaged -= Protect;
            Application.logMessageReceived -= OnLog;
            foreach (var pair in stateListeners) if (pair.Key) pair.Key.OnStateChanged -= pair.Value;
            foreach (var pair in hitListeners) if (pair.Key) pair.Key.HitWindowOpened -= pair.Value;
            record?.Invoke(new { stage = "f-projectile-protection-removed", completed, scope = Scope });
        }
    }

    private static string Identity(Enemy enemy) => enemy.name + "#" + enemy.GetInstanceID();
    private static void Increment(Dictionary<string, int> values, string key)
    { values.TryGetValue(key, out int old); values[key] = old + 1; }

    private static void RequirePlayer(PlayerController player, bool allowCinematic = false)
    {
        if (!player || !player.Health || player.Health.IsDead || !player.InputReader || player.RuntimeData == null ||
            (!allowCinematic && (!player.enabled || Time.timeScale <= 0f)))
            throw new InvalidOperationException("F audit lost its living unpaused QA player.");
    }

    private static Vector3 FindGroundedTriggerPoint(BoxCollider collider)
    {
        foreach (float z in new[] { 0f, -.3f, .3f })
        foreach (float x in new[] { 0f, -.3f, .3f })
        {
            Vector3 world = collider.transform.TransformPoint(collider.center + new Vector3(collider.size.x * x, 0f, collider.size.z * z));
            if (!GroundPoint(world, out Vector3 ground)) continue;
            if ((collider.ClosestPoint(ground + Vector3.up * .8f) - (ground + Vector3.up * .8f)).sqrMagnitude > .0001f) continue;
            return ground;
        }
        throw new InvalidOperationException("No safe grounded point inside the authored F entrance trigger.");
    }

    private static Vector3 FindRangePoint(Enemy anchor, Enemy[] enemies, GroupAiZone zone)
    {
        Vector3 forward = anchor.transform.forward; forward.y = 0f;
        if (forward.sqrMagnitude < .01f) forward = Vector3.forward;
        forward.Normalize();
        foreach (float radius in new[] { 7f, 8f, 6f })
        for (int index = 0; index < 36; index++)
        {
            Vector3 world = anchor.transform.position + Quaternion.AngleAxis(index * 10f, Vector3.up) * forward * radius;
            if (!GroundPoint(world, out Vector3 ground) || !zone.IsInZone(ground) || Mathf.Abs(ground.y - anchor.transform.position.y) > 2f) continue;
            if (enemies.Any(e => e && Vector3.Distance(e.transform.position, ground) < 5f)) continue;
            Vector3 sightEnd = anchor.transform.position + Vector3.up + (ground - anchor.transform.position).normalized * 1.2f;
            if (Physics.Linecast(ground + Vector3.up, sightEnd, LayerMask.GetMask("Wall", "Default"), QueryTriggerInteraction.Ignore)) continue;
            return ground;
        }
        throw new InvalidOperationException("No safe 6-8m grounded painted-zone point remains for F ranged exposure.");
    }

    private static bool GroundPoint(Vector3 near, out Vector3 result)
    {
        result = default;
        if (!Physics.Raycast(near + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 24f,
            LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore) || hit.normal.y < .75f) return false;
        result = hit.point + Vector3.up * .12f;
        return !Physics.CheckCapsule(result + Vector3.up * .4f, result + Vector3.up * 1.2f, .4f,
            LayerMask.GetMask("Wall", "Default"), QueryTriggerInteraction.Ignore);
    }

    private static void TeleportPlayer(PlayerController player, Vector3 point, Vector3 lookAt, string reason, Action<object> record)
    {
        Vector3 old = player.transform.position;
        var controller = player.GetComponent<CharacterController>();
        bool enabled = controller && controller.enabled;
        try
        {
            if (controller) controller.enabled = false;
            player.transform.position = point;
            Vector3 direction = lookAt - point; direction.y = 0f;
            if (direction.sqrMagnitude > .01f) player.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            Physics.SyncTransforms();
        }
        finally { if (controller) controller.enabled = enabled; }
        record?.Invoke(new { stage = "f-projectile-player-setup", scope = Scope, reason,
            from = old.ToString("F3"), to = point.ToString("F3"), frame = Time.frameCount });
    }
}
