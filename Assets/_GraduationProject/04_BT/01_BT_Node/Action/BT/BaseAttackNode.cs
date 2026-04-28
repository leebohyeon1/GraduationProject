using UnityEngine;
using BehaviorTree;
using Pathfinding;
using System;

/// <summary>
/// 怨듦꺽 ?몃뱶??踰좎씠???대옒?? Physics.NonAlloc???ъ슜?섏뿬 GC ?좊떦??諛⑹??⑸땲??
/// </summary>
public abstract class BaseAttackNode : Node
{
    [Header("Base Attack Properties")]
    [Tooltip("이 노드가 사용할 블랙보드 공격 데이터 키")]
    public string attackKey;
    [Tooltip("공격에 사용할 애니메이션 상태 태그/트리거 이름")]
    public string animationStateName = "";
    [Tooltip("태그 진입 대기 유예 시간")]
    public float transitionBuffer = 1f;
    [Tooltip("노드 최대 실행 시간(초)")]
    public float maxNodeDuration = 6.0f;
    [Tooltip("히트 확인 전까지 공격 상태 유지")]
    public EnemyUseAnything[] SO = null;
    [Tooltip("루프 공격 사용")]
    public bool LoopAttack = false;
    [Tooltip("즉시 성공 처리 후 다음 BT 분기 이동")]
    public bool NextBT = false;
    public float ExitDelay = 0f;
    [Tooltip("노드 내부 디버그 로그 사용 여부")]
    public bool debugMode = false;

    [Header("Escape Settings")]
    [Tooltip("히트 확인 시 조기 종료 허용")]
    public bool escapeOnHitConfirm = true;
    [Tooltip("히트 확인 후 종료까지 지연 시간(초)")]
    public float hitEscapeDelay = 0.5f;

    [Header("Execution Gate")]
    [Tooltip("공격 진입 전 거리 체크")]
    public bool checkRangeOnEnter = false;
    [Tooltip("거리 체크 여유값")]
    public float rangeThreshold = 1.0f;
    [Tooltip("거리 체크 시 Y축 무시")]
    public bool ignoreYDistance = true;
    [Tooltip("비전투 상태에서도 실행 허용")]
    public bool allowOutOfCombat = false;

    [Header("State Control")]
    [Tooltip("공격 상태 잠금 블랙보드 키")]
    public string ExceptKey = "IsAttacking";

    [Header("Hit Detection")]
    [Tooltip("히트 판정 레이어 마스크")]
    [SerializeField] private LayerMask _hitMasks = 1<<7;


    [Header("Speed Scaling Settings")]
    [SerializeField] private bool ChangeSpeed = false;
    [Tooltip("애니메이션 속도가 증가하기 시작하는 체력 비율 (0.5 = 50%)")]
    [SerializeField] private float _startHealthThreshold = 0.5f;
    
    [Tooltip("최대 애니메이션 속도에 도달하는 체력 비율 (0.1 = 10%)")]
    [SerializeField] private float _maxSpeedThreshold = 0.1f;
    
    [Tooltip("도달할 수 있는 최대 애니메이션 속도")]
    [SerializeField] private float _maxAnimationSpeed = 2.0f;


    protected EnemyAttackData _data;
    protected float _nodeEntryTime;
    protected int _entryFrame; 
    protected bool _isActionFinishedInternally;
    protected bool _hasTriggeredLoop;
    protected string _validationTag; 
    private float _originalStepOffset;
    private bool _hasSeenTag;
    private float _hitConfirmTime = -1f;
    private bool _didSetLock = false; 
    private bool _wasInTransition = false; 
     private bool _hasHaltedMovement = false;
    protected bool _wasParriedDuringAttack = false; 
    protected bool _wasStunnedDuringAttack = false;

    private static readonly Collider[] _hitBuffer = new Collider[16];

    public sealed override void OnEnter()
    {
        _nodeEntryTime = Time.time;
        _entryFrame = Time.frameCount; 
        _isActionFinishedInternally = false;
        _hasTriggeredLoop = false;
        _hasSeenTag = false;
        _hitConfirmTime = -1f;
        _didSetLock = false;
         _wasInTransition = false;
        _hasHaltedMovement = false;

        if (!brain.blackboard.GetValue<EnemyAttackData>(attackKey, out _data))
        {
            _isActionFinishedInternally = true;
            return;
        }

        bool isAlreadyInAttackState = runner.CurrentState == EnemyStateController.EnemyState.Attack || runner._animationBridge.IsAttacking;
        if (runner._stateController.IsStateLocked || isAlreadyInAttackState || runner.CurrentState == EnemyStateController.EnemyState.Stunned)
        {
            _isActionFinishedInternally = true;
            return;
        }

        if (!CanExecuteInternal())
        {
            _isActionFinishedInternally = true;
            return;
        }

        _validationTag = _data.AttackName; 
        if (Handler != null) Handler.ResetAllFlags();
        
        AttackOutcomeRecorder.ResetAttackHit(brain.blackboard);
        brain.blackboard.SetValue(ExceptKey, true);
        runner.AnimationBool("IsRushing", false);
        _data.damageData.AttackerTransform = runner.transform;
        runner.AnimationEvent(_data.AttackName);
        runner.SetState(EnemyStateController.EnemyState.Attack);
        runner.SetCurrentAttackData(_data);
        runner._stateController.SetLock(true);
        _didSetLock = true;

        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null) { ai.canMove = false; ai.isStopped = true; }

