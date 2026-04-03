using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using Pathfinding;
using System.Diagnostics;
using System;

/// <summary>
/// 공격 노드의 베이스 클래스. Physics.NonAlloc을 사용하여 GC 할당을 방지합니다.
/// </summary>
public abstract class BaseAttackNode : Node
{
    [Header("Base Attack Properties")]
    [Tooltip("이 노드가 사용할 블랙보드 공격 데이터 키")]
    public string attackKey;
    [Tooltip("이 공격에서 재생할 애니메이터 상태 태그 또는 트리거 이름")]
    public string animationStateName = "";
    [Tooltip("태그가 안 잡혔을 때 실패로 처리하기 전 유예 시간")]
    public float transitionBuffer = 1f;
    [Tooltip("이 노드가 유지될 수 있는 최대 시간(초)")]
    public float maxNodeDuration = 6.0f;
    [Tooltip("피격 확인 후에도 공격 상태를 유지")]
    public bool maintainAtk = false;
    [Tooltip("공격 중 실행되는 액션 ScriptableObject들")]
    public EnemyUseAnything[] SO = null;
    [Tooltip("연속(루프) 공격 동작 사용")]
    public bool LoopAttack = false;
    [Tooltip("즉시 성공 처리하여 다음 BT 분기로 이동")]
    public bool NextBT = false;
    [Tooltip("이 노드의 에디터 전용 디버그 로그 활성화")]
    public bool debugMode = false;

    [Header("Escape Settings")]
    [Tooltip("피격 확인 시 조기 종료(회피) 허용")]
    public bool escapeOnHitConfirm = true;
    [Tooltip("피격 확인 후 종료까지의 지연 시간(초)")]
    public float hitEscapeDelay = 0.5f;

    [Header("Execution Gate")]
    [Tooltip("공격 진입 전에 거리 체크")]
    public bool checkRangeOnEnter = false;
    [Tooltip("필요 사거리 대비 허용 여유 거리")]
    public float rangeThreshold = 1.0f;
    [Tooltip("거리 체크 시 Y축 높이 무시")]
    public bool ignoreYDistance = true;
    [Tooltip("전투 중이 아니어도 실행 허용")]
    public bool allowOutOfCombat = false;

    [Header("State Control")]
    [Tooltip("공격 상태 잠금에 사용하는 블랙보드 키")]
    public string ExceptKey = "IsAttacking";

    [Header("Hit Detection")]
    [Tooltip("히트 판정에 사용하는 레이어 마스크")]
    [SerializeField] private LayerMask _hitMasks = 1<<7;

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
        
        brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, false);
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
        if (isInTransition && !_wasInTransition) if (Handler != null) Handler.ResetAllFlags();
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
        CleanupAllStates();
        if (!_isActionFinishedInternally) brain.StartSkillCooldown(attackKey);
    }

    public sealed override void Abort() { if (isEntered) { CleanupAllStates(); isEntered = false; } }

    private void CleanupAllStates()
    {
        SpecificCleanup();
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
    protected virtual void SpecificCleanup() { }

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
            Vector3 dir = runner.player.transform.position - runner.transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f) runner.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void HandleHitDetection()
    {
        if (Handler == null || !Handler.IsHitWindowOpen) return;
        Vector3 origin = runner.transform.position + runner.transform.TransformDirection(_data.attackOffset);
        LayerMask hitMask = _hitMasks.value != 0 ? _hitMasks : LayerMask.GetMask("Player");
        
        int hitCount = 0;
        if (_data.shape == AttackShape.Sphere)
        {
            hitCount = Physics.OverlapSphereNonAlloc(origin, _data.damageRadius, _hitBuffer, hitMask, QueryTriggerInteraction.Collide);
        }
        else if (_data.shape == AttackShape.Box)
        {
            hitCount = Physics.OverlapBoxNonAlloc(origin, _data.boxSize * 0.5f, _hitBuffer, runner.transform.rotation, hitMask, QueryTriggerInteraction.Collide);
        }
        else if (_data.shape == AttackShape.Fan)
        {
            int rawCount = Physics.OverlapSphereNonAlloc(origin, _data.damageRadius, _hitBuffer, hitMask, QueryTriggerInteraction.Collide);
            float halfAngle = _data.fanAngle * 0.5f;
            for (int i = 0; i < rawCount; i++)
            {
                Collider col = _hitBuffer[i];
                if (col == null) continue;
                Vector3 toTarget = col.transform.position - origin;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude <= 0.0001f) continue;
                float angleToTarget = Vector3.Angle(runner.transform.forward, toTarget.normalized);
                if (angleToTarget <= halfAngle)
                {
                    _hitBuffer[hitCount] = col;
                    hitCount++;
                }
            }
        }

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = _hitBuffer[i];
            if (col.gameObject == runner.gameObject) continue;
            if (col.TryGetComponent<PlayerHealth>(out PlayerHealth Character))
            {
                UnityEngine.Debug.Log($"Hit detected on {Character.name} with attack {_data.AttackName}");
                _data.damageData.AttackerTransform = runner.transform;
                Character.TakeDamage(_data.damageData);
                brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
                if (LoopAttack && _hitConfirmTime < 0) _hitConfirmTime = Time.time;
                if (!maintainAtk) Handler.CloseHitWindow();
            }
        }
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
            return brain.blackboard.GetValueOrDefault<bool>(EnemyBlackboardKeys.DidLastAttackHit, false) ? NodeState.SUCCESS : NodeState.FAILURE;
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

    [Conditional("UNITY_EDITOR")]
    protected void Log(string message, bool isError = false)
    {
        if (!debugMode) return;
        string msg = string.Format("[{0} : {1}] {2}", this.GetType().Name, runner.name, message);
        if (isError) 
        {
            // UnityEngine.Debug.LogError(msg);
        } 
        else 
        {
            // UnityEngine.Debug.Log(msg);
        }
    }

    [Conditional("UNITY_EDITOR")]
    protected void LogStatus(string context)
    {
        if (!debugMode) return;
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        string status = string.Format("[{0}] State: {1}, Speed: {2}", context, runner.CurrentState, ai != null ? ai.maxSpeed.ToString() : "N/A");
        Log(status);
    }
}
