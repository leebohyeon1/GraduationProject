using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHeatBar : HeatBar
{
    [SerializeField] private Image _chargeGuageSlider;
    private PlayerHeat _playerHeat;

    protected override void Start()
    {
        base.Start();

        _playerHeat = p_object.GetComponent<PlayerHeat>();

        _chargeGuageSlider.fillAmount = 0f;
        _playerHeat.OnChargeGuageChanged += ChangeChargeBar;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_chargeGuageSlider != null)
        {
            _playerHeat.OnChargeGuageChanged -= ChangeChargeBar;
        }
    }

    private void ChangeChargeBar(float currentChargeGuage)
    {
        DOTween.Kill(_chargeGuageSlider, true);

        float heatPercent = currentChargeGuage / (float)p_heatable.MaxHeat;
        DOTween.To(() => _chargeGuageSlider.fillAmount,
                    x => _chargeGuageSlider.fillAmount = x,
                    heatPercent, 0.3f)
                    .SetEase(Ease.Linear)
                    .SetId(_chargeGuageSlider);
    }
}
