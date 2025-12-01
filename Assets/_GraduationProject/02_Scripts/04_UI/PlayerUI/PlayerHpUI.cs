using BH_Lib.Log;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private Image _hpImage;

    private void OnEnable()
    {
        _playerHealth.OnHealthChanged += UpdateHpUI;
    }
    
    private void Start()
    {
        _hpImage.fillAmount = _playerHealth.Health;
    }

    private void OnDisable()
    {
        _playerHealth.OnHealthChanged -= UpdateHpUI;
    }   

    private void UpdateHpUI(int previousHp, int currentHp)
    {
        DOTween.Kill(this);

        DOTween.To(
            () => _hpImage.fillAmount,
            X =>
            {
                _hpImage.fillAmount = X;
            },
            currentHp / _playerHealth.MaxHealth,
            0.3f)
            .SetEase(Ease.Linear)
            .SetId(this);
    }
}
