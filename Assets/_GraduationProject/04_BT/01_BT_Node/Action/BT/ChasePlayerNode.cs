using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "ChasePlayerNode", menuName = "BehaviorTree/ChasePlayerNode")]
public class ChasePlayerNode : Node
{
    AIPath aIPath;
    public override void OnEnter()
    {
        aIPath = runner.GetComponent<AIPath>();
        aIPath.enableRotation = false;
        aIPath.Teleport(runner.transform.position);
        
        runner.SetState(Enemy.EnemyState.Chase);

        var rvo = runner.GetComponent<Pathfinding.RVO.RVOController>();
        if (rvo != null)
        {
            rvo.velocity = Vector3.zero;
        }
    }
private void RotateTowards(Vector3 target)
    {
        Vector3 dir = (target - runner.transform.position).normalized;
        dir.y = 0; 
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            // 회전 속도를 빠르게(10f 이상) 주어 반응성을 높입니다.
            runner.transform.rotation = Quaternion.Slerp(runner.transform.rotation, lookRot, Time.deltaTime * 10f);
        }
    }
    protected override NodeState OnUpdate()
    {
        if (runner.player == null)
        {
            return NodeState.FAILURE;
        }
        runner.Movement.StartOrUpdateChase(runner.player.transform);
        RotateTowards(runner.player.transform.position);
        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        
        runner.Movement.StopMovement();
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}