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


        if (_aiPath != null) 
        {
            _aiPath.isStopped = true;
            _aiPath.canMove = false;
        }

        _dashDirection = -runner.transform.forward;

        // [以묒슂] ?쒖꽌 蹂寃? ?곹깭? ?좊땲硫붿씠?섏쓣 癒쇱? ?ㅼ젙????Lock??嫄몄뼱????
        runner.SetState(EnemyStateController.EnemyState.Rush); 
        runner.AnimationEvent(animationTrigger);
        
        // ?댁젣遺???몃? 媛꾩꽠 李⑤떒
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
