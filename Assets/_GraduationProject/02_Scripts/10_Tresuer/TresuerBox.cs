using System;
using UnityEngine;

public class TresuerBox : MonoBehaviour, IInteractable
{
    private PlayerController _playerController;
    [SerializeField] private TresuerBoxRewardSO _rewardSO;
    
    private bool _isInteracted = false;

    public void Interact()
    {
        if (_isInteracted)
        {
            return;
        }

        _isInteracted = true;

        _playerController.Money.GiveMoney(_rewardSO.MoneyAmount);
        _playerController.Money.GiveSpecialMoney(_rewardSO.SpecialMoneyAmount);
        _playerController.Potion.IncreaseMaxPotion(_rewardSO.MaxPotionAmount);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isInteracted)
        {
            return;
        }
        if (other.TryGetComponent<PlayerController>(out _playerController))
        {
            if (_playerController.Interact != null)
            {
                _playerController.Interact.SetInteractable(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController controller) && controller == _playerController)
        {
            if (_playerController.Interact != null && _playerController.Interact.Interactable.Equals(this))
            {
                _playerController.Interact.SetInteractable(null);
            }
        }
    }
}
