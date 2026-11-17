using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStiffnessUI : PlayerUIBase
{
    [Header("References")]
    [SerializeField] private Image _stifnessBarImage;                     // 체력바 이미지

    [Header("Animation Setting")]
    [SerializeField] private float _animationSpeed = 0.3f;          // 애니메이션 속도
    [SerializeField] private AnimationCurve _animationCurve;        // 애니메이션 커브
    private PlayerHealth _health;
    private Tween _fillTween;
    private bool _subscribed;

    /// <summary>
    /// 플레이어 스폰 이벤트 처리
    /// </summary>
    /// <param name="player">플레이어</param>
    public override void Initialize(PlayerController player)
    {
        Dispose();
        if (player == null || player.Health == null) return;
        base.Initialize(player);
        _health = player.Health;
        BindAndRefresh();
    }

    /// <summary>
    /// 객체 해제
    /// </summary>
    public override void Dispose()
    {
        Unbind();
        KillFillTween();
        _health = null;
        p_player = null;
        base.Dispose();
    }

    private void OnEnable() => BindAndRefresh();

    private void OnDisable()
    {
        Unbind();
        KillFillTween();
    }

    private void OnDestroy() => Dispose();

    private void BindAndRefresh()
    {
        if (_health == null || !isActiveAndEnabled) return;
        if (!_subscribed)
        {
            _health.OnStiffnessChanged += OnStiffnessChanged;
            _subscribed = true;
        }
        // Combat HUDs can be hidden without a scene reload. Restore the current
        // value when shown again instead of resuming a stale tween.
        if (_stifnessBarImage != null)
            _stifnessBarImage.fillAmount = GetFillAmount(_health.CurrentStiffness);
    }

    private void Unbind()
    {
        if (_subscribed && _health != null)
            _health.OnStiffnessChanged -= OnStiffnessChanged;
        _subscribed = false;
    }

    private void KillFillTween()
    {
        _fillTween?.Kill();
        _fillTween = null;
    }

    private float GetFillAmount(int stiffness) => _health != null && _health.StiffnessThreshold > 0
        ? Mathf.Clamp01((float)stiffness / _health.StiffnessThreshold) : 0f;

    // 체력 변경 이벤트 처리
    private void OnStiffnessChanged(int previouseStiffness, int currentStiffness)
    {
        if (!isActiveAndEnabled || _health == null || _stifnessBarImage == null) return;
        KillFillTween();
        _fillTween = _stifnessBarImage.DOFillAmount(GetFillAmount(currentStiffness), _animationSpeed)
            .SetEase(_animationCurve)
            .SetLink(_stifnessBarImage.gameObject)
            .OnKill(() => _fillTween = null);
    }
}
