using DanielIlett.SnapshotShaders2.URP;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class RadialBlurFeedback : MonoBehaviour
{
    public Volume postProcessVolume;

    [Header("DOTween 설정")]
    public int Intensity = 20;
    public float ActiveDuration = 0.5f;

    public AnimationCurve InCurve;
    public float DurationIn = 0.5f;

    public AnimationCurve OutCurve;
    public float DurationOut = 1.0f;

    private BlurSettings blurSettings;
    private Coroutine _pulseRoutine;

    void Start()
    {
        InitializeBlur();
    }

    private bool InitializeBlur()
    {
        if (blurSettings != null) return true;
        if (postProcessVolume == null)
        {
            Debug.LogError("Post Process Volume이 할당되지 않았습니다!");
            return false;
        }

        // 볼륨 프로필에서 BlurSettings를 찾아옵니다.
        if (!postProcessVolume.profile.TryGet<BlurSettings>(out blurSettings))
        {
            Debug.LogError("할당된 Volume Profile에서 BlurSettings를 찾을 수 없습니다.");
            return false;
        }

        // --- 초기 설정 ---
        // 강제로 값을 제어하기 위해 overrideState를 true로 설정
        blurSettings.blurMode.overrideState = true;
        blurSettings.strength.overrideState = true;

        // 초기에는 효과 비활성화
        blurSettings.active = false;
        blurSettings.strength.value = 0;
        return true;
    }

    /// <summary>
    /// 화면이 흐려졌다가 돌아오는 효과를 DOTween으로 구현합니다.
    /// 이 함수를 UnityEvent (예: 버튼 클릭)에 연결하세요.
    /// </summary>
    public void TriggerBlurPulse()
    {
        TriggerBlurPulse(Intensity, DurationIn, ActiveDuration, DurationOut);
    }

    /// <summary>
    /// Unity Event에서 intensity, durationIn, durationOut을 지정하여 호출할 수 있는 함수입니다.
    /// ActiveDuration은 인스펙터에 설정된 기본값을 사용합니다.
    /// </summary>
    public void TriggerBlurPulse(int intensity, float durationIn, float durationOut)
    {
        TriggerBlurPulse(intensity, durationIn, ActiveDuration, durationOut);
    }

    /// <summary>
    /// Unity Event에서 모든 매개변수를 지정하여 호출할 수 있는 함수입니다.
    /// </summary>
    public void TriggerBlurPulse(int intensity, float durationIn, float activeDuration, float durationOut)
    {
        if (!InitializeBlur()) return;

        if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
        _pulseRoutine = StartCoroutine(Pulse(Mathf.Max(2, intensity), Mathf.Max(0, durationIn),
            Mathf.Max(0, activeDuration), Mathf.Max(0, durationOut)));
    }

    private IEnumerator Pulse(int intensity, float durationIn, float activeDuration, float durationOut)
    {
        blurSettings.active = true;
        float startStrength = blurSettings.strength.value;
        for (float elapsed = 0f; elapsed < durationIn; elapsed += Time.unscaledDeltaTime)
        {
            float progress = elapsed / durationIn;
            blurSettings.strength.value = Mathf.RoundToInt(Mathf.Lerp(startStrength, intensity, 1f - (1f - progress) * (1f - progress)));
            yield return null;
        }
        // A shader/VFX loading hitch used to consume an entire short tween before
        // it rendered once. Always present the peak for at least one frame.
        blurSettings.strength.value = intensity;
        yield return null;
        for (float elapsed = 0f; elapsed < activeDuration; elapsed += Time.unscaledDeltaTime)
            yield return null;
        for (float elapsed = 0f; elapsed < durationOut; elapsed += Time.unscaledDeltaTime)
        {
            float progress = elapsed / durationOut;
            blurSettings.strength.value = Mathf.RoundToInt(Mathf.Lerp(intensity, 1f, progress * progress));
            yield return null;
        }
        blurSettings.strength.value = 1;
        blurSettings.active = false;
        _pulseRoutine = null;
    }

    /// <summary>
    /// Unity Event의 한계를 극복하기 위해 Vector3를 매개변수로 받는 함수입니다.
    /// 인스펙터의 Unity Event에서 X: Intensity, Y: DurationIn, Z: DurationOut으로 입력하세요.
    /// </summary>
    public void TriggerBlurPulse(Vector3 settings)
    {
        TriggerBlurPulse((int)settings.x, settings.y, ActiveDuration, settings.z);
    }

    /// <summary>
    /// Unity Event에서 Vector4를 매개변수로 받는 함수입니다.
    /// 인스펙터의 Unity Event에서 X: Intensity, Y: DurationIn, Z: ActiveDuration, W: DurationOut으로 입력하세요.
    /// </summary>
    public void TriggerBlurPulse(Vector4 settings)
    {
        TriggerBlurPulse((int)settings.x, settings.y, settings.z, settings.w);
    }

    // --- MMF Player 및 외부 제어를 위한 개별 설정 메서드 ---

    public void SetIntensity(int intensity) => Intensity = intensity;
    public void SetDurationIn(float duration) => DurationIn = duration;
    public void SetActiveDuration(float duration) => ActiveDuration = duration;
    public void SetDurationOut(float duration) => DurationOut = duration;

    /// <summary>
    /// 문자열 형식을 통해 한 번에 호출하는 방식입니다 (예: "30, 0.5, 0.5, 1.0")
    /// </summary>
    public void TriggerBlurPulseString(string settings)
    {
        string[] split = settings.Split(',');
        if (split.Length >= 4)
        {
            int intensity = int.Parse(split[0]);
            float inTime = float.Parse(split[1]);
            float activeTime = float.Parse(split[2]);
            float outTime = float.Parse(split[3]);
            TriggerBlurPulse(intensity, inTime, activeTime, outTime);
        }
        else if (split.Length >= 3)
        {
            int intensity = int.Parse(split[0]);
            float inTime = float.Parse(split[1]);
            float outTime = float.Parse(split[2]);
            TriggerBlurPulse(intensity, inTime, ActiveDuration, outTime);
        }
    }

    void OnDestroy()
    {
        StopPulse();
    }

    void OnDisable() => StopPulse();

    private void StopPulse()
    {
        if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
        _pulseRoutine = null;
        if (blurSettings != null)
        {
            blurSettings.strength.value = 0;
            blurSettings.active = false;
        }
    }
}
