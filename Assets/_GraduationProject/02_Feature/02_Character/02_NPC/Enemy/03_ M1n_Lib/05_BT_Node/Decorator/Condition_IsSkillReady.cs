using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "IsSkillReady_Condition", menuName = "BehaviorTree/Condition/IsSkillReady")]
public class Condition_IsSkillReady : ConditionNode
{
    [Tooltip("쿨타임을 확인할 스킬의 고유 이름입니다.")]
    public string skillName;

    [Tooltip("이 스킬의 쿨타임 시간(초)입니다.")]
    public float cooldownDuration;

    protected override bool CheckCondition()
    {
        if (string.IsNullOrEmpty(skillName))
        {
            Debug.LogWarning("Skill Name is not set in Condition_IsSkillReady node!", this);
            return false;
        }
        
        // runner(Enemy)에게 이 스킬이 사용 가능한지 물어봅니다.
        return brain.IsSkillReady(skillName, cooldownDuration);
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.skillName = this.skillName;
        node.cooldownDuration = this.cooldownDuration;
        return node;
    }
}