using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStiffnessUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private Image _stiffnessImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerHealth.OnStiffnessChanged += OnStiffneesChanged;
        _stiffnessImage.fillAmount = (float)_playerHealth.CurrentStiffness / _playerHealth.StiffnessThreshold;
    }

    private void OnDestroy()
    {
        _playerHealth.OnStiffnessChanged -= OnStiffneesChanged;
    }
    
    void Update()
    {
        
    }

    private void OnStiffneesChanged(int previousStiffness, int currentStiffness)
    {
        DOTween.Kill(this);

        DOTween.To(
            () => _stiffnessImage.fillAmount,
            X =>
            {
                _stiffnessImage.fillAmount = X;
            },
            (float)currentStiffness / _playerHealth.MaxHealth,
            0.3f)
            .SetEase(Ease.Linear)
            .SetId(this);
    }
}
