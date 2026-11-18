using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Reveals scenery only. The authored stone transforms and colliders never move.</summary>
public sealed class FireflyStonePathReveal : MonoBehaviour
{
    // Stored in the selected GameData, not a process-wide flag. The save system
    // serializes this set and a new game gets an empty set automatically.
    public const string CompletionSaveKey = "World.Part1.SideDungeon2.FireflyStonePathRevealed";

    [Serializable]
    public class Step
    {
        public Vector3 guidePosition;
        public MeshRenderer[] stoneRenderers;
    }

    public GameObject fireflyPrefab;
    public Vector3 startPosition;
    public Step[] steps = Array.Empty<Step>();
    public float activationDistance = 18f;
    public float guideSpeed = 9f;
    public float submergedDepth = 5f;
    public float riseDuration = 1.1f;
    public float arrivalPause = .2f;
    public bool Completed { get; private set; }
    public bool Playing { get; private set; }

    sealed class Visual
    {
        public MeshRenderer original;
        public bool wasEnabled;
        public Transform proxy;
        public float riseStarted = -1f;
    }

    readonly List<Visual[]> _visuals = new List<Visual[]>();
    PlayerController _player;
    GameObject _guide;
    Coroutine _routine;
    float _nextPlayerSearch;

    void Start()
    {
        if (!fireflyPrefab || steps.Length == 0) { enabled = false; return; }
        if (RestoreSavedCompletion()) return;
        foreach (var step in steps)
        {
            var visuals = new List<Visual>();
            foreach (var source in step.stoneRenderers ?? Array.Empty<MeshRenderer>())
            {
                if (!source || !source.enabled || !source.gameObject.activeInHierarchy) continue;
                var mesh = source.GetComponent<MeshFilter>();
                if (!mesh || !mesh.sharedMesh) continue;
                var proxy = new GameObject(source.name + "_RiseVisual");
                proxy.layer = source.gameObject.layer;
                proxy.transform.SetParent(source.transform, false);
                proxy.AddComponent<MeshFilter>().sharedMesh = mesh.sharedMesh;
                var renderer = proxy.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = source.sharedMaterials;
                renderer.shadowCastingMode = source.shadowCastingMode;
                renderer.receiveShadows = source.receiveShadows;
                renderer.lightProbeUsage = source.lightProbeUsage;
                renderer.reflectionProbeUsage = source.reflectionProbeUsage;
                renderer.probeAnchor = source.probeAnchor;
                renderer.renderingLayerMask = source.renderingLayerMask;
                renderer.lightmapIndex = source.lightmapIndex;
                renderer.lightmapScaleOffset = source.lightmapScaleOffset;
                var block = new MaterialPropertyBlock();
                source.GetPropertyBlock(block);
                renderer.SetPropertyBlock(block);
                for (int i = 0; i < source.sharedMaterials.Length; i++)
                {
                    block.Clear();
                    source.GetPropertyBlock(block, i);
                    renderer.SetPropertyBlock(block, i);
                }
                var visual = new Visual { original = source, wasEnabled = source.enabled, proxy = proxy.transform };
                visuals.Add(visual);
                source.enabled = false;
                SetDepth(visual, submergedDepth);
            }
            _visuals.Add(visuals.ToArray());
        }
    }

    void Update()
    {
        if (Completed || Playing || _visuals.Count == 0) return;
        if (RestoreSavedCompletion()) return;
        if (!_player && Time.unscaledTime >= _nextPlayerSearch)
        {
            _nextPlayerSearch = Time.unscaledTime + 1f;
            _player = FindFirstObjectByType<PlayerController>();
        }
        if (!_player || InputReaderSO.ActiveInputMode != InputReaderSO.InputMode.Gameplay) return;
        // Visiting from the dungeon side must also reveal the route before walking back.
        Vector3 position = _player.transform.position;
        bool nearStart = HorizontalDistance(position, startPosition) <= activationDistance;
        bool nearExit = HorizontalDistance(position, steps[steps.Length - 1].guidePosition) <= activationDistance;
        if ((nearStart || nearExit) && IsVisible(startPosition) && IsVisible(steps[0].guidePosition))
            _routine = StartCoroutine(Reveal());
        else if (nearExit && !nearStart && IsVisible(steps[steps.Length - 1].guidePosition))
        {
            // An older save can enter from the dungeon side without the new key.
            // Keep its already usable return path visible instead of playing an
            // unseen guide at the opposite shore.
            MarkRevealed();
            RestoreVisuals();
            Completed = true;
        }
    }

