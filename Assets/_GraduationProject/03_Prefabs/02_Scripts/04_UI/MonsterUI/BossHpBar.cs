using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BossHpBar : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _hpBarFront;
    [SerializeField] private Image _hpBarBack;

    [Header("Target Object")]
    [SerializeField] private GameObject _object;
    // _followOffset 변수 삭제됨

    private Camera _mainCamera;
    private RectTransform _transform;
    private IDamageable _damageable;

    private void Start()
    {
        _mainCamera = Camera.main;
        _transform = GetComponent<RectTransform>();

        if (_object != null)
        {
            _damageable = _object.GetComponent<IDamageable>();
            if (_damageable != null)
            {
                _damageable.OnHealthChanged += ChangeHpBar;

                // 초기화
                float initialRatio = (float)_damageable.CurrentHealth / _damageable.MaxHealth;
                _hpBarFront.fillAmount = initialRatio;
                _hpBarBack.fillAmount = initialRatio;
            }
        }
    }

    private void LateUpdate()
    {
        // 안전장치: 몬스터(_object)가 이미 파괴되어 사라졌다면, HP바도 즉시 파괴
        if (_object == null)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (_damageable != null)
        {
            _damageable.OnHealthChanged -= ChangeHpBar;
        }

        // DOTween 안전하게 종료
        DOTween.Kill(_hpBarFront);
        DOTween.Kill(_hpBarBack);
    }

    private void ChangeHpBar(int previousHp, int currentHp)
    {
        float targetFill = (float)currentHp / _damageable.MaxHealth;

        // 1. 앞쪽 게이지 (즉시 반영)
        _hpBarFront.fillAmount = targetFill;

        // 2. 뒤쪽 게이지 (잔상 효과)
        DOTween.Kill(_hpBarBack);
        DOTween.To(() => _hpBarBack.fillAmount,
                    x => _hpBarBack.fillAmount = x,
                    targetFill,
                    0.5f)
                    .SetDelay(0.1f)
                    .SetEase(Ease.OutCubic);

        // 3. 사망 처리: HP가 0 이하라면 UI 파괴
        if (currentHp <= 0)
        {
            // 잔상 애니메이션 시간(0.6s)만큼 기다렸다가 파괴
            Destroy(gameObject, 0.6f);
        }
    }
}