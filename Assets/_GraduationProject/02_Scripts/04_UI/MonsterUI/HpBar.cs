using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [Header("3D Components")]
    [SerializeField] private Renderer _hpRenderer; // 체력바 Quad의 MeshRenderer
    [SerializeField] private Renderer _stiffnessRenderer; // 경직도바 Quad의 MeshRenderer

    [Header("Target Object")]
    [SerializeField] private GameObject _object;
    [SerializeField] private Vector3 _followOffset = new Vector3(0, 2f, 0); // 머리 위로 띄울 오프셋

    private IDamageable _damageable;
    private Mon_Stiffness _monStiffness;
    private MaterialPropertyBlock _propBlock;

    // 셰이더 프로퍼티 ID 캐싱
    private int _frontFillId = Shader.PropertyToID("_FrontFill");
    private int _backFillId = Shader.PropertyToID("_BackFill");

    // DOTween을 위한 현재 잔상 값 저장용 변수
    private float _currentBackFill = 1f;
    private int _displayedHealth = -1;
    private float _currentStiffnessBackFill = 0f;

    private void Start()
    {
        if (_hpRenderer == null) _hpRenderer = GetComponent<Renderer>();

        _propBlock = new MaterialPropertyBlock();

        var owner = GetComponentInParent<EnemyHealth>();
        if (owner != null) _object = owner.gameObject;
        if (_object != null)
        {
            _damageable = _object.GetComponent<IDamageable>();
            _monStiffness = _object.GetComponent<Mon_Stiffness>();

            if (_damageable != null)
            {
                _damageable.OnHealthChanged += ChangeHpBar;

                // 초기화
                float initialRatio = (float)_damageable.CurrentHealth / _damageable.MaxHealth;
                _currentBackFill = initialRatio;

                _hpRenderer.GetPropertyBlock(_propBlock, 0);
                _propBlock.SetFloat(_frontFillId, initialRatio);
                _propBlock.SetFloat(_backFillId, initialRatio);
                _hpRenderer.SetPropertyBlock(_propBlock, 0);
            }

            if (_monStiffness != null)
            {
                _monStiffness.OnStiffnessChanged += ChangeStiffnessBar;

                if (_stiffnessRenderer != null)
                {
                    // 경직도 바 초기화
                    float stiffnessRatio = (float)_monStiffness.CurrentStiffness / _monStiffness.StiffnessThreshold;
                    _currentStiffnessBackFill = stiffnessRatio;

                    _stiffnessRenderer.GetPropertyBlock(_propBlock, 0);
                    _propBlock.SetFloat(_frontFillId, stiffnessRatio);
                    _propBlock.SetFloat(_backFillId, stiffnessRatio);
                    _stiffnessRenderer.SetPropertyBlock(_propBlock, 0);
                }
            }
        }

        transform.SetParent(null);
    }

    private void LateUpdate()
    {
        if (_object == null)
        {
            Destroy(gameObject);
            return;
        }

        if (_damageable != null && _damageable.CurrentHealth != _displayedHealth)
            ChangeHpBar(_displayedHealth, _damageable.CurrentHealth);
        // Hit/dissolve effects also use indexed material blocks. Preserve their properties
        // while keeping the gauges authoritative after those effects have updated.
        if (_propBlock != null && _damageable != null && _hpRenderer) {
            _hpRenderer.GetPropertyBlock(_propBlock,0);
            _propBlock.SetFloat(_frontFillId,Mathf.Clamp01((float)_damageable.CurrentHealth/Mathf.Max(1,_damageable.MaxHealth)));
            _propBlock.SetFloat(_backFillId,_currentBackFill);
            _hpRenderer.SetPropertyBlock(_propBlock,0);
        }
        if (_propBlock != null && _monStiffness != null && _stiffnessRenderer) {
            _stiffnessRenderer.GetPropertyBlock(_propBlock,0);
            _propBlock.SetFloat(_frontFillId,Mathf.Clamp01((float)_monStiffness.CurrentStiffness/Mathf.Max(1,_monStiffness.StiffnessThreshold)));
            _propBlock.SetFloat(_backFillId,_currentStiffnessBackFill);
            _stiffnessRenderer.SetPropertyBlock(_propBlock,0);
        }
        FollowObject();
    }

    private void OnDestroy()
    {
        if (_damageable != null)
        {
            _damageable.OnHealthChanged -= ChangeHpBar;
        }

        if (_monStiffness != null)
        {
            _monStiffness.OnStiffnessChanged -= ChangeStiffnessBar;
        }

        // DOTween 종료 (ID를 활용하여 이 객체에 걸린 트윈만 안전하게 종료)
        DOTween.Kill(_hpRenderer);
        DOTween.Kill(_stiffnessRenderer);
    }

    private void ChangeHpBar(int previousHp, int currentHp)
    {
        if (!_hpRenderer || _damageable == null) return;
        _displayedHealth = currentHp;
        float targetFill = Mathf.Clamp01((float)currentHp / Mathf.Max(1, _damageable.MaxHealth));

        _hpRenderer.GetPropertyBlock(_propBlock, 0);

        // 1. 앞쪽 게이지 (즉시 반영)
        _propBlock.SetFloat(_frontFillId, targetFill);
        _hpRenderer.SetPropertyBlock(_propBlock, 0);

        // 2. 뒤쪽 게이지 (잔상 효과 - MaterialPropertyBlock 값을 Tween)
        DOTween.Kill(_hpRenderer);
        DOTween.To(() => _currentBackFill, x =>
        {
            _currentBackFill = x;
            _hpRenderer.GetPropertyBlock(_propBlock, 0);
            _propBlock.SetFloat(_backFillId, _currentBackFill);
            _hpRenderer.SetPropertyBlock(_propBlock, 0);
        }, targetFill, 0.5f)
        .SetDelay(0.1f)
        .SetEase(Ease.OutCubic)
        .SetId(_hpRenderer); // 트윈 ID 설정

        // 3. 사망 처리
        if (currentHp <= 0)
        {
            Destroy(gameObject, 0.6f);
        }
    }

    private void ChangeStiffnessBar(int previousStiffness, int currentStiffness)
    {
        if (_stiffnessRenderer == null) return;

        float targetFill = (float)currentStiffness / _monStiffness.StiffnessThreshold;

        _stiffnessRenderer.GetPropertyBlock(_propBlock, 0);

        // 1. 앞쪽 게이지 (즉시 반영)
        _propBlock.SetFloat(_frontFillId, targetFill);
        _stiffnessRenderer.SetPropertyBlock(_propBlock, 0);

        // 2. 뒤쪽 게이지 (잔상 효과)
        DOTween.Kill(_stiffnessRenderer);
        DOTween.To(() => _currentStiffnessBackFill, x =>
        {
            _currentStiffnessBackFill = x;
            _stiffnessRenderer.GetPropertyBlock(_propBlock, 0);
            _propBlock.SetFloat(_backFillId, _currentStiffnessBackFill);
            _stiffnessRenderer.SetPropertyBlock(_propBlock, 0);
        }, targetFill, 0.5f)
        .SetDelay(0.1f)
        .SetEase(Ease.OutCubic)
        .SetId(_stiffnessRenderer);
    }

    private void FollowObject()
    {
        // UI가 아니므로 WorldToScreenPoint가 필요 없습니다.
        // 타겟의 월드 좌표 + 오프셋 위치로 바로 이동시킵니다.
        transform.position = _object.transform.position + _followOffset;
    }
}
