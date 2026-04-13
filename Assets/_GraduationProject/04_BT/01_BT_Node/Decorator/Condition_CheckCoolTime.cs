using UnityEngine;
using BehaviorTree;

public class Condition_CheckCoolTime : ConditionNode
{
    [Tooltip("荑⑦??꾩쓣 ?뺤씤???ㅽ궗??怨좎쑀 ?대쫫?낅땲??")]
    public string skillName;

    [Tooltip("???ㅽ궗??荑⑦????쒓컙(珥??낅땲??")]
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
            return brain.IsSkillReady(skillName, cooldownDuration);
        }
        if (string.IsNullOrEmpty(skillName))
        {
            // // Debug.LogWarning("鍮꾩뼱?덉쓬: skillName???ㅼ젙?섏? ?딆븯?듬땲??");
            return false;
        }
    
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
