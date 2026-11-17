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
    private Sequence _reveal;
    private CanvasGroup Group(GameObject target)
    {
        var group=target.GetComponent<CanvasGroup>();
        if (!group) group=target.AddComponent<CanvasGroup>();
        return group;
    }
    private void OnDestroy() { _reveal?.Kill(); }

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
            SceneLoadingManager.Instance.TeleportToScene(SceneLoadingManager.Instance.GetSceneDataByName(gameData.PlayerData.RespawnSceneName), gameData.PlayerData.RespawnPosition);
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
        var settings=Resources.Load<GameUIIntegrationSettings>("GameUIIntegrationSettings");
        if(settings && settings.gameOver && MoreMountains.Tools.MMSoundManager.HasInstance)
            MoreMountains.Tools.MMSoundManager.Instance.PlaySound(settings.gameOver,MoreMountains.Tools.MMSoundManager.MMSoundManagerTracks.UI,Vector3.zero,volume:.6f);
        _reveal?.Kill();
        _panel.SetActive(true);
        _isActionProcessing=true;
        var panelGroup=Group(_panel);panelGroup.alpha=0;panelGroup.interactable=false;panelGroup.blocksRaycasts=true;
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
        var title=_panel.transform.Find("Tiphography");
        var description=_panel.transform.Find("DescriptionText");
        CanvasGroup titleGroup=title?Group(title.gameObject):null;
        CanvasGroup descriptionGroup=description?Group(description.gameObject):null;
        if(titleGroup)titleGroup.alpha=0;
        if(descriptionGroup)descriptionGroup.alpha=0;
        var buttons=_panel.GetComponentsInChildren<Button>(true);
        foreach(var button in buttons) Group(button.gameObject).alpha=0;
        
        if (_inputReader != null)
        {
            _inputReader.SetInputMode(InputReaderSO.InputMode.UI);
        }

        _reveal=DOTween.Sequence().SetUpdate(true);
        _reveal.Append(panelGroup.DOFade(1,.8f));
        if(titleGroup)_reveal.Insert(.8f,titleGroup.DOFade(1,.6f));
        if(descriptionGroup)_reveal.Insert(.8f,descriptionGroup.DOFade(1,.6f));
        foreach(var button in buttons)_reveal.Insert(1.4f,Group(button.gameObject).DOFade(1,.6f));
        _reveal.OnComplete(()=>{panelGroup.interactable=true;_isActionProcessing=false;if(_firstButton)_firstButton.Select();});
    }

}
