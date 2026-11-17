using System;
using System.Collections;
using DanielIlett.SnapshotShaders2.URP;
using UnityEngine;

/// <summary>Explicitly invoked QA only; does not start itself or save gameplay data.</summary>
public static class CombatFeedbackRuntimeAudit
{
    public static IEnumerator Run(PlayerController player, Action<object> record)
    {
        if (!player || !player.Health || player.Health.IsDead)
            throw new InvalidOperationException("Combat feedback audit requires a living test player.");
        var impact = player.GetComponent<CombatImpactFeedback>();
        if (!impact) throw new InvalidOperationException("CombatImpactFeedback is missing from the test player.");
        var radials = player.GetComponentsInChildren<RadialBlurFeedback>(true);
        string[] cases = { "normal-parry-success-event", "strong-parry-success-event", "heavy-hit-feedback-direct-api" };

        for (int index = 0; index < cases.Length; ++index)
        {
            int startFrame = Time.frameCount;
            double started = Time.realtimeSinceStartupAsDouble;
            float firstFrameDelta = Time.unscaledDeltaTime;
            float maxFrameDelta = firstFrameDelta;
            float minTimeScale = Time.timeScale;
            int maxRadialStrength = 0;
            bool slowSeen = false;
            bool blurSeen = false;
            if (index == 0) player.Events.TriggerCounterSucceeded(player.transform, AttackType.Normal_Counter);
            else if (index == 1) player.Events.TriggerCounterSucceeded(player.transform, AttackType.Strong_Counter);
            else impact.PlayHeavyHit();

            do
            {
                yield return null;
                if (!player || !player.Health || player.Health.IsDead)
                    throw new InvalidOperationException("Test player disappeared or died during feedback audit.");
                minTimeScale = Mathf.Min(minTimeScale, Time.timeScale);
                maxFrameDelta = Mathf.Max(maxFrameDelta, Time.unscaledDeltaTime);
                slowSeen |= Time.timeScale > 0f && Time.timeScale < 0.99f;
                foreach (var radial in radials)
                {
                    if (!radial || !radial.postProcessVolume) continue;
                    var volume = radial.postProcessVolume;
                    var profile = volume.HasInstantiatedProfile() ? volume.profile : volume.sharedProfile;
                    if (!profile || !profile.TryGet<BlurSettings>(out var blur)) continue;
                    maxRadialStrength = Mathf.Max(maxRadialStrength, blur.strength.value);
                    blurSeen |= radial.isActiveAndEnabled && volume.isActiveAndEnabled && volume.isGlobal &&
                        volume.weight > 0f && blur.active && blur.blurMode.value == BlurMode.Radial && blur.strength.value > 1;
                }
            }
            while (Time.realtimeSinceStartupAsDouble - started < 1d);

            bool restored = Mathf.Abs(Time.timeScale - 1f) <= 0.01f;
            bool passed = slowSeen && blurSeen && restored;
            record?.Invoke(new
            {
                stage = "combat-feedback-audit",
                caseName = cases[index],
                scene = player.gameObject.scene.name,
                invocation = index < 2 ? "actual CounterSucceeded event; naturally adds a temporary parry stack" : "direct PlayHeavyHit feedback API; no attack or damage simulated",
                startFrame,
                frameDelta = Time.frameCount - startFrame,
                elapsedSeconds = Time.realtimeSinceStartupAsDouble - started,
                firstFrameDelta,
                maxFrameDelta,
                minTimeScale,
                maxRadialStrength,
                slowSeen,
                blurSeen,
                restored,
                finalTimeScale = Time.timeScale,
                pass = passed
            });
            if (!passed)
                throw new InvalidOperationException($"Combat feedback audit failed ({cases[index]}): slow={slowSeen}, radial={blurSeen}, restored={restored}, max frame delta={maxFrameDelta:F3}s.");
        }
    }
}
