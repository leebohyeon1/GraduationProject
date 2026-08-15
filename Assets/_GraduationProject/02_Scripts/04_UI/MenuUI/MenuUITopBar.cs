using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class MenuUITopBar : MenuUIComponent 
{
    private MenuUI _menu;
    private PlayerMoney _playerMoney;

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

    [Header("Money Animation")]
    [SerializeField] private float _moneyCountDuration = 0.5f;
    [SerializeField] private float _moneyPunchDuration = 0.3f;
    [SerializeField] private Vector3 _moneyPunchScale = new Vector3(0.15f, 0.15f, 0.15f);

    private int _displayedMoney;
    private Vector3 _moneyTextOriginScale;
    private bool _isInitialized;

    public override void Initialize(MenuUI menu)
    {
        base.Initialize(menu);
        _menu = menu;
        _moneyTextOriginScale = _moneyText.rectTransform.localScale;
        _isInitialized = true;

        _menu.UIComponentUpdated += OnUIComponentUpdated;

        if (_menu.Player != null)
        {
            BindPlayer(_menu.Player);
        }
    }

    public void BindPlayer(PlayerController player)
    {
        DOTween.Kill(this);

        if (_isInitialized)
        {
            _moneyText.rectTransform.localScale = _moneyTextOriginScale;
        }

        if (_playerMoney != null)
        {
            _playerMoney.MoneyChanged -= AnimateMoneyText;
        }

        _playerMoney = player != null ? player.Money : null;

        if (_playerMoney == null)
        {
            return;
        }

        _playerMoney.MoneyChanged += AnimateMoneyText;
        SetMoneyText(_playerMoney.CurrentMoney);
    }

    public override void Dispose()
    {
        base.Dispose();

        _menu.UIComponentUpdated -= OnUIComponentUpdated;

        if (_playerMoney != null)
        {
            _playerMoney.MoneyChanged -= AnimateMoneyText;
            _playerMoney = null;
        }

        DOTween.Kill(this);
        _moneyText.rectTransform.localScale = _moneyTextOriginScale;
    }

    private void AnimateMoneyText(int money)
    {
        DOTween.Kill(this);
        _moneyText.rectTransform.localScale = _moneyTextOriginScale;

        int startMoney = _displayedMoney;
        DOTween.To(
                () => startMoney,
                value => SetMoneyText(value),
                money,
                _moneyCountDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .SetId(this);

        _moneyText.rectTransform
            .DOPunchScale(_moneyPunchScale, _moneyPunchDuration)
            .SetUpdate(true)
            .SetId(this);
    }

    private void SetMoneyText(int money)
    {
        _displayedMoney = money;
        _moneyText.text = money.ToString();
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
