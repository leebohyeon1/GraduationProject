using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// 런타임 공격 설정 인터페이스입니다.
/// 일반 공격과 차지 공격을 공통으로 다루기 위해 사용합니다.
/// </summary>
public interface IRuntimeAttackConfig
{
    Stat Damage { get; }
    Stat Stamina { get; }
    Stat Regain { get; }
    AttackType AttackType { get; }
    Vector3 AttackRadius { get; }
    StepData AttackMoveConfig { get; }
    StepData KnockbackConfig { get; }
    StepData DeathKnockbackConfig { get; }
    PlayerAttackConfig BaseAttackConfig { get; }
}

/// <summary>
/// 런타임 공격 설정 래퍼 클래스입니다.
/// 원본 SO 데이터를 참조하며, 데미지 등 수정이 필요한 수치만 Stat으로 관리합니다.
/// </summary>
public class RuntimeAttackConfig : IRuntimeAttackConfig
{
    private PlayerAttackConfig _source;
    public PlayerAttackConfig BaseAttackConfig => _source;

    public Stat Damage { get; } // 버프 적용이 가능한 데미지 스탯
    public Stat Stamina { get; }
    public Stat Regain { get; }

    public RuntimeAttackConfig(PlayerAttackConfig source)
    {
        _source = source;
        // 원본 SO의 공격력을 실시간으로 바라보는 Stat 생성
        Damage = new Stat(() => _source.AttackDamage);
        Stamina = new Stat(() => _source.AttackStamina);
        Regain = new Stat(() => _source.RegainRate);
    }

    // 변하지 않는 데이터는 원본에서 직접 참조 (프록시 패턴)
    public AttackType AttackType => _source.AttackType;
    public Vector3 AttackRadius => _source.AttackRadius;
    public StepData AttackMoveConfig => _source.AttackMoveConfig;
    public StepData KnockbackConfig => _source.KnockbackConfig;
    public StepData DeathKnockbackConfig => _source.DeathKnockbackConfig;
}

/// <summary>
/// 런타임 공격 설정 래퍼 클래스입니다.
/// 원본 SO 데이터를 참조하며, 데미지 등 수정이 필요한 수치만 Stat으로 관리합니다.
/// </summary>
public class RuntimeChargeAttackConfig : IRuntimeAttackConfig
{
    private PlayerChargeConfig _source;
    public PlayerAttackConfig BaseAttackConfig => _source.AttackConfig;

    public Stat Damage { get; } // 버프 적용이 가능한 데미지 스탯
    public Stat Stamina { get; }
    public Stat Regain { get; }

    public RuntimeChargeAttackConfig(PlayerChargeConfig source)
    {
        _source = source;
        // 원본 SO의 공격력을 실시간으로 바라보는 Stat 생성
        Damage = new Stat(() => _source.AttackConfig.AttackDamage);
        Stamina = new Stat(() => _source.AttackConfig.AttackStamina);
        Regain = new Stat(() => _source.AttackConfig.RegainRate);
    }

    // 변하지 않는 데이터는 원본에서 직접 참조 (프록시 패턴)
    public float ChargeTime => _source.ChargeTime;
    public AttackType AttackType => _source.AttackConfig.AttackType;
    public Vector3 AttackRadius => _source.AttackConfig.AttackRadius;
    public StepData AttackMoveConfig => _source.AttackConfig.AttackMoveConfig;
    public StepData KnockbackConfig => _source.AttackConfig.KnockbackConfig;
    public StepData DeathKnockbackConfig => _source.AttackConfig.DeathKnockbackConfig;
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
    public bool IsInvincible => _source.IsInvincible; // SO 오타 맞춰줌
    public StepData MoveConfig => _source.MoveConfig;
}
