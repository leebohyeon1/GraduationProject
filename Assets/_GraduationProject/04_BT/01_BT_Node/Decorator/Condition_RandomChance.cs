using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "RandomChance_Condition", menuName = "BehaviorTree/Condition/RandomChance")]
public class Condition_RandomChance : ConditionNode
{
   [Tooltip("???뺣쪧 泥댄겕瑜??앸퀎??怨좎쑀 ?대쫫?낅땲??")]
    public string checkName;

    [Tooltip("?뺣쪧 泥댄겕瑜??ㅼ떆 ?쒕룄?섍린源뚯????湲??쒓컙(荑⑦????낅땲??")]
    public float cooldownDuration;

    [Tooltip("??議곌굔???깃났(SUCCESS)???뺣쪧?낅땲??(0~100).")]
    [Range(0, 100)]
    public float successChance = 50f;

    protected override bool CheckCondition()
    {
        if (!brain.IsSkillReady(checkName, cooldownDuration))
        {
            return false;
        }
Debug.Log($"[Condition_RandomChance] Checking random chance condition: Check Name: {checkName}, Cooldown Duration: {cooldownDuration}, Success Chance: {successChance}");   
        brain.StartSkillCooldown(checkName);
        bool cnt = Random.Range(0f, 100f) <= successChance;
        return cnt;
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.successChance = this.successChance;
        node.checkName = this.checkName;
        node.cooldownDuration = this.cooldownDuration;
        return node;
    }
}
