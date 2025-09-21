using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HeatBar : MonoBehaviour
{
    [SerializeField] private Image _heatBarSlider;
    [SerializeField] private GameObject _object;
    private IHeatable _heatable;
    
    private void Start()
    {
        _heatable = _object.GetComponent<IHeatable>();
        
        _heatBarSlider.fillAmount = _heatable.CurrentHeat / (float)_heatable.MaxHeat;
        _heatable.OnHeatChanged += ChangeHeatBar;
    }

    private void OnDestroy()
    {
        _heatable.OnHeatChanged -= ChangeHeatBar;
    }

    private void ChangeHeatBar(int previousHeat, int currentHeat)
    {
        DOTween.Kill(_heatBarSlider, true);

        float healthPercent = currentHeat / (float)_heatable.MaxHeat;
        DOTween.To(() => _heatBarSlider.fillAmount,
                    x => _heatBarSlider.fillAmount = x,
                    healthPercent, 0.1f)
                    .SetEase(Ease.Linear)
                    .SetId(_heatBarSlider);
    }
}
