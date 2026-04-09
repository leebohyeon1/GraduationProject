using HighlightPlus;
using UnityEngine;
using DG.Tweening; // DOTween 네임스페이스 추가

[System.Serializable]
public struct InnerGlowSettings
{
    [Range(0f, 5f)]
    public float Intensity;
    [Range(0, 2f)]
    public float Width;
    [ColorUsage(true, true)]
    public Color Color;
    public InnerGlowBlendMode InnerGlowBlendMode;
    public Visibility InnerGlowVisibility;
}

public class InnerGlowController : MonoBehaviour
{
    [Header("Weapon Highlight Effect")]
    [SerializeField] private HighlightEffect _highLigthEffect;
    [SerializeField] private InnerGlowSettings[] _innerGlowSettings;

    [Header("Animation Settings")]
    [SerializeField] private float _glowDuration = 0.5f; // 변경되는 데 걸리는 시간
    [SerializeField] private AnimationCurve _glowCurve = AnimationCurve.Linear(0, 0, 1, 1); // 적용할 커브

    private Tween _glowTween; // 실행 중인 트윈을 저장할 변수

    private void Start()
    {
        if (_highLigthEffect == null)
        {
            _highLigthEffect = GetComponent<HighlightEffect>();
        }
    }

    public void SetInnerGlowEffect(int glowLevel)
    {
        if (_highLigthEffect == null || glowLevel >= _innerGlowSettings.Length)
        {
            return;
        }

        // 기존에 실행 중인 트윈 취소 (DOTween.IsTweening과 중복되므로 통합)
        if (_glowTween != null && _glowTween.IsActive())
        {
            _glowTween.Kill();
        }

        // 목표 설정값 가져오기
        var targetSetting = _innerGlowSettings[glowLevel];

        // 1. 애니메이션이 필요 없는 값들(블렌드 모드, 가시성 등)은 즉시 적용
        _highLigthEffect.innerGlowBlendMode = targetSetting.InnerGlowBlendMode;
        _highLigthEffect.innerGlowVisibility = targetSetting.InnerGlowVisibility;

        // 2. DOTween Sequence를 사용해 여러 수치를 동시에 부드럽게 변경
        Sequence glowSeq = DOTween.Sequence().SetId(this);

        // [수정됨] Intensity 트윈
        glowSeq.Join(DOTween.To(
            () => _highLigthEffect.innerGlow,
            x => _highLigthEffect.innerGlow = x,
            targetSetting.Intensity,
            _glowDuration
        ).SetEase(_glowCurve));

        // [추가됨] Width(두께) 트윈 - 버그의 핵심 원인 해결!
        glowSeq.Join(DOTween.To(
            () => _highLigthEffect.innerGlowWidth,
            x => _highLigthEffect.innerGlowWidth = x,
            targetSetting.Width,
            _glowDuration
        ).SetEase(_glowCurve));

        // [추가됨] Color(색상) 트윈 - 색상도 함께 보간하면 연출이 훨씬 자연스럽습니다.
        glowSeq.Join(DOTween.To(
            () => _highLigthEffect.innerGlowColor,
            x => _highLigthEffect.innerGlowColor = x,
            targetSetting.Color,
            _glowDuration
        ).SetEase(_glowCurve));

        _glowTween = glowSeq;
    }

    // 객체가 파괴될 때 트윈도 안전하게 정리
    private void OnDestroy()
    {
        if (_glowTween != null)
        {
            _glowTween.Kill();
        }
    }
}