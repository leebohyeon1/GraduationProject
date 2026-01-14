using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using System.Collections;
using Pathfinding;
using UnityEditorInternal;

public class Task_NormalAttackNode : Node
{
    [Header("Attack Properties")]
    public string attackKey;
    public bool maintainAtk;
    private bool _didHitPlayer;
    private EnemyAttackData _data;
    bool tracking = false;
    bool _parryEffectPlayed = false;
    public EnemyUseAnything SO = null;
    bool _isCooldownDenied = false;
    public string ExceptKey = "IsAttacking";

    public override void OnEnter()
    {
        
        // 데이터 로드
        if (!brain.blackboard.GetValue<EnemyAttackData>(attackKey, out _data))
        {
            Debug.LogError("No Attack Data Found for key: " + attackKey);
            return;
        }

        // 스턴 체크
        if (runner.ParrySystem.CurrentState == ParrySystem.EnemyState.StunnedExit)
        {
            return;
        }

        // 쿨타임 체크
        if (!brain.IsSkillReady(attackKey, _data.Cooltime))
        {
            _isCooldownDenied = true;
            return;
        }

        Handler.ResetAllFlags();
        _didHitPlayer = false;
        _parryEffectPlayed = false;
        _isCooldownDenied = false; // 진입 성공했으므로 false 확인

        _data.damageData.AttackerTransform = runner.transform;
        
        runner.AnimationEvent(_data.AttackName);
        
        runner.SetCurrentAttackData(_data);
        runner.SetStiffness(_data.damageData.StiffnessAmount);
        runner.Movement.StopMovement();
    }

    protected override NodeState OnUpdate()
    {
        if (_data == null || _isCooldownDenied)
        {
            // OnEnter에서 초기화가 안 됐거나 쿨타임 중임
            return NodeState.FAILURE;
        }

        // 스턴 체크
        if (runner.ParrySystem.CurrentState == ParrySystem.EnemyState.StunnedExit)
        {
            return NodeState.FAILURE;
        }

        if (runner.animator.IsInTransition(0))
        {
            // 전환 중 발생하는 이벤트는 찌꺼기일 확률이 높으므로 무시 및 초기화
            Handler.ResetAllFlags(); 
            return NodeState.RUNNING;
        }

        var stateInfo = runner.animator.GetCurrentAnimatorStateInfo(0);
        // if(!stateInfo.IsTag("Attack") )
        // {
        //     Debug.LogWarning($"[Task_NormalAttackNode] 현재 애니메이션이 공격 태그가 아닙니다: {stateInfo.fullPathHash}");
        //     return NodeState.FAILURE; 
        // }
        if (!stateInfo.IsTag(_data.AttackName) && runner.CurrentState == EnemyStateController.EnemyState.Attack)
        {
            runner.animator.ResetTrigger(_data.AttackName);
            Debug.LogWarning($"[Task_NormalAttackNode] 현재 애니메이션이 지정된 공격이 아닙니다: {stateInfo.fullPathHash}");
            return NodeState.FAILURE; 
        }

        if (stateInfo.IsTag(_data.AttackName))
        {
            if (SO != null) SO.OnUpdate(runner);
        }

        // 회전 및 추적 로직
        Vector3 attackOrigin = runner.transform.position + runner.transform.TransformDirection(_data.attackOffset);
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;

        if (Handler.IsActionSO)
        {
            // 현재 재생 중인 애니메이션이 내 공격이 맞는지 재확인
            if (stateInfo.IsTag(_data.AttackName))
            {
                if (SO != null)
                {
                    SO.OnEnter(runner);
                }
            }
            Handler.EndSO(); 
        }

        if (Handler.IsSound)
        {
            Handler.EndSound();
        }

        // 추적 상태 (IsActive)
        if (Handler.IsActive)
        {
            tracking = true;
            runner.SetState(EnemyStateController.EnemyState.Attack);
        }
        else
        {
            tracking = false;
        }

        if (!tracking)
        {
            runner.transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }

        if (Handler.IsHitWindowOpen)
        {
            
            Collider[] hitColliders = GetHitColliders(attackOrigin);
            brain.blackboard.SetValue(ExceptKey, true);
            
            foreach (var col in hitColliders)
            {
                if (col.gameObject == runner.gameObject) continue;
                if (col.TryGetComponent<PlayerHealth>(out PlayerHealth Character))
                {
                    Character.TakeDamage(_data.damageData);
                    _didHitPlayer = true;
                    if (!maintainAtk)
                    {
                        Handler.CloseHitWindow();
                    }
                }
            }
        }

        if (Handler.IsHitWindowOpen && !_parryEffectPlayed)
        {
            Handler.CloseHitWindow();
            _parryEffectPlayed = true;
        }

        if (Handler.IsActionFinished)
        {
                return _didHitPlayer ? NodeState.SUCCESS : NodeState.FAILURE;
            
        }

        return NodeState.RUNNING;
    }
    public override void OnExit()
    {
        Debug.Log($"[Task_NormalAttackNode] OnExit: {this.name}");
        tracking = false;
        runner.ParrySystem.StateNormal();
        brain.blackboard.SetValue(ExceptKey, false);
        Handler.ResetAllFlags();
        runner.SetState(EnemyStateController.EnemyState.Idle);
        runner.aIPath.enableRotation = true;
        runner.SetStiffness(0);

        if (SO != null)
        {
            SO.OnExit(runner);
            Handler.EndSO(); // 혹시 켜져있으면 끄기
        }
        
        if (!_isCooldownDenied)
        {
            brain.StartSkillCooldown(attackKey);
        }
        runner.ParrySystem.DeactivateImmunity();
        
    }

