using UnityEngine;

/// <summary>
/// 강공격 중에 슈퍼아머(경직 면역)를 부여하는 능력입니다.
/// </summary>
[CreateAssetMenu(fileName = "PlayerHeavyAttackSuperArmorAbility", menuName = "Project/Player/Ability/Ability/HeavyAttackSuperArmor")]
public class PlayerHeavyAttackSuperArmorAbilitySO : PlayerAbilitySO
{
    public override void RegisterAbility(PlayerAbility ability)
    {
        // 기본 초기화 (p_owner, p_ability 설정 등)
        base.RegisterAbility(ability);
        
        // RegisterAbility 호출 시점에는 base.AddAllSkillTags()가 이미 실행되므로 초기 상태에서는 제거해 둠
        // (상태 변경 이벤트에서만 관리하기 위함)
        RemoveAllSkillTags();

        // 상태 변경 이벤트 구독
        p_owner.FSM.OnStateChanged += HandleStateChanged;
        
        // 현재 상태 체크
        HandleStateChanged(p_owner.FSM.CurrentState);
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        // 이벤트 해제
        if (p_owner != null && p_owner.FSM != null)
        {
            p_owner.FSM.OnStateChanged -= HandleStateChanged;
        }

        // 부여된 태그가 있다면 제거
        RemoveAllSkillTags();
        
        base.UnregisterAbility(ability);
    }

    private void HandleStateChanged(IState newState)
    {
        if (newState is PlayerHeavyAttackState)
        {
            // 강공격 상태 진입 시 태그 부여 (경직 면역 태그 등)
            AddAllSkillTags();
        }
        else
        {
            // 그 외 상태 시 태그 제거
            RemoveAllSkillTags();
        }
    }
}
