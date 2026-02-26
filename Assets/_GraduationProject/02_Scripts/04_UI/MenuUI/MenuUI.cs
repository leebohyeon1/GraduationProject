using System.Collections.Generic;
using UnityEngine;

public class MenuUI : MonoBehaviour, IEventListener<PlayerController>
{
    private PlayerController _playerController;
    public PlayerController Player => _playerController;

    [Header("SO")]
    [SerializeField] private OnPlayerSpawnedSO _playerSpawnedSO;
    [SerializeField] private InputReaderSO _inputReaderSO;

    [Header("UI Components")]
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private List<MenuUIComponent> _menuUIComponents;

    private int _currentComponentIndex = -1; // 현재 활성화된 UI 컴포넌트 인덱스

    public void Awake()
    {
        _playerSpawnedSO.Subscribe(this);
    }

    public void Start()
    {
        foreach (var ui in _menuUIComponents)
        {
            ui.Initialize(this);
        }

        _inputReaderSO.EscapeEvent += OnEscape;
    }

    public void OnDestroy()
    {
        foreach (var ui in _menuUIComponents)
        {
            ui.Dispose();
        }

        _inputReaderSO.EscapeEvent -= OnEscape;
        _playerSpawnedSO.Unsubscribe(this);
    }

    public void TogglePanel()
    {
        if (_menuPanel.activeSelf)
        {
            _menuPanel.SetActive(false);
        }
        else
        {
            UpdateComponenet();

            _menuPanel.SetActive(true);
        }
    }

    public void UpdateComponenet()
    {
        if (_currentComponentIndex == -1)
        {
            _currentComponentIndex = 0;
        }

        for (int i =0; i < _menuUIComponents.Count; i++)
        {
            if (i == _currentComponentIndex)
            {
                _menuUIComponents[i].gameObject.SetActive(true);
            }
            else
            {
                _menuUIComponents[i].gameObject.SetActive(false);
            }
        }
    }

    //==========================================================================================================================
    // Event Handler ===========================================================================================================
    //==========================================================================================================================

    public void OnEventTrigger(PlayerController player)
    {
        _playerController = player;
    }

    private void OnEscape()
    {
        TogglePanel();
    }
}
