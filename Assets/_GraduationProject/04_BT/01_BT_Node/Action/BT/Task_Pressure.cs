using UnityEngine;
using BehaviorTree;
using Pathfinding;
using Pathfinding.RVO;

public class Task_Pressure : Node
{
    public string Pos_Key = "PressurePos";
    public float MoveSpeed = 4.0f;
    public float StoppingDist = 0.5f;
    public float RotationSpeed = 5.0f;
    private AIPath ai;
    private Vector3? currentTargetDebug; 

    public override void OnEnter()
    {
        base.OnEnter();
        ai = runner.aIPath;
        
        if (ai != null)
        {
            ai.canMove = true;
            ai.isStopped = false;
            ai.maxSpeed = MoveSpeed;
            ai.enableRotation = false;
            ai.SetPath(null); // 吏꾩엯 ???붿뿬 寃쎈줈 ?쒓굅
        }
        runner._stateController.SetLock(false);

    }
    protected override NodeState OnUpdate()
    {
        // [?섏젙] ?좊땲硫붿씠??釉뚮┸吏??IsAttacking??true?붾씪?? ?꾩옱 ?곹깭媛 Attack???꾨땲硫??대룞 ?덉슜 怨좊젮
        // ?섏?留??덉쟾???꾪빐 濡쒓렇瑜??④린怨??ㅽ뙣 泥섎━ ?좎? (BaseAttackNode?먯꽌 媛뺤젣 ?댁젣?섎?濡??댁젣 諛쒖깮 ????
        if(runner._animationBridge.IsAttacking)
        {
            return NodeState.RUNNING; // ?ㅽ뙣 ????湲고븯???몃━媛 ?吏 ?딄쾶 ??
        }   

        if(runner.CurrentState == EnemyStateController.EnemyState.Attack)
        {
            return NodeState.FAILURE;
        }
        
        object val = brain.blackboard.GetValue<Vector3>(Pos_Key);
        if (val == null)
        {
            return NodeState.FAILURE;
        }

        Vector3 targetPos = (Vector3)val;
        currentTargetDebug = targetPos; 
        

        RotateTowardsPlayer();
        runner.Movement.UpdateStrafeAnim();
        // A* 寃쎈줈 ?낅뜲?댄듃 媛뺤젣
        runner.Movement.StartOrUpdateChase(targetPos, EnemyStateController.EnemyState.Chase, MoveSpeed);

        return NodeState.RUNNING;
    }
    public override void Abort()
    {
        base.Abort();
    }
    public override void OnExit()
    {
        base.OnExit();
        
        runner._stateController.SetLock(false);
    }
    private void RotateTowardsPlayer()
    {
        if (runner.player == null) return;
        ai.enableRotation = false;
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0; 

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            runner.transform.rotation = Quaternion.Slerp(runner.transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
    }

    public void OnDrawGizmos()
    {
        if (runner != null && currentTargetDebug.HasValue)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentTargetDebug.Value, 0.3f); 
            Gizmos.DrawLine(runner.transform.position, currentTargetDebug.Value); 
        }
    }
    public override Node Clone()
    {
        var node = Instantiate(this); // [?섏젙] CreateInstance ???Instantiate ?ъ슜 (SO 蹂듭젣 ?쒖?)
        node.Pos_Key = this.Pos_Key;
        node.MoveSpeed = this.MoveSpeed;
        node.StoppingDist = this.StoppingDist;
        node.RotationSpeed = this.RotationSpeed;
        return node;
    }
}
