using System.Collections.Generic;
using TMPro;
using Unity.AppUI.UI;
using UnityEngine;

public class MenuUITopBar : MenuUIComponent 
{
    [Header("Name")]
    [SerializeField] private TMP_Text _menuNameText;
    
    [Header("Page")]
    [SerializeField] private RectTransform _activePagePoint;
    [SerializeField] private List<RectTransform> _pageList;

    [Header("Money")]
    [SerializeField] private TMP_Text _specialMoneyText;
    [SerializeField] private TMP_Text _moneyText;

    public override void Initialize(MenuUI menu)
    {
        base.Initialize(menu);

        menu.Player.Money.MoneyChanged += OnMoneyChanged;
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    private void OnMoneyChanged(int amount)
    {

    }
}