        foreach (var s in SO) if (s != null) { s.Reset(runner); s.OnEnter(runner); s.OnActionTriggered(runner); }

        CharacterController cc = runner.GetComponent<CharacterController>();
        if (cc != null) { _originalStepOffset = cc.stepOffset; cc.stepOffset = 0f; }

        InitialMovementSetup();
        SpeedUp();
    }
    private void SpeedRecovery()
    {
        if (!ChangeSpeed) return;

        runner.animator.speed = 1.0f;
        if (Handler != null)
        {
            Handler.SpeedMultiplier = 1.0f;
        }
    }
    private void SpeedUp()
    {
        if (!ChangeSpeed) return;

        float targetSpeed = 1.0f;

        // 현재 체력 비율 계산
        float healthRatio = (float)runner.EnemyHealth.CurrentHealth / runner.EnemyHealth.MaxHealth;

        // 공격 관련 상태인지 확인
        if (runner.CurrentState == EnemyStateController.EnemyState.Attack)
        {
            
            // 체력이 임계치 이하일 때 속도 계산
            if (healthRatio <= _startHealthThreshold)
            {
                float t = Mathf.InverseLerp(_startHealthThreshold, _maxSpeedThreshold, healthRatio);
                targetSpeed = Mathf.Lerp(1.0f, _maxAnimationSpeed, t);
            }
        }

        // 애니메이터 속도 적용
        runner.animator.speed = targetSpeed;

        // 이펙트(피드백) 핸들러 속도 적용
        if (Handler != null)
        {
            Handler.SpeedMultiplier = targetSpeed;
        }
    }
    protected sealed override NodeState OnUpdate()
    {
        if (_isActionFinishedInternally) return NodeState.FAILURE;
        if (_data == null || runner.animator == null) return NodeState.FAILURE;
        if (runner.CurrentState != EnemyStateController.EnemyState.Attack) return NodeState.FAILURE;

        var stateInfo = runner.animator.GetCurrentAnimatorStateInfo(0);
        var nextStateInfo = runner.animator.GetNextAnimatorStateInfo(0);
        float elapsedTime = Time.time - _nodeEntryTime;

        bool isTagActive = stateInfo.IsTag(_validationTag) || nextStateInfo.IsTag(_validationTag);
        if (isTagActive && !_hasSeenTag) { _hasSeenTag = true; if (Handler != null) Handler.ResetAllFlags(); }

        if (LoopAttack && escapeOnHitConfirm && _hitConfirmTime > 0)
        {
            if (Time.time - _hitConfirmTime >= hitEscapeDelay && !brain.blackboard.GetValueOrDefault<bool>(LoopAction.EndKey, false))
            {
                runner.AnimationBool("IsRushing", true);
                brain.blackboard.SetValue(LoopAction.EndKey, true);
            }
        }

        if (Time.frameCount > _entryFrame + 1)
        {
            NodeState finishState = CheckActionFinished(elapsedTime);
            if (finishState != NodeState.RUNNING) return finishState;
        }

        if (!isTagActive) return elapsedTime > transitionBuffer ? NodeState.FAILURE : NodeState.RUNNING;

        bool isInTransition = runner.animator.IsInTransition(0);
        if (isInTransition && !_wasInTransition) 
        _wasInTransition = isInTransition;

        HandleCommonSystems(stateInfo, nextStateInfo);

        bool isLoopEnded = (LoopAttack && _hasTriggeredLoop && brain.blackboard.GetValueOrDefault<bool>(LoopAction.EndKey, false));
        if (!IsMovementFinished && !isLoopEnded) { _hasHaltedMovement = false; UpdateMovement(); }
        else if (!_hasHaltedMovement) { _hasHaltedMovement = true; HaltMovement(); }

        return NodeState.RUNNING;
    }

    private void HaltMovement()
    {
        Rigidbody rb = runner.GetComponent<Rigidbody>();
        if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null) { ai.isStopped = true; ai.maxSpeed = 0; }
    }

    public sealed override void OnExit()
    {
        if(ExitDelay > 0f)
        {
            runner.StartCoroutine(WaitAndExit(ExitDelay));
        }
        else
        {
            PerformExit();
        }
    }

    private System.Collections.IEnumerator WaitAndExit(float exitDelay)
    {
        yield return new UnityEngine.WaitForSeconds(exitDelay);
        PerformExit();
    }

    private void PerformExit()
    {
        // Debug.Log($"[Attack Node Exit] {runner.name} exited attack node for {_data.AttackName}");
        runner._aiController._aiBrain.StartSkillCooldown(attackKey);
        CleanupAllStates();
        SpeedRecovery();
    }

    public sealed override void Abort() { if (isEntered) { CleanupAllStates(); isEntered = false; } }

    private void CleanupAllStates()
    {
        SpecificCleanup();
        SpeedRecovery();

        if (_didSetLock && runner._stateController != null) { runner._stateController.SetLock(false); _didSetLock = false; }
        if (runner._animationBridge != null) runner._animationBridge.ClearIsAttacking();
        runner.ParrySystem.StateNormal();
        brain.blackboard.SetValue(ExceptKey, false);
        if (Handler != null) Handler.ResetAllFlags();
        if (runner.CurrentState == EnemyStateController.EnemyState.Attack) runner.SetState(EnemyStateController.EnemyState.Idle);
        
        StopMovementInternal(); 
        
        foreach (var s in SO) if (s != null) s.OnExit(runner);
    }

    private bool CanExecuteInternal()
    {
        if (!allowOutOfCombat && !brain._isCombat) return false;
        if (!brain.IsSkillReady(attackKey, _data.Cooltime)) return false;
        if (runner.CurrentState == EnemyStateController.EnemyState.Stunned) return false;
        if (checkRangeOnEnter)
        {
            float dist = Vector3.Distance(runner.transform.position, runner.player.transform.position);
            float range = GetRequiredRange();
            if (dist > range + rangeThreshold) return false;
        }
        return CheckCustomPreconditions();
    }

    protected virtual float GetRequiredRange() => _data != null ? _data.damageRadius : 2.0f;
    protected virtual bool CheckCustomPreconditions() => true;
    protected abstract void InitialMovementSetup();
    protected abstract void UpdateMovement();
    protected abstract bool IsMovementFinished { get; }
    protected virtual void SpecificCleanup() 
    {
        // ?뺤긽?곸씤 怨듦꺽 ?꾨즺 ???쇰컲 荑⑦????곸슜
        if (!_isActionFinishedInternally)
        {
            brain.StartSkillCooldown(attackKey);
        }
    }

    private void HandleCommonSystems(AnimatorStateInfo stateInfo, AnimatorStateInfo nextStateInfo)
    {
        if (Handler != null && Handler.IsActionSO && (stateInfo.IsTag(_validationTag) || nextStateInfo.IsTag(_validationTag)))
        {
            _hasHaltedMovement = false;
            OnActionSOTriggered();
            foreach (var s in SO) if (s != null) s.OnEnter(runner);
            Handler.EndSO();
        }
        if (stateInfo.IsTag(_validationTag)) HandleLoopAttackLogic();
        HandleRotation();
        HandleHitDetection();
    }

    protected virtual void OnActionSOTriggered() { }

    private void HandleRotation()
    {
        if (!Handler.IsActive && runner.player != null)
        {
            Debug.Log("Rotating towards player at attack moment.");
            Vector3 dir = runner.player.transform.position - runner.transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f) runner.transform.rotation = Quaternion.LookRotation(dir);
        }
    }
    private void HandleHitDetection()
    {
        if (Handler == null || !Handler.IsHitWindowOpen) return;
        var hitResult = AttackHitResolver.ResolveFirstPlayerHit(runner, _data, _hitMasks, _hitBuffer);
        if (!hitResult.DidHit || hitResult.Target == null)
        {
            return;
        }

        Handler.CloseHitWindow();
        _data.damageData.AttackerTransform = runner.transform;
        hitResult.Target.TakeDamage(_data.damageData);
        AttackOutcomeRecorder.RecordSuccessfulHit(brain.blackboard);
        if (LoopAttack && _hitConfirmTime < 0) _hitConfirmTime = Time.time;
    }

    private void HandleLoopAttackLogic()
    {
        if (brain.blackboard.GetValueOrDefault<bool>(EnemyBlackboardKeys.DidLastAttackHit, false) && LoopAttack && !_hasTriggeredLoop)
        {
            _hasTriggeredLoop = true;
            foreach (var s in SO) if (s != null) s.UseSomeThing(runner);
        }
    }

    private NodeState CheckActionFinished(float elapsedTime)
    {
        if (!_hasSeenTag && elapsedTime < transitionBuffer + 0.3f) return NodeState.RUNNING;
        bool isTimedOut = elapsedTime >= maxNodeDuration; 
        if (LoopAttack && _hasTriggeredLoop && !brain.blackboard.GetValueOrDefault<bool>(LoopAction.EndKey, false) && !isTimedOut) return NodeState.RUNNING;

        if ((Handler != null && Handler.IsActionFinished) || isTimedOut)
        {
            if (NextBT) return NodeState.SUCCESS;
            
            bool didHit = brain.blackboard.GetValueOrDefault<bool>(EnemyBlackboardKeys.DidLastAttackHit, false);
            
            // Only check parry status for Boss_Fake_Attack
            if (_wasParriedDuringAttack && _data != null && _data.AttackName == "Boss_Fake_Attack")
            {
                return NodeState.FAILURE;
            }
            
            return NodeState.SUCCESS;
        }
        return NodeState.RUNNING;
    }

    protected void StopMovementInternal()
    {
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null) 
        { 
            ai.canMove = true; 
            ai.isStopped = false; 
            ai.maxSpeed = runner.Movement._normalSpeed; 
        }
        CharacterController cc = runner.GetComponent<CharacterController>();
        if (cc != null) cc.stepOffset = _originalStepOffset;
        if (runner.aIPath != null) runner.aIPath.enableRotation = true;
    }


}

