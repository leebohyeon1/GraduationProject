using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private Image _hpImage;

    private void Start()
    {
        _playerHealth.OnHealthChanged += UpdateHpUI;
        _hpImage.fillAmount = (float)_playerHealth.Health / _playerHealth.MaxHealth;
    }

    private void OnDestroy()
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
            (float) currentHp / _playerHealth.MaxHealth,
            0.3f)
            .SetEase(Ease.Linear)
            .SetId(this);
    }
}
