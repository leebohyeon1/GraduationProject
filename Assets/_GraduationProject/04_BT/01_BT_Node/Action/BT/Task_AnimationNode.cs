using UnityEngine;
using BehaviorTree;

/// <summary>
/// 특정 애니메이션 트리거를 발생시켜 행동을 시작(혹은 루프 탈출)하고, 
/// 해당 애니메이션이 완전히 종료(FinishAction 신호)된 후 추가적인 postDelayTime만큼 대기한 뒤 SUCCESS를 반환하는 노드입니다.
/// </summary>
[CreateAssetMenu(fileName = "Task_AnimationNode", menuName = "BehaviorTree/Action/Task_AnimationNode")]
public class Task_AnimationNode : Node
{
    [Header("Settings")]
    [Tooltip("실행할 애니메이션 트리거 이름 (Animator Trigger 혹은 이벤트 이름)")]
    public string triggerName;
    
    [Tooltip("애니메이션 종료(FinishAction) 후 추가로 대기할 시간 (초)")]
    public float postDelayTime;

    private bool _isAnimFinished;
    private float _endTime;

    private bool _didSetLock = false;
    public override void OnEnter()
    {
        base.OnEnter();
        _isAnimFinished = false;

        // 1. 애니메이션 신호 초기화 (이전 행동 잔상 제거)
        if (Handler != null) Handler.ResetAllFlags();

        // 2. 이동 정지 (애니메이션 연출 집중)
        if (runner != null && runner.Movement != null)
        {
            runner.Movement.StopMovement();
        }

        // 3. 트리거 발생
        // 보스전의 경우 특정 상태에서 전이하거나 루프를 탈출할 때 트리거가 더 관리하기 쉽습니다.
        if (runner != null && !string.IsNullOrEmpty(triggerName))
        {
            runner.AnimationEvent(triggerName);
            runner._stateController.SetLock(true); // 행동 도중 다른 행동이 끼어들지 못하도록 잠금
            _didSetLock = true;
        }
        
        Debug.Log($"<color=white>[Task_AnimationNode]</color> '{triggerName}' 트리거 발송. 애니메이션 완료 및 {postDelayTime}초 대기 시작.");
    }

    protected override NodeState OnUpdate()
    {
        if (runner == null) return NodeState.FAILURE;

        // 상태 1: 애니메이션 종료(FinishAction 이벤트) 대기
        if (!_isAnimFinished)
        {
            if (Handler != null && Handler.IsActionFinished)
            {
                _isAnimFinished = true;
                _endTime = Time.time;
                
                Debug.Log($"<color=white>[Task_AnimationNode]</color> 애니메이션 종료 신호 감지. 포스트 딜레이({postDelayTime}s) 대기 시작.");
            }
            return NodeState.RUNNING;
        }

        // 상태 2: 애니메이션 종료 후 추가 지연 시간 대기
        if (Time.time - _endTime >= postDelayTime)
        {
            Debug.Log($"<color=white>[Task_AnimationNode]</color> 모든 대기 완료. SUCCESS 반환.");
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        base.OnExit();
        // 트리거는 Bool과 달리 별도의 false 처리가 필요 없으므로 구조가 더 깔끔합니다.
        if (runner != null && runner._stateController != null && _didSetLock)
        {
            runner._stateController.SetLock(false);
        }
    }
    public override void Abort()
    {
        base.Abort();
        if (runner != null && runner._stateController != null && _didSetLock)
        {   
            runner._stateController.SetLock(false);
        }
    }


    public override Node Clone()
    {
        Task_AnimationNode node = Instantiate(this);
        node.triggerName = triggerName;
        node.postDelayTime = postDelayTime;
        return node;
    }
}