/// <summary>
/// 공격 결과를 블랙보드 규약에 맞게 기록하는 단일 진입점입니다.
/// </summary>
public static class AttackOutcomeRecorder
{
    public static void ResetAttackHit(BlackBoard blackboard)
    {
        if (blackboard == null)
        {
            return;
        }

        blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, false);
    }

    public static void RecordSuccessfulHit(BlackBoard blackboard)
    {
        if (blackboard == null)
        {
            return;
        }

        blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
        blackboard.SetValue(EnemyBlackboardKeys.LastAttackSuccessTime, Time.time);
    }
}

/// <summary>
/// 공격 히트 판정 책임을 BaseAttackNode 밖으로 분리한 해석기입니다.
/// </summary>
public static class AttackHitResolver
{
    public readonly struct AttackHitResult
    {
        public bool DidHit { get; }
        public PlayerHealth Target { get; }

        public AttackHitResult(PlayerHealth target)
        {
            Target = target;
            DidHit = target != null;
        }
    }

    public static AttackHitResult ResolveFirstPlayerHit(Enemy runner, EnemyAttackData attackData, LayerMask hitMasks, Collider[] hitBuffer)
    {
        if (runner == null || attackData == null || hitBuffer == null || hitBuffer.Length == 0)
        {
            return default;
        }

        Vector3 origin = runner.transform.position + runner.transform.TransformDirection(attackData.attackOffset);
        LayerMask hitMask = hitMasks.value != 0 ? hitMasks : LayerMask.GetMask("Player");
        int hitCount = CollectHits(runner, attackData, origin, hitMask, hitBuffer);

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = hitBuffer[i];
            if (col == null || col.gameObject == runner.gameObject)
            {
                continue;
            }

            if (col.TryGetComponent<PlayerHealth>(out PlayerHealth character))
            {
                return new AttackHitResult(character);
            }
        }

