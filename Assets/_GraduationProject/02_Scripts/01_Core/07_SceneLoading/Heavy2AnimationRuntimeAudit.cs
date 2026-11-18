using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>Opt-in QA: enters the real heavy attack state and observes its authored animation events.</summary>
public static class Heavy2AnimationRuntimeAudit
{
    private static readonly FieldInfo PlayedSound = typeof(MMF_MMSoundManagerSound).GetField("_playedAudioSource", BindingFlags.Instance | BindingFlags.NonPublic);

    public static IEnumerator Run(PlayerController player, string reportPath, Action<object> record)
    {
        if (!player || player.FSM == null || !player.Animator || !player.Combat || !player.Stamina)
            throw new InvalidOperationException("Heavy2 audit requires an initialized player.");
        if (!(player.FSM.CurrentState is PlayerIdleState))
            throw new InvalidOperationException("Heavy2 audit requires an idle player at the isolated safe spawn.");
        var feedback = player.GetComponentsInChildren<MMF_Player>(true).Single(f => f.name == "Atk_Strong_02_FB");
        var particles = feedback.FeedbacksList.OfType<MMF_ParticlesInstantiation>().Single(f => f.Label == "Blue flame slam explosion");
        var explosionSound = feedback.FeedbacksList.OfType<MMF_MMSoundManagerSound>().Single(f => f.Label == "Blue flame explosion layer");
        var impactSound = feedback.FeedbacksList.OfType<MMF_MMSoundManagerSound>().Single(f => f.Label == "Heavy slam impact");
        var clip = player.Animator.runtimeAnimatorController.animationClips.First(c => c.name == "Player_StrongAttack2");
        var clipEvent = clip.events.Single(e => e.functionName == "PlayFeedback" && e.stringParameter == "Atk_Strong_02_FB");
        int oldStacks = player.Combat.CounterStacks;
        float oldStamina = player.Stamina.CurrentStamina;
        var position = player.transform.position;
        var rotation = player.transform.rotation;
        string sourceData = JsonUtility.ToJson(player.Data);
        string prefix = Path.Combine(Path.GetDirectoryName(reportPath), Path.GetFileNameWithoutExtension(reportPath) + "-heavy2-");
        int feedbackCount = 0, attackStarted = 0, attackPerformed = 0;
        bool clipSeen = false, explosionHeard = false, impactHeard = false;
        float feedbackAt = -1, eventClipTime = -1, eventNormalized = -1, anchorError = float.PositiveInfinity;
        float started = Time.time;
        int maxParticles = 0, captures = 0;
        bool oldEvents = feedback.Events.TriggerMMFeedbacksEvents;
        Vector3 spawnAnchor = position;
        Action onStart = () => attackStarted++;
        Action onPerform = () => attackPerformed++;
        MMFeedbacksEvent.Delegate onFeedback = (source, kind) =>
        {
            if (source != feedback || kind != MMFeedbacksEvent.EventTypes.Play) return;
            feedbackCount++; feedbackAt = Time.time;
            eventNormalized = player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            eventClipTime = (eventNormalized % 1f) * clip.length;
            spawnAnchor = particles.PositionMode == MMF_ParticlesInstantiation.PositionModes.Transform && particles.InstantiateParticlesPosition
                ? particles.InstantiateParticlesPosition.position + particles.Offset : feedback.transform.position + particles.Offset;
        };
        try
        {
            player.Events.AttackStarted += onStart;
            player.Events.AttackPerformed += onPerform;
            MMFeedbacksEvent.Register(onFeedback); feedback.Events.TriggerMMFeedbacksEvents = true;
            record?.Invoke(new { stage = "heavy2-animation-start", method = "API enters actual FSM state; Animator advances normally; no direct feedback or attack-event invocation",
                clip = clip.name, clip.length, authoredFeedbackSeconds = clipEvent.time,
                particlePrefab = particles.ParticlesPrefab.name, explosionClip = explosionSound.Sfx.name, impactClip = impactSound.Sfx.name,
                sourceDataUnchanged = sourceData == JsonUtility.ToJson(player.Data) });
            var capture = SceneLoadingSmokeTest.CaptureWorld(prefix + "before.png");
            while (capture.MoveNext()) yield return capture.Current;
            player.Combat.ResetCounterStack();
            for (int i = 0; i < 3; i++) player.Combat.AddCounterStack();
            player.Combat.ResetHeavyAttackComboIndex();
            player.Combat.IncreaseHeavyAttackComboIndex(); // The real OnEnter increments 0 to the second attack (1).
            started = Time.time;
            double deadline = Time.realtimeSinceStartupAsDouble + 20;
            player.FSM.ChangeState<PlayerHeavyAttackState>();
            while (Time.time - started < 2.5f)
            {
                if (Time.realtimeSinceStartupAsDouble > deadline) throw new TimeoutException("Heavy2 animation stalled.");
                clipSeen |= player.Animator.GetCurrentAnimatorClipInfo(0).Any(info => info.clip == clip);
                var explosion = particles.InstantiatedParticleSystem;
                var sound = PlayedSound?.GetValue(explosionSound) as AudioSource;
                var impact = PlayedSound?.GetValue(impactSound) as AudioSource;
                explosionHeard |= sound && sound.isPlaying && sound.clip == explosionSound.Sfx;
                impactHeard |= impact && impact.isPlaying && impact.clip == impactSound.Sfx;
                if (feedbackAt >= 0 && explosion && explosion.gameObject.activeInHierarchy)
                {
                    maxParticles = Mathf.Max(maxParticles, explosion.GetComponentsInChildren<ParticleSystem>().Sum(p => p.particleCount));
                    anchorError = Mathf.Min(anchorError, Vector3.Distance(explosion.transform.position, spawnAnchor));
                    float[] captureTimes = { .06f, .18f, .35f };
                    if (captures < captureTimes.Length && Time.time - feedbackAt >= captureTimes[captures])
                    {
                        string png = prefix + "impact-" + captures + ".png";
                        capture = SceneLoadingSmokeTest.CaptureWorld(png);
                        while (capture.MoveNext()) yield return capture.Current;
                        record?.Invoke(new { stage = "heavy2-animation-capture", screenshot = png, elapsed = Time.time - started,
                            sinceFeedback = Time.time - feedbackAt, particleCount = maxParticles,
                            position = new[] { explosion.transform.position.x, explosion.transform.position.y, explosion.transform.position.z },
                            anchorError, explosionSoundPlaying = sound && sound.isPlaying, impactSoundPlaying = impact && impact.isPlaying,
                            audioListenerPaused = AudioListener.pause, audioListenerVolume = AudioListener.volume });
                        captures++;
                    }
                }
                yield return null;
            }
            bool sourceUnchanged = sourceData == JsonUtility.ToJson(player.Data);
            record?.Invoke(new { stage = "heavy2-animation-result", clipSeen, feedbackCount, attackStarted, attackPerformed,
                authoredFeedbackSeconds = clipEvent.time, observedEventClipSeconds = eventClipTime, eventNormalized,
                maxParticles, anchorError, explosionSoundPlayed = explosionHeard, originalImpactPlayed = impactHeard,
                captures, sourceUnchanged, returnedToIdle = player.FSM.CurrentState is PlayerIdleState });
            if (!clipSeen || feedbackCount != 1 || attackStarted < 1 || attackPerformed < 1 || maxParticles == 0 ||
                anchorError > .15f || !explosionHeard || !impactHeard || captures != 3 || !sourceUnchanged)
                throw new InvalidOperationException("Heavy2 animation/feedback/audio/ground-anchor assertion failed.");
        }
        finally
        {
            player.Events.AttackStarted -= onStart; player.Events.AttackPerformed -= onPerform;
            MMFeedbacksEvent.Unregister(onFeedback); feedback.Events.TriggerMMFeedbacksEvents = oldEvents;
            player.FSM.ChangeState<PlayerIdleState>();
            player.Combat.ResetCounterStack();
            for (int i = 0; i < oldStacks; i++) player.Combat.AddCounterStack();
            player.Stamina.ChangeStamina(oldStamina - player.Stamina.CurrentStamina);
            var controller = player.GetComponent<CharacterController>(); bool enabled = controller && controller.enabled;
            if (enabled) controller.enabled = false;
            player.transform.SetPositionAndRotation(position, rotation);
            if (enabled) controller.enabled = true;
        }
    }
}
