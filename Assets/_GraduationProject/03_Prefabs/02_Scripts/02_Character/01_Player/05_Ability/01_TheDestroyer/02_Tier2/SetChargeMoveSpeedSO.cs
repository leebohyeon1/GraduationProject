using UnityEngine;

[CreateAssetMenu(fileName = "SetChargeMoveSpeedSO", menuName = "Project/Player/Ability/TheDestroyer/Tier2/SetChargeMoveSpeedSO")]
public class SetChargeMoveSpeedSO : PlayerAbilitySO
{
    public float ChargeMoveSpeed; // 차지 이동속도

    /// <summary>
    /// 기능 등록
    /// </summary>
    /// <param name="ability">플레이어</param>
    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();

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
    }
}
