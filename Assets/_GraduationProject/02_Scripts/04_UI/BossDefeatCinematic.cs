using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using MoreMountains.Feedbacks;
using UnityEngine.Rendering;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyHealth))]
public sealed class BossDefeatCinematic : MonoBehaviour
{
    public float duration = 5f;
    public float slowScale = .25f;
    private EnemyHealth _health;
    private CinemachineCamera _camera;
    private bool _played;
    private SkinnedMeshRenderer[] _renderers;
    private CinemachineCore.GetBlendOverrideDelegate _previousBlendOverride;
    private bool _blendOverrideInstalled;
    private readonly List<VolumeComponent> _suppressedBlur = new();
    private void OnEnable()
    {
        _health = GetComponent<EnemyHealth>();
        _health.OnDied += Play;
        _played = false;
    }
    private void Play()
    {
        if (_played) return;
        _played = true;
        StartCoroutine(Focus());
    }
    private IEnumerator Focus()
    {
        var main = Camera.main;
        if (!main) yield break;
        // Combat has ended. Discard queued hit/stun slowdowns so they cannot
        // resume underneath the defeat shot after its five seconds expire.
        MMTimeScaleEvent.Reset();
        MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, slowScale, duration, false, 0f, false);
        var cameraObject = new GameObject("BossDefeatCamera");
        _camera = cameraObject.AddComponent<CinemachineCamera>();
        _previousBlendOverride = CinemachineCore.GetBlendOverride;
        CinemachineCore.GetBlendOverride = OverrideEntryBlend;
        _blendOverrideInstalled = true;
        _camera.Lens.FieldOfView = main.fieldOfView;
        _camera.Priority = 100;
        Vector3 start = main.transform.position;
        Quaternion rotation = main.transform.rotation;
        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        var bounds = GetVisualBounds();
        Vector3 focus = bounds.center;
        float radius = Mathf.Max(bounds.extents.magnitude, 2f);
        float distance = Mathf.Max(8f, radius / Mathf.Sin(main.fieldOfView * .5f * Mathf.Deg2Rad) * 1.1f);
        Vector3 offset = (start - focus).normalized * distance;
        foreach (var volume in FindObjectsByType<Volume>(FindObjectsSortMode.None))
            foreach (var effect in volume.profile.components)
                if (effect && effect.active && (effect.GetType().Name.Contains("Blur") || effect.GetType().Name == "DepthOfField"))
                { _suppressedBlur.Add(effect); effect.active = false; }
        offset.y = Mathf.Max(offset.y, 3f);
        _camera.transform.SetPositionAndRotation(start, rotation);
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            if (!_camera) yield break;
            focus = Vector3.Lerp(focus, GetVisualBounds().center, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
            float blend = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / .2f));
            _camera.transform.position = Vector3.Lerp(start, focus + offset, blend);
            _camera.transform.rotation = Quaternion.Slerp(rotation, Quaternion.LookRotation(focus - _camera.transform.position), blend);
            yield return null;
        }
        ReleaseCamera();
    }
    private Bounds GetVisualBounds()
    {
        Bounds bounds = new Bounds(transform.position + Vector3.up * 2.5f, Vector3.one);
        bool found = false;
        if (_renderers != null)
            foreach (var renderer in _renderers)
                if (renderer && renderer.enabled && renderer.gameObject.activeInHierarchy)
                { if (!found) { bounds = renderer.bounds; found = true; } else bounds.Encapsulate(renderer.bounds); }
        return bounds;
    }
    private void ReleaseCamera()
    {
        if (_blendOverrideInstalled)
        {
            if (CinemachineCore.GetBlendOverride == OverrideEntryBlend)
                CinemachineCore.GetBlendOverride = _previousBlendOverride;
            _blendOverrideInstalled = false;
        }
        if (_camera) { _camera.Priority = -1; Destroy(_camera.gameObject); }
        _camera = null;
        foreach (var effect in _suppressedBlur) if (effect) effect.active = true;
        _suppressedBlur.Clear();
    }
    private CinemachineBlendDefinition OverrideEntryBlend(ICinemachineCamera from, ICinemachineCamera to,
        CinemachineBlendDefinition blend, UnityEngine.Object owner)
    {
        // The camera itself interpolates over 0.2 real seconds; avoid a second, scaled brain blend.
        if (to == _camera) return new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0);
        return _previousBlendOverride != null ? _previousBlendOverride(from, to, blend, owner) : blend;
    }
    private void OnDisable()
    {
        if (_health) _health.OnDied -= Play;
        StopAllCoroutines();
        ReleaseCamera();
    }
}
