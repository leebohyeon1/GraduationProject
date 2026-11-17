using DanielIlett.SnapshotShaders2.URP;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

/// <summary>Player impact feedback independent of a level's local atmosphere volume.</summary>
public sealed class CombatImpactFeedback : MonoBehaviour
{
    private VolumeProfile _profile;
    private RadialBlurFeedback _radial;
    private MMF_Player _heavyHit;
    private int _lastHeavyHitFrame = -1;

    private void Awake()
    {
        var effects = new GameObject("Combat impact feedback");
        effects.transform.SetParent(transform, false);
        effects.layer = 0;
        _profile = ScriptableObject.CreateInstance<VolumeProfile>();
        var blur = _profile.Add<BlurSettings>(true);
        blur.blurMode.value = BlurMode.Radial;
        blur.strength.value = 1;
        blur.active = false;
        var volume = effects.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 100;
        volume.profile = _profile;
        _radial = effects.AddComponent<RadialBlurFeedback>();
        _radial.postProcessVolume = volume;

        _heavyHit = effects.AddComponent<MMF_Player>();
        _heavyHit.InitializationMode = MMFeedbacks.InitializationModes.Script;
        _heavyHit.AddFeedback(new MMF_TimescaleModifier
        {
            TimeScale = 0.2f,
            TimeScaleDuration = 0.18f,
            Mode = MMF_TimescaleModifier.Modes.Shake
        });
        var pulse = new MMF_Events { PlayEvents = new UnityEvent() };
        pulse.PlayEvents.AddListener(() => _radial.TriggerBlurPulse(26, 0.02f, 0.05f, 0.18f));
        _heavyHit.AddFeedback(pulse);
        _heavyHit.Initialization();
    }

    public void PlayParry()
    {
        if (isActiveAndEnabled && Time.timeScale > 0f)
            _radial.TriggerBlurPulse(20, 0.02f, 0.04f, 0.15f);
    }

    public void PlayHeavyHit()
    {
        // One swing hitting several colliders/enemies must not stack slow requests.
        if (!isActiveAndEnabled || Time.timeScale <= 0f || _lastHeavyHitFrame == Time.frameCount) return;
        _lastHeavyHitFrame = Time.frameCount;
        _heavyHit.PlayFeedbacks(transform.position);
    }

    private void OnDestroy()
    {
        if (_profile == null) return;
        foreach (var component in _profile.components)
            if (component != null) Destroy(component);
        Destroy(_profile);
    }
}
