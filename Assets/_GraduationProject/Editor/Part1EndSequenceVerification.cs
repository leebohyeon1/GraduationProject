#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

internal static class Part1EndSequenceVerification
{
    private const string MenuPath = "Graduation/Diagnostics/Run Part1 End Verification";
    private static readonly string ResultPath =
        Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/part1_end_verification.txt"));
    private static double _nextStepAt;
    private static int _step;
    private static float _platformBeforeY;

    [MenuItem(MenuPath)]
    private static void Run()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ResultPath));
        File.WriteAllText(ResultPath, $"PLAYING={EditorApplication.isPlaying}{Environment.NewLine}");
        if (!EditorApplication.isPlaying)
            return;

        _step = 0;
        _nextStepAt = EditorApplication.timeSinceStartup;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup < _nextStepAt)
            return;

        try
        {
            if (_step == 0)
            {
                GameObject platform = FindSceneObject("Part1_trim_circle_parts 1");
                GameObject karanza = FindSceneObject("Karanza");
                if (platform == null || karanza == null)
                    throw new InvalidOperationException("Karanza 또는 발판을 찾지 못했습니다.");

                ActivateHierarchy(karanza.transform.parent);
                _platformBeforeY = platform.transform.position.y;
                _step = 1;
                _nextStepAt = EditorApplication.timeSinceStartup + 1d;
                return;
            }

            if (_step == 1)
            {
                GameObject karanza = FindSceneObject("Karanza");
                EnemyHealth health = karanza != null ? karanza.GetComponent<EnemyHealth>() : null;
                if (health == null)
                    throw new InvalidOperationException("Karanza EnemyHealth를 찾지 못했습니다.");

                health.Die(null);
                _step = 2;
                _nextStepAt = EditorApplication.timeSinceStartup + 0.8d;
                return;
            }

            if (_step == 2)
            {
                GameObject platform = FindSceneObject("Part1_trim_circle_parts 1");
                float afterY = platform != null ? platform.transform.position.y : float.NaN;
                bool lowered = afterY < _platformBeforeY - 3.5f;
                Record($"PLATFORM beforeY={_platformBeforeY:F3} afterY={afterY:F3} lowered={lowered}");

                InteractableObject whale = FindFinalWhaleInteractable();
                if (whale == null)
                    throw new InvalidOperationException("마지막 고래 InteractableObject를 찾지 못했습니다.");

                whale.Interact();
                Record("WHALE_INTERACT invoked=True");
                _step = 3;
                _nextStepAt = EditorApplication.timeSinceStartup + 0.8d;
                return;
            }

            GameObject timelineObject = FindSceneObject("Whale_TimeLine (1)");
            PlayableDirector director = timelineObject != null ? timelineObject.GetComponent<PlayableDirector>() : null;
            bool timelineActive = timelineObject != null && timelineObject.activeInHierarchy;
            bool directorPlaying = director != null && director.state == PlayState.Playing;
            double directorTime = director != null ? director.time : -1d;
            Record($"CREDITS timelineActive={timelineActive} directorPlaying={directorPlaying} directorTime={directorTime:F3}");
            Record($"RESULT pass={timelineActive && directorPlaying && directorTime > 0d}");
            Stop();
        }
        catch (Exception exception)
        {
            Record("EXCEPTION " + exception);
            Record("RESULT pass=False");
            Stop();
        }
    }

    private static void Record(string message)
    {
        File.AppendAllText(ResultPath, message + Environment.NewLine);
        Debug.Log("[PART1_VERIFY] " + message);
    }

    private static void Stop()
    {
        EditorApplication.update -= Tick;
    }

    private static GameObject FindSceneObject(string objectName)
    {
        foreach (Transform candidate in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (candidate.name == objectName && candidate.gameObject.scene.IsValid())
                return candidate.gameObject;
        }
        return null;
    }

    private static InteractableObject FindFinalWhaleInteractable()
    {
        foreach (InteractableObject candidate in Resources.FindObjectsOfTypeAll<InteractableObject>())
        {
            if (candidate.name == "Whale_Transport_Totem_idle" &&
                candidate.gameObject.scene.IsValid() &&
                candidate.transform.parent != null &&
                candidate.transform.parent.name == "WhaleTimeline")
            {
                return candidate;
            }
        }
        return null;
    }

    private static void ActivateHierarchy(Transform leaf)
    {
        while (leaf != null)
        {
            leaf.gameObject.SetActive(true);
            leaf = leaf.parent;
        }
    }
}
#endif
