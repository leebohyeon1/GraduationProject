using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "RecentlyUsedSkill_Condition", menuName = "BehaviorTree/Condition/RecentlyUsedSkill")]
public class Condition_RecentlyUsedSkill : ConditionNode
{
    [Tooltip("어떤 스킬 그룹의 마지막 사용 시간을 확인할지 정합니다.")]
    public string skillName;
    [Tooltip("스킬 사용 후, 이 시간(초)이 지나지 않았을 때만 성공합니다.")]
    public float withinSeconds;


    protected override bool CheckCondition()
    {
        // 스킬 사용후 withinSeconds 이내인지 
        return brain.GetLastSkillUseTime(skillName) + withinSeconds > Time.time;
    }
    public override Node Clone()
    {
        var clone = CreateInstance<Condition_RecentlyUsedSkill>();
        clone.skillName = skillName;
        clone.withinSeconds = withinSeconds;
        return clone;
    }
    
}