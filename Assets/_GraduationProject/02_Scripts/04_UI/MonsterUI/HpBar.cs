using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [Header("3D Components")]
    [SerializeField] private Renderer _renderer; // 체력바 Quad의 MeshRenderer

    [Header("Target Object")]
    [SerializeField] private GameObject _object;
    [SerializeField] private Vector3 _followOffset = new Vector3(0, 2f, 0); // 머리 위로 띄울 오프셋

    private IDamageable _damageable;
    private MaterialPropertyBlock _propBlock;

    // 셰이더 프로퍼티 ID 캐싱
    private int _frontFillId = Shader.PropertyToID("_FrontFill");
    private int _backFillId = Shader.PropertyToID("_BackFill");

    // DOTween을 위한 현재 잔상 값 저장용 변수
    private float _currentBackFill = 1f;

    private void Start()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();

        if (_object != null)
        {
            _damageable = _object.GetComponent<IDamageable>();
            if (_damageable != null)
            {
                _damageable.OnHealthChanged += ChangeHpBar;

                // 초기화
                float initialRatio = (float)_damageable.CurrentHealth / _damageable.MaxHealth;
                _currentBackFill = initialRatio;

                _renderer.GetPropertyBlock(_propBlock);
                _propBlock.SetFloat(_frontFillId, initialRatio);
                _propBlock.SetFloat(_backFillId, initialRatio);
                _renderer.SetPropertyBlock(_propBlock);
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

        FollowObject();
    }

    private void OnDestroy()
    {
        if (_damageable != null)
        {
            _damageable.OnHealthChanged -= ChangeHpBar;
        }

        // DOTween 종료 (ID를 활용하여 이 객체에 걸린 트윈만 안전하게 종료)
        DOTween.Kill(this);
    }

    private void ChangeHpBar(int previousHp, int currentHp)
    {
        float targetFill = (float)currentHp / _damageable.MaxHealth;

        _renderer.GetPropertyBlock(_propBlock);

        // 1. 앞쪽 게이지 (즉시 반영)
        _propBlock.SetFloat(_frontFillId, targetFill);
        _renderer.SetPropertyBlock(_propBlock);

        // 2. 뒤쪽 게이지 (잔상 효과 - MaterialPropertyBlock 값을 Tween)
        DOTween.Kill(this);
        DOTween.To(() => _currentBackFill, x =>
        {
            _currentBackFill = x;
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(_backFillId, _currentBackFill);
            _renderer.SetPropertyBlock(_propBlock);
        }, targetFill, 0.5f)
        .SetDelay(0.1f)
        .SetEase(Ease.OutCubic)
        .SetId(this); // 트윈 ID 설정

        // 3. 사망 처리
        if (currentHp <= 0)
        {
            Destroy(gameObject, 0.6f);
        }
    }

    private void FollowObject()
    {
        // UI가 아니므로 WorldToScreenPoint가 필요 없습니다.
        // 타겟의 월드 좌표 + 오프셋 위치로 바로 이동시킵니다.
        transform.position = _object.transform.position + _followOffset;
    }
}