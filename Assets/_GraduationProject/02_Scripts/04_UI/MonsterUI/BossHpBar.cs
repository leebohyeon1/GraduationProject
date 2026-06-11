using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHpBar : MonoBehaviour, IEventListener<EnemyStateData>
{
    [Header("UI Components")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _bossNameText;
    [SerializeField] private Image _hpBarFront;
    [SerializeField] private Image _hpBarBack;
    [SerializeField] private Image _stiffnessBar;

    [Header("UI Settings")]
    [SerializeField] private float _fadeDuration = 0.5f;  // UI 페이드 인/아웃 시간
    [SerializeField] private float _stiffnessTime = 0.5f; // 잔상이 머무는 시간
    [SerializeField] private float _lerpSpeed = 0.5f;    // 잔상이 줄어드는 속도

    [SerializeField] private EnemyStateEventSO _playerStateEvent;

    [Header("Target Object")]
    [SerializeField] private GameObject _object;
    // _followOffset 변수 삭제됨

    private Camera _mainCamera;
    private RectTransform _transform;
    private IDamageable _damageable;
    private IStiffness _stiffness;

    private void Awake()
    {
        _transform = GetComponent<RectTransform>();
        _mainCamera = Camera.main;
        
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }
    }

    private void OnEnable()
    {
        if (_playerStateEvent != null)
        {
            _playerStateEvent.Subscribe(this);
        }
    }

    private void Start()
    {
        if (_object != null)
        {
            Initialize(_object);
        }
    }

    private void LateUpdate()
    {
        // 보스가 할당되어 있었는데 사라진 경우 UI 제거
        if (_damageable != null && _object == null)
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        CleanupDamageable();
        CleanupStiffness();

        if (_playerStateEvent != null)
        {
            _playerStateEvent.Unsubscribe(this);
        }

        // DOTween 안전하게 종료
        DOTween.Kill(_hpBarFront);
        DOTween.Kill(_hpBarBack);
        if (_stiffnessBar != null) DOTween.Kill(_stiffnessBar);
        if (_canvasGroup != null) DOTween.Kill(_canvasGroup);
    }

    public void OnEventTrigger(EnemyStateData data)
    {
        if (data.stateType == EnemyStateType.SummonBoss)
        {
            Initialize(data.enemy.gameObject);
        }
    }

    private void Initialize(GameObject boss)
    {
        if (boss == null) return;

        // 기존 이벤트 해제
        CleanupDamageable();
        CleanupStiffness();

        _object = boss;
        _bossNameText.text = boss.name;

        // 1. 체력 관련 초기화
        _damageable = boss.GetComponent<IDamageable>();
        if (_damageable != null)
        {
            _damageable.OnHealthChanged += ChangeHpBar;
            _damageable.OnDied += HandleBossDied;

            float initialRatio = (float)_damageable.CurrentHealth / _damageable.MaxHealth;
            _hpBarFront.fillAmount = initialRatio;
            _hpBarBack.fillAmount = initialRatio;
        }

        // 2. 강인함(Stiffness) 관련 초기화
        _stiffness = boss.GetComponent<IStiffness>();
        if (_stiffness == null)
        {
            _stiffness = boss.GetComponentInChildren<IStiffness>();
        }

        if (_stiffness != null)
        {
            _stiffness.OnStiffnessChanged += ChangeStiffnessBar;
            
            float initialStiffRatio = (float)_stiffness.CurrentStiffness / _stiffness.StiffnessThreshold;
            if (_stiffnessBar != null) _stiffnessBar.fillAmount = initialStiffRatio;
        }

        // 3. 등장 시 페이드 인
        if (_canvasGroup != null)
        {
            DOTween.Kill(_canvasGroup);
            _canvasGroup.DOFade(1f, _fadeDuration).SetEase(Ease.OutQuad);
        }
    }

    private void HandleBossDied()
    {
        // 사망 시 페이드 아웃 후 파괴
        if (_canvasGroup != null)
        {
            DOTween.Kill(_canvasGroup);
            _canvasGroup.DOFade(0f, _fadeDuration).SetEase(Ease.InQuad);
        }
    }

    private void CleanupDamageable()
    {
        if (_damageable != null)
        {
            _damageable.OnHealthChanged -= ChangeHpBar;
            _damageable.OnDied -= HandleBossDied;
            _damageable = null;
        }
    }

    private void CleanupStiffness()
    {
        if (_stiffness != null)
        {
            _stiffness.OnStiffnessChanged -= ChangeStiffnessBar;
            _stiffness = null;
        }
    }

    private void ChangeStiffnessBar(int previousStiff, int currentStiff)
    {
        if (_stiffness == null || _stiffnessBar == null) return;

        float targetFill = (float)currentStiff / _stiffness.StiffnessThreshold;
        
        // 강인함 게이지는 즉시 혹은 부드럽게 반영 (여기선 부드럽게 연출)
        DOTween.Kill(_stiffnessBar);
        _stiffnessBar.DOFillAmount(targetFill, 0.2f).SetEase(Ease.OutQuad);
    }

    private void ChangeHpBar(int previousHp, int currentHp)
    {
        if (_damageable == null) return;

        float targetFill = (float)currentHp / _damageable.MaxHealth;

        // 1. 앞쪽 게이지 (즉시 반영)
        _hpBarFront.fillAmount = targetFill;

        // 2. 뒤쪽 게이지 (Stiffness 및 잔상 효과)
        DOTween.Kill(_hpBarBack);

        if (currentHp < previousHp)
        {
            // 데미지를 입었을 때: Stiffness (대기 후 서서히 감소)
            DOTween.To(() => _hpBarBack.fillAmount,
                        x => _hpBarBack.fillAmount = x,
                        targetFill,
                        _lerpSpeed)
                        .SetDelay(_stiffnessTime)
                        .SetEase(Ease.OutCubic);
        }
        else
        {
            // 힐을 받았을 때: 즉시 반영
            _hpBarBack.fillAmount = targetFill;
        }
    }

}