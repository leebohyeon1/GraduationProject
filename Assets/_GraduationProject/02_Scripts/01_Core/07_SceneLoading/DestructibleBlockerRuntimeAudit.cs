using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RayFire;
using UnityEngine;

/// <summary>Opt-in check of authored Part1 blockers and pre-cut Rayfire physics.</summary>
public static class DestructibleBlockerRuntimeAudit
{
    private const string Scope = "Isolated QA: one scripted Strong_Counter hit, base charged-parry damage 10 for the hard wall, remaining HP for fragile totem. No keyboard input, asset edits or save operations.";

    public static IEnumerator Run(PlayerController player, Action<object> record)
    {
        var args = SceneLoadingSmokeTest.GetArguments();
        Require(args.Contains("--soom-loading-smoke") && args.Contains("--soom-destructible-audit"),
            "Destructible blocker audit requires both explicit QA flags.");
        Require(player, "Player is unavailable for destructible blocker audit.");
        string playerStats = JsonUtility.ToJson(player.Data);
        var totems = UnityEngine.Object.FindObjectsByType<TotemDestructibleObject>(FindObjectsSortMode.None);
        int passed = 0;
        foreach (string name in new[] { "Destroyed_Totem", "Hard_Destroyed_Totem" })
        {
            var matches = totems.Where(t => t && t.name == name && t.gameObject.scene.name == "Build_Part1").ToArray();
            Require(matches.Length == 1, "Expected one active authored Part1 totem: " + name);
            var totem = matches[0];
            Require(!totem.IsDead && totem.CurrentHealth > 0, "Totem was already destroyed before QA: " + name);
            var rootBoxes = totem.GetComponents<BoxCollider>().Where(c => !c.isTrigger).ToArray();
            // Identify the actual scene's missed helper by its authored hierarchy, not the fix's cache.
            var childBoxes = totem.transform.Cast<Transform>()
                .Where(t => t.name == "GameObject")
                .SelectMany(t => t.GetComponents<BoxCollider>()).Where(c => !c.isTrigger).ToArray();
            Require(rootBoxes.Length > 0, "No authored root blocker: " + name);
            Require(name != "Destroyed_Totem" || childBoxes.Length == 1,
                "Missing exact regression target Destroyed_Totem/GameObject BoxCollider.");
            Require(rootBoxes.Concat(childBoxes).All(c => c.enabled && c.gameObject.activeInHierarchy),
                "A passage blocker was disabled before destruction: " + name);

            var triggers = totem.GetComponentsInChildren<Collider>(true).Where(c => c.isTrigger).ToArray();
            var triggerEnabled = triggers.Select(c => c.enabled).ToArray();
            var rigids = totem.GetComponentsInChildren<RayfireRigid>(true);
            var fragments = rigids.Where(r => r.objectType == ObjectType.Mesh && r.GetComponent<Collider>()).ToArray();
            var fragmentColliders = fragments.SelectMany(r => r.GetComponents<Collider>()).ToArray();
            var fragmentEnabled = fragmentColliders.Select(c => c.enabled).ToArray();
            var staticRenderers = totem.GetComponentsInChildren<Renderer>(true)
                .Where(r => !r.GetComponentInParent<RayfireRigid>()).ToArray();
            var rendererEnabled = staticRenderers.Select(r => r.enabled).ToArray();
            Require(triggers.Length > 0 && fragmentColliders.Length > 0,
                "Authored tutorial/fragment coverage is unavailable: " + name);
            Require(name != "Hard_Destroyed_Totem" || staticRenderers.Length >= 2,
                "Hard totem side-stone renderer coverage is unavailable.");

            int healthBefore = totem.CurrentHealth, maxHealthBefore = totem.MaxHealth;
            int diedEvents = 0, activationEvents = 0, demolitionEvents = 0;
            Action onDied = () => diedEvents++;
            RFEvent.EventAction onActivated = rigid => activationEvents++;
            RFEvent.EventAction onDemolished = rigid => demolitionEvents++;
            totem.OnDied += onDied;
            foreach (var rigid in rigids)
            {
                rigid.activationEvent.LocalEvent += onActivated;
                rigid.demolitionEvent.LocalEvent += onDemolished;
            }
            try
            {
                record?.Invoke(new { stage = "destructible-blocker-before", scope = Scope, totem = name,
                    healthBefore, maxHealthBefore, rootBlockers = rootBoxes.Length, childBlockers = childBoxes.Length,
                    triggers = triggers.Length, fragmentColliders = fragmentColliders.Length,
                    staticRenderers = staticRenderers.Length, beforeFrame = Time.frameCount });
                int hitFrame = Time.frameCount;
                int hitDamage = name == "Hard_Destroyed_Totem" ? 10 : healthBefore;
                totem.TakeDamage(new DamageData { AttackerTransform = player.transform,
                    AttackType = AttackType.Strong_Counter, DamageAmount = hitDamage, StiffnessAmount = 0 });

                bool rootsOff = rootBoxes.All(c => c && !c.enabled);
                bool childOff = childBoxes.All(c => c && !c.enabled);
                bool triggersPreserved = Preserved(triggers, triggerEnabled);
                bool fragmentsPreserved = Preserved(fragmentColliders, fragmentEnabled);
                bool renderersPreserved = Enumerable.Range(0, staticRenderers.Length)
                    .All(i => staticRenderers[i] && staticRenderers[i].enabled == rendererEnabled[i]);
                bool immediatePass = totem.IsDead && totem.CurrentHealth == 0 && diedEvents == 1 &&
                    Time.frameCount == hitFrame && rootsOff && childOff && triggersPreserved &&
                    fragmentsPreserved && renderersPreserved && totem.MaxHealth == maxHealthBefore;
                record?.Invoke(new { stage = "destructible-blocker-same-frame", scope = Scope, totem = name,
                    pass = immediatePass, hitFrame, afterFrame = Time.frameCount, rootsOff, childOff,
                    triggersPreserved, fragmentsPreserved, renderersPreserved, diedEvents,
                    activationEvents, demolitionEvents, currentHealth = totem.CurrentHealth,
                    maxHealthUnchanged = totem.MaxHealth == maxHealthBefore });
                Require(immediatePass, "Same-frame blocker/preservation regression: " + name);

                float physicsStart = Time.time;
                double realStart = Time.realtimeSinceStartupAsDouble;
                float nextSample = .35f;
                int maxActiveDynamicFragments = 0;
                bool delayedPass = true;
                while (Time.time - physicsStart < 1f)
                {
                    Require(Time.realtimeSinceStartupAsDouble - realStart < 12d,
                        "Timed out waiting for one scaled second of fragment physics: " + name);
                    yield return null;
                    int activeDynamic = fragments.Count(r => r && r.gameObject.activeInHierarchy &&
                        r.GetComponent<Rigidbody>() is Rigidbody body && !body.isKinematic && body.detectCollisions &&
                        r.GetComponents<Collider>().Any(c => c.enabled && !c.isTrigger));
                    maxActiveDynamicFragments = Mathf.Max(maxActiveDynamicFragments, activeDynamic);
                    bool stillOff = rootBoxes.Concat(childBoxes).All(c => c && !c.enabled);
                    bool triggersStillPreserved = Preserved(triggers, triggerEnabled);
                    bool fragmentsStillPreserved = Preserved(fragmentColliders, fragmentEnabled);
                    bool stonesStillPreserved = Enumerable.Range(0, staticRenderers.Length)
                        .All(i => staticRenderers[i] && staticRenderers[i].enabled == rendererEnabled[i]);
                    delayedPass &= stillOff && triggersStillPreserved && fragmentsStillPreserved && stonesStillPreserved;
                    float elapsed = Time.time - physicsStart;
                    if (elapsed >= nextSample)
                    {
                        record?.Invoke(new { stage = "destructible-fragment-physics", scope = Scope, totem = name,
                            scaledSeconds = elapsed, realSeconds = Time.realtimeSinceStartupAsDouble - realStart,
                            timeScale = Time.timeScale, stillOff, triggersStillPreserved, fragmentsStillPreserved,
                            stonesStillPreserved, activeDynamicFragments = activeDynamic, maxActiveDynamicFragments,
                            activationEvents, demolitionEvents });
                        nextSample += .3f;
                    }
                }
                bool statsUnchanged = playerStats == JsonUtility.ToJson(player.Data) && totem.MaxHealth == maxHealthBefore;
                bool pass = immediatePass && delayedPass && maxActiveDynamicFragments > 0 && statsUnchanged;
                record?.Invoke(new { stage = "destructible-blocker-case-completed", scope = Scope, totem = name,
                    pass, immediatePass, delayedPass, maxActiveDynamicFragments, activationEvents, demolitionEvents, statsUnchanged });
                Require(pass, "Rayfire physics/preservation coverage failed: " + name);
                passed++;
            }
            finally
            {
                if (totem) totem.OnDied -= onDied;
                foreach (var rigid in rigids)
                {
                    if (!rigid) continue;
                    rigid.activationEvent.LocalEvent -= onActivated;
                    rigid.demolitionEvent.LocalEvent -= onDemolished;
                }
            }
        }
        record?.Invoke(new { stage = "destructible-blocker-audit-completed", pass = passed == 2,
            cases = passed, scope = Scope });
    }

    private static bool Preserved(Collider[] colliders, bool[] enabled)
    {
        return Enumerable.Range(0, colliders.Length).All(i => colliders[i] && colliders[i].enabled == enabled[i]);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
