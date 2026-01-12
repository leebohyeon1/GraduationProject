using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [SerializeField] private Image _hpBarSlider;
    [SerializeField] private Image _stiffnessBarSlider;
    [SerializeField] private GameObject _object;
    [SerializeField] private Vector3 _followOffset;
    private Camera _mainCamera;
    private RectTransform _transform;
    private IDamageable _damageable;
    private IStiffness _stiffness;
    
    private void Start()
    {
        _mainCamera = Camera.main;
        _transform = GetComponent<RectTransform>();
        _damageable = _object.GetComponent<IDamageable>();
        _stiffness = _object.GetComponent<IStiffness>();

        _damageable.OnHealthChanged += ChangeHpBar;
        _stiffness.OnStiffnessChanged += ChangeStiffness;
    }
    
    private void FixedUpdate()
    {
        FollowObject();
    }

    private void OnDestroy()
    {
        if (_damageable != null)
        {
            _damageable.OnHealthChanged -= ChangeHpBar;
        }

        if(_stiffness != null)
        {
            _stiffness.OnStiffnessChanged -= ChangeStiffness;
        }
    }

    private void ChangeHpBar(int previousHp, int currentHp)
    {
        DOTween.Kill(_hpBarSlider, true);

        DOTween.To(() => _hpBarSlider.fillAmount,
                    x => _hpBarSlider.fillAmount = x,
                    currentHp/(float)_damageable.MaxHealth, 0.3f)
                    .SetEase(Ease.Linear)
                    .SetId(_hpBarSlider);
    }

    private void ChangeStiffness(int previousStiffness, int currentStiffness)
    {
        DOTween.Kill(_stiffnessBarSlider, true);

        DOTween.To(() => _stiffnessBarSlider.fillAmount,
                    x => _stiffnessBarSlider.fillAmount = x,
                    currentStiffness / 100.0f, 0.3f)
                    .SetEase(Ease.Linear)
                    .SetId(_hpBarSlider);
    }

    private void FollowObject()
    {
        if (_object != null)
        {
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(_object.transform.position) + _followOffset;
            _transform.position = screenPos;
        }
    }

}
