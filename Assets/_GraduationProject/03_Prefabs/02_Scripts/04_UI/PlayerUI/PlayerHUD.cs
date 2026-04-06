using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour, IEventListener<PlayerController>
{
    private PlayerController _playerController;

    [SerializeField] private OnPlayerSpawnedSO _playerSpawnedSO;
    [SerializeField] private List<PlayerUIBase> _playerUIList;

    public void Awake()
    {
        _playerSpawnedSO.Subscribe(this);
    }

    public void OnDestroy()
    {
        _playerSpawnedSO.Unsubscribe(this);
    }

    public void OnEventTrigger(PlayerController player)
    {
        _playerController = player;

        foreach(var ui in _playerUIList)
        {
            ui.Initialize(player);
        }
    }

}
