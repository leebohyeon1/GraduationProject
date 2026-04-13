using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "RandomChance_Condition", menuName = "BehaviorTree/Condition/RandomChance")]
public class Condition_RandomChance : ConditionNode
{
   [Tooltip("이 확률 체크를 식별할 고유 이름입니다.")]
    public string checkName;

    [Tooltip("확률 체크를 다시 시도하기까지의 대기 시간(쿨타임)입니다.")]
    public float cooldownDuration;

    [Tooltip("이 조건이 성공(SUCCESS)할 확률입니다 (0~100).")]
    [Range(0, 100)]
    public float successChance = 50f;

    protected override bool CheckCondition()
    {
        if (!brain.IsSkillReady(checkName, cooldownDuration))
        {
            BTDebug.Log($"[Condition_RandomChance] '{checkName}' 쿨다운 중 - 조건 실패");
            return false;
        }

        brain.StartSkillCooldown(checkName);
        bool cnt = Random.Range(0f, 100f) <= successChance;
        BTDebug.Log($"[Condition_RandomChance] {(cnt ? "성공" : "실패")}");
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