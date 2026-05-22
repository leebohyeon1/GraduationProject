using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TitleSettingUI : TitleView
{
    [Header("References")]
    [SerializeField] private InputReaderSO _inputReader;
    [SerializeField] private List<SettingPageUI> _settingComponentList;

    [SerializeField] private RectTransform _activePagePoint;
    [SerializeField] private List<RectTransform> _pointList;

    [Header("UI")]
    [SerializeField] private TMP_Text _currentSettingTitle;
    [SerializeField] private TMP_Text _nextSettingTitle;
    [SerializeField] private TMP_Text _previousSettingTitle;

    [Header("Settings")]
    private int _currentIndex = 0;

    [Header("Events")]
    public UnityEvent OnCancelEvent;

    private void OnEnable()
    {
        _currentIndex = 0;
        UpdateUI();

        _inputReader.NextEvent += OnNext;
        _inputReader.PreviousEvent += OnPrevious;
        _inputReader.CancelEvent += OnCancel;
    }

    private void OnDisable()
    {
        _inputReader.NextEvent -= OnNext;
        _inputReader.PreviousEvent -= OnPrevious;
        _inputReader.CancelEvent -= OnCancel;
    }

    private void UpdateUI()
    {
        if (_settingComponentList.Count == 0)
        {
            return;
        }

        for (int i = 0; i < _settingComponentList.Count; i++)
        {
            _settingComponentList[i].gameObject.SetActive(i == _currentIndex);
        }

        int prevIndex = (_currentIndex - 1 + _settingComponentList.Count) % _settingComponentList.Count;
        int nextIndex = (_currentIndex + 1) % _settingComponentList.Count;

        _currentSettingTitle.text = _settingComponentList[_currentIndex].PageTitle;
        _nextSettingTitle.text = _settingComponentList[nextIndex].SettingName;
        _previousSettingTitle.text = _settingComponentList[prevIndex].SettingName;

        // 페이지 포인트(인디케이터) 업데이트
        if (_activePagePoint != null && _pointList != null && _currentIndex < _pointList.Count)
        {
            _activePagePoint.SetParent(_pointList[_currentIndex], false);
            _activePagePoint.anchoredPosition = Vector2.zero;
        }
    }

    private void OnNext()
    {
        if (_settingComponentList.Count == 0)
        {
            return;
        }

        _currentIndex = (_currentIndex + 1) % _settingComponentList.Count;
        UpdateUI();
    }

    private void OnPrevious()
    {
        if (_settingComponentList.Count == 0)
        {
            return;
        }

        _currentIndex = (_currentIndex - 1 + _settingComponentList.Count) % _settingComponentList.Count;
        UpdateUI();
    }

    private void OnCancel()
    {
        OnCancelEvent?.Invoke();
    }
}
