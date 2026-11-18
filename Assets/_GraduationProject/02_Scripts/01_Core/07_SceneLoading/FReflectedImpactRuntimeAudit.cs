using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Isolated-save opt-in QA: real authored F entrance, then production reflected
/// explosion APIs against live F. Only the QA player is protected/repositioned.
/// Does not claim natural button timing, flight collision, or received-player damage.
/// </summary>
public static class FReflectedImpactRuntimeAudit
{
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static readonly MethodInfo Explode = typeof(ParabolicMineProjectile).GetMethod("ExplodeReflected", Private);
    static readonly FieldInfo AppliedDamage = typeof(ParabolicMineProjectile).GetField("_damageData", Private);
    static readonly MethodInfo CanKnockback = typeof(EnemyHealth).GetMethod("CanStartKnockback", Private);
    static readonly FieldInfo Knockback = typeof(EnemyHealth).GetField("_KnockbackCoroutine", Private);
    const string Scope = "Real authored trigger/intro, then reflected-explosion API hits on live F. Protected QA player; player-only setup teleports. No enemy HP/stiffness/stat/AI/position assignments.";

    public static IEnumerator Run(PlayerController player, Action<object> record)
    {
        var args = SceneLoadingSmokeTest.GetArguments();
        if (!args.Contains("--soom-loading-smoke") || !args.Contains("--soom-f-parry-audit"))
            throw new InvalidOperationException("F impact audit needs explicit isolated smoke/F-parry flags.");
        if (!player || !player.Health || player.Health.IsDead || SceneLoadingManager.Instance.CurrentActiveChunk.name != "Part2-B")
            throw new InvalidOperationException("An initialized living Part2-B QA player is required.");
        if (Explode == null || AppliedDamage == null || CanKnockback == null || Knockback == null)
            throw new MissingMemberException("F impact observation API changed.");
        var enemies = UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(e=>e && e.name.IndexOf("Monster_f", StringComparison.OrdinalIgnoreCase)>=0 &&
                e.GetComponentInParent<GroupAiZone>(true) is GroupAiZone zone && zone.name.Replace("_", "").ToLowerInvariant().EndsWith("zone1")).ToArray();
        if (enemies.Length == 0) throw new InvalidOperationException("Authored dungeon F group missing.");
        var sourceStats = enemies.Select(e=>e.enemyStat).Distinct().ToDictionary(s=>s, s=>JsonUtility.ToJson(s));
        string playerSource = JsonUtility.ToJson(player.Data);
        int protectedHealth = player.Health.CurrentHealth;
        int interceptedAttacks = 0;
        void Protect(ref PlayerDamageContext context)
        {
            interceptedAttacks++;
            context.Data.DamageAmount = 0; context.Data.StiffnessAmount = 0;
            context.Data.KnockbackForce = 0; context.Data.KnockbackDuration = 0;
            context.HasSuperArmor = true;
        }
        var events = player.Events;
        events.BeforeDamaged += Protect;
        ParabolicMineProjectile projectile = null;
        Mon_Stiffness observedStiffness = null;
        var stiffnessEvents = new List<Vector2Int>();
        void ObserveStiffness(int before, int after) => stiffnessEvents.Add(new Vector2Int(before, after));
        bool completed = false;
        try
        {
            var group = enemies[0].GetComponentInParent<GroupAiZone>(true);
            var director = group.GetComponent<PlayableDirector>();
            if (!GamePlayTagManager.Instance.HasTag("SubDungeon2Enter"))
            {
                var tagsField = typeof(GamePlayTagTrigger).GetField("_tagsToAdd", Private);
                var trigger = UnityEngine.Object.FindObjectsByType<GamePlayTagTrigger>(FindObjectsSortMode.None)
                    .FirstOrDefault(t=>t.gameObject.scene==group.gameObject.scene && tagsField.GetValue(t) is List<GamePlayTagSO> tags && tags.Any(tag=>tag && tag.ID=="SubDungeon2Enter"));
                if (!trigger || !director) throw new InvalidOperationException("Actual F entrance trigger/timeline missing.");
                var box = trigger.GetComponent<BoxCollider>();
                if (!box || !box.enabled) throw new InvalidOperationException("F entrance trigger collider missing.");
                Vector3 entry = EntryPoint(box);
                MovePlayer(player, entry, enemies[0].transform.position, "authored-entry-trigger", record);
                double deadline = Time.realtimeSinceStartupAsDouble + 35d;
                bool playingSeen = false, entryReady = false;
                while (Time.realtimeSinceStartupAsDouble < deadline)
                {
                    playingSeen |= director.state == PlayState.Playing;
                    entryReady = GamePlayTagManager.Instance.HasTag("SubDungeon2Enter") && playingSeen && director.state != PlayState.Playing &&
                        enemies.All(e=>e && e.gameObject.activeInHierarchy && e._initializer != null && e._initializer.IsInitialized("Final")) &&
                        player.InputReader.CurrentInputMode == InputReaderSO.InputMode.Gameplay;
                    if (entryReady) break;
                    yield return null;
                }
                record?.Invoke(new { stage="f-impact-authored-entry", pass=entryReady, playingSeen, scope=Scope });
                if (!entryReady) throw new TimeoutException("F authored entry/intro/Final/Gameplay did not complete naturally.");
            }
            var enemy = enemies.Where(e=>e && e.gameObject.activeInHierarchy && e._initializer != null && e._initializer.IsInitialized("Final") && e.EnemyHealth && !e.EnemyHealth.IsDead)
                .OrderByDescending(e=>e.EnemyHealth.CurrentHealth).FirstOrDefault();
            if (!enemy) throw new InvalidOperationException("No living initialized F after authored entry.");
            var template = Resources.FindObjectsOfTypeAll<Task_ParabolicMine>().FirstOrDefault(t=>t && t.projectilePrefab && t.projectilePrefab.GetComponent<ParabolicMineProjectile>());
            if (!template) throw new InvalidOperationException("Authored F mine configuration missing.");
            string projectileSource = JsonUtility.ToJson(template);
            observedStiffness = enemy.StiffnessSystem;
            observedStiffness.OnStiffnessChanged += ObserveStiffness;
            int threshold = observedStiffness.StiffnessThreshold;
            int shots = Mathf.CeilToInt((threshold-observedStiffness.CurrentStiffness)/30f);
            int reflectedDamage = Mathf.Max(template.explosionDamage, template.damageData.DamageAmount)*2;
            if (shots < 1 || shots > 6 || enemy.EnemyHealth.CurrentHealth <= shots*reflectedDamage)
                throw new InvalidOperationException("Authored F HP cannot survive the required reflected threshold test; no HP/stat override was made.");
            bool allPassed = true, anyKnockback = false;
            for (int hit = 0; hit < shots; hit++)
            {
                Vector3 setup = default;
                double deadline = Time.realtimeSinceStartupAsDouble + 15d;
                bool found = false;
                while (!(found=TryKnockbackSetup(enemy, group, out setup)) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                if (!found) throw new InvalidOperationException("No authored floor direction permits live F knockback; diagnostics retained, enemy not relocated.");
                MovePlayer(player, setup, enemy.transform.position, "safe-live-knockback-direction", record);
                int hpBefore = enemy.EnemyHealth.CurrentHealth;
                int stiffnessBefore = observedStiffness.CurrentStiffness;
                Vector3 positionBefore = enemy.transform.position;
                stiffnessEvents.Clear();
                projectile = FProjectileParryRuntimeAudit.Spawn(template, enemy, player);
                bool reflected = projectile.Reflect(player, 0, AttackType.Strong_Counter);
                DamageData payload = (DamageData)AppliedDamage.GetValue(projectile);
                if (!reflected) throw new InvalidOperationException("Live F mine reflection API failed.");
                var body = enemy.GetComponent<Collider>();
                projectile.transform.position = body ? body.bounds.center : enemy.transform.position + Vector3.up;
                // Impact placement is explicit QA setup; the production explosion performs all damage/stagger.
                Explode.Invoke(projectile, null);
                int hpAfter = enemy.EnemyHealth.CurrentHealth;
                int stiffnessAfter = observedStiffness.CurrentStiffness;
                bool fullStun = enemy.ParrySystem.CurrentStun == StunType.Full;
                bool thresholdReached = stiffnessBefore + 30 >= threshold;
                bool accumulated30 = stiffnessEvents.Any(v=>v.x==stiffnessBefore && v.y==stiffnessBefore+30);
                bool startedKnockback = Knockback.GetValue(enemy.EnemyHealth) != null;
                float start = Time.time;
                double waitDeadline = Time.realtimeSinceStartupAsDouble + 4d;
                while (Time.time-start < .45f && Time.realtimeSinceStartupAsDouble < waitDeadline) yield return null;
                Vector3 displacement = enemy.transform.position-positionBefore; displacement.y=0;
                bool moved = startedKnockback && displacement.magnitude>.05f;
                anyKnockback |= moved;
                bool pass = payload.StiffnessAmount==30 && Mathf.Approximately(payload.KnockbackForce,18f) && Mathf.Approximately(payload.KnockbackDuration,.3f) &&
                    hpBefore-hpAfter==reflectedDamage && accumulated30 && (thresholdReached ? fullStun && stiffnessAfter==0 : stiffnessAfter==stiffnessBefore+30);
                allPassed &= pass;
                record?.Invoke(new { stage="f-reflected-live-impact", pass, scope=Scope, hit=hit+1, enemy=enemy.name, reflected,
                    hpBefore, hpAfter, reflectedDamage, stiffnessBefore, stiffnessAfter, threshold, thresholdReached, fullStun, accumulated30,
                    stiffnessEvents=stiffnessEvents.Select(v=>new { before=v.x, after=v.y }).ToArray(),
                    payload.StiffnessAmount, payload.KnockbackForce, payload.KnockbackDuration, startedKnockback, moved,
                    horizontalDisplacement=displacement.magnitude, sourcePosition=positionBefore.ToString("F3"), endPosition=enemy.transform.position.ToString("F3") });
                FProjectileParryRuntimeAudit.Release(ref projectile);
                if (!pass) throw new InvalidOperationException("Live F reflected HP/stiffness/threshold invariant failed.");
            }
            bool sourceUnchanged = playerSource==JsonUtility.ToJson(player.Data) && projectileSource==JsonUtility.ToJson(template) && sourceStats.All(s=>s.Key && JsonUtility.ToJson(s.Key)==s.Value);
            completed = allPassed && anyKnockback && sourceUnchanged && player.Health.CurrentHealth==protectedHealth;
            record?.Invoke(new { stage="f-reflected-impact-complete", pass=completed, scope=Scope, shots, anyKnockback,
                sourceUnchanged, playerHealth=player.Health.CurrentHealth, protectedHealth, interceptedAttacks });
            if (!completed) throw new InvalidOperationException("F reflected knockback/source preservation coverage failed.");
        }
        finally
        {
            events.BeforeDamaged -= Protect;
            if (observedStiffness) observedStiffness.OnStiffnessChanged -= ObserveStiffness;
            FProjectileParryRuntimeAudit.Release(ref projectile);
            record?.Invoke(new { stage="f-impact-protection-removed", completed, scope=Scope });
        }
    }

    static bool TryKnockbackSetup(Enemy enemy, GroupAiZone zone, out Vector3 point)
    {
        point=default;
        if (!enemy || !enemy.EnemyHealth || enemy.EnemyHealth.IsDead || !enemy.EnemyHealth.Knockbackable) return false;
        if (enemy.EnemyHealth.CheckStunImmunity != null && enemy.EnemyHealth.CheckStunImmunity(AttackType.Strong_Counter)) return false;
        for (int i=0; i<36; i++)
        {
            Vector3 direction=Quaternion.AngleAxis(i*10f,Vector3.up)*Vector3.forward;
            if (!Ground(enemy.transform.position-direction*5f,out Vector3 ground) || !zone.IsInZone(ground)) continue;
            if (enemy.Shield && enemy.Shield.CheckBlock(ground)) continue;
            if (!(bool)CanKnockback.Invoke(enemy.EnemyHealth,new object[]{direction,18f,.3f,AnimationCurve.EaseInOut(0,1,1,0)})) continue;
            point=ground; return true;
        }
        return false;
    }
    static Vector3 EntryPoint(BoxCollider box)
    {
        foreach(float z in new[]{0f,-.3f,.3f}) foreach(float x in new[]{0f,-.3f,.3f})
        {
            Vector3 p=box.transform.TransformPoint(box.center+new Vector3(box.size.x*x,0,box.size.z*z));
            if (Ground(p,out Vector3 ground) && (box.ClosestPoint(ground+Vector3.up*.8f)-(ground+Vector3.up*.8f)).sqrMagnitude<.0001f) return ground;
        }
        throw new InvalidOperationException("No safe authored trigger entry point.");
    }
    static bool Ground(Vector3 p,out Vector3 point)
    {
        point=default;
        if (!Physics.Raycast(p+Vector3.up*10f,Vector3.down,out var hit,24f,LayerMask.GetMask("Ground"),QueryTriggerInteraction.Ignore) || hit.normal.y<.75f) return false;
        point=hit.point+Vector3.up*.12f;
        return !Physics.CheckCapsule(point+Vector3.up*.4f,point+Vector3.up*1.2f,.4f,LayerMask.GetMask("Wall","Default"),QueryTriggerInteraction.Ignore);
    }
    static void MovePlayer(PlayerController player,Vector3 point,Vector3 lookAt,string reason,Action<object> record)
    {
        var controller=player.GetComponent<CharacterController>(); bool wasEnabled=controller && controller.enabled;
        Vector3 before=player.transform.position;
        try
        {
            if(controller)controller.enabled=false;
            player.transform.position=point;
            Vector3 direction=lookAt-point;direction.y=0;
            if(direction.sqrMagnitude>.01f)player.transform.rotation=Quaternion.LookRotation(direction);
        }
        finally {if(controller)controller.enabled=wasEnabled;}
        Physics.SyncTransforms();
        record?.Invoke(new {stage="f-impact-player-setup",reason,from=before.ToString("F3"),to=point.ToString("F3"),scope=Scope});
    }
}
