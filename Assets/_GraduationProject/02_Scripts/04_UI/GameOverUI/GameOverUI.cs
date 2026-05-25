using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class GameOverUI : MonoBehaviour, IEventListener<PlayerController>, IDisposable
{
    [Header("Input")]
    [SerializeField] private InputReaderSO _inputReader;

    [Header("UI")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _firstButton;

    [Header("Animation Settings")]
    [SerializeField] private float _clickScaleAmount = 0.95f;
    [SerializeField] private float _clickDuration = 0.1f;

    [Header("Events")]
    [SerializeField] private OnPlayerSpawnedSO _playerSpawned;
    private PlayerController _player;
    private bool _isActionProcessing = false;

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
        if (_isActionProcessing) return;
        
        PlayClickAnimation(EventSystem.current.currentSelectedGameObject, () => {
            GameData gameData = DataManager.Instance.GetGameData();
            DataManager.Instance.ResetPlayer(); // 플레이어 데이터 초기화
            
            // 죽은 몬스터 목록 초기화 (리스폰 시 모든 적 부활)
            gameData.ClearDeadMonsters();
            
            DataManager.Instance.SaveGame(); // 게임 저장
            SceneLoadingManager.Instance.TeleportToSceneByName(gameData.PlayerData.RespawnSceneName, SceneLoadingManager.SpawnMode.LastPosition);
        });
    }

    public void OnQuitButton()
    {
        if (_isActionProcessing) return;

        PlayClickAnimation(EventSystem.current.currentSelectedGameObject, () => 
        {
            DataManager.Instance.ResetPlayer(); // 플레이어 데이터 초기화
            DataManager.Instance.SaveGame(); // 게임 저장
            SceneLoadingManager.Instance.TeleportToSceneByName("Title");
        });
    }

    private void PlayClickAnimation(GameObject target, Action onComplete)
    {
        if (target == null)
        {
            onComplete?.Invoke();
            return;
        }

        _isActionProcessing = true;
        
        // 버튼을 살짝 눌렀다 떼는 느낌의 연출
        target.transform.DOScale(_clickScaleAmount, _clickDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true) // 타임스케일 영향 받지 않도록
            .OnComplete(() => {
                target.transform.DOScale(1f, _clickDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true)
                    .OnComplete(() => {
                        onComplete?.Invoke();
                        _isActionProcessing = false;
                    });
            });
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
        
        if (_inputReader != null)
        {
            _inputReader.SetInputMode(InputReaderSO.InputMode.UI);
        }

        if (_firstButton != null)
        {
            _firstButton.Select();
        }
    }

}
