using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 돈 UI
/// </summary>
public class PlayerMoneyUI : PlayerUIBase
{ 
    [Header("References")]
    [SerializeField] private Image _moneyBar;               // 돈 UI 이미지
    [SerializeField] private TMP_Text _moneyText;           // 돈 Text

    public override void Initialize(PlayerController player)
    {
        base.Initialize(player);
        p_player = player;

        p_player.Money.MoneyChanged += OnMoneyChanged;
        SetMoneyText(p_player.Money.CurrentMoney);
    }

    public override void Dispose()
    {
        p_player.Money.MoneyChanged -= OnMoneyChanged;
    }

    private void OnMoneyChanged(int  amount)
    {
        int start = int.Parse(_moneyText.text);
        DOTween.To(
            () => start, 
            x =>
            {
                SetMoneyText((int)x);
            }
            , amount, 2f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true); 
    }

    private void SetMoneyText(int amount)
    {
        _moneyText.text = amount.ToString();
    }
}
