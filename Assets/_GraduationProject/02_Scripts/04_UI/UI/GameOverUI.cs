using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour, IEventListener<PlayerController>, IDisposable
{
    [Header("UI")]
    [SerializeField] private GameObject _panel;

    [Header("Events")]
    [SerializeField] private OnPlayerSpawnedSO _playerSpawned;
    private PlayerController _player;

    private void Awake()
    {
        _playerSpawned.Subscribe(this);
    }


    public void Dispose()
    {
        _player.Health.OnDied -= OnDied;

        _playerSpawned.Unsubscribe(this);
    }

    public void OnContinueButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);   
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }

    public void OnEventTrigger(PlayerController player)
    {
        _player = player;
     
        _player.Health.OnDied += OnDied;
        _player.RegisterDisposable(this);
    }

    private void OnDied()
    {
        _panel.SetActive(true);
    }

}
