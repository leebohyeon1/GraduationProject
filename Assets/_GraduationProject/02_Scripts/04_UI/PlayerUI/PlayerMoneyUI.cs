using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 돈 UI
/// </summary>
public class PlayerMoneyUI : MonoBehaviour, IEventListener<PlayerController>, IDisposable
{
    [Header("References")]
    [SerializeField] private Image _moneyBar;               // 돈 UI 이미지
    [SerializeField] private TMP_Text _moneyText;           // 돈 Text
    private PlayerController _playerController;             // 플레이어


    [SerializeField] private OnPlayerSpawnedSO _onPlayerSpawned; // 플레이어 스폰 이벤트

    private void OnEnable()
    {
        _onPlayerSpawned.Subscribe(this);
    }

    private void OnDisable()
    {
        _onPlayerSpawned.Unsubscribe(this);
    }


    public void OnEventTrigger(PlayerController player)
    {
        _playerController = player;

        _playerController.Money.MoneyChanged += OnMoneyChanged;
        SetMoneyText(_playerController.Money.CurrentMoney);

        player.RegisterDisposable(this);
    }

    public void Dispose()
    {
        _playerController.Money.MoneyChanged -= OnMoneyChanged;
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
