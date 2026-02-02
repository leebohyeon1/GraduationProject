using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 체력 UI
/// </summary>
public class PlayerHpUI : PlayerUIBase
{
    [Header("References")]
    [SerializeField] private Image _hpBarImage;           // 앞쪽 체력바 (즉시 변경됨, 빨강/초록 등)
    [SerializeField] private Image _backHpBarImage;       // 뒤쪽 체력바 (천천히 줄어듦, 흰색/노랑 등)

    [Header("Animation Setting")]
    [SerializeField] private float _animationSpeed = 0.5f;      // 애니메이션 속도 (기존보다 조금 더 길게 잡는 것 추천)
    [SerializeField] private float _animationDelay = 0.1f;      // 데미지 후 대기 시간
    [SerializeField] private AnimationCurve _animationCurve;    // 애니메이션 커브

    /// <summary>
    /// 객체 초기화
    /// </summary>
    /// <param name="player"></param>
    public override void Initialize(PlayerController player)
    {
        base.Initialize(player);

        float initialRatio = (float)p_player.Health.CurrentHealth / p_player.Health.MaxHealth;
        _hpBarImage.fillAmount = initialRatio;

        if (_backHpBarImage != null)
        {
            _backHpBarImage.fillAmount = initialRatio;
        }

        p_player.Health.OnHealthChanged += OnHealthChanged;
    }

    /// <summary>
    /// 객체 해제
    /// </summary>
    public override void Dispose()
    {
        if (p_player != null && p_player.Health != null)
        {
            p_player.Health.OnHealthChanged -= OnHealthChanged;
        }

        // 트윈이 돌고 있는 상태에서 오브젝트가 해제되면 에러가 날 수 있으므로 Kill
        DOTween.Kill(_backHpBarImage);
    }

    // 체력 변경 이벤트 처리
    private void OnHealthChanged(int previousHealth, int currentHealth)
    {
        float targetFill = (float)currentHealth / p_player.Health.MaxHealth;

        // 1. 메인 체력바: 즉시 반영 (타격감)
        _hpBarImage.fillAmount = targetFill;

        // 2. 배경 체력바: 서서히 따라옴 (연출)
        if (_backHpBarImage != null)
        {
            // 기존 트윈 취소 (연속 피격 시 겹침 방지)
            DOTween.Kill(_backHpBarImage);

            // 체력이 회복된 경우(현재 > 이전)에는 잔상도 즉시 채워줌
            if (currentHealth > previousHealth)
            {
                _backHpBarImage.fillAmount = targetFill;
            }
            else
            {
                // 데미지 입은 경우: 잠시 대기 후 서서히 줄어듦
                DOTween.To(
                    () => _backHpBarImage.fillAmount,
                    x => _backHpBarImage.fillAmount = x,
                    targetFill,
                    _animationSpeed)
                    .SetDelay(_animationDelay)
                    .SetEase(_animationCurve)
                    .SetLink(gameObject); // 게임오브젝트 파괴 시 트윈 자동 정리 안전장치
            }
        }
    }
}