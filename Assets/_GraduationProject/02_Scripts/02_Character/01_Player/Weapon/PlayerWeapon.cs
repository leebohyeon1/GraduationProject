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
    public Color Color;
    public InnerGlowBlendMode InnerGlowBlendMode;
    public Visibility InnerGlowVisibility;
}

public class PlayerWeapon : MonoBehaviour
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

    public void SetWeaponInnerGlowEffect(int chargeTier)
    {
        if (_highLigthEffect == null || chargeTier >= _innerGlowSettings.Length)
        {
            return;
        }

        // 목표 설정값 가져오기
        var targetSetting = _innerGlowSettings[chargeTier];

        // 1. 애니메이션이 필요 없는 값들은 즉시 적용
        _highLigthEffect.innerGlowWidth = targetSetting.Width;
        _highLigthEffect.innerGlowColor = targetSetting.Color;
        _highLigthEffect.innerGlowBlendMode = targetSetting.InnerGlowBlendMode;
        _highLigthEffect.innerGlowVisibility = targetSetting.InnerGlowVisibility;

        // 2. Intensity 값 애니메이션 (DOTween)

        // 기존에 실행 중인 트윈이 있다면 중지 (중복 실행 방지)
        if (_glowTween != null && _glowTween.IsActive())
        {
            _glowTween.Kill();
        }

        // 현재 값에서 목표 값(targetSetting.Intensity)까지 _glowDuration 동안 변경
        _glowTween = DOTween.To(
            () => _highLigthEffect.innerGlow,                // Getter: 현재 값 가져오기
            x => _highLigthEffect.innerGlow = x,             // Setter: 값 적용하기
            targetSetting.Intensity,                         // Target: 목표 값
            _glowDuration                                    // Duration: 지속 시간
        )
        .SetEase(_glowCurve);                                // Ease: 인스펙터에서 설정한 커브 적용
    }

    // 객체가 파괴될 때 트윈도 안전하게 정리
    private void OnDestroy()
    {
        if (_glowTween != null) _glowTween.Kill();
    }
}