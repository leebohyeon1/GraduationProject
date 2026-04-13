using UnityEngine;
using BehaviorTree;

/// <summary>
/// ?쇨꺽(Hit) ?좊땲硫붿씠?섏씠 ?ъ깮?섎뒗 ?숈븞 BT???ㅻⅨ ?몃뱶 ?ㅽ뻾??李⑤떒?섍퀬, 
/// ?좊땲硫붿씠?섏씠 ?앸굹硫??뚮옒洹몃? ?뺣━?섎뒗 ?몃뱶?낅땲??
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

        // 1. ?좊땲硫붿씠???좏샇 珥덇린??(?댁쟾 ?됰룞???붿긽 ?쒓굅)
        if (Handler != null) Handler.ResetAllFlags();
        
        // 2. ?곹깭瑜?Hit?쇰줈 ?뺤떎???ㅼ젙
        runner.SetState(EnemyStateController.EnemyState.Hit);
        
        // 3. ?대룞 ?뺤?
        if (runner.Movement != null) runner.Movement.StopMovement();
        
    }

    protected override NodeState OnUpdate()
    {
        if (runner == null) return NodeState.FAILURE;

        // ?좊땲硫붿씠???곹깭 媛깆떊???꾪븳 理쒖냼 ?꾨젅???湲?
        if (Time.frameCount <= _entryFrame + 1) return NodeState.RUNNING;

        // 1. ?좊땲硫붿씠??醫낅즺 ?대깽??FinishAction) 媛먯?
        if (Handler != null && Handler.IsActionFinished)
        {
            return NodeState.SUCCESS;
        }

        // 2. ?덉쟾 ??꾩븘??(?좊땲硫붿씠???대깽???꾨씫 ?鍮? 蹂댄넻 1珥덈㈃ 異⑸텇)
        if (Time.time - _entryTime > 1.2f)
        {
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        base.OnExit();
        
        // 1. ?쇨꺽 ?뚮옒洹??댁젣 (留ㅼ슦 以묒슂: ?ㅼ쓬 ?좏깮 濡쒖쭅???묐룞?????덇쾶 ??
        brain.blackboard.SetValue(EnemyBlackboardKeys.OnTakeHit, false);
        
        // 2. ?곹깭瑜?Idle濡?蹂듦뎄
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
