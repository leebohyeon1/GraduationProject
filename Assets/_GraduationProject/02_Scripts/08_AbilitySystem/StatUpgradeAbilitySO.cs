using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StatUpgradeAbilitySO", menuName = "Scriptable Objects/Player/StatUpgradeAbilitySO")]
public class StatUpgradeAbilitySO : AbilitySO
{
    [SerializeField] private PlusPlayerStat _statIncreases;

    public override void ApplyAbility(GameObject player)
    {
        PlayerStats stats = player.GetComponent<Player>().Stats;

        stats.RuntimeData.MaxHealth += _statIncreases.Health;
        stats.RuntimeData.MaxStamina += _statIncreases.Stamina;
        stats.RuntimeData.StaminaRegenPerSecond += _statIncreases.StaminaRegenPerSecond;

        stats.RuntimeData.CombatData.DodgeStamina += _statIncreases.DodgeStamina;
        stats.RuntimeData.CombatData.DodgeDistance += _statIncreases.DodgeDistance;
        stats.RuntimeData.CombatData.DodgeDuration += _statIncreases.DodgeDuration;

        for(int i = 0; i < stats.RuntimeData.CombatData.AttackDatas.Count; i++)
        {
            stats.RuntimeData.CombatData.AttackDatas[i].AttackConfig.AttackDamage += _statIncreases.AttackDamage;
            stats.RuntimeData.CombatData.AttackDatas[i].AttackConfig.AttackStamina += _statIncreases.AttackStamina;
            stats.RuntimeData.CombatData.AttackDatas[i].AttackConfig.AttackRadius *= _statIncreases.AttackRadius;

            for(int j = 0; j < stats.RuntimeData.CombatData.AttackDatas[i].ChargeConfigs.Count; j++)
            {
                stats.RuntimeData.CombatData.AttackDatas[i].ChargeConfigs[j].AttackConfig.AttackDamage += _statIncreases.ChargeAttackDamage;
                stats.RuntimeData.CombatData.AttackDatas[i].ChargeConfigs[j].AttackConfig.AttackStamina += _statIncreases.ChargeAttackStamina;
                stats.RuntimeData.CombatData.AttackDatas[i].ChargeConfigs[j].AttackConfig.AttackRadius *= _statIncreases.ChargeAttackRadius;
            }
        }

        stats.RuntimeData.CombatData.ChargeMoveSpeed += _statIncreases.ChargeMoveSpeed;
        stats.RuntimeData.CombatData.ChargeStamina += _statIncreases.ChargeStamina;
        stats.RuntimeData.CombatData.MaxChargeTime += _statIncreases.MaxChargeTime;
    }
}



//[Serializable]
//public class PlusPlayerStat
//{
//    public int Health = 10; // 최대 체력
//    public float Stamina = 10;
//    public float StaminaRegenPerSecond = 5;

//    [Header("Dodge")]
//    public float DodgeStamina = 10;
//    public float DodgeDistance; // 회피 거리
//    public float DodgeDuration;

//    [Header("ChargeAttack")]
//    public float ChargeMoveSpeed;
//    public float ChargeStamina;
//    public float MaxChargeTime = 5f;

//    [Header("Parry")]
//    public float ParryStamina;
//}
