using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "IsSkillReady_Condition", menuName = "BehaviorTree/Condition/IsSkillReady")]
public class Condition_IsSkillReady : ConditionNode
{
    [Tooltip("쿨타임을 확인할 스킬의 고유 이름입니다.")]
    public string skillName;

    [Tooltip("이 스킬의 쿨타임 시간(초)입니다.")]
    public float cooldownDuration;
    public bool HeatCooldown = false;
    CalculationResult stat;
    public override void OnEnter()
    {
        base.OnEnter();
        stat = runner.heatSystem.CalculationHeat("Test", ActorType.Monster, runner.heatSystem.GetTier(), 0);
    }
    protected override bool CheckCondition()
    {
        if (string.IsNullOrEmpty(skillName))
        {
            return false;
        }
        
        if (HeatCooldown == false)
            return brain.IsSkillReady(skillName, cooldownDuration);
        else
            return brain.IsSkillReady(skillName, cooldownDuration / stat.FinalSpeed);
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.skillName = this.skillName;
        node.cooldownDuration = this.cooldownDuration;
        node.HeatCooldown = this.HeatCooldown;
        return node;
    }
}