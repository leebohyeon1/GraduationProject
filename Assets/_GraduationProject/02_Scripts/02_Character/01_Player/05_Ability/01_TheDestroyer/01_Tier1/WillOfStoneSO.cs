using UnityEngine;

/// <summary>
/// 중량형 1티어 
/// 차지 시작 시 태그를 넣고
/// 차지 종료 시 태그 제거
/// </summary>
[CreateAssetMenu(fileName = "WillOfStoneSO", menuName = "Project/Player/Ability/TheDestroyer/Tier1/WillOfStoneSO")]
public class WillOfStoneSO : PlayerAbilitySO
{

    /// <summary>
    /// 기능 등록
    /// </summary>
    /// <param name="ability">플레이어</param>
    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();

        p_owner.Events.ChargeStarted += OnChargeStarted;
        p_owner.Events.ChargeFinished += OnChargeFinished;
    }

    /// <summary>
    /// 기능 해제
    /// </summary>
    /// <param name="ability">플레이어</param>
    public override void UnregisterAbility(PlayerAbility ability)
    {
        p_ability = null;
        p_owner = null;

        p_owner.Events.ChargeStarted -= OnChargeStarted;
        p_owner.Events.ChargeFinished -= OnChargeFinished;
    }



    private void OnChargeStarted()
    {
        AddAllSkillTags();
    }

    private void OnChargeFinished()
    {
        RemoveAllSkillTags();
    }
}
