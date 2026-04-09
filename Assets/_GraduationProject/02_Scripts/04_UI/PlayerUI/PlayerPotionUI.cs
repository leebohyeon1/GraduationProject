using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 포션 UI
/// </summary>
public class PlayerPotionUI : PlayerUIBase
{
    [Header("References")]
    [SerializeField] private List<GameObject> _potionImages;

    public override void Initialize(PlayerController player)
    {
        base.Initialize(player);

        p_player.Potion.OnPotionChange += OnPotionChange;

        OnPotionChange(p_player.Potion.CurrentPotion);
    }

    public override void Dispose()
    {
        p_player.Potion.OnPotionChange -= OnPotionChange;
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
