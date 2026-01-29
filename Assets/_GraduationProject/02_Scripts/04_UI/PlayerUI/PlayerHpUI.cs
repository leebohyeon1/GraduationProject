using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 체력 UI
/// </summary>
public class PlayerHpUI : MonoBehaviour, IEventListener<PlayerController>, IDisposable
{
    [Header("References")]
    [SerializeField] private Image _hpBarImage;                     // 체력바 이미지
    private PlayerController _playerController;                     // 플레이어

    [Header("Animation Setting")]
    [SerializeField] private float _animationSpeed = 0.3f;          // 애니메이션 속도
    [SerializeField] private AnimationCurve _animationCurve;        // 애니메이션 커브

    [SerializeField] private OnPlayerSpawnedSO _onPlayerSpawned;    // 플레이어 스폰 이벤트

    private void OnEnable()
    {
        _onPlayerSpawned.Subscribe(this);
    }

    private void OnDisable()
    {
        _onPlayerSpawned.Unsubscribe(this);
    }

    /// <summary>
    /// 플레이어 스폰 이벤트 처리
    /// </summary>
    /// <param name="player">플레이어</param>
    public void OnEventTrigger(PlayerController player)
    {
        _playerController = player;

        _playerController.Health.OnHealthChanged += OnHealthChanged;

        // UI 초기화
        OnHealthChanged(_playerController.Health.CurrentHealth, _playerController.Health.CurrentHealth);

        player.RegisterDisposable(this);
    }

    /// <summary>
    /// 객체 해제
    /// </summary>
    public void Dispose()
    {
        _playerController.Health.OnHealthChanged -= OnHealthChanged;
    }

    // 체력 변경 이벤트 처리
    private void OnHealthChanged(int previouseHealth, int currentHealth)
    {
        DOTween.To(
            () => _hpBarImage.fillAmount,
            x =>
            {
                _hpBarImage.fillAmount = x;
            },
            (float) currentHealth / _playerController.Health.MaxHealth,
            _animationSpeed)
            .SetEase(_animationCurve);
    }
}
