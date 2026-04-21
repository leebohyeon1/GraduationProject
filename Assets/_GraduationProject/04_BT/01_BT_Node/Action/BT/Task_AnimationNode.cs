using UnityEngine;
using BehaviorTree;

/// <summary>
/// ?뱀젙 ?좊땲硫붿씠???몃━嫄곕? 諛쒖깮?쒖폒 ?됰룞???쒖옉(?뱀? 猷⑦봽 ?덉텧)?섍퀬, 
/// ?대떦 ?좊땲硫붿씠?섏씠 ?꾩쟾??醫낅즺(FinishAction ?좏샇)????異붽??곸씤 postDelayTime留뚰겮 ?湲고븳 ??SUCCESS瑜?諛섑솚?섎뒗 ?몃뱶?낅땲??
/// </summary>
[CreateAssetMenu(fileName = "Task_AnimationNode", menuName = "BehaviorTree/Action/Task_AnimationNode")]
public class Task_AnimationNode : Node
{
    [Header("Settings")]
    [Tooltip("?ㅽ뻾???좊땲硫붿씠???몃━嫄??대쫫 (Animator Trigger ?뱀? ?대깽???대쫫)")]
    public string triggerName;
    
    [Tooltip("?좊땲硫붿씠??醫낅즺(FinishAction) ??異붽?濡??湲고븷 ?쒓컙 (珥?")]
    public float postDelayTime;
    
    [Tooltip("Lock 중에도 Attack 상태 전환을 허용할지 여부 (특정 연출 -> 공격 전환용)")]
    public bool allowAttackTransitionWhileLocked = false;

    private bool _isAnimFinished;
    private float _endTime;

    private bool _didSetLock = false;
    public override void OnEnter()
    {
        base.OnEnter();
        _isAnimFinished = false;

        // 1. ?좊땲硫붿씠???좏샇 珥덇린??(?댁쟾 ?됰룞 ?붿긽 ?쒓굅)
        if (Handler != null) Handler.ResetAllFlags();

        // 2. ?대룞 ?뺤? (?좊땲硫붿씠???곗텧 吏묒쨷)
        if (runner != null && runner.Movement != null)
        {
            runner.Movement.StopMovement();
        }

        // 3. ?몃━嫄?諛쒖깮
        // 蹂댁뒪?꾩쓽 寃쎌슦 ?뱀젙 ?곹깭?먯꽌 ?꾩씠?섍굅??猷⑦봽瑜??덉텧?????몃━嫄곌? ??愿由ы븯湲??쎌뒿?덈떎.
        if (runner != null && !string.IsNullOrEmpty(triggerName))
        {
            runner.AnimationEvent(triggerName);
            runner._stateController.SetLock(true); // ?됰룞 ?꾩쨷 ?ㅻⅨ ?됰룞???쇱뼱?ㅼ? 紐삵븯?꾨줉 ?좉툑
            runner._stateController.SetLockedTransitionAllowance(EnemyStateController.EnemyState.Attack, allowAttackTransitionWhileLocked);
            _didSetLock = true;
        }
        
    }

    protected override NodeState OnUpdate()
    {
        if (runner == null) return NodeState.FAILURE;

        // ?곹깭 1: ?좊땲硫붿씠??醫낅즺(FinishAction ?대깽?? ?湲?
        if (!_isAnimFinished)
        {
            if (Handler != null && Handler.IsActionFinished)
            {
                _isAnimFinished = true;
                _endTime = Time.time;
                
            }
            return NodeState.RUNNING;
        }

        // ?곹깭 2: ?좊땲硫붿씠??醫낅즺 ??異붽? 吏???쒓컙 ?湲?
        if (Time.time - _endTime >= postDelayTime)
        {
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        base.OnExit();
        // ?몃━嫄곕뒗 Bool怨??щ━ 蹂꾨룄??false 泥섎━媛 ?꾩슂 ?놁쑝誘濡?援ъ“媛 ??源붾걫?⑸땲??
        if (runner != null && runner._stateController != null && _didSetLock)
        {
            runner._stateController.SetLockedTransitionAllowance(EnemyStateController.EnemyState.Attack, false);
            runner._stateController.SetLock(false);
        }
    }
    public override void Abort()
    {
        base.Abort();
        if (runner != null && runner._stateController != null && _didSetLock)
        {
            runner._stateController.SetLockedTransitionAllowance(EnemyStateController.EnemyState.Attack, false);
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
