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
    private Sequence blurSequence; // DOTween 시퀀스 관리를 위함

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
        // 코드로 제어할 것임을 알리기 위해 overrideState를 true로 설정
        blurSettings.blurMode.overrideState = true;
        blurSettings.strength.overrideState = true;

        // 블러 모드를 Radial로 강제 설정
        blurSettings.blurMode.value = BlurMode.Radial;

        // 시작할 때는 효과를 꺼둠
        blurSettings.active = false;
        blurSettings.strength.value = 0;
    }

    /// <summary>
    /// 블러를 켰다가 끄는 효과를 DOTween으로 실행합니다.
    /// 이 함수를 UnityEvent (예: 버튼 클릭)에 연결하세요.
    /// </summary>
    public void TriggerBlurPulse()
    {
        if (blurSettings == null) return;

        // 이전에 실행 중인 트윈이 있다면 중복 실행을 막기 위해 종료
        if (blurSequence != null && blurSequence.IsActive())
        {
            blurSequence.Kill();
        }

        // 1. 즉시 효과를 활성화합니다.
        blurSettings.active = true;

        // 2. DOTween 시퀀스를 생성합니다.
        // (VolumeParameter.value는 직접 트윈할 수 없으므로 DOTween.To 사용)
        blurSequence = DOTween.Sequence();

        // 3. "블러 In" (강도가 0에서 maxBlurStrength까지 증가)
        blurSequence.Append(
            DOTween.To(
                () => blurSettings.strength.value,      // getter: 현재 값
                x => blurSettings.strength.value = x, // setter: 값 설정
                Intensity,                        // targetValue: 목표 값
                DurationIn // duration: 지속 시간
            ).SetEase(Ease.OutQuad) // 부드럽게 시작
        );

        blurSequence.AppendInterval(ActiveDuration); // 최대 강도에서 잠시 유지   

        // 4. "블러 Out" (강도가 maxBlurStrength에서 0으로 감소)
        blurSequence.Append(
            DOTween.To(
                () => blurSettings.strength.value,
                x => blurSettings.strength.value = x,
                0,                                      // 0으로 되돌림
                DurationOut
            ).SetEase(Ease.InQuad) // 부드럽게 감속
        );

        // 5. 시퀀스가 모두 완료되면 효과를 비활성화합니다.
        blurSequence.OnComplete(() =>
        {
            blurSettings.active = false;
        });
    }

    void OnDestroy()
    {
        // 오브젝트가 파괴될 때 실행 중인 트윈을 안전하게 종료
        if (blurSequence != null)
        {
            blurSequence.Kill();
        }
    }
}
