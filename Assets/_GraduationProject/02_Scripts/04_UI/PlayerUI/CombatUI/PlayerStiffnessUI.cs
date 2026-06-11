using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStiffnessUI : PlayerUIBase
{
    [Header("References")]
    [SerializeField] private Image _stifnessBarImage;                     // 체력바 이미지

    [Header("Animation Setting")]
    [SerializeField] private float _animationSpeed = 0.3f;          // 애니메이션 속도
    [SerializeField] private AnimationCurve _animationCurve;        // 애니메이션 커브


    /// <summary>
    /// 플레이어 스폰 이벤트 처리
    /// </summary>
    /// <param name="player">플레이어</param>
    public override void Initialize(PlayerController player)
    {
        base.Initialize(player);

        p_player.Health.OnStiffnessChanged += OnStiffnessChanged;

        // UI 초기화
        OnStiffnessChanged(p_player.Health.CurrentStiffness, p_player.Health.CurrentStiffness);
    }

    /// <summary>
    /// 객체 해제
    /// </summary>
    public override void Dispose()
    {
        p_player.Health.OnStiffnessChanged -= OnStiffnessChanged;
    }

    // 체력 변경 이벤트 처리
    private void OnStiffnessChanged(int previouseStiffness, int currentStiffness)
    {
        DOTween.To(
            () => _stifnessBarImage.fillAmount,
            x =>
            {
                _stifnessBarImage.fillAmount = x;
            },
            (float) currentStiffness / p_player.Health.StiffnessThreshold,
            _animationSpeed)
            .SetEase(_animationCurve);
    }
}
