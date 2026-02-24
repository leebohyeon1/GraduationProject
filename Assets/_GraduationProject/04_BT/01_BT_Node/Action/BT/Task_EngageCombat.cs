using UnityEngine;
using BehaviorTree;

public class Task_EngageCombat : Node
{
    private float _entryTime;
    public float transitionBuffer = 0.5f;
    public string animationTagName = "Discover_Player";

    public override void OnEnter()
    {
        _entryTime = Time.time;
        Debug.Log("[Task_EngageCombat] " + runner.name + " 전투 돌입.");
        
        if (!brain._isCombat)
        {
            runner.AnimationEvent(animationTagName);
            
            // 상태 잠금: 발견 연출 도중 다른 공격이 끼어들지 못하게 함
            runner._stateController.SetLock(true);
            
            // 전역 전투 상태로 전환
            brain.CombatEnter(true);
            if (runner.groupAi != null) runner.groupAi.CombatAll();
            
            Handler.ResetAllFlags();
        }
    }

    protected override NodeState OnUpdate()
    {
        // [중요] 상태 중단 체크: 스턴이나 사망 시 즉시 종료 (락은 OnExit/Abort에서 해제됨)
        if (runner.CurrentState == EnemyStateController.EnemyState.Stunned || runner.CurrentState == EnemyStateController.EnemyState.Die)
        {
            return NodeState.FAILURE;
        }

        float elapsedTime = Time.time - _entryTime;

        var stateInfo = runner.animator.GetCurrentAnimatorStateInfo(0);
        var nextStateInfo = runner.animator.GetNextAnimatorStateInfo(0);
        bool isTagActive = stateInfo.IsTag(animationTagName) || nextStateInfo.IsTag(animationTagName);

        if (Handler.IsActionFinished)
        {
            Debug.Log("[Task_EngageCombat] 행동 종료 감지.");
            return NodeState.SUCCESS;
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

    public override void OnExit()
    {
        if (runner._stateController != null)
        {
            runner._stateController.SetLock(false);
        }
        Handler.ResetAllFlags();
    }

    public override void Abort()
    {
        if (runner._stateController != null)
        {
            runner._stateController.SetLock(false);
        }
        Handler.ResetAllFlags();
    }

    public override Node Clone()
    {
        Task_EngageCombat node = Instantiate(this);
        node.transitionBuffer = this.transitionBuffer;
        node.animationTagName = this.animationTagName;
        return node;
    }
}
