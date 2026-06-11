using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 포션 UI
/// </summary>
public class PlayerPotionUI : PlayerUIBase
{
    [Header("References")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private List<GameObject> _potionImages;
    
    [Header("Settings")]
    [SerializeField] private List<Sprite> _backgroundSprites;

    public override void Initialize(PlayerController player)
    {
        base.Initialize(player);

        p_player.Potion.OnPotionChange += OnPotionChange;
        p_player.Potion.OnMaxPotionChange += UpdateMaxPotionUI;

        UpdateMaxPotionUI(p_player.Potion.MaxPotion);
        OnPotionChange(p_player.Potion.CurrentPotion);
    }

    public override void Dispose()
    {
        p_player.Potion.OnPotionChange -= OnPotionChange;
        p_player.Potion.OnMaxPotionChange -= UpdateMaxPotionUI;
    }

    // 최대 포션 개수에 따른 UI 설정
    private void UpdateMaxPotionUI(int maxPotion)
    {
        // 배경 이미지 변경 (최대 포션 개수에 맞는 스프라이트 선택)
        // 인덱스는 maxPotion - 1 (1개일 때 0번, 2개일 때 1번...)
        int spriteIndex = maxPotion - 3;
        if (_backgroundImage != null && _backgroundSprites != null && spriteIndex >= 0 && spriteIndex < _backgroundSprites.Count)
        {
            _backgroundImage.sprite = _backgroundSprites[spriteIndex];
        }
    }

    // 포션 변경 이벤트 처리
    private void OnPotionChange(int curentPotion)
    {
        for (int i = 0; i < _potionImages.Count; i++)
        {
            if (i < curentPotion)
            {
                _potionImages[i].SetActive(true);
            }
            else
            {
                _potionImages[i].SetActive(false);
            }
        }
    }

}
