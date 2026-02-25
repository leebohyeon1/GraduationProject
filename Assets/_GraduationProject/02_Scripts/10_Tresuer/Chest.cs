using System;
using Unity.Collections;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    private PlayerController _playerController;
    [SerializeField] private ChestRewardSO _rewardSO;
    
    private bool _isInteracted = false;

    [ReadOnly]
    [SerializeField] private string _chestID;
    public string ChestID => _chestID;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_chestID))
        {
            _chestID = Guid.NewGuid().ToString();
            Debug.Log($"{gameObject.name}에 새로운 고유 ID가 부여되었습니다!");
        }
    }

    private void Start()
    {
        if (DataManager.Instance != null)
        {
            if(DataManager.Instance.GetGameData().IsChestOpened(_chestID))
            {
                _isInteracted = true;
            }
        }
    }

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

        // GUID를 통해서 상자 구분
        if(DataManager.Instance != null)
        {
            DataManager.Instance.GetGameData().AddOpendChest(_chestID);
        }
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
