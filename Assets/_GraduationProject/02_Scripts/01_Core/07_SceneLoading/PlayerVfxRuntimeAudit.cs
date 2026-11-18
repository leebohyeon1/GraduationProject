using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>Explicit QA only: records loaded prefab materials through the real game camera.</summary>
public static class PlayerVfxRuntimeAudit
{
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    public static IEnumerator Run(PlayerController player, string reportPath, Action<object> record)
    {
        var visuals = player ? player.GetComponent<PlayerBuffVisuals>() : null;
        if (!visuals || player.RuntimeData == null || !player.Combat || !Camera.main)
            throw new InvalidOperationException("Player VFX audit requires an initialized, visible game player.");
        var speed = player.RuntimeData.AttackSpeed;
        var modifier = new StatModifier(.5f, StatModifierType.Flat, new object());
        int stacks = player.Combat.CounterStacks;
        string prefix = Path.Combine(Path.GetDirectoryName(reportPath), Path.GetFileNameWithoutExtension(reportPath) + "-vfx-");
        string sourceData = JsonUtility.ToJson(player.Data);
        var previousBlend = CinemachineCore.GetBlendOverride;
        var focusObject = new GameObject("Player VFX audit camera");
        var focus = focusObject.AddComponent<CinemachineCamera>();
        focus.Priority = -100;
        CinemachineCore.GetBlendOverrideDelegate blendOverride = (from, to, blend, owner) =>
            to == focus ? new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0) :
            previousBlend != null ? previousBlend(from, to, blend, owner) : blend;
        var existingProtection = player.GetComponent<RevivalProtection>();
        bool createdProtection = false;
        RevivalProtection protection = existingProtection;
        try
        {
            record?.Invoke(new { stage = "player-vfx-loaded-references", scene = player.gameObject.scene.name,
                speed = visuals.speedPrefab ? visuals.speedPrefab.name : null,
                flame = visuals.blueFlamePrefab ? visuals.blueFlamePrefab.name : null,
                armor = visuals.armorPrefab ? visuals.armorPrefab.name : null,
                weapon = visuals.weaponRenderer ? visuals.weaponRenderer.name : null,
                sourceDataUnchanged = sourceData == JsonUtility.ToJson(player.Data),
                method = "Temporary QA stat modifier and stack API; not natural buff activation; source balance assets unchanged" });
            speed.AddModifier(modifier);
            for (int i = 0; i < 3; i++) player.Combat.AddCounterStack();
            yield return new WaitForSecondsRealtime(1.2f);
            var capture = SceneLoadingSmokeTest.CaptureWorld(prefix + "game-camera.png");
            while (capture.MoveNext()) yield return capture.Current;
            record?.Invoke(new { stage = "player-vfx-original-camera", screenshot = prefix + "game-camera.png", effects = CaptureEffects(visuals) });

            // Preserve the actual main camera's pipeline, volumes and renderer while framing the player for inspection.
            var camera = Camera.main;
            focus.Lens.FieldOfView = camera.fieldOfView;
            focus.transform.SetPositionAndRotation(player.transform.position + Vector3.up * 1.1f - camera.transform.forward * 7f, camera.transform.rotation);
            CinemachineCore.GetBlendOverride = blendOverride; focus.Priority = 1000;
            yield return null; yield return null;
            for (int index = 0; index < 3; index++)
            {
                yield return new WaitForSecondsRealtime(.35f);
                capture = SceneLoadingSmokeTest.CaptureWorld(prefix + "closeup-" + index + ".png");
                while (capture.MoveNext()) yield return capture.Current;
                record?.Invoke(new { stage = "player-vfx-closeup", index, screenshot = prefix + "closeup-" + index + ".png", effects = CaptureEffects(visuals) });
            }
            var life = Resources.FindObjectsOfTypeAll<SphereLife>().FirstOrDefault(s => s.protectionEffectPrefab);
            if (!life) throw new InvalidOperationException("Loaded revival skill has no shield prefab.");
            if (protection && protection.IsProtected) throw new InvalidOperationException("Do not interrupt existing revival protection.");
            if (!protection) { protection = player.gameObject.AddComponent<RevivalProtection>(); createdProtection = true; }
            float started = Time.time;
            double deadline = Time.realtimeSinceStartupAsDouble + 30;
            protection.Activate(player, life._invincibleDuration, null, life.protectionEffectPrefab);
            yield return new WaitForSeconds(.3f);
            capture = SceneLoadingSmokeTest.CaptureWorld(prefix + "shield.png");
            while (capture.MoveNext()) yield return capture.Current;
            var sizedShield = player.transform.Find("RevivalProtectionShield");
            var bodyController = player.GetComponent<CharacterController>();
            record?.Invoke(new { stage = "player-revival-vfx-size", screenshot = prefix + "shield.png",
                bodyHeight = bodyController ? bodyController.height * Mathf.Abs(player.transform.lossyScale.y) : 0f,
                shieldLocalScale = sizedShield ? sizedShield.localScale.ToString("F3") : null,
                shieldLocalCenter = sizedShield ? sizedShield.localPosition.ToString("F3") : null,
                renderBounds = sizedShield ? sizedShield.GetComponentsInChildren<Renderer>().Select(r => new {
                    r.name, size = r.bounds.size.ToString("F3"), center = r.bounds.center.ToString("F3") }).ToArray() : null });
            bool damageBlocked = true, blinkOn = false, blinkOff = false;
            while (Time.time - started < 5.15f)
            {
                if (Time.realtimeSinceStartupAsDouble > deadline) throw new TimeoutException("Revival audit was paused for over 30 seconds.");
                float elapsed = Time.time - started;
                var shield = player.transform.Find("RevivalProtectionShield");
                if (elapsed < 4.9f)
                {
                    var context = new PlayerDamageContext { Data = new DamageData { AttackerTransform = focusObject.transform,
                        AttackType = AttackType.Normal_1, IsMagic = true, DamageAmount = 17, StiffnessAmount = 23,
                        KnockbackForce = 3, KnockbackDuration = .2f } };
                    player.Events.TriggerBeforeDamaged(ref context);
                    damageBlocked &= context.Data.DamageAmount == 0 && context.Data.StiffnessAmount == 0 && context.Data.KnockbackForce == 0;
                }
                if (elapsed > 4.4f && elapsed < 5f && shield)
                { bool enabled = shield.GetComponentsInChildren<Renderer>().Any(r => r.enabled); blinkOn |= enabled; blinkOff |= !enabled; }
                yield return null;
            }
            bool ended = !protection.IsProtected && !player.transform.Find("RevivalProtectionShield");
            bool sourceUnchanged = sourceData == JsonUtility.ToJson(player.Data);
            record?.Invoke(new { stage = "player-revival-vfx-audit", life._invincibleDuration,
                shieldPrefab = life.protectionEffectPrefab.name, screenshot = prefix + "shield.png",
                damageBlocked, blinkOn, blinkOff, ended, sourceUnchanged });
            if (!Mathf.Approximately(life._invincibleDuration, 5) || !damageBlocked || !blinkOn || !blinkOff || !ended || !sourceUnchanged)
                throw new InvalidOperationException("Revival shield timing/damage hook/source preservation assertion failed.");
            // Observe the real second-heavy animation at normal attack speed, including its authored VFX/SFX event.
            speed.RemoveModifier(modifier);
            player.Combat.ResetCounterStack();
            var heavy = Heavy2AnimationRuntimeAudit.Run(player, reportPath, record);
            while (heavy.MoveNext()) yield return heavy.Current;
        }
        finally
        {
            speed.RemoveModifier(modifier);
            if (player && player.Combat) { player.Combat.ResetCounterStack(); for (int i = 0; i < stacks; i++) player.Combat.AddCounterStack(); }
            if (CinemachineCore.GetBlendOverride == blendOverride) CinemachineCore.GetBlendOverride = previousBlend;
            if (focusObject) UnityEngine.Object.Destroy(focusObject);
            if (createdProtection && protection) UnityEngine.Object.Destroy(protection);
        }
    }
    static object[] CaptureEffects(PlayerBuffVisuals visuals)
    {
        return new[] { "_speed", "_flame" }.Select(name => new { name,
            instance = typeof(PlayerBuffVisuals).GetField(name, Private)?.GetValue(visuals) as GameObject })
            .Where(item => item.instance).Select(item => (object)new { item.name, instance = item.instance.name,
                active = item.instance.activeInHierarchy,
                localScale = item.instance.transform.localScale.ToString("F3"),
                particles = item.instance.GetComponentsInChildren<ParticleSystem>(true).Select(ps => new { ps.name, ps.particleCount,
                    localScale = ps.transform.localScale.ToString("F3"),
                    worldScale = ps.transform.lossyScale.ToString("F3"),
                    active = ps.gameObject.activeInHierarchy, ps.isPlaying,
                    propertyBlock = ps.GetComponent<Renderer>() && ps.GetComponent<Renderer>().HasPropertyBlock(),
                    simulationSpace = ps.main.simulationSpace.ToString(),
                    shapeRenderer = ps.shape.skinnedMeshRenderer ? ps.shape.skinnedMeshRenderer.name : null,
                    materials = ps.GetComponent<Renderer>() ? ps.GetComponent<Renderer>().sharedMaterials.Where(m => m).Select(m => new {
                        m.name, shader = m.shader ? m.shader.name : null, supported = m.shader && m.shader.isSupported,
                        texture = m.HasProperty("_Texture") && m.GetTexture("_Texture") ? m.GetTexture("_Texture").name : null,
                        mainTexture = m.HasProperty("_MainTex") && m.GetTexture("_MainTex") ? m.GetTexture("_MainTex").name : null,
                        keywords = m.shaderKeywords,
                        m.renderQueue }).ToArray() : null }).ToArray() }).ToArray();
    }
}
