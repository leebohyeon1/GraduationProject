using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어의 스태미나(Stamina)를 표시하는 Radial UI 클래스입니다.
/// 플레이어 주변의 일정 오프셋 위치를 부드럽게 따라다닙니다.
/// </summary>
public class PlayerStaminaUI : PlayerUIBase
{
    [Header("References")]
    [SerializeField] private Image _positiveImage;    // + 이미지 (양수 Stamina)
    [SerializeField] private Image _negativeImage;    // - 이미지 (음수 Stamina)
    [SerializeField] private CanvasGroup _canvasGroup; // UI 노출 제어를 위한 CanvasGroup

    [Header("Positioning Settings")]
    [SerializeField] private Vector3 _worldOffset = new Vector3(1.5f, 1.0f, 0f); // 플레이어 기준 월드 오프셋 (옆쪽)
    [SerializeField] private float _smoothTime = 0.1f;                         // 따라다니는 부드러움 정도

    [Header("Animation Setting")]
    [SerializeField] private float _animationSpeed = 0.3f;          // 애니메이션 속도
    [SerializeField] private AnimationCurve _animationCurve;        // 애니메이션 커브
    [SerializeField] private float _fadeDuration = 0.2f;            // 나타나고 사라지는 시간

    private RectTransform _rectTransform;
    private Camera _mainCamera;
    private Vector3 _currentVelocity; // SmoothDamp용

    /// <summary>
    /// 플레이어 스폰 이벤트 처리 및 초기화
    /// </summary>
    public override void Initialize(PlayerController player)
    {
        base.Initialize(player);

        _rectTransform = GetComponent<RectTransform>();
        _mainCamera = Camera.main;

        p_player.Stamina.OnStaminaChanged += OnStaminaChanged;

        // UI 초기 상태 설정 (전투 중이 아니면 숨김)
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }

        // 초기 위치 설정
        UpdatePosition(true);
        // UI 초기화 (현재 값 반영)
        UpdateStaminaUI(p_player.Stamina.CurrentStamina);
    }

    /// <summary>
    /// 객체 해제
    /// </summary>
    public override void Dispose()
    {
        if (p_player != null)
        {
            if (p_player.Stamina != null)
                p_player.Stamina.OnStaminaChanged -= OnStaminaChanged;
        }

        _positiveImage?.DOKill();
        _negativeImage?.DOKill();
    }

    private void LateUpdate()
    {
        if (p_player == null || _mainCamera == null) return;

        // 전투 중일 때만 위치를 업데이트하거나 투명도가 0보다 클 때만 업데이트하여 최적화 가능
        if (_canvasGroup != null && _canvasGroup.alpha > 0.01f)
        {
            UpdatePosition(false);
        }
    }

    /// <summary>
    /// 플레이어의 위치를 추적하여 UI 위치를 갱신합니다.
    /// </summary>
    private void UpdatePosition(bool isInstant)
    {
        // 1. 플레이어 위치 + 오프셋 계산
        Vector3 targetWorldPos = p_player.transform.position + _worldOffset;

        // 2. 월드 좌표를 스크린 좌표로 변환
        Vector3 targetScreenPos = _mainCamera.WorldToScreenPoint(targetWorldPos);

        if (isInstant)
        {
            _rectTransform.position = targetScreenPos;
            _currentVelocity = Vector3.zero;
        }
        else
        {
            // 3. SmoothDamp를 이용한 부드러운 추적
            _rectTransform.position = Vector3.SmoothDamp(
                _rectTransform.position,
                targetScreenPos,
                ref _currentVelocity,
                _smoothTime
            );
        }
    }

    /// <summary>
    /// Stamina 변경 이벤트 처리
    /// </summary>
    private void OnStaminaChanged(float previousStamina, float currentStamina)
    {
        UpdateStaminaUI(currentStamina);

        if (_canvasGroup == null) return;

        // 스테미나가 최대치가 아니면 항상 UI를 보여줌
        if (currentStamina < p_player.Stamina.MaxStamina)
        {
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(1f, _fadeDuration);
        }
        else if (!p_player.Combat.IsBattleState)
        {
            // 최대치이고 전투 상태가 아니라면 숨김
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(0f, _fadeDuration);
        }
    }

    /// <summary>
    /// Stamina 값에 따라 UI를 업데이트합니다.
    /// </summary>
    private void UpdateStaminaUI(float currentStamina)
    {
        float threshold = p_player.Stamina.MaxStamina;
        if (threshold <= 0) threshold = 100f;

        float positiveTarget = 0f;
        float negativeTarget = 0f;

        if (currentStamina >= 0)
        {
            positiveTarget = currentStamina / threshold;
        }
        else
        {
            negativeTarget = Mathf.Abs(currentStamina) / threshold;
        }

        // 두 이미지의 애니메이션을 동시에 실행하여 타이밍을 맞춤
        AnimateImage(_positiveImage, positiveTarget);
        AnimateImage(_negativeImage, negativeTarget);
    }

    private void AnimateImage(Image image, float targetFill)
    {
        if (image == null) return;

        image.DOKill();
        
        // 타겟값이 현재와 다를 때만 트윈 실행 (최적화 및 떨림 방지)
        if (Mathf.Abs(image.fillAmount - targetFill) > 0.001f)
        {
            image.DOFillAmount(targetFill, _animationSpeed)
                 .SetEase(_animationCurve);
        }
        else
        {
            image.fillAmount = targetFill;
        }
    }
}
