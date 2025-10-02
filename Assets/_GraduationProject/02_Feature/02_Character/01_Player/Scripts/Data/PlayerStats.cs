using System;

/// <summary>
/// 런타임 데이터를 담을 플레이어 데이터 클래스
/// </summary>
[Serializable]
public class PlayerStats
{
    public float AnimatorSpeed;

    // State
    public bool IsDefending;
    public bool IsInvincible;
    public bool IsCounterAttack;
    public bool IsLightHit;
    public bool IsHeavyHit;
    public bool IsDamaged => IsLightHit || IsHeavyHit;


    // Stat
    public int MaxHealth;
    public int CurrentHealth;
    public int CurrentHeat;

    public float MoveSpeed;
    public float RotateSpeed;

    public float BattleOutTime = 8f;
    public PlayerCombatData CombatData = new PlayerCombatData();

    public event Action<float> OnAnimationSpeedChanged;

    public PlayerStats(BasePlayerDatasSO baseData)
    {
        ResetData(baseData);
    }

    public void UpdateData(BasePlayerDatasSO baseData, TierStatData tierStatData)
    {
        MoveSpeed = baseData.MoveSpeed * tierStatData.SpeedMultiply;
        RotateSpeed = baseData.RotateSpeed * tierStatData.SpeedMultiply;
        AnimatorSpeed = tierStatData.AnimSpeedMultiply;
        OnAnimationSpeedChanged?.Invoke(AnimatorSpeed);

        CombatData = baseData.CombatData;
    }

    public void ResetData(BasePlayerDatasSO baseData)
    {
        MaxHealth = baseData.MaxHealth;
        MoveSpeed = baseData.MoveSpeed;
        RotateSpeed = baseData.RotateSpeed;
        CombatData = baseData.CombatData;
    }

    public void SetDamaged(PlayerDamagedType damagedType)
    {
        switch (damagedType)
        {
            case PlayerDamagedType.Normal:
                IsLightHit = true;
                break;
            case PlayerDamagedType.Strong:
                IsHeavyHit = true;
                break;
        }
    }

    public void ResetDamaged()
    {
        IsLightHit = false;
        IsHeavyHit = false;
    }
}
