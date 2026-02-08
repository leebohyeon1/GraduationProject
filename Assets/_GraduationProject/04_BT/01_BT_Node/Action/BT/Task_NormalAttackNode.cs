using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using System.Collections;
using Pathfinding;

public class Task_NormalAttackNode : Node
{
    [Header("Attack Properties")]
    [Tooltip("블랙보드에서 불러올 EnemyAttackData의 키 이름")]
    public string attackKey;

    [Tooltip("애니메이션 상태 이름 (태그가 없을 때 사용됨, 비워두면 자동 감지)")]
    public string animationStateName = "";

    [Tooltip("OnActionTriggered() 호출 여부 (이동공격/특수한 공격에서 사용)")]
    public bool useActionTriggered = false;

    [Tooltip("공격 성공 시 히트 윈도우를 닫지 않음")]
    public bool maintainAtk;

    [Tooltip("추가 공격 효과 (EnemyUseAnything 스크립터블 오브젝트)")]
    public EnemyUseAnything[] SO = null;

    [Tooltip("공격 중일 때 설정할 블랙보드 키 이름")]
    public string ExceptKey = "IsAttacking";

    [Tooltip("히트 시 LoopAttack 효과 실행 여부")]
    public bool LoopAttack = false;

    [Tooltip("공격 종료 시 항상 SUCCESS 반환 여부")]
    public bool NextBT = false;

    [Header("Timing Settings")]
    [Range(0.1f, 5f)]
    [Tooltip("애니메이션 태그 전환 대기 시간 (초)")]
    public float transitionBuffer = 1f;

    [Header("Debug Settings")]
    [Tooltip("디버그 모드: 상세 로그 출력")]
    public bool debugMode = false;

    private EnemyAttackData _data;
    private bool _isCooldownDenied = false;
    private bool OtherAttackAnimationPlaying = false;
    private float _nodeEntryTime;
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
        if (runner._animationBridge.IsAttacking)
        {
            // Debug.LogWarning("attack act : " + this.name);
            OtherAttackAnimationPlaying = true;
            return;
        }
        // 스턴 체크
        if (runner.ParrySystem.CurrentState == ParrySystem.EnemyState.StunnedExit)
        {
            return;
        }

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
                SO[i].OnActionTriggered(runner);
            }
        }
    }

    protected override NodeState OnUpdate()
    {
        if (!ValidateState()) return NodeState.FAILURE;

        var stateInfo = runner.animator.GetCurrentAnimatorStateInfo(0);
        var nextStateInfo = runner.animator.GetNextAnimatorStateInfo(0);
        float elapsedTime = Time.time - _nodeEntryTime;

        bool isTagValid = stateInfo.IsTag(_data.AttackName) || nextStateInfo.IsTag(_data.AttackName);

        if (!isTagValid)
        {
            if (elapsedTime < transitionBuffer)
            {
                return NodeState.RUNNING;
            }
            return NodeState.FAILURE;
        }

        if (runner.animator.IsInTransition(0))
        {
            Handler.ResetAllFlags();
        }

        HandleSOUpdates(stateInfo);
        HandleRotation();
        HandleHitDetection();

        if (stateInfo.IsTag(_data.AttackName))
        {
            HandleLoopAttackLogic(stateInfo);
        }

        return CheckActionFinished();
    }

    private bool ValidateState()
    {
        if (_data == null || _isCooldownDenied || OtherAttackAnimationPlaying)
        {
            return false;
        }

        if (brain.blackboard.HasKey("GoHome") && brain.blackboard.GetValue<bool>("GoHome"))
        {
            return false;
        }

        if (runner.ParrySystem.CurrentState == ParrySystem.EnemyState.StunnedExit)
        {
            Debug.LogWarning("Enemy is stunned, cannot perform attack.");
            return false;
        }

        if (runner.ParrySystem.CurrentState == ParrySystem.EnemyState.Stunned)
        {
            Debug.LogWarning("Enemy is stunned, cannot perform attack.");
            return false;
        }

        return true;
    }

    private void HandleSOUpdates(AnimatorStateInfo stateInfo)
    {
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

        if (Handler.IsActionSO)
        {
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
    }

    private void HandleRotation()
    {
        if (!Handler.IsActive)
        {
            Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
            directionToPlayer.y = 0;
            runner.transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
    }

    private void HandleHitDetection()
    {
        if (!Handler.IsHitWindowOpen) return;

        Vector3 attackOrigin = runner.transform.position + runner.transform.TransformDirection(_data.attackOffset);
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

    private void HandleLoopAttackLogic(AnimatorStateInfo stateInfo)
    {
        bool hasHit = brain.blackboard.GetValue<bool>(EnemyBlackboardKeys.DidLastAttackHit);

        if (hasHit && LoopAttack)
        {
            for (int i = 0; i < SO.Length; i++)
            {
                if (SO[i] != null)
                {
                    SO[i].UseSomeThing(runner);
                }
            }
        }
    }

    private NodeState CheckActionFinished()
    {
        if (!Handler.IsActionFinished) return NodeState.RUNNING;

        if (NextBT)
        {
            return NodeState.SUCCESS;
        }

        bool hasHit = brain.blackboard.GetValue<bool>(EnemyBlackboardKeys.DidLastAttackHit);
        return hasHit ? NodeState.SUCCESS : NodeState.FAILURE;
    }
    
    public override void OnExit()
    {
        runner.ParrySystem.StateNormal();
        brain.blackboard.SetValue(ExceptKey, false);
        Handler.ResetAllFlags();
        runner.SetState(EnemyStateController.EnemyState.Idle);
        runner.aIPath.enableRotation = true;
        runner.SetStiffness(0);
        // Debug.Log($"[Task_NormalAttackNode] {runner.name}가 '{_data.AttackName}' 공격을 종료합니다.");
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
            // Debug.Log($"[Task_NormalAttackNode] {runner.name}가 '{_data.AttackName}' 공격을 종료합니다. 쿨타임 시작.");
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
        node.animationStateName = this.animationStateName;
        node.maintainAtk = this.maintainAtk;
        node.SO = this.SO;
        node.ExceptKey = this.ExceptKey;
        node.LoopAttack = this.LoopAttack;
        node.NextBT = this.NextBT;
        node.transitionBuffer = this.transitionBuffer;
        node.debugMode = this.debugMode;
        node.useActionTriggered = this.useActionTriggered;
        // SO는 ScriptableObject라 공유되어도 되지만, 필요하다면 복제
        // node.SO = this.SO;
        return node;
    }
    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(attackKey))
        {
            Debug.LogWarning("[Task_NormalAttackNode] AttackKey가 설정되지 않았습니다. 블랙보드에서 공격 데이터를 찾을 수 없습니다.", this);
            return;
        }
    }
#endif

    private Collider[] GetHitColliders(Vector3 origin)
    {
        List<Collider> validHits = new List<Collider>();
        Collider[] rawHits = null;

        switch (_data.shape)
        {
            case AttackShape.Sphere:
                return Physics.OverlapSphere(origin, _data.damageRadius);

            case AttackShape.Box:
                return Physics.OverlapBox(origin, _data.boxSize * 0.5f, runner.transform.rotation);

            case AttackShape.Fan:
                rawHits = Physics.OverlapSphere(origin, _data.damageRadius);

                foreach (var col in rawHits)
                {
                    Vector3 directionToTarget = (col.transform.position - origin).normalized;
                    float angleToTarget = Vector3.Angle(runner.transform.forward, directionToTarget);

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