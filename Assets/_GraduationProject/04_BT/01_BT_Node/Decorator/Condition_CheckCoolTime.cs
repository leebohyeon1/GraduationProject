using UnityEngine;
using BehaviorTree;

public class Condition_CheckCoolTime : ConditionNode
{
    [Tooltip("쿨타임을 확인할 스킬의 고유 이름입니다.")]
    public string skillName;

    [Tooltip("이 스킬의 쿨타임 시간(초)입니다.")]
    public float cooldownDuration;
    public bool EnemyAttackData;
    public EnemyAttackData attackData;
    public override void OnEnter()
    {
        base.OnEnter();
    }
    protected override bool CheckCondition()
    {
        if(EnemyAttackData)
        {
            skillName = attackData.AttackName;
            cooldownDuration = attackData.Cooltime;
            // // Debug.Log($"[Condition_CheckCoolTime] Checking cooldown for skill: {skillName} with duration: {cooldownDuration}");
            return brain.IsSkillReady(skillName, cooldownDuration);
        }
        if (string.IsNullOrEmpty(skillName))
        {
            // Debug.LogWarning("비어있음: skillName이 설정되지 않았습니다.");
            return false;
        }
        // // Debug.Log($"success : {this.name} {brain.IsSkillReady(skillName, cooldownDuration)}");
    
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