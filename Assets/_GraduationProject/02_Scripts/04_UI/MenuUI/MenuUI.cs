using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class MenuUI : MonoBehaviour, IEventListener<PlayerController>
{
    private PlayerController _playerController;
    public PlayerController Player => _playerController;

    [Header("SO")]
    [SerializeField] private OnPlayerSpawnedSO _playerSpawnedSO;
    [SerializeField] private InputReaderSO _inputReaderSO;

    [Header("UI Components")]
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private MenuUITopBar _menuUITopBar;
    [SerializeField] private List<MenuUIComponent> _mainUIComponents;
    public List<MenuUIComponent> MainUIComponents => _mainUIComponents;

    private int _currentComponentIndex = -1; // 현재 활성화된 UI 컴포넌트 인덱스

    public event Action<int> UIComponentUpdated;

    public void Awake()
    {
        _playerSpawnedSO.Subscribe(this);
    }

    public void Start()
    {
        _menuUITopBar.Initialize(this);

        foreach (var ui in _mainUIComponents)
        {
            ui.Initialize(this);
        }

        _inputReaderSO.EscapeEvent += OnEscape;
        _inputReaderSO.CancelEvent += OnEscape; // Cancel 입력 시에도 메뉴 닫기
        _inputReaderSO.NextEvent += OnNext;
        _inputReaderSO.PreviousEvent += OnPrevious;
    }

    public void OnDestroy()
    {
        _menuUITopBar.Dispose();

        foreach (var ui in _mainUIComponents)
        {
            ui.Dispose();
        }

        _inputReaderSO.EscapeEvent -= OnEscape;
        _inputReaderSO.CancelEvent -= OnEscape;
        _inputReaderSO.NextEvent -= OnNext;
        _inputReaderSO.PreviousEvent -= OnPrevious;
        _playerSpawnedSO.Unsubscribe(this);
    }

    public void TogglePanel()
    {
        if (_menuPanel.activeSelf)
        {
            _menuPanel.SetActive(false);
            _inputReaderSO.SetInputMode(InputReaderSO.InputMode.Gameplay);
        }
        else
        {
           UpdateComponenet();

            _menuPanel.SetActive(true);
            _inputReaderSO.SetInputMode(InputReaderSO.InputMode.UI);
        }
    }

    public void NextComponent()
    {
        _currentComponentIndex = (_currentComponentIndex + 1) % _mainUIComponents.Count;

        UpdateComponenet();
    }

    public void PreviousComponent()
    {
        _currentComponentIndex = (_currentComponentIndex - 1 + _mainUIComponents.Count) % _mainUIComponents.Count;

        UpdateComponenet();
    }

    public void UpdateComponenet()
    {
        if (_currentComponentIndex == -1)
        {
            _currentComponentIndex = 0;
        }

        for (int i = 0; i < _mainUIComponents.Count; i++)
        {
            if (i == _currentComponentIndex)
            {
                _mainUIComponents[i].gameObject.SetActive(true);
                _mainUIComponents[i].OnOpen(); // 페이지가 열릴 때 초기화 로직(포커싱 등) 호출
            }
            else
            {
                _mainUIComponents[i].gameObject.SetActive(false);
            }
        }


        UIComponentUpdated?.Invoke(_currentComponentIndex);
    }


    public void OpenSkillPanel(int skillIndex = -1)
    {
        _currentComponentIndex = 2; // 스킬 패널 인덱스
        UpdateComponenet();

        _menuPanel.SetActive(true);
        _inputReaderSO.SetInputMode(InputReaderSO.InputMode.UI);

        // 특정 스킬 인덱스가 제공되지 않았다면 기본값으로 0번 선택
        int targetIndex = (skillIndex == -1) ? 0 : skillIndex;

        if (_mainUIComponents.Count > 2)
        {
            if (_mainUIComponents[2] is SkillUI skillUI)
            {
                skillUI.SelectSkillByIndex(targetIndex);
            }
        }
    }

    //==========================================================================================================================
    // Event Handler ===========================================================================================================
    //==========================================================================================================================

    public void OnEventTrigger(PlayerController player)
    {
        _playerController = player;
        _menuUITopBar.BindPlayer(player);
    }

    private void OnEscape()
    {
        TogglePanel();
    }

    private void OnNext()
    {
        NextComponent();
    }

    private void OnPrevious()
    {
        PreviousComponent();
    }
}
