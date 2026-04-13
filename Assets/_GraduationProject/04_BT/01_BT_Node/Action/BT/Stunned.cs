using UnityEngine;
using BehaviorTree;
using Pathfinding;
using System.Linq;

[CreateAssetMenu(fileName = "Stunned", menuName = "BehaviorTree/Stunned")]
public class Stunned : Node
{
    [Header("Attack Block Settings")]
    [Tooltip("스턴 종료 후 모든 공격 차단 활성화")]
    public bool enableAttackBlock = true;
    [Tooltip("스턴 종료 후 공격 차단 지속시간(초)")]
    public float attackBlockDuration = 1.0f;
    
    private int _enterFrame;
    string[] attackSkills;
    public override void initNode()
    {
        base.initNode();
        attackSkills = runner._aiController.enemyAttackDatas.Select(data => data.AttackName).ToArray();
    }


    public override void OnEnter()
    {
        base.OnEnter();
        _enterFrame = Time.frameCount;
                // ?ㅽ꽩 ?쒖옉 ?쒖젏遺??紐⑤뱺 怨듦꺽 李⑤떒 (?ъ슜???붿껌)
        if (enableAttackBlock)
        {
            BlockAllAttacks();
        }
            
        // 1. ?좊땲硫붿씠???좏샇 諛?怨듦꺽 ?곹깭 利됱떆 珥덇린??(?댁쟾 ?됰룞???붿긽 ?쒓굅)
        if (Handler != null) Handler.ResetAllFlags();
        
        if (runner._animationBridge != null)
        {
            runner._animationBridge.ResetAllAnimationStates(); // 紐⑤뱺 ?좊땲硫붿씠???곹깭 ?꾩쟾 珥덇린??
        }

        // 2. 吏꾩엯 ??臾쇰━ 愿???쒓굅
        Rigidbody rb = runner.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 3. ?대룞 ?뺤? 紐낅졊 (A* 紐⑹쟻吏 珥덇린???ы븿)
        runner.Movement.StopMovement();
        
        runner.SetState(EnemyStateController.EnemyState.Stunned);
        if(runner.Shield != null)
            runner.Shield.IsActive = false;
            

    }

    protected override NodeState OnUpdate()
    {
        // 理쒖냼 2?꾨젅??踰꾪띁: ?좊땲硫붿씠???곹깭 ?꾩씠 ?숆린???쒓컙 ?뺣낫
        if (Time.frameCount <= _enterFrame + 1) return NodeState.RUNNING;

        // ?덉텧 議곌굔: ?좊땲硫붿씠???대깽??FinishAction) 諛쒖깮 ??
        if (Handler.IsActionFinished && runner.ParrySystem._isStunned)
        {
            // 議곌린 ?뚮씪誘명꽣 ?뺣━
            runner.ParrySystem.ClearStun();
            return NodeState.SUCCESS;
        }

        if(!runner.ParrySystem._isStunned)
        {
            return NodeState.FAILURE;
        }
        else
        {
            // ?ㅽ꽩 以묒뿉??異붽??곸씤 ?대룞??李⑤떒?⑸땲??
            return NodeState.RUNNING;
        }
    }

    /// <summary>
    /// ?ㅽ꽩 ?쒖옉/醫낅즺 ?쒖젏??紐⑤뱺 Boss 怨듦꺽???쇱젙?쒓컙 李⑤떒
    /// </summary>
    private void BlockAllAttacks()
    {
        // WeakCounter Random Chance ?몃뱶 李⑤떒
        brain.StartSkillCooldown("WeakCounter", attackBlockDuration);
        
        // 紐⑤뱺 Boss 怨듦꺽 ?ㅽ궗 李⑤떒
        for(int i = 0; i < attackSkills.Length; i++)
        {
            string attackSkill = attackSkills[i];
            brain.StartSkillCooldown(attackSkill, attackBlockDuration);
        }
    }

    public override void OnExit()
    {
        // [?ъ슜???붿껌] ?ㅽ꽩 醫낅즺 ???덇린移??딄쾶 ?뺣━?섏? ?딆? ? ?몃뱶?ㅼ쓽 ?곹깭瑜?媛뺤젣 珥덇린??(Total Cleanup)
        
        // 1. ?ㅽ꽩 ?쒖뒪??醫낅즺 泥섎━
        runner.ParrySystem.ClearStun();
                // 1. ?좊땲硫붿씠???좏샇 諛?怨듦꺽 ?곹깭 利됱떆 珥덇린??(?댁쟾 ?됰룞???붿긽 ?쒓굅)
        if (Handler != null) Handler.ResetAllFlags();
        
        if (runner._animationBridge != null)
        {
            runner._animationBridge.ResetAllAnimationStates(); // 紐⑤뱺 ?좊땲硫붿씠???곹깭 ?꾩쟾 珥덇린??
        }
        
        // 2. 臾쇰━??愿??諛??붾쪟 ?띾룄 ?꾩쟾 ?뚭굅 (誘몃걚?ъ쭚 諛⑹?)
        Rigidbody rb = runner.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // 3. A* 寃쎈줈 諛?紐⑹쟻吏 ?곗씠???꾨꼍 ?뚭굅
        if (runner.aIPath != null)
        {
            runner.aIPath.SetPath(null);
            runner.aIPath.destination = runner.transform.position;
            runner.Movement.StopMovement();

            // [異붽?] ?ㅽ꽩 醫낅즺 ?쒖뿉??媛?띾룄瑜?Default濡?由ъ뀑
            if (runner.aIPath is AIPath aiPath)
            {
                aiPath.maxAcceleration = float.PositiveInfinity;
            }
        }

        // 4. ?꾩뿭 ?곹깭 ?좉툑(Lock) 諛?怨듦꺽 ?뚮옒洹?媛뺤젣 ?댁젣 (媛??以묒슂)
        if (runner._stateController != null)
        {
            runner._stateController.SetLock(false);
            runner._stateController.RecordStunEnd(); // 0.5珥??뚮났 吏???쒖옉
        }
        
        if (runner._animationBridge != null)
        {
            runner._animationBridge.ResetAllAnimationStates(); // ?ㅽ꽩 醫낅즺???좊땲硫붿씠???곹깭 ?꾩쟾 珥덇린??
        }

        // 5. 釉붾옓蹂대뱶 ?꾪닾 愿??蹂??珥덇린??
        brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, false);
        brain.blackboard.SetValue(EnemyBlackboardKeys.OnTakeHit, false);
        
        // 6. 遺媛 ?쒖뒪??蹂듦뎄 (?대뱶 ??
        if(runner.Shield != null)
            runner.Shield.IsActive = true;
            
        // 7. ?ㅽ꽩 醫낅즺 ??怨듦꺽 李⑤떒 (?ъ슜???붿껌 湲곕뒫)
        if (enableAttackBlock)
        {
            BlockAllAttacks();
        }
        
        
        runner.SetState(EnemyStateController.EnemyState.Idle);
        if (Handler != null) Handler.ResetAllFlags();
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}
