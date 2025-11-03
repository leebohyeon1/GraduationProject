using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HeatBar : MonoBehaviour
{
    [SerializeField] protected GameObject p_object;
    [SerializeField] protected Image p_heatBarSlider;
    protected IHeatable p_heatable;
    
    protected virtual void Start()
    {
        p_heatable = p_object.GetComponent<IHeatable>();
        
        p_heatBarSlider.fillAmount = p_heatable.CurrentHeat / (float)p_heatable.MaxHeat;
        p_heatable.OnHeatChanged += ChangeHeatBar;
    }

    protected virtual void OnDestroy()
    {
        if(p_heatBarSlider != null )
        {
            p_heatable.OnHeatChanged -= ChangeHeatBar;
        }
    }

    protected virtual void ChangeHeatBar(int previousHeat, int currentHeat)
    {
        DOTween.Kill(p_heatBarSlider, true);

        float heatPercent =  currentHeat / (float)p_heatable.MaxHeat;
        DOTween.To(() => p_heatBarSlider.fillAmount,
                    x => p_heatBarSlider.fillAmount = x,
                    heatPercent, 0.3f)
                    .SetEase(Ease.Linear)
                    .SetId(p_heatBarSlider);
    }
}
