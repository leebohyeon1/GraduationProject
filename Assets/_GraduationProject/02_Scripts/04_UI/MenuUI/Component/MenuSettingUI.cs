using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public enum SettingPageType
{
    Display = 0,
    Audio = 1,
    Exit = 2
}

public class MenuSettingUI : MenuUIComponent
{
    [Header("Settings Pages")]
    [SerializeField] private GameObject[] _settingPages; // 베이스 클래스 배열로 변경
    [SerializeField] private List<SettingPageUI> _pageComponents; // 베이스 클래스 배열로 변경
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
            _inputReader.SubPreviousEvent += OnSubPrevious;
        }

        foreach (var page in _settingPages)
        {
            _pageComponents.Add(page.GetComponent<SettingPageUI>());
        }
        UpdatePageUI();
    }

    public override void Dispose()
    {
        if (_inputReader != null)
        {
            _inputReader.SubNextEvent -= OnSubNext;
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
        if (_pageComponents == null || _pageComponents.Count == 0)
        {
            return;
        }

        // 모든 페이지를 닫고 현재 페이지만 엶
        for (int i = 0; i < _pageComponents.Count; i++)
        {
            if (_pageComponents[i] != null)
            {
                if (i == _currentPageIndex)
                {
                    _pageComponents[i].OnPageOpen();
                    
                    // 현재 페이지의 타이틀 정보로 UI 업데이트
                    if (_pageTitle != null)
                    {
                        _pageTitle.text = _pageComponents[i].PageTitle;
                    }
                }
                else
                {
                    _pageComponents[i].OnPageClose();
                }
            }
        }

        // 페이지 포인트(인디케이터) 업데이트
        if (_activePagePoint != null && _pointList != null && _currentPageIndex < _pointList.Count)
        {
            _activePagePoint.SetParent(_pointList[_currentPageIndex], false);
            _activePagePoint.anchoredPosition = Vector2.zero;
        }
    }

    public void OnQuitToTitle()
    {
        DataManager.Instance.SaveGame();
        SceneLoadingManager.Instance.TeleportToSceneByName("Title");
    }

    public void OnQuitToDesktop()
    {
        DataManager.Instance.SaveGame();
        Application.Quit();
    }
}
