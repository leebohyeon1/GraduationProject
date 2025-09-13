using System.Collections;
using System.Threading;
using BH_Lib.DI;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : DIMonoBehaviour
{
    [SerializeField] private Image _hpBarSlider;
    [SerializeField] private GameObject _object;
    private IDamageable _damageable;
    private Coroutine _hpBarCoroutine;
    
    private void Start()
    {
        _damageable = _object.GetComponent<IDamageable>();
        _damageable.OnHealthChanged += ChangeHpBar;
    }

    private void OnDestroy()
    {
        _damageable.OnHealthChanged -= ChangeHpBar;        
    }

    private void ChangeHpBar(HealthChangeEventData eventData)
    {
        if (_hpBarCoroutine != null)
        {
            StopCoroutine(_hpBarCoroutine);
        }
        _hpBarCoroutine = StartCoroutine(CoChangeHpBar(eventData.HealthPercent));
    }

    private IEnumerator CoChangeHpBar(float targetFillAmount)
    {
        float startFillAmount = _hpBarSlider.fillAmount;
        float elapsedTimer = 0.0f;
        float duration = 0.5f;

        while (startFillAmount != targetFillAmount)
        {
            elapsedTimer += Time.deltaTime;
            _hpBarSlider.fillAmount = Mathf.Lerp(_hpBarSlider.fillAmount, targetFillAmount, elapsedTimer / duration);
            yield return null;
        }

        _hpBarSlider.fillAmount = targetFillAmount;
    }
}
