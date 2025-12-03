using System.Collections;
using System.Threading;
using BH_Lib.DI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [SerializeField] private Image _hpBarSlider;
    [SerializeField] private GameObject _object;
    [SerializeField] private Vector3 _followOffset;
    private Camera _mainCamera;
    private RectTransform _transform;
    private IDamageable _damageable;
    
    private void Start()
    {
        _mainCamera = Camera.main;
        _transform = GetComponent<RectTransform>();
        _damageable = _object.GetComponent<IDamageable>();
        _damageable.OnHealthChanged += ChangeHpBar;
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

    private void FollowObject()
    {
        if (_object != null)
        {
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(_object.transform.position) + _followOffset;
            _transform.position = screenPos;
        }
    }

}
