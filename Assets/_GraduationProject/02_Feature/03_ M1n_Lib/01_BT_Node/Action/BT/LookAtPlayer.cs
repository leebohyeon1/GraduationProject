// --- FILE: Action_LookAtPlayer.cs ---

using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "LookAtPlayer", menuName = "BehaviorTree/Action/LookAtPlayer")]
public class Action_LookAtPlayer : Node
{

    public bool IsSee = true;


    protected override NodeState OnUpdate()
    {
        

        // 플레이어를 향하는 방향 벡터 계산 (Y축은 무시하여 수평으로만 회전)
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            runner.transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
        
        return IsSee ? NodeState.SUCCESS : NodeState.FAILURE;
    }

    public override Node Clone()
    {
        Action_LookAtPlayer newNode = Instantiate(this);
        newNode.IsSee = this.IsSee;
        return newNode;
    }
}
