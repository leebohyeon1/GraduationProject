using UnityEngine;
using BehaviorTree;

public class Task_Discover_Player : Node
{
    private float _entryTime;
    public float transitionBuffer = 0.5f;
    public string animationTagName = "Discover_Player";
    private bool _didSetLock = false;

    public override void OnEnter()
    {
        _entryTime = Time.time;
        _didSetLock = false;
        // // Debug.Log("[Task_Discover_Player : " + runner.name + "] 전투 돌입.");
        
        if (!brain._isCombat)
        {
            // runner._aiController._aiBrain.blackboard.SetValue("Engage", true);
            runner.AnimationEvent(animationTagName);
            if (Handler != null) Handler.ResetAllFlags();
            // 상태 잠금: 발견 연출 도중 다른 공격이 끼어들지 못하게 함
            runner._stateController.SetLock(true);
            _didSetLock = true;
            
            // 전역 전투 상태로 전환

            
        }
    }

    protected override NodeState OnUpdate()
    {
        // 상태 중단 체크: 스턴이나 사망 시 즉시 종료
        if (runner.CurrentState == EnemyStateController.EnemyState.Stunned || runner.CurrentState == EnemyStateController.EnemyState.Die)
        {
            // // Debug.Log("[Task_EngageCombat : " + runner.name + "] 상태 이상으로 인한 중단.");
            return NodeState.FAILURE;
        }

        float elapsedTime = Time.time - _entryTime;

        var stateInfo = runner.animator.GetCurrentAnimatorStateInfo(0);
        var nextStateInfo = runner.animator.GetNextAnimatorStateInfo(0);
        bool isTagActive = stateInfo.IsTag(animationTagName) || nextStateInfo.IsTag(animationTagName);

        if (Handler != null && Handler.IsActionFinished)
        {
            Debug.Log("[Task_EngageCombat : " + runner.name + "] 행동 종료 감지.");
            return NodeState.SUCCESS;
        }

        if (isTagActive || elapsedTime < transitionBuffer)
        {
            return NodeState.RUNNING;
        }

        if (elapsedTime > transitionBuffer + 2.0f)
        {
             Debug.Log("[Task_EngageCombat : " + runner.name + "] 타임아웃 종료.");
             return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        if (_didSetLock && runner._stateController != null)
        {
            runner._stateController.SetLock(false);
            _didSetLock = false;
            // brain.CombatEnter(true);
            // if (runner.groupAi != null) runner.groupAi.CombatAll();
        }
        if (Handler != null) Handler.ResetAllFlags();
    }

    public override void Abort()
    {
        if (_didSetLock && runner._stateController != null)
        {
            runner._stateController.SetLock(false);
            _didSetLock = false;
        }
        if (Handler != null) Handler.ResetAllFlags();
        base.Abort();
    }

    public override Node Clone()
    {
        Task_Discover_Player node = Instantiate(this);
        node.transitionBuffer = this.transitionBuffer;
        node.animationTagName = this.animationTagName;
        return node;
    }
}
