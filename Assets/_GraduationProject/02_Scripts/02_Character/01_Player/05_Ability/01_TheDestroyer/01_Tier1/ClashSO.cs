using UnityEngine;

/// <summary>
/// 중량형 1티어 
/// 회피가 구르기에서 스텝으로 바뀜
/// 차지 중에 회피 가능
/// </summary>
[CreateAssetMenu(fileName = "ClashSO", menuName = "Project/Player/Ability/TheDestroyer/Tier1/ClashSO")]
public class ClashSO : PlayerAbilitySO
{
    public DodgeTagSO StepTagSO;
    public DodgeTagSO ChargeStepTagSO;

    /// <summary>
    /// 기능 등록
    /// </summary>
    /// <param name="ability">플레이어</param>
    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();

        StepTagSO.Apply(p_owner);  // 회피 태그 적용

        p_owner.Events.DodgeStarted += OnDodgeStarted;
        p_owner.Events.DodgeFinished += OnDodgeFinished;
    }

    /// <summary>
    /// 기능 해제
    /// </summary>
    /// <param name="ability">플레이어</param>
    public override void UnregisterAbility(PlayerAbility ability)
    {
        p_ability = null;
        p_owner = null;

        StepTagSO.Revert(p_owner);  // 회피 태그 해제


        p_owner.Events.DodgeStarted -= OnDodgeStarted;
        p_owner.Events.DodgeFinished -= OnDodgeFinished;
    }

    private void OnDodgeStarted()
    {
        AddAllSkillTags();
    }
    
    private void OnDodgeFinished()
    {
        RemoveAllSkillTags();
    }
}