    IEnumerator Reveal()
    {
        Playing = true;
        // Reserve the one-shot before the first yield: a save/death/transition
        // during the rise must restore the finished path on the next load.
        MarkRevealed();
        _guide = Instantiate(fireflyPrefab, startPosition, Quaternion.identity, transform);
        _guide.SetActive(true);
        foreach (var particles in _guide.GetComponentsInChildren<ParticleSystem>(true))
        {
            // The dedicated guide prefab owns size, brightness, shape and density.
            // Do not shrink user-authored particles back to ambient firefly values.
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            particles.Play(true);
        }
        for (int i = 0; i < steps.Length; i++)
        {
            Vector3 destination = steps[i].guidePosition;
            while (_guide && Vector3.Distance(_guide.transform.position, destination) > .05f)
            {
                _guide.transform.position = Vector3.MoveTowards(_guide.transform.position, destination, Mathf.Max(.1f, guideSpeed) * Time.deltaTime);
                UpdateRises();
                yield return null;
            }
            foreach (var visual in _visuals[i]) visual.riseStarted = Time.time;
            float end = Time.time + Mathf.Max(.05f, arrivalPause);
            while (Time.time < end) { UpdateRises(); yield return null; }
        }
        float settleEnd = Time.time + Mathf.Max(.05f, riseDuration);
        while (Time.time < settleEnd) { UpdateRises(); yield return null; }
        RestoreVisuals();
        Completed = true;
        Playing = false;
        _routine = null;
        if (_guide)
        {
            foreach (var particles in _guide.GetComponentsInChildren<ParticleSystem>(true))
                particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(_guide, 3f);
        }
    }

    void UpdateRises()
    {
        foreach (var step in _visuals)
            foreach (var visual in step)
            {
                if (visual.riseStarted < 0f) continue;
                float progress = Mathf.Clamp01((Time.time - visual.riseStarted) / Mathf.Max(.05f, riseDuration));
                SetDepth(visual, submergedDepth * (1f - Mathf.SmoothStep(0f, 1f, progress)));
            }
    }

    static float HorizontalDistance(Vector3 a, Vector3 b) { a.y = b.y; return Vector3.Distance(a, b); }
    static bool IsVisible(Vector3 position)
    {
        var camera = Camera.main;
        if (!camera || !camera.isActiveAndEnabled) return false;
        Vector3 viewport = camera.WorldToViewportPoint(position);
        return viewport.z > camera.nearClipPlane && viewport.x >= .06f && viewport.x <= .94f &&
            viewport.y >= .06f && viewport.y <= .94f;
    }

    static GameData CurrentSave => DataManager.Instance ? DataManager.Instance.GetGameData() : null;
    bool RestoreSavedCompletion()
    {
        var save = CurrentSave;
        if (save == null || !save.GamePlayTagIdSet.Contains(CompletionSaveKey)) return false;
        RestoreVisuals();
        Completed = true;
        return true;
    }

    static void MarkRevealed()
    {
        // This presentation key does not need a quest/tag ScriptableObject or
        // global TagManager event. SavePoint and restart serialize this same data.
        var save = CurrentSave;
        if (save != null) save.AddGamePlayTag(CompletionSaveKey);
    }

    static void SetDepth(Visual visual, float depth)
    {
        if (visual.proxy && visual.original)
            visual.proxy.localPosition = visual.original.transform.InverseTransformVector(Vector3.down * depth);
    }

    void RestoreVisuals()
    {
        foreach (var step in _visuals)
            foreach (var visual in step)
            {
                if (visual.original) visual.original.enabled = visual.wasEnabled;
                if (visual.proxy) { visual.proxy.gameObject.SetActive(false); Destroy(visual.proxy.gameObject); }
            }
        _visuals.Clear();
    }

    void OnDisable()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
        if (Playing) Completed = true;
        Playing = false;
        // Interruption must leave the actual path visibly usable.
        RestoreVisuals();
        if (_guide) Destroy(_guide);
    }
}
