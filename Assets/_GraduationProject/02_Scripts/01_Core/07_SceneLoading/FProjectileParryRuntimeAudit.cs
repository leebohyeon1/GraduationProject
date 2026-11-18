using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Explicit isolated-save QA only. Uses the authored F prefab/configuration and
/// live player health, but invokes contact/parry APIs, not keyboard timing or AI.
/// No bootstrap, source edits, tag changes or direct stat/health assignments;
/// received damage flows through the real PlayerHealth and is not restored here.
/// </summary>
public static class FProjectileParryRuntimeAudit
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly MethodInfo Contact = typeof(ParabolicMineProjectile).GetMethod("ExplodeNow", PrivateInstance);

    public static IEnumerator Run(PlayerController player, Action<object> record)
    {
        var args = SceneLoadingSmokeTest.GetArguments();
        if (!args.Contains("--soom-loading-smoke") || !args.Contains("--soom-f-parry-audit"))
            throw new InvalidOperationException("F parry audit requires explicit smoke and F-parry flags.");
        if (!player || !player.Health || player.Health.IsDead || player.Ability == null)
            throw new InvalidOperationException("F parry audit requires an initialized living player.");
        if (SceneLoadingManager.Instance.CurrentActiveChunk.name != "Part2-B")
            throw new InvalidOperationException("F parry audit requires the actual Part2-B portal load.");
        if (Contact == null) throw new MissingMethodException("ParabolicMineProjectile.ExplodeNow");

        // Part2-B is a chunk of Build_Part2, not necessarily the Unity scene name.
        Enemy owner = UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(e => e && e.gameObject.scene.IsValid() && e.gameObject.scene.isLoaded &&
                e.name.IndexOf("Monster_f", StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderBy(e => Vector3.Distance(player.transform.position, e.transform.position)).FirstOrDefault();
        var template = Resources.FindObjectsOfTypeAll<Task_ParabolicMine>()
            .FirstOrDefault(t => t && t.projectilePrefab && t.projectilePrefab.GetComponent<ParabolicMineProjectile>());
        if (!owner || !template) throw new InvalidOperationException("Actual dungeon F owner/prefab configuration is unavailable.");
        int damage = template.explosionDamage > 0 ? template.explosionDamage : template.damageData.DamageAmount;
        if (damage <= 0 || player.Health.CurrentHealth <= damage * 3)
            throw new InvalidOperationException("Insufficient unmodified player HP for the F contact-damage audit.");
        string sourceBefore = JsonUtility.ToJson(template);
        string playerBefore = JsonUtility.ToJson(player.Data);
        var health = player.Health;
        ParabolicMineProjectile mine = null;
        int damageCalls = 0;
        int receivedDamage = 0;
        void ObserveDamage(int amount) { damageCalls++; receivedDamage += amount; }
        health.TakeDamged += ObserveDamage;
        try
        {
            yield return Settle(player);
            record?.Invoke(new { stage = "f-parry-ready", scope = "Actual prefab + live health, contact/parry API calls; no input/AI validation", owner = owner.name,
                ownerActive = owner.gameObject.activeInHierarchy, template = template.name, prefab = template.projectilePrefab.name,
                authoredDamage = damage, health = health.CurrentHealth, invincible = player.Ability.HasTag("Invincible"), timeScale = Time.timeScale });

            int before = health.CurrentHealth;
            mine = Spawn(template, owner, player);
            float began = Time.time;
            Contact.Invoke(mine, new object[] { health });
            double realBegan = Time.realtimeSinceStartupAsDouble;
            // This must land inside the NEW interval, not merely any time before expiry.
            while (Time.time - began < .18f && Time.realtimeSinceStartupAsDouble - realBegan < 2d) yield return null;
            float elapsed = Time.time - began;
            bool wasLive = mine && mine.gameObject.activeInHierarchy;
            bool reflected = wasLive && mine.Reflect(player, 0, AttackType.Strong_Counter);
            bool duplicate = wasLive && mine.Reflect(player, 0, AttackType.Strong_Counter);
            bool gracePass = elapsed > .14f && elapsed < .24f && reflected && !duplicate && health.CurrentHealth == before && damageCalls == 0;
            record?.Invoke(new { stage = "f-parry-contact-grace", pass = gracePass, elapsedSeconds = elapsed, wasLive, reflected,
                duplicateReflection = duplicate, healthBefore = before, healthAfter = health.CurrentHealth, damageCalls });
            Release(ref mine);
            if (!gracePass) throw new InvalidOperationException("F reflection in the .14-.24s contact interval failed (see measured timing).");

            yield return Settle(player);
            before = health.CurrentHealth; damageCalls = 0; receivedDamage = 0;
            mine = Spawn(template, owner, player);
            began = Time.time; realBegan = Time.realtimeSinceStartupAsDouble;
            Contact.Invoke(mine, new object[] { health });
            bool prematureDamage = false;
            while (mine && mine.gameObject.activeInHierarchy && Time.realtimeSinceStartupAsDouble - realBegan < 3d)
            {
                if (Time.time - began < .239f && health.CurrentHealth != before) prematureDamage = true;
                yield return null;
            }
            elapsed = Time.time - began;
            bool expiryPass = !prematureDamage && elapsed >= .239f && mine && !mine.gameObject.activeInHierarchy &&
                health.CurrentHealth == before - damage && damageCalls == 1 && receivedDamage == damage;
            record?.Invoke(new { stage = "f-parry-contact-expiry", pass = expiryPass, elapsedSeconds = elapsed, prematureDamage,
                healthBefore = before, healthAfter = health.CurrentHealth, expectedDamage = damage, damageCalls, receivedDamage,
                returnedToPool = mine && !mine.gameObject.activeInHierarchy });
            Release(ref mine);
            if (!expiryPass) throw new InvalidOperationException("F contact expiry did not apply exactly one authored damage hit and return to pool.");

            yield return Settle(player);
            before = health.CurrentHealth; damageCalls = 0; receivedDamage = 0;
            mine = Spawn(template, owner, player);
            int frame = Time.frameCount;
            mine.DetonateOnNormalParry(player);
            // Duplicate collider delivery must not apply a second explosion or damage.
            mine.DetonateOnNormalParry(player);
            bool normalPass = health.CurrentHealth == before - damage && damageCalls == 1 && receivedDamage == damage &&
                !mine.gameObject.activeInHierarchy && Time.frameCount == frame;
            record?.Invoke(new { stage = "f-parry-normal-explosion", pass = normalPass, immediate = Time.frameCount == frame,
                healthBefore = before, healthAfter = health.CurrentHealth, expectedDamage = damage, damageCalls, receivedDamage,
                returnedToPool = !mine.gameObject.activeInHierarchy });
            Release(ref mine);
            if (!normalPass) throw new InvalidOperationException("Normal parry must immediately explode F's projectile and damage the player once.");
            if (sourceBefore != JsonUtility.ToJson(template) || playerBefore != JsonUtility.ToJson(player.Data))
                throw new InvalidOperationException("F audit unexpectedly modified a source configuration.");
            record?.Invoke(new { stage = "f-parry-complete", pass = true, cases = 3, health = health.CurrentHealth,
                sourceAssetsUnchanged = true, healthRestoredByAudit = false, inputTimingTested = false });
        }
        finally
        {
            if (health) health.TakeDamged -= ObserveDamage;
            Release(ref mine);
        }
        yield return FReflectedImpactRuntimeAudit.Run(player, record);
    }

    private static IEnumerator Settle(PlayerController player)
    {
        double deadline = Time.realtimeSinceStartupAsDouble + 5d;
        double began = Time.realtimeSinceStartupAsDouble;
        while (Time.realtimeSinceStartupAsDouble < deadline)
        {
            var revival = player.GetComponent<RevivalProtection>();
            if (Time.realtimeSinceStartupAsDouble - began >= .8d && !player.Ability.HasTag("Invincible") &&
                (!revival || !revival.IsProtected) && Mathf.Abs(Time.timeScale - 1f) < .01f) yield break;
            yield return null;
        }
        throw new InvalidOperationException("F parry damage audit blocked by existing invulnerability or non-unit time scale; no protection was removed.");
    }

    internal static ParabolicMineProjectile Spawn(Task_ParabolicMine t, Enemy owner, PlayerController player)
    {
        var body = player.GetComponent<Collider>();
        Vector3 position = body ? body.bounds.center : player.transform.position + Vector3.up;
        GameObject obj = ProjectilePoolManager.GetProjectile(t.projectilePrefab, position, Quaternion.identity);
        var mine = obj.GetComponent<ParabolicMineProjectile>();
        // Inactive authored F has no initialized AI blackboard. Setup the real prefab
        // with unchanged authored values, then bind only this QA projectile's caches.
        mine.Setup(null, position, position, owner.gameObject, t.projectileDuration, t.jumpHeight, t.mineDuration,
            t.detectRadius, t.explodeDelay, t.damageData, t.explosionDamage, t.explosionRadius,
            t.groundLayer, t.wallLayer, t.playerLayer, t.feedbackName);
        Set(mine, "_enemy", owner);
        Set(mine, "_playerTransform", player.transform);
        Set(mine, "_playerHealth", player.Health);
        Set(mine, "_playerCollider", body);
        return mine;
    }

    private static void Set(ParabolicMineProjectile mine, string name, object value)
    {
        var field = typeof(ParabolicMineProjectile).GetField(name, PrivateInstance);
        if (field == null) throw new MissingFieldException(typeof(ParabolicMineProjectile).Name, name);
        field.SetValue(mine, value);
    }

    internal static void Release(ref ParabolicMineProjectile mine)
    {
        if (mine && mine.gameObject.activeInHierarchy) ProjectilePoolManager.ReleaseProjectile(mine.gameObject);
        mine = null;
    }
}
