using UnityEngine;
using BehaviorTree;

/// <summary>
/// 특정 애니메이터 bool 파라미터를 켜서 애니메이션을 시작하고, 
/// 해당 애니메이션이 완전히 종료(FinishAction 신호)된 후 추가적인 delayTime만큼 대기한 뒤 SUCCESS를 반환하는 노드입니다.
/// </summary>
[CreateAssetMenu(fileName = "Task_AnimationNode", menuName = "BehaviorTree/Action/Task_AnimationNode")]
public class Task_AnimationNode : Node
{
    [Header("Settings")]
    [Tooltip("제어할 애니메이터의 bool 파라미터 이름")]
    public string parameterName;
    
    [Tooltip("애니메이션 종료 후 추가로 대기할 시간 (초)")]
    public float postDelayTime;

    private bool _isAnimFinished;
    private float _endTime;

    public override void OnEnter()
    {
        base.OnEnter();
        _isAnimFinished = false;

        // 1. 애니메이션 신호 초기화
        if (Handler != null) Handler.ResetAllFlags();

        // 2. 이동 정지
        if (runner != null && runner.Movement != null)
        {
            runner.Movement.StopMovement();
        }

        // 3. 루프/애니메이션 시작
        if (runner != null && !string.IsNullOrEmpty(parameterName))
        {
            runner.AnimationBool(parameterName, true);
        }
        
        Debug.Log($"<color=white>[Task_AnimationNode]</color> '{parameterName}' 시작. 종료 신호 대기 중...");
    }

    protected override NodeState OnUpdate()
    {
        if (runner == null) return NodeState.FAILURE;

        // 상태 1: 애니메이션 종료(FinishAction 이벤트) 대기
        if (!_isAnimFinished)
        {
            if (Handler != null && Handler.IsActionFinished)
            {
                // 애니메이션 종료 감지
                if (!string.IsNullOrEmpty(parameterName))
                {
                    runner.AnimationBool(parameterName, false);
                }
                
                _isAnimFinished = true;
                _endTime = Time.time;
                
                Debug.Log($"<color=white>[Task_AnimationNode]</color> 애니메이션 종료 감지. 추가 대기 시작: {postDelayTime}s");
            }
            return NodeState.RUNNING;
        }

        // 상태 2: 애니메이션 종료 후 추가 지연 시간(postDelayTime) 대기
        if (Time.time - _endTime >= postDelayTime)
        {
            Debug.Log($"<color=white>[Task_AnimationNode]</color> 모든 대기 완료. SUCCESS.");
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        base.OnExit();
        
        // 중단되거나 종료될 때 파라미터 안전하게 해제
        if (runner != null && !string.IsNullOrEmpty(parameterName))
        {
            runner.AnimationBool(parameterName, false);
        }
    }

    public override Node Clone()
    {
        Task_AnimationNode node = Instantiate(this);
        node.parameterName = parameterName;
        node.postDelayTime = postDelayTime;
        return node;
    }
}
