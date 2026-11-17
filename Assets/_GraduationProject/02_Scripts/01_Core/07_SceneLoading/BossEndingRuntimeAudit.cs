using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoreMountains.Feedbacks;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Playables;
using UnityEngine.Events;
using UnityEngine.Timeline;

/// <summary>
/// Opt-in ending/defeat regression. The caller must redirect saves first.
/// Uses the real five-second title chord and PlayerCombat.Attack, but deliberately
/// delivers one scripted lethal hit. This does NOT validate a normal boss fight.
/// </summary>
public static class BossEndingRuntimeAudit
{
    private static double Now => Time.realtimeSinceStartupAsDouble;
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }

    public static IEnumerator Run(Action<object> record)
    {
        var loader = SceneLoadingManager.Instance;
        Require(loader && loader.CurrentActiveChunk && loader.CurrentActiveChunk.name == "Title", "Boss ending audit requires loaded Title.");
        var shortcut = UnityEngine.Object.FindFirstObjectByType<TitleBossShortcut>();
        var title = UnityEngine.Object.FindFirstObjectByType<TitleManager>();
        Require(shortcut && title && shortcut.bossScene, "Title boss shortcut references are missing.");

        Gamepad pad = null;
        try
        {
            double deadline = Now + 20;
            while (!title.CanUseBossShortcut)
            {
                Require(Now < deadline, "Title never accepted the boss chord.");
                yield return null;
            }
            pad = InputSystem.AddDevice<Gamepad>();
            double chordStart = Now;
            InputSystem.QueueStateEvent(pad, new GamepadState()
                .WithButton(GamepadButton.LeftShoulder).WithButton(GamepadButton.RightShoulder));
            deadline = Now + shortcut.holdSeconds + 10;
            while (!DataManager.Instance.IsTemporaryGame)
            {
                Require(Now < deadline, "Five-second boss shortcut did not start a temporary game.");
                yield return null;
            }
            InputSystem.QueueStateEvent(pad, new GamepadState());
            record?.Invoke(new { stage = "boss-shortcut", chordSeconds = Now - chordStart,
                temporaryGame = DataManager.Instance.IsTemporaryGame,
                testKind = "virtual LB+RB input; scripted lethal hit later; not normal combat validation" });
            yield return null;
        }
        finally
        {
            if (pad != null && pad.added) InputSystem.RemoveDevice(pad);
        }

        double loadDeadline = Now + 240;
        BossQuickStartArena arena = null;
        while (loader.IsTeleporting || !(arena = UnityEngine.Object.FindFirstObjectByType<BossQuickStartArena>()))
        {
            Require(Now < loadDeadline, "Temporary boss scene failed to load.");
            Require(loader.LastLoadError == null, "Temporary boss scene load error: " + loader.LastLoadError);
            yield return null;
        }
        Require(arena.boss && arena.entrance && arena.ending && arena.endingTotem, "Quick arena references are missing.");
        var boss = arena.boss;
        double entranceDeadline = Now + 90;
        bool entranceSeen = false;
        while (!entranceSeen || arena.entrance.state == PlayState.Playing || boss.EnemyHealth == null)
        {
            entranceSeen |= arena.entrance.state == PlayState.Playing || arena.entrance.time > .1;
            Require(Now < entranceDeadline, "Boss entrance timeline did not finish.");
            yield return null;
        }
        yield return null; // BossQuickStartArena calls CombatEnter after the entrance completes.
        var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        Require(player && player.Combat != null && boss.EnemyHealth != null, "Boss/player combat components are unavailable.");
        var health = boss.EnemyHealth;
        var cinematic = boss.GetComponent<BossDefeatCinematic>();
        Require(cinematic && Mathf.Abs(cinematic.duration - 3f) < .01f, "Bound boss defeat duration is not three seconds.");
        Require(Mathf.Abs(cinematic.slowScale - .25f) < .01f, "Bound boss defeat slow scale differs from authored .25.");
        Require(health.MaxHealth == 1500 && !health.IsDead, "Boss HP does not match approved 1500 baseline or boss is already dead.");

        var deathFeedback = boss.GetComponentsInChildren<MMF_Player>(true)
            .FirstOrDefault(feedback => feedback.name == "Monster_Boss_Die_FB");
        var wallFeedback = deathFeedback?.FeedbacksList?.OfType<MMF_Position>()
            .FirstOrDefault(feedback => feedback.Active && feedback.AnimatePositionTarget && feedback.DestinationPosition.y < -.5f);
        Require(wallFeedback != null, "Authored death feedback has no bound descending arena wall.");
        Transform wall = wallFeedback.AnimatePositionTarget.transform;
        float wallBeforeY = wall.position.y;
        var ending = arena.ending;
        double endingDuration = ending.duration;
        var returnSignal = FindTitleReturnSignal(ending);
        bool returnSignalReceived = false;
        UnityAction onReturnSignal = () => returnSignalReceived = true;
        returnSignal.reaction.AddListener(onReturnSignal);
        bool endingPlayed = false, endingStopped = false;
        Action<PlayableDirector> onEndingPlayed = director => endingPlayed = true;
        Action<PlayableDirector> onEndingStopped = director => endingStopped = true;
        ending.played += onEndingPlayed;
        ending.stopped += onEndingStopped;
        try
        {
            record?.Invoke(new { stage = "boss-ready", boss = boss.name, health = health.CurrentHealth,
                maxHealth = health.MaxHealth, entranceObserved = entranceSeen,
                cinematic.duration, cinematic.slowScale, wall = wall.name, wallBeforeY, endingDuration,
                returnSignalTime = returnSignal.time,
                balance = BalanceRuntimeAudit.Capture(player) });

            if (SceneLoadingSmokeTest.GetArguments().Contains("--soom-boss-pattern-audit"))
            {
                var patterns = BossPatternRuntimeAudit.Run(player, boss, record);
                try { while (patterns.MoveNext()) yield return patterns.Current; }
                finally { (patterns as IDisposable)?.Dispose(); }
            }

            // Reproduce the former stun-to-death progression failure, then pass through
            // the actual TakeDamage + AttackEvent path to test the strong-hit VFX guard.
            boss.ParrySystem.Parry(AttackType.Strong_Counter,
                Mathf.Max(0, boss.StiffnessSystem.StiffnessThreshold - boss.StiffnessSystem.CurrentStiffness));
            bool stunnedBeforeHit = boss.ParrySystem.CurrentStun == StunType.Full;
            Require(stunnedBeforeHit, "Boss did not enter full stun through its normal parry threshold.");
            var hit = new DamageData { AttackerTransform = player.transform, AttackType = AttackType.Strong_1,
                DamageAmount = health.CurrentHealth + 1, StiffnessAmount = 0 };
            double defeatStart = Now;
            player.Combat.Attack(health, hit);
            Require(health.IsDead, "Scripted test hit did not reach the boss death path.");
            record?.Invoke(new { stage = "boss-scripted-lethal-hit", stunnedBeforeHit,
                damage = hit.DamageAmount, maxHealthUnchanged = health.MaxHealth == 1500,
                method = "PlayerCombat.Attack -> EnemyHealth.TakeDamage -> AttackEvent",
                testKind = "scripted lethal damage; attacks/HP configuration unchanged; not a natural combat win" });

            bool cameraSeen = false;
            double cameraLastSeen = -1;
            float minScale = float.PositiveInfinity, maxScale = float.NegativeInfinity;
            int slowSamples = 0, badSlowSamples = 0;
            double nextRecord = 0;
            while (Now - defeatStart < 3.6)
            {
                double elapsed = Now - defeatStart;
                var focus = UnityEngine.Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None)
                    .FirstOrDefault(camera => camera.name == "BossDefeatCamera");
                if (focus) { cameraSeen = true; cameraLastSeen = elapsed; }
                if (elapsed >= .1 && elapsed <= 2.8)
                {
                    slowSamples++;
                    minScale = Mathf.Min(minScale, Time.timeScale);
                    maxScale = Mathf.Max(maxScale, Time.timeScale);
                    if (Mathf.Abs(Time.timeScale - .25f) > .035f) badSlowSamples++;
                }
                if (elapsed >= nextRecord)
                {
                    record?.Invoke(new { stage = "boss-defeat-sample", elapsed, timeScale = Time.timeScale,
                        focusCamera = focus != null, cameraPosition = focus ? focus.transform.position.ToString("F3") : null });
                    nextRecord += .5;
                }
                yield return null;
            }
            Require(cameraSeen && cameraLastSeen >= 2.75 && cameraLastSeen <= 3.5, "Boss focus camera did not last about three real seconds.");
            Require(slowSamples > 0 && badSlowSamples == 0, "Strong-hit feedback overwrote the boss defeat slow motion.");
            Require(Mathf.Abs(Time.timeScale - 1f) < .035f, "Time scale did not recover after boss defeat.");
            record?.Invoke(new { stage = "boss-defeat-timing", cameraLastSeen, slowSamples, badSlowSamples, minScale, maxScale,
                timeScaleRestored = true });

            double endingDeadline = Now + 45;
            while (!endingPlayed)
            {
                Require(Now < endingDeadline, "Boss ending totem did not start the ending timeline.");
                yield return null;
            }
            Require(GamePlayTagManager.Instance.HasTag("Boss_Clear"), "Boss_Clear was not granted before the ending.");
            Require(wall && wall.position.y < wallBeforeY - .5f, "The authored arena wall did not descend after boss death.");
            record?.Invoke(new { stage = "boss-ending-started", secondsAfterDeath = Now - defeatStart,
                bossClear = true, wallBeforeY, wallAfterY = wall.position.y, ending = ending.name, endingDuration });

            var creditTexts = UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(text => IsCreditLabel(text.text)).ToArray();
            var visibleLabels = new HashSet<string>();
            double maxEndingTime = 0;
            endingDeadline = Now + Math.Max(endingDuration + 30, 90);
            while (!returnSignalReceived && !endingStopped && ending)
            {
                Require(Now < endingDeadline, "Ending timeline did not complete.");
                maxEndingTime = Math.Max(maxEndingTime, ending.time);
                foreach (var text in creditTexts)
                    if (text && IsVisible(text) && visibleLabels.Add(text.text))
                        record?.Invoke(new { stage = "credit-visible", label = text.text, timelineTime = ending.time });
                yield return null;
            }
            if (ending) maxEndingTime = Math.Max(maxEndingTime, ending.time);
            Require(returnSignalReceived, "Ending stopped without its authored Title transition signal.");
            Require(visibleLabels.Count >= 2, "Fewer than two authored credit role/team labels became visibly on screen.");
            record?.Invoke(new { stage = "boss-title-return-signal", returnSignalTime = returnSignal.time,
                returnSignalReceived, maxEndingTime, endingDuration });

            // The authored Title signal precedes the longest animation clip. Verify
            // its actual outcome instead of requiring the director's full duration.
            double titleDeadline = Now + 240;
            while (true)
            {
                Require(loader && DataManager.Instance, "Persistent managers disappeared during Title return.");
                Require(loader.LastLoadError == null, "Ending Title return failed: " + loader.LastLoadError);
                var returnedTitle = UnityEngine.Object.FindFirstObjectByType<TitleManager>();
                if (!loader.IsTeleporting && loader.CurrentActiveChunk && loader.CurrentActiveChunk.name == "Title" &&
                    !DataManager.Instance.IsTemporaryGame && returnedTitle && returnedTitle.CanUseBossShortcut) break;
                Require(Now < titleDeadline, "Ending did not return to a ready Title and close its temporary session.");
                yield return null;
            }
            record?.Invoke(new { stage = "boss-ending-complete", passed = true, maxEndingTime, endingDuration,
                returnSignalTime = returnSignal.time, returnSignalReceived,
                returnedScene = loader.CurrentActiveChunk.name, temporaryGame = DataManager.Instance.IsTemporaryGame,
                visibleCreditLabels = visibleLabels.ToArray(), wallDescended = true, bossClear = true,
                testKind = "temporary session; scripted stunned-boss lethal hit; defeat/ending pipeline only" });
        }
        finally
        {
            returnSignal.reaction.RemoveListener(onReturnSignal);
            if (ending) { ending.played -= onEndingPlayed; ending.stopped -= onEndingStopped; }
        }
    }

    private static (double time, UnityEvent reaction) FindTitleReturnSignal(PlayableDirector director)
    {
        var timeline = director.playableAsset as TimelineAsset;
        var receiver = director.GetComponent<SignalReceiver>();
        Require(timeline && receiver, "Ending TimelineAsset/SignalReceiver is missing.");
        var targetField = typeof(SceneTransitionInteractor).GetField("_targetSceneData", BindingFlags.Instance | BindingFlags.NonPublic);
        Require(targetField != null, "SceneTransitionInteractor target field changed.");
        var tracks = new List<TrackAsset>();
        if (timeline.markerTrack) tracks.Add(timeline.markerTrack);
        foreach (var track in timeline.GetRootTracks()) AddTracks(track, tracks);
        foreach (var signal in tracks.Distinct().SelectMany(track => track.GetMarkers()).OfType<SignalEmitter>())
        {
            if (!signal.asset) continue;
            var reaction = receiver.GetReaction(signal.asset);
            if (reaction == null) continue;
            for (int i = 0; i < reaction.GetPersistentEventCount(); i++)
            {
                var transition = reaction.GetPersistentTarget(i) as SceneTransitionInteractor;
                if (!transition || reaction.GetPersistentMethodName(i) != nameof(SceneTransitionInteractor.TransitionToScene)) continue;
                var target = targetField.GetValue(transition) as SceneDataSO;
                if (target && target.name == "Title") return (signal.time, reaction);
            }
        }
        throw new InvalidOperationException("Ending has no signal wired to the authored Title transition.");
    }

    private static void AddTracks(TrackAsset track, List<TrackAsset> tracks)
    {
        if (!track) return;
        tracks.Add(track);
        foreach (var child in track.GetChildTracks()) AddTracks(child, tracks);
    }

    private static bool IsCreditLabel(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        string value = text.Replace(" ", "").ToLowerInvariant();
        return value.Contains("programing") || value.Contains("programming") || value.Contains("gamedesign") ||
               value.Contains("3dart") || value.Contains("2dart") || value.Contains("team.smallboots");
    }

    private static bool IsVisible(TMP_Text text)
    {
        if (!text.isActiveAndEnabled || text.color.a < .05f || text.canvasRenderer.GetInheritedAlpha() < .05f) return false;
        var canvas = text.GetComponentInParent<Canvas>();
        if (!canvas || !canvas.isActiveAndEnabled) return false;
        var corners = new Vector3[4];
        text.rectTransform.GetWorldCorners(corners);
        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 min = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        Vector2 max = min;
        for (int i = 1; i < corners.Length; i++)
        {
            var point = RectTransformUtility.WorldToScreenPoint(camera, corners[i]);
            min = Vector2.Min(min, point); max = Vector2.Max(max, point);
        }
        return new Rect(min, max - min).Overlaps(new Rect(0, 0, Screen.width, Screen.height));
    }
}
