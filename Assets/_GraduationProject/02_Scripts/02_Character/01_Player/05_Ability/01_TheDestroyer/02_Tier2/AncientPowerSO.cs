using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 중량형 2티어 
/// 차지 단계 증가
/// 차지 이동속도 증가
/// </summary>
[CreateAssetMenu(fileName = "AncientPowerSO", menuName = "Project/Player/Ability/TheDestroyer/Tier2/AncientPowerSO")]
public class AncientPowerSO : PlayerAbilitySO
{
    // 추가 차지 공격 리스트
    public List<PlayerChargeConfig> AdditionalChargeDataList = new List<PlayerChargeConfig>();
    public float ChargeMoveSpeed; // 차지 이동속도

    private int _defaultIndex = -1;

    /// <summary>
    /// 기능 등록
    /// </summary>
    /// <param name="ability">플레이어</param>
    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();

        _defaultIndex = p_owner.Combat.HeavyCounterAttackConfigList.Count;

        p_owner.Combat.HeavyCounterAttackConfigList.AddRange(AdditionalChargeDataList);
        p_owner.Movement.SetChargeMoveSpeed(ChargeMoveSpeed);
    }

    /// <summary>
    /// 기능 해제
    /// </summary>
    /// <param name="ability">플레이어</param>
    public override void UnregisterAbility(PlayerAbility ability)
    {
        p_ability = null;
        p_owner = null;

        p_owner.Combat.HeavyCounterAttackConfigList
            .RemoveRange(_defaultIndex, AdditionalChargeDataList.Count);

        _defaultIndex = -1;
    }
}
