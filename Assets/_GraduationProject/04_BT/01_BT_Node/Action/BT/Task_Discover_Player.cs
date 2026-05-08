using UnityEngine;
using BehaviorTree;

/// <summary>
/// 발견 연출 동안 애니메이션 태그와 Y축 이동을 AnimationCurve 기반으로 처리하는 BT 노드입니다.
/// </summary>
public class Task_Discover_Player : Node
{
    private float _entryTime;

    /// <summary>
    /// 전이 직후에도 노드를 유지하기 위한 최소 지속 시간입니다.
    /// </summary>
    public float transitionBuffer = 0.5f;

    /// <summary>
    /// 발견 연출에 사용하는 애니메이션 태그 이름입니다.
    /// </summary>
    public string animationTagName = "Discover_Player";
    private bool _didSetLock = false;

    /// <summary>
    /// 상승 구간의 지속 시간입니다.
    /// </summary>
    public float upduration = 1.0f;

    /// <summary>
    /// 하강 구간의 지속 시간입니다.
    /// </summary>
    public float downduration = 1.0f;

    /// <summary>
    /// 시작 위치에서 위로 이동할 거리입니다.
    /// </summary>
    public float moveUpDistance;

    /// <summary>
    /// 최고점에서 아래로 이동할 거리입니다.
    /// </summary>
    public float moveDownDistance;

    /// <summary>
    /// 상승 구간 보간 비율을 결정하는 커브입니다.
    /// </summary>
    public AnimationCurve upCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    /// <summary>
    /// 하강 구간 보간 비율을 결정하는 커브입니다.
    /// </summary>
    public AnimationCurve downCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private float _baseY;
    private float _peakY;
    private float _movePhaseStartTime;
    private bool _isMoving;
    private bool _isMovingDown;

    /// <summary>
    /// 발견 연출 시작 시 애니메이션과 커브 이동 상태를 초기화합니다.
    /// </summary>
    public override void OnEnter()
    {
        _entryTime = Time.time;
        _didSetLock = false;
        _baseY = runner.transform.position.y;
        _peakY = _baseY + moveUpDistance;
        _movePhaseStartTime = 0f;
        _isMoving = false;
        _isMovingDown = false;
        
        if (!brain._isCombat)
        {
            // runner._aiController._aiBrain.blackboard.SetValue("Engage", true);
            runner.AnimationEvent(animationTagName);
            if (Handler != null) Handler.ResetAllFlags();
            runner._stateController.SetState(EnemyStateController.EnemyState.Discover);
            runner._stateController.SetLock(true);
            _didSetLock = true;
        }

        if (moveUpDistance > 0f)
        {
            _isMoving = true;
            _movePhaseStartTime = Time.time;
        }
    }

    protected override NodeState OnUpdate()
    {
        UpdateMovement();

        if (runner.CurrentState == EnemyStateController.EnemyState.Stunned || runner.CurrentState == EnemyStateController.EnemyState.Die)
        {
            return NodeState.FAILURE;
        }

        float elapsedTime = Time.time - _entryTime;

        var stateInfo = runner.animator.GetCurrentAnimatorStateInfo(0);
        var nextStateInfo = runner.animator.GetNextAnimatorStateInfo(0);
        bool isTagActive = stateInfo.IsTag(animationTagName) || nextStateInfo.IsTag(animationTagName);

        if (Handler != null && Handler.IsActionFinished)
        {
            return NodeState.SUCCESS;
        }
        if(brain.blackboard.GetValue<bool>(EnemyBlackboardKeys.OnTakeHit))
        {
            return NodeState.FAILURE;
        }
        if (isTagActive || elapsedTime < transitionBuffer)
        {
            return NodeState.RUNNING;
        }

        if (elapsedTime > transitionBuffer + 2.0f)
        {
             return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    private void UpdateMovement()
    {
        if (!_isMoving)
        {
            return;
        }

        if (!_isMovingDown)
        {
            float normalizedTime = GetNormalizedTime(upduration);
            SetYPosition(Mathf.LerpUnclamped(_baseY, _peakY, upCurve.Evaluate(normalizedTime)));

            if (normalizedTime >= 1f)
            {
                if (moveDownDistance > 0f)
                {
                    _isMovingDown = true;
                    _movePhaseStartTime = Time.time;
                    SetYPosition(_peakY);
                    Debug.Log("Completed move up.");
                }
                else
                {
                    Debug.Log("Completed move up.");
                    _isMoving = false;
                }
            }

            return;
        }

        float downNormalizedTime = GetNormalizedTime(downduration);
        float targetY = _peakY - moveDownDistance;
        SetYPosition(Mathf.LerpUnclamped(_peakY, targetY, downCurve.Evaluate(downNormalizedTime)));

        if (downNormalizedTime >= 1f)
        {
            SetYPosition(targetY);
            _isMoving = false;
            Debug.Log("Completed move up and down sequence.");
        }
    }

    private float GetNormalizedTime(float duration)
    {
        if (duration <= 0f)
        {
            return 1f;
        }

        return Mathf.Clamp01((Time.time - _movePhaseStartTime) / duration);
    }

    private void SetYPosition(float y)
    {
        Vector3 currentPosition = runner.transform.position;
        currentPosition.y = y;
        runner.transform.position = currentPosition;
    }

    /// <summary>
    /// 노드 종료 시 이동 상태와 상태 잠금을 정리합니다.
    /// </summary>
    public override void OnExit()
    {
        _isMoving = false;
        _isMovingDown = false;

        if (_didSetLock && runner._stateController != null)
        {
            runner._stateController.SetLock(false);
            _didSetLock = false;
            // brain.CombatEnter(true);
            // if (runner.groupAi != null) runner.groupAi.CombatAll();
        }
        if (Handler != null) Handler.ResetAllFlags();
    }

    /// <summary>
    /// 노드 중단 시 이동 상태와 잠금을 즉시 해제합니다.
    /// </summary>
    public override void Abort()
    {
        if (_didSetLock && runner._stateController != null)
        {
            runner._stateController.SetLock(false);
            _didSetLock = false;
        }

        _isMoving = false;
        _isMovingDown = false;

        if (Handler != null) Handler.ResetAllFlags();
        base.Abort();
    }

    /// <summary>
    /// 런타임에서 사용할 노드 인스턴스를 복제합니다.
    /// </summary>
    public override Node Clone()
    {
        Task_Discover_Player node = Instantiate(this);
        node.transitionBuffer = this.transitionBuffer;
        node.animationTagName = this.animationTagName;
        node.upduration = this.upduration;
        node.downduration = this.downduration;
        node.moveUpDistance = this.moveUpDistance;
        node.moveDownDistance = this.moveDownDistance;
        node.upCurve = this.upCurve;
        node.downCurve = this.downCurve;
        
        return node;
    }
}
