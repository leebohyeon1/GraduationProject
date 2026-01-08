using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using System.Collections;
using Pathfinding;

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
    // bool isOtherAttacking = false; // [삭제] 불필요한 변수

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
        
        runner.SetCurrentAttackData(_data.damageRadius, _data.attackOffset);
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

        if (stateInfo.IsTag("Attack") && !stateInfo.IsName(_data.AttackName))
        {
            return NodeState.FAILURE; 
        }

        if (stateInfo.IsName(_data.AttackName))
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
            if (stateInfo.IsName(_data.AttackName))
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
            runner.SetState(Enemy.EnemyState.Attack);
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
            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, _data.damageRadius);
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
            // 애니메이션이 확실히 내 것일 때만 종료 처리 (선택 사항)
            if (stateInfo.IsName(_data.AttackName)) 
            {
                return _didHitPlayer ? NodeState.SUCCESS : NodeState.FAILURE;
            }
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        
        tracking = false;
        runner.ParrySystem.StateNormal();
        brain.blackboard.SetValue(ExceptKey, false);
        Handler.ResetAllFlags();
        runner.SetState(Enemy.EnemyState.Idle);
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
        runner.SetState(Enemy.EnemyState.Idle);
        brain.blackboard.SetValue(ExceptKey, false);
        runner.aIPath.enableRotation = true;

        if (!_parryEffectPlayed) _parryEffectPlayed = true;
        
        runner.SetStiffness(0);
        
        if (SO != null)
        {
            Handler.EndSO();
            SO.OnExit(runner);
        }
        
        var RVO = runner.GetComponent<Pathfinding.RVO.RVOController>();
        if (RVO != null)
        {
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
}