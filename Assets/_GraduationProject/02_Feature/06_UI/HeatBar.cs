using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeatBar : MonoBehaviour
{
    [SerializeField] private Image _heatBarSlider;
    [SerializeField] private GameObject _object;
    private IHeatable _heatable;
    private Coroutine _heatBarCoroutine;
    
    private void Start()
    {
        _heatable = _object.GetComponent<IHeatable>();
        
        _heatBarSlider.fillAmount = _heatable.CurrentHeat / (float)_heatable.MaxHeat;
        _heatable.OnHeatChanged +=(currentHeat, maxHeat) => ChangeHeatBar(currentHeat, maxHeat);
    }

    private void OnDestroy()
    {
        _heatable.OnHeatChanged -= (currentHeat, maxHeat) => ChangeHeatBar(currentHeat, maxHeat);
    }

    private void ChangeHeatBar(int currentHeat, int maxHeat)
    {
        float healthPercent = (float)currentHeat / maxHeat;
        if (_heatBarCoroutine != null)
        {
            StopCoroutine(_heatBarCoroutine);
        }
        _heatBarCoroutine = StartCoroutine(CoChangeHeatBar(healthPercent));
    }

    private IEnumerator CoChangeHeatBar(float targetFillAmount)
    {
        float startFillAmount = _heatBarSlider.fillAmount;
        float elapsedTimer = 0.0f;
        float duration = 0.05f;

        while (startFillAmount != targetFillAmount)
        {
            elapsedTimer += Time.deltaTime;
            _heatBarSlider.fillAmount = Mathf.Lerp(_heatBarSlider.fillAmount, targetFillAmount, elapsedTimer / duration);
            yield return null;
        }

        _heatBarSlider.fillAmount = targetFillAmount;
    }
}
