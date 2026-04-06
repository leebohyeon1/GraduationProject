using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuUITopBar : MenuUIComponent 
{
    private MenuUI _menu;

    [Header("Name")]
    [SerializeField] private TMP_Text _menuNameText;
    [SerializeField] private TMP_Text _nextMenuNameText;
    [SerializeField] private TMP_Text _previousMenuNameText;
    
    [Header("Page")]
    [SerializeField] private RectTransform _activePagePoint;
    [SerializeField] private List<RectTransform> _pageList;

    [Header("Money")]
    [SerializeField] private TMP_Text _specialMoneyText;
    [SerializeField] private TMP_Text _moneyText;

    public override void Initialize(MenuUI menu)
    {
        base.Initialize(menu);
        _menu = menu;

        _menu.UIComponentUpdated += OnUIComponentUpdated;
    }

    private void OnEnable()
    {
        _specialMoneyText.text = _menu.Player.Money.CurrentSpecialMoney.ToString();
        _moneyText.text = _menu.Player.Money.CurrentMoney.ToString();
    }

    public override void Dispose()
    {
        base.Dispose();

        _menu.UIComponentUpdated -= OnUIComponentUpdated;
    }

    public void OnUIComponentUpdated(int currentIndex)
    {
        var components = _menu.MainUIComponents;
        int count = components.Count;

        // 안전 장치: 컴포넌트가 없으면 에러 방지
        if (count == 0) return;

        // 1. 나머지 연산자(%)를 활용한 인덱스 순환 계산
        int nextIndex = (currentIndex + 1) % count;
        int prevIndex = (currentIndex - 1 + count) % count;

        // 2. UI 텍스트 업데이트
        _menuNameText.text = components[currentIndex].ComponentName;
        _nextMenuNameText.text = components[nextIndex].ComponentName;
        _previousMenuNameText.text = components[prevIndex].ComponentName; // 기존 버그 수정됨

        // 3. UI Transform 최적화 (worldPositionStays를 false로 설정)
        _activePagePoint.SetParent(_pageList[currentIndex], false);
        _activePagePoint.anchoredPosition = Vector2.zero;
    }
}
