using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "StartSkillCooldown", menuName = "BehaviorTree/Action/StartSkillCooldown")]
public class StartSkillCooldown : Node
{
    [Tooltip("쿨타임을 시작할 스킬의 고유 이름입니다.")]
    public string skillName;

    protected override NodeState OnUpdate()
    {
        if (string.IsNullOrEmpty(skillName))
        {
            Debug.LogError("Skill Name is not set in Action_StartSkillCooldown node!", this);
            return NodeState.FAILURE;
        }
        
        // runner(Enemy)에게 이 스킬의 쿨타임을 시작하라고 명령합니다.
        brain.StartSkillCooldown(skillName);
        
        // 이 노드는 즉시 성공을 반환하고 역할을 마칩니다.
        return NodeState.SUCCESS;
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.skillName = this.skillName;
        return node;
    }
}