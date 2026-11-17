using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour, IEventListener<PlayerController>
{
    private PlayerController _playerController;
    private Canvas _hudCanvas;
    private bool _hiddenForCutscene;
    private bool _canvasWasEnabled;
    public UnityEngine.Playables.PlayableDirector[] hideDuringTimelines;
    private void LateUpdate()
    {
        if (!_hudCanvas) _hudCanvas=GetComponentInParent<Canvas>();
        if (!_hudCanvas) return;
        bool hide=InputReaderSO.ActiveInputMode==InputReaderSO.InputMode.CutScene;
        if (hideDuringTimelines != null)
            foreach (var timeline in hideDuringTimelines)
                hide |= timeline && timeline.state == UnityEngine.Playables.PlayState.Playing;
        if (KeepPlayerHUDDuringTimeline.IsActive) hide=false;
        if (hide && !_hiddenForCutscene) { _canvasWasEnabled=_hudCanvas.enabled; _hudCanvas.enabled=false; }
        else if (!hide && _hiddenForCutscene) _hudCanvas.enabled=_canvasWasEnabled;
        if (hide) _hudCanvas.enabled=false;
        _hiddenForCutscene=hide;
    }

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
