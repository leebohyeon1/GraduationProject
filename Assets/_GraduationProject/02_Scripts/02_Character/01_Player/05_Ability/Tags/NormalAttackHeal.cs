using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "NormalAttackHeal", menuName = "Project/Player/Ability/Tag/NormalAttackHeal")]
public class NormalAttackHeal : PlayerAbilityTagSO
{
    [SerializeField] private int _healAmount = 5;
    private PlayerController _controller;

    public override void Apply(PlayerController player)
    {
        _controller = player;
        _controller.Combat.AttackEvent += OnAttackEvent;
    }

    public override void Revert(PlayerController player)
    {
        _controller.Combat.AttackEvent -= OnAttackEvent;
        _controller = null;
    }


    private void OnAttackEvent(IDamageable damageable, DamageData data)
    {
        if(data.AttackType == AttackType.Normal)
        {
            _controller.Health.Heal(_healAmount);
        }
    }
}
