// --- FILE: Action_LookAtPlayer.cs ---

using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "LookAtPlayer", menuName = "BehaviorTree/Action/LookAtPlayer")]
public class Action_LookAtPlayer : Node
{

    public bool IsSee = true;
    public float rotationSpeed = 5f;
    float realRotationSpeed = 0f;
    override public void OnEnter()
    {
        base.OnEnter();
        realRotationSpeed = rotationSpeed;
    }
    protected override NodeState OnUpdate()
    {
        

        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            // 1. 목표 회전값(Quaternion)을 계산합니다.
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            // 2. 현재 회전값에서 목표 회전값으로 부드럽게 회전시킵니다.
            // Quaternion.Slerp(현재 회전, 목표 회전, 회전 속도)
            runner.transform.rotation = Quaternion.Slerp(
                runner.transform.rotation, 
                targetRotation, 
                realRotationSpeed * Time.deltaTime
            );
        }
        
        return IsSee ? NodeState.SUCCESS : NodeState.RUNNING;
    }
    public override void OnExit()
    {
        base.OnExit();
        realRotationSpeed = 0f;
    }
    public override Node Clone()
    {
        Action_LookAtPlayer newNode = Instantiate(this);
        newNode.IsSee = this.IsSee;
        return newNode;
    }
}
