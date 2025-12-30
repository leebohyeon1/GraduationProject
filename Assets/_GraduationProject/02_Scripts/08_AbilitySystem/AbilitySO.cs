using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilitySO", menuName = "Scriptable Objects/Player/AbilitySO")]
public class AbilitySO : ScriptableObject
{
    [Header("기본 정보")]
    public string AbilityName;
    [TextArea]
    public string Description;
    public Sprite Icon;

    public virtual void ApplyAbility(GameObject player)
    {
        // 능력 적용 로직을 여기에 작성합니다.
        // 예: 스탯 강화, 새로운 스킬 추가 등
    }
}

[Serializable]
public class PlusPlayerStat
{
    public int Health = 10; // 최대 체력
    public float Stamina = 10;
    public float StaminaRegenPerSecond = 5;

    [Header("Dodge")]
    public float DodgeStamina = 10;
    public float DodgeDistance; // 회피 거리
    public float DodgeDuration;

    [Header("Attack")]
    public int AttackDamage = 5; // 공격력 증가
    public float AttackStamina;
    public float AttackRadius; // 공격 범위 증가

    [Header("ChargeAttack")]
    public float ChargeMoveSpeed;
    public float ChargeStamina;
    public float MaxChargeTime = 5f;

    public int ChargeAttackDamage = 10; // 차지 공격력 증가    
    public float ChargeAttackStamina;
    public float ChargeAttackRadius; // 차지 공격 범위 증가


    [Header("Parry")]
    public float ParryStamina;
}
