using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Explicit QA only. The smoke runner owns startup, isolated saves and scene travel.
/// Uses the real gate API and temporarily adds/removes one real clear tag in QA data.
/// This proves API/lifecycle behavior, not physical input or dungeon completion play.
/// </summary>
public static class DungeonDirectionGuideRuntimeAudit
{
    private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private static int _guideInstanceId;
    private static HashSet<int> _firstEntranceIds;

    public static IEnumerator Run(PlayerController player, Action<object> record)
    {
        RequireIsolatedSave();
        RequireScene("Part1");
        if (!player || !player.Health || player.Health.IsDead)
            throw new InvalidOperationException("Dungeon guide audit needs the current living player.");
        if (InputReaderSO.ActiveInputMode != InputReaderSO.InputMode.Gameplay)
            throw new InvalidOperationException("Dungeon guide audit must start in Gameplay input mode.");
        var tags = GamePlayTagManager.Instance;
        if (!tags) throw new InvalidOperationException("Gameplay tags are not ready.");
        var entrances = UnityEngine.Object.FindObjectsByType<SceneTransitionInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(x => x.TargetSceneData && IsDungeon(x.TargetSceneData.name) && !x.completesDungeonOnExit && x.completedDungeonTag)
            .GroupBy(x => x.TargetSceneData.name).Select(x => x.First()).ToArray();
        if (entrances.Length != 3 || entrances.Any(x => tags.HasTag(x.completedDungeonTag.ID)))
            throw new InvalidOperationException("Use a fresh isolated QA game with three unfinished dungeon entrances.");
        var gate = UnityEngine.Object.FindObjectsByType<DragonGateSequence>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(x => x.isActiveAndEnabled && x.requiredTotems != null && x.requiredTotems.Length > 0);
        if (!gate || Read<bool>(gate, "_busy") || Read<bool>(gate, "_encounterStarted"))
            throw new InvalidOperationException("No ready real DragonGateSequence in Part1.");
        if (gate.requiredTotems.Any(x => x && tags.HasTag(x.ID)))
            throw new InvalidOperationException("Gate-failure audit expects fresh QA totem progress.");
        if (FindGuide()) throw new InvalidOperationException("Fresh QA unexpectedly has an existing dungeon guide.");

        double start = Time.realtimeSinceStartupAsDouble;
        gate.Interact(); // Public method used by the actual gate interaction.
        while (!FindGuide() || Read<bool>(gate, "_busy"))
        {
            if (Time.realtimeSinceStartupAsDouble - start > 30)
                throw new TimeoutException("Real dragon-gate failure sequence did not finish within 30 seconds.");
            yield return null;
        }
        var wait = WaitForVisible(3);
        while (wait.MoveNext()) yield return wait.Current;
        var first = Capture();
        _guideInstanceId = first.instance;
        _firstEntranceIds = new HashSet<int>(first.markers.Select(x => x.entranceId));
        AssertMarkers(first, 3);
        if (Read<bool>(gate, "_encounterStarted"))
            throw new InvalidOperationException("Missing totems incorrectly started the gate encounter.");
        record(new { stage = "dungeon-guide-gate-failed", valid = true, seconds = Time.realtimeSinceStartupAsDouble - start,
            trigger = "Actual DragonGateSequence.Interact public API; no physical interaction input", snapshot = first });

        // The tag API updates QA data and its real listeners. Restore it even on failure.
        var clearTag = entrances.OrderBy(x => x.TargetSceneData.name).First().completedDungeonTag;
        try
        {
            tags.AddTag(clearTag);
            wait = WaitForVisible(2);
            while (wait.MoveNext()) yield return wait.Current;
            var cleared = Capture();
            AssertMarkers(cleared, 2);
            var marker = cleared.markers.Single(x => x.tag == clearTag.ID);
            if (marker.visible || marker.active)
                throw new InvalidOperationException("Cleared dungeon indicator is still visible.");
            record(new { stage = "dungeon-guide-clear-tag", valid = true, clearTag = clearTag.ID,
                trigger = "QA-only GamePlayTagManager.AddTag; does not claim actual exit/clear gameplay", snapshot = cleared });
        }
        finally
        {
            if (tags) tags.RemoveTag(clearTag);
        }
        wait = WaitForVisible(3);
        while (wait.MoveNext()) yield return wait.Current;
        var restored = Capture();
        AssertMarkers(restored, 3);
        if (tags.HasTag(clearTag.ID)) throw new InvalidOperationException("QA clear tag was not restored.");
        record(new { stage = "dungeon-guide-clear-tag-restored", valid = true, snapshot = restored });
    }

    // Call after each real smoke transition: Part2-A, Part1, then Title.
    public static IEnumerator VerifyScene(string target, Action<object> record)
    {
        RequireIsolatedSave();
        RequireScene(target);
        if (_guideInstanceId == 0 || _firstEntranceIds == null)
            throw new InvalidOperationException("Run the gate audit before scene lifecycle assertions.");
        if (target == "Part1")
        {
            var wait = WaitForVisible(3);
            while (wait.MoveNext()) yield return wait.Current;
        }
        else
        {
            // Allow the guide's LateUpdate to apply scene/input gating.
            yield return null;
            yield return null;
        }
        var state = Capture();
        bool sameInstance = state.instance == _guideInstanceId;
        bool valid;
        if (target == "Part1")
        {
            AssertMarkers(state, 3);
            bool referencesRebuilt = state.markers.All(x => !_firstEntranceIds.Contains(x.entranceId));
            valid = sameInstance && referencesRebuilt;
        }
        else if (target == "Title")
            valid = sameInstance && !state.requested && !state.canvasEnabled && state.alpha == 0 && state.markers.Length == 0;
        else
            valid = sameInstance && state.requested && !state.canvasEnabled && state.alpha == 0;
        record(new { stage = "dungeon-guide-scene", target, valid, sameInstance, snapshot = state });
        if (!valid) throw new InvalidOperationException("Dungeon guide scene lifecycle failed: " + target);
    }

