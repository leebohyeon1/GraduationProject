using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillUI : PlayerUIBase
{
    [SerializeField] private List<PlayerSkillUpgradeButtonUI> _upgradeButtonList;

    public override void Initialize(PlayerController player)
    {
        base.Initialize(player);

        p_player.InputReader.EscapeEvent += OnEscape;

        foreach (var button in _upgradeButtonList)
        {
            button.Initialize(player);
        }
    }

    public override void Dispose()
    {
        p_player.InputReader.EscapeEvent -= OnEscape;
    }

    private void OnEscape()
    {
        if(gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}
