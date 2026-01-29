using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillUI : MonoBehaviour, IEventListener<PlayerController>, IDisposable
{
    private PlayerController _playerController;

    [SerializeField] private List<PlayerSkillUpgradeButtonUI> _upgradeButtonList;

    [Header("Events")]
    [SerializeField] private OnPlayerSpawnedSO _playerSpawned;

    private void Awake()
    {
        _playerSpawned.Subscribe(this);

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _playerSpawned.Unsubscribe(this);
    }

    public void OnEventTrigger(PlayerController player)
    {
        _playerController = player;

        _playerController.InputReader.EscapeEvent += OnEscape;
        _playerController.RegisterDisposable(this);

        foreach (var button in _upgradeButtonList)
        {
            button.Initialize(player);
        }
    }

    public void Dispose()
    {
        _playerController.InputReader.EscapeEvent -= OnEscape;
    }

    private void OnEscape()
    {
        if(gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}
