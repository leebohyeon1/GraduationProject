using UnityEngine;
using BehaviorTree;

/// <summary>
/// 특정 애니메이터 bool 파라미터를 제어하여 루프 애니메이션을 시작하고, 
/// 지정된 시간이 지나면 해당 파라미터를 해제(false)하여 루프를 탈출하게 하는 단순화된 노드입니다.
/// </summary>
[CreateAssetMenu(fileName = "Task_AttackPostDelay", menuName = "BehaviorTree/Action/Task_AttackPostDelay")]
public class Task_AttackPostDelay : Node
{
    [Header("Settings")]
    [Tooltip("제어할 애니메이터의 bool 파라미터 이름")]
    public string parameterName;
    
    [Tooltip("루프를 유지할 시간 (초)")]
    public float delayTime;

    private float _startTime;

    public override void OnEnter()
    {
        base.OnEnter();
        _startTime = Time.time;

        // 이동 정지 (애니메이션 도중 이동 방지)
        if (runner != null && runner.Movement != null)
        {
            runner.Movement.StopMovement();
        }

        // 루프 애니메이션 진입을 위해 bool 파라미터를 true로 설정
        if (runner != null && !string.IsNullOrEmpty(parameterName))
        {
            runner.AnimationBool(parameterName, true);
        }
    }

    protected override NodeState OnUpdate()
    {
        if (runner == null) return NodeState.FAILURE;

        // 경과 시간 체크: 설정된 delayTime이 지나면 SUCCESS 반환
        if (Time.time - _startTime >= delayTime)
        {
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        base.OnExit();
        
        // 노드가 성공적으로 끝나거나 중단(Abort)될 때, 파라미터를 false로 리셋하여 반드시 루프를 탈출하게 함
        if (runner != null && !string.IsNullOrEmpty(parameterName))
        {
            runner.AnimationBool(parameterName, false);
        }
    }

    public override Node Clone()
    {
        Task_AttackPostDelay node = Instantiate(this);
        node.parameterName = parameterName;
        node.delayTime = delayTime;
        return node;
    }
}
