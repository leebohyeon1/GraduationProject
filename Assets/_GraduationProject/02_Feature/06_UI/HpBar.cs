using System.Collections;
using System.Threading;
using BH_Lib.DI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : DIMonoBehaviour
{
    [SerializeField] private Image _hpBarSlider;
    [SerializeField] private GameObject _object;
    private IDamageable _damageable;
    
    private void Start()
    {
        _damageable = _object.GetComponent<IDamageable>();
        _damageable.OnHealthChanged += ChangeHpBar;
    }

    private void OnDestroy()
    {
        _damageable.OnHealthChanged -= ChangeHpBar;        
    }

    private void ChangeHpBar(int previousHp, int currentHp)
    {
        DOTween.Kill(_hpBarSlider, true);

        DOTween.To(() => _hpBarSlider.fillAmount,
                    x => _hpBarSlider.fillAmount = x,
                    currentHp/(float)_damageable.MaxHealth, 0.5f)
                    .SetEase(Ease.Linear)
                    .SetId(_hpBarSlider);
    }
}