    public override void Abort()
    {
        Debug.Log($"[Task_NormalAttackNode] Abort: {this.name}");
        
        // OnExit과 동일한 정리 로직 수행
        tracking = false;
        Handler.ResetAllFlags();
        runner.ParrySystem.StateNormal();
        runner.SetState(EnemyStateController.EnemyState.Idle);
        brain.blackboard.SetValue(ExceptKey, false);
        runner.aIPath.enableRotation = true;

        if (!_parryEffectPlayed) _parryEffectPlayed = true;
        
        runner.SetStiffness(0);
        
        if (SO != null)
        {
            SO.OnExit(runner);
            Handler.EndSO();
        }
        Rigidbody rb = runner.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.Teleport(runner.transform.position);
            ai.canMove = true;      
            ai.isStopped = false;    
            ai.maxSpeed = runner.Movement._normalSpeed; 
            ai.destination = runner.transform.position;
            if (ai is AIPath aiPath) aiPath.enableRotation = true;
        }
        var RVO = runner.GetComponent<Pathfinding.RVO.RVOController>();
        if (RVO != null)
        {
            Debug.Log("[Task_NormalAttackNode] Abort: RVOController found, unlocking RVO.");
            RVO.locked = false;
            RVO.lockWhenNotMoving = true;
        }
        runner.ParrySystem.DeactivateImmunity();
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = this.attackKey;
        node.maintainAtk = this.maintainAtk;
        // SO는 ScriptableObject라 공유되어도 되지만, 필요하다면 복제
        // node.SO = this.SO; 
        return node;
    }
private Collider[] GetHitColliders(Vector3 origin)
    {
        List<Collider> validHits = new List<Collider>();
        Collider[] rawHits = null;

        switch (_data.shape)
        {
            case AttackShape.Sphere:
                return Physics.OverlapSphere(origin, _data.damageRadius);

            case AttackShape.Box:
                // OverlapBox는 HalfExtents(사이즈의 절반)를 받습니다.
                // 회전은 공격자(runner)의 회전을 따라갑니다.
                return Physics.OverlapBox(origin, _data.boxSize * 0.5f, runner.transform.rotation);

            case AttackShape.Fan:
                // 1차로 구체 범위 내의 적을 모두 찾습니다.
                rawHits = Physics.OverlapSphere(origin, _data.damageRadius);
                
                foreach (var col in rawHits)
                {
                    // 각도 계산을 위해 적의 방향 벡터를 구합니다.
                    Vector3 directionToTarget = (col.transform.position - origin).normalized;
                    
                    // 내 정면(transform.forward)과 적 방향 사이의 각도를 구합니다.
                    // (높이차 무시를 위해 y를 0으로 할 수도 있음)
                    float angleToTarget = Vector3.Angle(runner.transform.forward, directionToTarget);

                    // 설정한 각도의 절반 이내에 있다면 범위 안입니다.
                    if (angleToTarget <= _data.fanAngle * 0.5f)
                    {
                        validHits.Add(col);
                    }
                }
                return validHits.ToArray();
            
            default:
                return new Collider[0];
        }
    }
}