        return default;
    }

    private static int CollectHits(Enemy runner, EnemyAttackData attackData, Vector3 origin, LayerMask hitMask, Collider[] hitBuffer)
    {
        if (attackData.shape == AttackShape.Sphere)
        {
            return Physics.OverlapSphereNonAlloc(origin, attackData.damageRadius, hitBuffer, hitMask, QueryTriggerInteraction.Collide);
        }

        if (attackData.shape == AttackShape.Box)
        {
            return Physics.OverlapBoxNonAlloc(origin, attackData.boxSize * 0.5f, hitBuffer, runner.transform.rotation, hitMask, QueryTriggerInteraction.Collide);
        }

        if (attackData.shape != AttackShape.Fan)
        {
            return 0;
        }

        int rawCount = Physics.OverlapSphereNonAlloc(origin, attackData.damageRadius, hitBuffer, hitMask, QueryTriggerInteraction.Collide);
        int filteredCount = 0;
        float halfAngle = attackData.fanAngle * 0.5f;

        for (int i = 0; i < rawCount; i++)
        {
            Collider col = hitBuffer[i];
            if (col == null)
            {
                continue;
            }

            Vector3 toTarget = col.transform.position - origin;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                continue;
            }

            float angleToTarget = Vector3.Angle(runner.transform.forward, toTarget.normalized);
            if (angleToTarget <= halfAngle)
            {
                hitBuffer[filteredCount] = col;
                filteredCount++;
            }
        }

        return filteredCount;
    }
}
