using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 런타임 공격 설정 래퍼 클래스입니다.
/// 원본 SO 데이터를 참조하며, 데미지 등 수정이 필요한 수치만 Stat으로 관리합니다.
/// </summary>
public class RuntimeAttackConfig
{
    private PlayerAttackConfig _source;
    public PlayerAttackConfig RawData => _source;

    public Stat Damage; // 버프 적용이 가능한 데미지 스탯
    public Stat Stamina;

    public RuntimeAttackConfig(PlayerAttackConfig source)
    {
        _source = source;
        // 원본 SO의 공격력을 실시간으로 바라보는 Stat 생성
        Damage = new Stat(() => _source.AttackDamage);
        Stamina = new Stat(() => _source.AttackStamina);
    }

    // 변하지 않는 데이터는 원본에서 직접 참조 (프록시 패턴)
    public AttackType AttackType => _source.AttackType;
    public Vector3 AttackRadius => _source.AttackRadius;
    public StepData AttackMoveConfig => _source.AttackMoveConfig;
    public StepData KnockbackConfig => _source.KnockbackCofig;
}

/// <summary>
/// 런타임 공격 설정 래퍼 클래스입니다.
/// 원본 SO 데이터를 참조하며, 데미지 등 수정이 필요한 수치만 Stat으로 관리합니다.
/// </summary>
public class RuntimeChargeAttackConfig
{
    private PlayerChargeConfig _source;
    public PlayerChargeConfig RawData => _source;

    public Stat Damage; // 버프 적용이 가능한 데미지 스탯
    public Stat Stamina;

    public RuntimeChargeAttackConfig(PlayerChargeConfig source)
    {
        _source = source;
        // 원본 SO의 공격력을 실시간으로 바라보는 Stat 생성
        Damage = new Stat(() => _source.AttackConfig.AttackDamage);
        Stamina = new Stat(() => _source.AttackConfig.AttackStamina);
    }

    // 변하지 않는 데이터는 원본에서 직접 참조 (프록시 패턴)
    public float ChargeTime => _source.ChargeTime;
    public AttackType AttackType => _source.AttackConfig.AttackType;
    public Vector3 AttackRadius => _source.AttackConfig.AttackRadius;
    public StepData AttackMoveConfig => _source.AttackConfig.AttackMoveConfig;
    public StepData KnockbackConfig => _source.AttackConfig.KnockbackCofig;
}

/// <summary>
/// 런타임 회피 설정 래퍼 클래스입니다.
/// </summary>
public class RuntimeDodgeConfig
{
    private DodgeData _source;
    public DodgeData RawData => _source;

    public Stat StaminaConsumption; // 스테미나 소모량 스탯 (버프 가능)
    public Stat Cooldown;           // 쿨타임 스탯 (버프 가능)

    public RuntimeDodgeConfig(DodgeData source)
    {
        _source = source;
        StaminaConsumption = new Stat(() => _source.StaminaAmount);
        Cooldown = new Stat(() => _source.Cooldown);
    }

    public string AnimationStateName => _source.AnimationStateName;
    public DodgeData.DodgeType Type => _source.Type;
    public bool IsInvincible => _source.isInivicible;
    public StepData MoveConfig => _source.MoveConfig;
}
