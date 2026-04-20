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
        if (enableAttackBlock)
        {
            BlockAllAttacks();
        }
            
        if (Handler != null) Handler.ResetAllFlags();
        
        if (runner._animationBridge != null)
        {
            runner._animationBridge.ResetAllAnimationStates(); 
        }

        Rigidbody rb = runner.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        runner.Movement.StopMovement();
        
        runner.SetState(EnemyStateController.EnemyState.Stunned);
        if(runner.Shield != null)
            runner.Shield.IsActive = false;
            

    }

    protected override NodeState OnUpdate()
    {
        if (Time.frameCount <= _enterFrame + 1) return NodeState.RUNNING;

        if (Handler.IsActionFinished && runner.ParrySystem._isStunned)
        {
            runner.ParrySystem.ClearStun();
            return NodeState.SUCCESS;
        }

        if(!runner.ParrySystem._isStunned)
        {
            return NodeState.FAILURE;
        }
        else
        {
            return NodeState.RUNNING;
        }
    }

    private void BlockAllAttacks()
    {
        // WeakCounter Random Chance ?몃뱶 李⑤떒
        brain.StartSkillCooldown("WeakCounter", attackBlockDuration);
        
        for(int i = 0; i < attackSkills.Length; i++)
        {
            string attackSkill = attackSkills[i];
            brain.StartSkillCooldown(attackSkill, attackBlockDuration);
        }
    }

    public override void OnExit()
    {
        
        runner.ParrySystem.ClearStun();
        if (Handler != null) Handler.ResetAllFlags();
        
        if (runner._animationBridge != null)
        {
            runner._animationBridge.ResetAllAnimationStates();
        }
        
        Rigidbody rb = runner.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        if (runner.aIPath != null)
        {
            runner.aIPath.SetPath(null);
            runner.aIPath.destination = runner.transform.position;
            runner.Movement.StopMovement();
            if (runner.aIPath is AIPath aiPath)
            {
                aiPath.maxAcceleration = float.PositiveInfinity;
            }
        }

        if (runner._stateController != null)
        {
            runner._stateController.SetLock(false);
            runner._stateController.RecordStunEnd(); 
        }
        
        if (runner._animationBridge != null)
        {
            runner._animationBridge.ResetAllAnimationStates();
        }

        brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, false);
        brain.blackboard.SetValue(EnemyBlackboardKeys.OnTakeHit, false);
        
        if(runner.Shield != null)
            runner.Shield.IsActive = true;
            
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
