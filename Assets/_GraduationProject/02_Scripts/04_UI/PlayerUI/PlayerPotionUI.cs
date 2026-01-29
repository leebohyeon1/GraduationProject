using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 포션 UI
/// </summary>
public class PlayerPotionUI : MonoBehaviour, IEventListener<PlayerController>, IDisposable
{
    [Header("References")]
    [SerializeField] private List<GameObject> _potionImages;
    private PlayerController _playerController;                     // 플레이어

    [Header("Event")]
    [SerializeField] private OnPlayerSpawnedSO _onPlayerSpawned;    // 플레이어 스폰 이벤트


    private void OnEnable()
    {
        _onPlayerSpawned.Subscribe(this);
    }

    private void OnDisable()
    {
        _onPlayerSpawned.Unsubscribe(this);
    }


    public void OnEventTrigger(PlayerController player)
    {
        _playerController = player;

        _playerController.Potion.OnPotionChange += OnPotionChange;

        OnPotionChange(_playerController.Potion.CurrentPotion);

        player.RegisterDisposable(this);
    }

    public void Dispose()
    {
        _playerController.Potion.OnPotionChange -= OnPotionChange;
    }

    // 포션 변경 이벤트 처리
    private void OnPotionChange(int curentPotion)
    {
        for (int i = 0; i < _potionImages.Count; i++)
        {
            if (i < curentPotion)
            {
                _potionImages[i].SetActive(true);
            }
            else
            {
                _potionImages[i].SetActive(false);
            }
        }
    }

}
