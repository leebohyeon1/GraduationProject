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
    [Tooltip("스턴 종료 후 공격 차단 지속시간 (초)")]
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
        Debug.Log("<color=red>--STUNNED--: OnEnter Triggered</color>");
        base.OnEnter();
        _enterFrame = Time.frameCount;
                // 스턴 시작 시점부터 모든 공격 차단 (사용자 요청)
        if (enableAttackBlock)
        {
            BlockAllAttacks();
        }
            
        // 1. 애니메이션 신호 및 공격 상태 즉시 초기화 (이전 행동의 잔상 제거)
        if (Handler != null) Handler.ResetAllFlags();
        
        if (runner._animationBridge != null)
        {
            runner._animationBridge.ResetAllAnimationStates(); // 모든 애니메이션 상태 완전 초기화
        }

        // 2. 진입 시 물리 관성 제거
        Rigidbody rb = runner.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 3. 이동 정지 명령 (A* 목적지 초기화 포함)
        runner.Movement.StopMovement();
        
        runner.SetState(EnemyStateController.EnemyState.Stunned);
        if(runner.Shield != null)
            runner.Shield.IsActive = false;
            

        // // Debug.Log("<color=red>--STUNNED--: OnEnter (Initial Cleanup Done)</color>");
    }

    protected override NodeState OnUpdate()
    {
        // 최소 2프레임 버퍼: 애니메이터 상태 전이 동기화 시간 확보
        if (Time.frameCount <= _enterFrame + 1) return NodeState.RUNNING;

        // 탈출 조건: 애니메이션 이벤트(FinishAction) 발생 시
        if (Handler.IsActionFinished && runner.ParrySystem._isStunned)
        {
            // 조기 파라미터 정리
            runner.ParrySystem.ClearStun();
            // // Debug.Log("<color=red>--STUNNED--: OnUpdate Finished (Signal Received)</color>");
            return NodeState.SUCCESS;
        }

        if(!runner.ParrySystem._isStunned)
        {
            return NodeState.FAILURE;
        }
        else
        {
            // 스턴 중에는 추가적인 이동을 차단합니다.
            return NodeState.RUNNING;
        }
    }

    /// <summary>
    /// 스턴 시작/종료 시점에 모든 Boss 공격을 일정시간 차단
    /// </summary>
    private void BlockAllAttacks()
    {
        // WeakCounter Random Chance 노드 차단
        brain.StartSkillCooldown("WeakCounter", attackBlockDuration);
        UnityEngine.Debug.Log($"[Stunned] WeakCounter 차단 시작 ({attackBlockDuration}초)");
        
        // 모든 Boss 공격 스킬 차단
        for(int i = 0; i < attackSkills.Length; i++)
        {
            string attackSkill = attackSkills[i];
            brain.StartSkillCooldown(attackSkill, attackBlockDuration);
            UnityEngine.Debug.Log($"[Stunned] {attackSkill} 차단 시작 ({attackBlockDuration}초)");
        }
    }

    public override void OnExit()
    {
        // [사용자 요청] 스턴 종료 시 예기치 않게 정리되지 않은 타 노드들의 상태를 강제 초기화 (Total Cleanup)
        
        // 1. 스턴 시스템 종료 처리
        runner.ParrySystem.ClearStun();
                // 1. 애니메이션 신호 및 공격 상태 즉시 초기화 (이전 행동의 잔상 제거)
        if (Handler != null) Handler.ResetAllFlags();
        
        if (runner._animationBridge != null)
        {
            runner._animationBridge.ResetAllAnimationStates(); // 모든 애니메이션 상태 완전 초기화
        }
        
        // 2. 물리적 관성 및 잔류 속도 완전 소거 (미끄러짐 방지)
        Rigidbody rb = runner.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // 3. A* 경로 및 목적지 데이터 완벽 소거
        if (runner.aIPath != null)
        {
            runner.aIPath.SetPath(null);
            runner.aIPath.destination = runner.transform.position;
            runner.Movement.StopMovement();

            // [추가] 스턴 종료 시에도 가속도를 Default로 리셋
            if (runner.aIPath is AIPath aiPath)
            {
                aiPath.maxAcceleration = float.PositiveInfinity;
            }
        }

        // 4. 전역 상태 잠금(Lock) 및 공격 플래그 강제 해제 (가장 중요)
        if (runner._stateController != null)
        {
            runner._stateController.SetLock(false);
            runner._stateController.RecordStunEnd(); // 0.5초 회복 지연 시작
        }
        
        if (runner._animationBridge != null)
        {
            runner._animationBridge.ResetAllAnimationStates(); // 스턴 종료시 애니메이션 상태 완전 초기화
        }

        // 5. 블랙보드 전투 관련 변수 초기화
        brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, false);
        brain.blackboard.SetValue(EnemyBlackboardKeys.OnTakeHit, false);
        
        // 6. 부가 시스템 복구 (쉴드 등)
        if(runner.Shield != null)
            runner.Shield.IsActive = true;
            
        // 7. 스턴 종료 후 공격 차단 (사용자 요청 기능)
        if (enableAttackBlock)
        {
            BlockAllAttacks();
        }
        
        // // Debug.Log("<color=red>--STUNNED EXIT--: Total State Cleanup Performed</color>");
        
        runner.SetState(EnemyStateController.EnemyState.Idle);
        if (Handler != null) Handler.ResetAllFlags();
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}
