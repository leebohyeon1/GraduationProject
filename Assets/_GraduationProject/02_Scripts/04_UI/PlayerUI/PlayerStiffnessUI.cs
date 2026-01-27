using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStiffnessUI : MonoBehaviour, IEventListener<PlayerController>, IDisposable
{
    [Header("References")]
    [SerializeField] private Image _stifnessBarImage;                     // 체력바 이미지
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

        _playerController.Health.OnStiffnessChanged += OnStiffnessChanged;

        // UI 초기화
        OnStiffnessChanged(_playerController.Health.CurrentStiffness, _playerController.Health.CurrentStiffness);

        player.RegisterDisposable(this);
    }

    /// <summary>
    /// 객체 해제
    /// </summary>
    public void Dispose()
    {
        _playerController.Health.OnStiffnessChanged -= OnStiffnessChanged;
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
            (float) currentStiffness / _playerController.Health.StiffnessThreshold,
            _animationSpeed)
            .SetEase(_animationCurve);
    }
}
