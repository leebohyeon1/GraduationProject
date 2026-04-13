using UnityEngine;
using BehaviorTree;
using Pathfinding;

public class Task_BackStep : Node
{
    [Header("Settings")]
    public float backStepSpeed = 15f;
    public float duration = 0.4f;
    public string animationTrigger = "Do_BackStep";
    public float wallCheckDist = 1.0f;

    private float _startTime;
    private Vector3 _dashDirection;
    private AIPath _aiPath;
    private CharacterController _cc;
    private Vector3 _verticalVelocity; 

    public override void OnEnter()
    {
        _aiPath = runner.GetComponent<AIPath>();
        _cc = runner.GetComponent<CharacterController>();
        _startTime = Time.time;
        _verticalVelocity = Vector3.zero;

        // // BTDebug.Log(string.Format("[Task_BackStep : {0}] OnEnter 진입. 현재 상태: {1}", runner.name, runner.CurrentState));

        if (_aiPath != null) 
        {
            _aiPath.isStopped = true;
            _aiPath.canMove = false;
        }

        _dashDirection = -runner.transform.forward;

        // [중요] 순서 변경: 상태와 애니메이션을 먼저 설정한 후 Lock을 걸어야 함
        runner.SetState(EnemyStateController.EnemyState.Rush); 
        runner.AnimationEvent(animationTrigger);
        
        // 이제부터 외부 간섭 차단
        runner._stateController.SetLock(true);
    }

    protected override NodeState OnUpdate()
    {
        if (Time.time - _startTime > duration)
        {
            return NodeState.SUCCESS;
        }

        Vector3 rayOrigin = runner.transform.position + Vector3.up * 0.8f;
        if (Physics.Raycast(rayOrigin, _dashDirection, wallCheckDist, LayerMask.GetMask("Wall", "Default")))
        {
            return NodeState.SUCCESS;
        }

        if (_cc != null)
        {
            if (_cc.isGrounded && _verticalVelocity.y < 0)
            {
                _verticalVelocity.y = -2f;
            }
            _verticalVelocity.y += Physics.gravity.y * Time.deltaTime;

            Vector3 move = (_dashDirection * backStepSpeed * Time.deltaTime) + (_verticalVelocity * Time.deltaTime);
            _cc.Move(move);
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        if (runner._stateController != null)
        {
            runner._stateController.SetLock(false);
        }

        if (_aiPath != null)
        {
            _aiPath.Teleport(runner.transform.position, false);
            _aiPath.isStopped = false;
            _aiPath.canMove = true;
        }

        runner.Movement.StopMovement(); 
        runner.SetState(EnemyStateController.EnemyState.Idle);
    }

    public override void Abort()
    {
        if (runner._stateController != null)
        {
            runner._stateController.SetLock(false);
        }
        base.Abort();
    }

    public override Node Clone()
    {
        Task_BackStep node = Instantiate(this);
        node.backStepSpeed = this.backStepSpeed;
        node.duration = this.duration;
        node.animationTrigger = this.animationTrigger;
        node.wallCheckDist = this.wallCheckDist;
        return node;
    }
}
