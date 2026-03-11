using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class MenuSettingUI : MenuUIComponent
{
    [Header("Settings Pages")]
    [SerializeField] private GameObject[] _settingPages; // 0: Graphic, 1: Audio, 2: Exit
    [SerializeField] private InputReaderSO _inputReader;

    [Header("Page")]
    [SerializeField] private TMP_Text _pageTitle;
    [SerializeField] private RectTransform _activePagePoint;
    [SerializeField] private List<RectTransform> _pointList;

    private int _currentPageIndex = 0;

    public override void Initialize(MenuUI menu)
    {
        base.Initialize(menu);
        
        if (_inputReader != null)
        {
            _inputReader.SubNextEvent += OnSubNext;
        }

        UpdatePageUI();
    }

    public override void Dispose()
    {
        if (_inputReader != null)
        {
            _inputReader.SubPreviousEvent -= OnSubPrevious;
        }
    }

    private void OnSubNext()
    {
        if(gameObject.activeSelf)
        {
            ChangePage(1);
        }
    }
    private void OnSubPrevious()
    {
        if (gameObject.activeSelf)
        {
            ChangePage(-1);
        }
    }
    private void ChangePage(int direction)
    {
        _currentPageIndex += direction;

        // 페이지 인덱스 순환
        if (_currentPageIndex < 0)
        {
            _currentPageIndex = _settingPages.Length - 1;
        }
        else if (_currentPageIndex >= _settingPages.Length)
        {
            _currentPageIndex = 0;
        }

        UpdatePageUI();
    }

    private void UpdatePageUI()
    {
        if (_settingPages == null || _settingPages.Length == 0)
        {
            return;
        }

        for (int i = 0; i < _settingPages.Length; i++)
        {
            if (_settingPages[i] != null)
            {
                _settingPages[i].SetActive(i == _currentPageIndex);

                _activePagePoint.SetParent(_pointList[_currentPageIndex], false);
                _activePagePoint.anchoredPosition = Vector2.zero;
            }
        }
        
        Debug.Log($"Setting Page Changed: {((SettingPageType)_currentPageIndex)}");
    }

    private enum SettingPageType
    {
        Graphic = 0,
        Audio = 1,
        Exit = 2
    }
}