    private static IEnumerator WaitForVisible(int expected)
    {
        double deadline = Time.realtimeSinceStartupAsDouble + 8;
        while (true)
        {
            var state = Capture();
            if (state.canvasEnabled && state.alpha >= .99f && state.markers.Count(x => x.active && x.visible) == expected) yield break;
            if (Time.realtimeSinceStartupAsDouble > deadline)
                throw new TimeoutException("Dungeon guide did not show " + expected + " markers; current " + state.markers.Count(x => x.active));
            yield return null;
        }
    }

    private static void AssertMarkers(GuideSnapshot state, int visibleCount)
    {
        if (!state.requested || !state.canvasEnabled || state.alpha < .99f || state.markers.Length != 3 ||
            state.markers.Count(x => x.visible && x.active) != visibleCount || state.markers.Any(x => !x.entranceAlive || !x.matchesMapSprite))
            throw new InvalidOperationException("Dungeon guide markers do not match real entrances/map artwork.");
        var guide = FindGuide();
        if (guide.GetComponent<UnityEngine.UI.GraphicRaycaster>() || guide.GetComponent<CanvasGroup>().blocksRaycasts)
            throw new InvalidOperationException("Passive dungeon guide must not intercept UI input.");
    }

    private static GuideSnapshot Capture()
    {
        var guide = FindGuide();
        if (!guide) return new GuideSnapshot();
        var state = new GuideSnapshot { instance = guide.GetInstanceID(), requested = Read<bool>(guide, "_requested"),
            canvasEnabled = guide.GetComponent<Canvas>().enabled, alpha = guide.GetComponent<CanvasGroup>().alpha };
        var maps = UnityEngine.Object.FindObjectsByType<MapUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var rows = new List<MarkerSnapshot>();
        foreach (object marker in Read<IList>(guide, "_markers"))
        {
            var entrance = Read<SceneTransitionInteractor>(marker, "entrance");
            var tag = Read<GamePlayTagSO>(marker, "clearTag");
            var rect = Read<RectTransform>(marker, "root");
            var arrow = Read<RectTransform>(marker, "arrow");
            var image = rect ? rect.GetComponent<UnityEngine.UI.Image>() : null;
            Sprite mapSprite = null;
            foreach (var map in maps)
            {
                if (map.DungeonMarkers == null) continue;
                var entry = map.DungeonMarkers.FirstOrDefault(x => x != null && x.clearTag && tag && x.clearTag.ID == tag.ID && x.icon);
                if (entry == null) continue;
                var source = entry.icon.GetComponent<UnityEngine.UI.Image>();
                if (!source) source = entry.icon.GetComponentInChildren<UnityEngine.UI.Image>(true);
                if (source) { mapSprite = source.sprite; break; }
            }
            rows.Add(new MarkerSnapshot { target = entrance && entrance.TargetSceneData ? entrance.TargetSceneData.name : null,
                tag = tag ? tag.ID : null, entranceId = entrance ? entrance.GetInstanceID() : 0,
                entranceAlive = entrance && entrance.gameObject.scene.IsValid() && entrance.gameObject.scene.isLoaded,
                active = rect && rect.gameObject.activeInHierarchy, visible = Read<bool>(marker, "visible"),
                edge = Read<bool>(marker, "edge"), arrow = arrow && arrow.gameObject.activeInHierarchy,
                matchesMapSprite = image && image.sprite && image.sprite == mapSprite,
                position = rect ? rect.anchoredPosition.ToString("F2") : null });
        }
        state.markers = rows.ToArray();
        return state;
    }

    private static void RequireIsolatedSave()
    {
        if (!SceneLoadingSmokeTest.GetArguments().Contains("--soom-loading-smoke") || !DataManager.Instance)
            throw new InvalidOperationException("This audit only runs under the opt-in isolated smoke runner.");
        string save = Read<string>(DataManager.Instance, "_saveFileName");
        string userDirectory = Path.GetFullPath(Application.persistentDataPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (string.IsNullOrEmpty(save) || !Path.IsPathRooted(save) || !save.EndsWith(".smoke-save.json", StringComparison.OrdinalIgnoreCase) ||
            Path.GetFullPath(save).StartsWith(userDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Refusing dungeon QA tag changes without an isolated smoke save.");
    }

    private static void RequireScene(string target)
    {
        var loader = SceneLoadingManager.Instance;
        if (!loader || loader.IsTeleporting || !loader.CurrentActiveChunk || loader.CurrentActiveChunk.name != target || loader.LastLoadError != null)
            throw new InvalidOperationException("Dungeon guide audit requires fully loaded " + target);
    }

    private static bool IsDungeon(string name) => name == "Part2-A" || name == "Part2-B" || name == "Part2-C";
    private static DungeonDirectionGuide FindGuide() => UnityEngine.Object.FindFirstObjectByType<DungeonDirectionGuide>(FindObjectsInactive.Include);
    private static T Read<T>(object target, string field)
    {
        var info = target.GetType().GetField(field, InstanceFields);
        if (info == null) throw new MissingFieldException(target.GetType().Name, field);
        return (T)info.GetValue(target);
    }

    private sealed class GuideSnapshot
    {
        public int instance;
        public bool requested, canvasEnabled;
        public float alpha;
        public MarkerSnapshot[] markers = Array.Empty<MarkerSnapshot>();
    }

    private sealed class MarkerSnapshot
    {
        public string target, tag, position;
        public int entranceId;
        public bool entranceAlive, active, visible, edge, arrow, matchesMapSprite;
    }
}
