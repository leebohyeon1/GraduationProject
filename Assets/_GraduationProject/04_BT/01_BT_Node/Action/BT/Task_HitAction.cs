using UnityEngine;
using BehaviorTree;

/// <summary>
/// ?¼ê²©(Hit) ? ë‹ˆë©”ì´?˜ì´ ?¬ìƒ?˜ëŠ” ?™ì•ˆ BT???¤ë¥¸ ?¸ë“œ ?¤í–‰??ì°¨ë‹¨?˜ê³ , 
/// ? ë‹ˆë©”ì´?˜ì´ ?ë‚˜ë©??Œë˜ê·¸ë? ?•ë¦¬?˜ëŠ” ?¸ë“œ?…ë‹ˆ??
/// </summary>
[CreateAssetMenu(fileName = "Task_HitAction", menuName = "BehaviorTree/Action/HitAction")]
public class Task_HitAction : Node
{
    private float _entryTime;
    private int _entryFrame;

    public override void OnEnter()
    {
        base.OnEnter();
        _entryTime = Time.time;
        _entryFrame = Time.frameCount;

        // 1. ? ë‹ˆë©”ì´??? í˜¸ ì´ˆê¸°??(?´ì „ ?‰ë™???”ìƒ ?œê±°)
        if (Handler != null) Handler.ResetAllFlags();
        
        // 2. ?íƒœë¥?Hit?¼ë¡œ ?•ì‹¤???¤ì •
        runner.SetState(EnemyStateController.EnemyState.Hit);
        
        // 3. ?´ë™ ?•ì?
        if (runner.Movement != null) runner.Movement.StopMovement();
        
    }

    protected override NodeState OnUpdate()
    {
        if (runner == null) return NodeState.FAILURE;

        // ? ë‹ˆë©”ì´???íƒœ ê°±ì‹ ???„í•œ ìµœì†Œ ?„ë ˆ????ê¸?
        if (Time.frameCount <= _entryFrame + 1) return NodeState.RUNNING;

        // 1. ? ë‹ˆë©”ì´??ì¢…ë£Œ ?´ë²¤??FinishAction) ê°ì?
        if (Handler != null && Handler.IsActionFinished)
        {
            return NodeState.SUCCESS;
        }

        // 2. ?ˆì „ ???„ì•„??(? ë‹ˆë©”ì´???´ë²¤???„ë½ ??ë¹? ë³´í†µ 1ì´ˆë©´ ì¶©ë¶„)
        if (Time.time - _entryTime > 1.2f)
        {
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        base.OnExit();
        
        // 1. ?¼ê²© ?Œë˜ê·??´ì œ (ë§¤ìš° ì¤‘ìš”: ?¤ìŒ ? íƒ ë¡œì§???‘ë™?????ˆê²Œ ??
        brain.blackboard.SetValue(EnemyBlackboardKeys.OnTakeHit, false);
        
        // 2. ?íƒœë¥?Idleë¡?ë³µêµ¬
        if (runner.CurrentState == EnemyStateController.EnemyState.Hit)
        {
            runner.SetState(EnemyStateController.EnemyState.Idle);
        }

        if (Handler != null) Handler.ResetAllFlags();
        
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}
