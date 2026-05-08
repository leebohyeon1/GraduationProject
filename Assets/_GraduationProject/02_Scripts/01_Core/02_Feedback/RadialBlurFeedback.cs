using DanielIlett.SnapshotShaders2.URP;
using DG.Tweening;
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
    private Sequence blurSequence; // DOTween 시퀀스 관리를 위한 변수

    void Start()
    {
        if (postProcessVolume == null)
        {
            Debug.LogError("Post Process Volume이 할당되지 않았습니다!");
            return;
        }

        // 볼륨 프로필에서 BlurSettings를 찾아옵니다.
        if (!postProcessVolume.profile.TryGet<BlurSettings>(out blurSettings))
        {
            Debug.LogError("할당된 Volume Profile에서 BlurSettings를 찾을 수 없습니다.");
            return;
        }

        // --- 초기 설정 ---
        // 강제로 값을 제어하기 위해 overrideState를 true로 설정
        blurSettings.blurMode.overrideState = true;
        blurSettings.strength.overrideState = true;

        // 초기에는 효과 비활성화
        blurSettings.active = false;
        blurSettings.strength.value = 0;
    }

    /// <summary>
    /// 화면이 흐려졌다가 돌아오는 효과를 DOTween으로 구현합니다.
    /// 이 함수를 UnityEvent (예: 버튼 클릭)에 연결하세요.
    /// </summary>
    public void TriggerBlurPulse()
    {
        TriggerBlurPulse(Intensity, DurationIn, DurationOut);
    }

    /// <summary>
    /// Unity Event에서 intensity와 duration을 지정하여 호출할 수 있는 함수입니다.
    /// </summary>
    public void TriggerBlurPulse(int intensity, float durationIn, float durationOut)
    {
        if (blurSettings == null) return;

        // 실행 중인 시퀀스가 있다면 중복 실행을 막기 위해 종료
        if (blurSequence != null && blurSequence.IsActive())
        {
            blurSequence.Kill();
        }

        // 1. 블러 효과를 활성화합니다.
        blurSettings.active = true;

        // 2. DOTween 시퀀스를 생성합니다.
        blurSequence = DOTween.Sequence();

        // 3. "페이드 In" (강도를 0에서 intensity까지 증가)
        blurSequence.Append(
            DOTween.To(
                () => blurSettings.strength.value,      // getter: 현재 값
                x => blurSettings.strength.value = x, // setter: 값 설정
                intensity,                        // targetValue: 목표 값
                durationIn // duration: 소요 시간
            ).SetEase(Ease.OutQuad) // 부드럽게 설정
        );

        blurSequence.AppendInterval(ActiveDuration); // 최대 강도에서 잠시 유지   

        // 4. "페이드 Out" (강도를 intensity에서 0으로 감소)
        blurSequence.Append(
            DOTween.To(
                () => blurSettings.strength.value,
                x => blurSettings.strength.value = x,
                0,                                      // 0으로 되돌림
                durationOut
            ).SetEase(Ease.InQuad) // 부드럽게 설정
        );

        // 5. 시퀀스가 완료되면 효과를 비활성화합니다.
        blurSequence.OnComplete(() =>
        {
            blurSettings.active = false;
        });
    }

    /// <summary>
    /// Unity Event의 한계를 극복하기 위해 Vector3를 매개변수로 받는 함수입니다.
    /// 인스펙터의 Unity Event에서 X: Intensity, Y: DurationIn, Z: DurationOut으로 입력하세요.
    /// </summary>
    public void TriggerBlurPulse(Vector3 settings)
    {
        TriggerBlurPulse((int)settings.x, settings.y, settings.z);
    }

    // --- MMF Player 및 외부 제어를 위한 개별 설정 메서드 ---

    public void SetIntensity(int intensity) => Intensity = intensity;
    public void SetDurationIn(float duration) => DurationIn = duration;
    public void SetDurationOut(float duration) => DurationOut = duration;

    /// <summary>
    /// 문자열 형식을 통해 한 번에 호출하는 방식입니다 (예: "30, 0.5, 1.0")
    /// </summary>
    public void TriggerBlurPulseString(string settings)
    {
        string[] split = settings.Split(',');
        if (split.Length >= 3)
        {
            int intensity = int.Parse(split[0]);
            float inTime = float.Parse(split[1]);
            float outTime = float.Parse(split[2]);
            TriggerBlurPulse(intensity, inTime, outTime);
        }
    }

    void OnDestroy()
    {
        // 오브젝트가 파괴될 때 실행 중인 트윈을 중단
        if (blurSequence != null)
        {
            blurSequence.Kill();
        }
    }
}
