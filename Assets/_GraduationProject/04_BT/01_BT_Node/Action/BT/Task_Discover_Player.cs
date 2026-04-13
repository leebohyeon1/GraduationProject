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
        
        if (!brain._isCombat)
        {
            // runner._aiController._aiBrain.blackboard.SetValue("Engage", true);
            runner.AnimationEvent(animationTagName);
            if (Handler != null) Handler.ResetAllFlags();
            // ?곹깭 ?좉툑: 諛쒓껄 ?곗텧 ?꾩쨷 ?ㅻⅨ 怨듦꺽???쇱뼱?ㅼ? 紐삵븯寃???
            runner._stateController.SetLock(true);
            _didSetLock = true;
            
            // ?꾩뿭 ?꾪닾 ?곹깭濡??꾪솚

            
        }
    }

    protected override NodeState OnUpdate()
    {
        // ?곹깭 以묐떒 泥댄겕: ?ㅽ꽩?대굹 ?щ쭩 ??利됱떆 醫낅즺
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
