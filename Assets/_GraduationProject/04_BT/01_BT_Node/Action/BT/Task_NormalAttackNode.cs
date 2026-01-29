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
    [Tooltip("공격 성공 여부를 저장할 블랙보드 키 이름")]
    private EnemyAttackData _data;
    public EnemyUseAnything[] SO = null;
    bool _isCooldownDenied = false;
    public string ExceptKey = "IsAttacking";
    public bool LoopAttack = false;
    bool OtherAttackAnimationPlaying = false;
    public bool NextBT = false;
    private float _nodeEntryTime; 
    private const float TRANSITION_BUFFER = 1f;
    public override void OnEnter()
    {
        _nodeEntryTime = Time.time;
        _isCooldownDenied = false; // 진입 성공했으므로 false 확인
        // 쿨타임 체크
        // 데이터 로드
        if (!brain.blackboard.HasKey(attackKey) || !brain.blackboard.GetValue<EnemyAttackData>(attackKey, out _data))
        {
            Debug.LogError($"[Task_NormalAttackNode] '{attackKey}' 키를 블랙보드에서 찾을 수 없거나 데이터 타입이 맞지 않습니다.");
            return;
        }
        // if (!brain.IsSkillReady(attackKey, _data.Cooltime))
        // {
        //     Debug.LogWarning("Attack on Cooldown: " + attackKey);
        //     _isCooldownDenied = true;
        //     return;
        // }
        if (runner._animationBridge.IsAttacking)
        {
            Debug.LogWarning("attack act : " + this.name);
            OtherAttackAnimationPlaying = true;
            return;
        }
        // 스턴 체크
        if (runner.ParrySystem.CurrentState == ParrySystem.EnemyState.StunnedExit)
        {
            return;
        }

        Debug.Log($"[Task_NormalAttackNode] {runner.name}가 '{_data.AttackName}'{_isCooldownDenied} 공격을 시작합니다.");
        Handler.ResetAllFlags();
        // [변경] 블랙보드 초기화 (공격 시작 시 '명중 안 함'으로 설정)
        brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, false);
        _data.damageData.AttackerTransform = runner.transform;
        runner.AnimationEvent(_data.AttackName);
        runner.SetState(EnemyStateController.EnemyState.Attack);
        runner.SetCurrentAttackData(_data);
        runner.Movement.StopMovement();
        for (int i = 0; i < SO.Length; i++)
        {
            if (SO[i] != null)
            {
                SO[i].Reset(runner);
            }
        }
    }

    protected override NodeState OnUpdate()
    {
        if (_data == null || _isCooldownDenied || OtherAttackAnimationPlaying)
        {
            // OnEnter에서 초기화가 안 됐거나 쿨타임 중임
            return NodeState.FAILURE;
        }
        if(brain.blackboard.HasKey("GoHome") && brain.blackboard.GetValue<bool>("GoHome"))
        {
            return NodeState.FAILURE;
        }

        // 스턴 체크
        if (runner.ParrySystem.CurrentState == ParrySystem.EnemyState.StunnedExit)
        {
            Debug.LogWarning("Enemy is stunned, cannot perform attack.");
            return NodeState.FAILURE;
        }
        if (runner.ParrySystem.CurrentState == ParrySystem.EnemyState.Stunned)
        {
            Debug.LogWarning("Enemy is stunned, cannot perform attack.");
            return NodeState.FAILURE;
        }
        if (runner.animator.IsInTransition(0))
        {
            // 전환 중 발생하는 이벤트는 찌꺼기일 확률이 높으므로 무시 및 초기화
            Handler.ResetAllFlags();
        }


    // 1. 애니메이터 상태 상세 추출
    var stateInfo = runner.animator.GetCurrentAnimatorStateInfo(0);
    float elapsedTime = Time.time - _nodeEntryTime;

    // 현재 애니메이터가 'Attack' 상태로 전환되었는지 확인
    bool isTagValid = stateInfo.IsTag(_data.AttackName);

    // 0.4초(TRANSITION_BUFFER) 동안은 태그가 없어도 FAILURE를 내지 않고 기다림
    if (elapsedTime < TRANSITION_BUFFER)
    {
        return NodeState.RUNNING;
    }

    // 4. 애니메이션 태그 및 상태 체크
    if (!isTagValid && runner.CurrentState == EnemyStateController.EnemyState.Attack)
    {
        Debug.LogWarning($"[Task_NormalAttackNode] 공격 중단: 현재 태그 '{ stateInfo.IsTag(_data.AttackName)}' 태그가 아닙니다.");
        runner.animator.ResetTrigger(_data.AttackName);
        Debug.LogError($"[Task_NormalAttackNode] {runner.name} 공격 중단: '{_data.AttackName}' 태그를 찾을 수 없음.");
        return NodeState.FAILURE;
    }
    if (stateInfo.IsTag(_data.AttackName))
        {
            for (int i = 0; i < SO.Length; i++)
            {
                if (SO[i] != null)
                {
                    SO[i].OnUpdate(runner);
                }
            }

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
                for (int i = 0; i < SO.Length; i++)
                {
                    if (SO[i] != null)
                    {
                        SO[i].OnEnter(runner);
                    }
                }
            }
            Handler.EndSO();
        }

        if (Handler.IsSound)
        {
            Handler.EndSound();
        }
        if(!Handler.IsActive)
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
                    brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
                    
                    if (!maintainAtk)
                    {
                        Handler.CloseHitWindow();
                    }
                }
            }
        }
        if (stateInfo.IsTag(_data.AttackName))
        {
            bool hasHit = brain.blackboard.GetValue<bool>(EnemyBlackboardKeys.DidLastAttackHit);
            if (hasHit & LoopAttack)
            {
                for(int i = 0; i < SO.Length; i++)
                {
                    if (SO[i] != null)
                    {
                        SO[i].UseSomeThing(runner);
                    }
                }
            }
        }
        if (Handler.IsActionFinished)
        {
            if (NextBT)
            {
                return NodeState.SUCCESS;
            }
            bool hasHit = brain.blackboard.GetValue<bool>(EnemyBlackboardKeys.DidLastAttackHit);
            return hasHit ? NodeState.SUCCESS : NodeState.FAILURE;

        }

        return NodeState.RUNNING;
    }
    
    public override void OnExit()
    {
        runner.ParrySystem.StateNormal();
        brain.blackboard.SetValue(ExceptKey, false);
        Handler.ResetAllFlags();
        runner.SetState(EnemyStateController.EnemyState.Idle);
        runner.aIPath.enableRotation = true;
        runner.SetStiffness(0);
        Debug.Log($"[Task_NormalAttackNode] {runner.name}가 '{_data.AttackName}' 공격을 종료합니다.");
        for (int i = 0; i < SO.Length; i++)
        {
            if (SO[i] != null)
            {
                SO[i].OnExit(runner);
            }
        }
        Handler.EndSO(); // 혹시 켜져있으면 끄기
        Rigidbody rb = runner.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (!_isCooldownDenied)
        {
            Debug.Log($"[Task_NormalAttackNode] {runner.name}가 '{_data.AttackName}' 공격을 종료합니다. 쿨타임 시작.");
            brain.StartSkillCooldown(attackKey);
        }

    }

    public override void Abort()
    {

        // OnExit과 동일한 정리 로직 수행
        Handler.ResetAllFlags();
        runner.ParrySystem.StateNormal();
        runner.SetState(EnemyStateController.EnemyState.Idle);
        brain.blackboard.SetValue(ExceptKey, false);
        runner.aIPath.enableRotation = true;


        for (int i = 0; i < SO.Length; i++)
        {
            if (SO[i] != null)
            {
                SO[i].OnExit(runner);
            }
        }
        Handler.EndSO();
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
            RVO.locked = false;
            RVO.lockWhenNotMoving = true;
        }
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = this.attackKey;
        node.maintainAtk = this.maintainAtk;
        node.SO = this.SO;
        node.ExceptKey = this.ExceptKey;
        node.LoopAttack = this.LoopAttack;
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