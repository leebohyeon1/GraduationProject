using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using Pathfinding;

/// <summary>
/// 모든 공격형 노드의 기본 클래스. 
/// 애니메이션 동기화, 히트 판정, 회전 로직 등 공통 공격 엔진을 포함하며 상세 로깅 시스템을 지원합니다.
/// </summary>
public abstract class BaseAttackNode : Node
{
    [Header("Base Attack Properties")]
    public string attackKey;
    public string animationStateName = "";
    public float transitionBuffer = 1f;
    /// <summary> 노드 진입 후 강제로 종료할 최대 시간 (초) </summary>
    public float maxNodeDuration = 6.0f;
    public bool maintainAtk = false;
    public EnemyUseAnything[] SO = null;
    public bool LoopAttack = false;
    public bool NextBT = false;
    public bool debugMode = true;

    [Header("Escape Settings (When Enemy Attacks Player)")]
    [Tooltip("플레이어 타격 성공 시 루프를 탈출할지 여부")]
    public bool escapeOnHitConfirm = true;
    [Tooltip("타격 성공 후 루프를 탈출하기까지의 지연 시간 (초)")]
    public float hitEscapeDelay = 0.5f;

    [Header("Execution Gate")]
    public bool checkRangeOnEnter = true;
    public float rangeThreshold = 1.0f;
    public bool ignoreYDistance = true;
    public bool allowOutOfCombat = false;

    [Header("State Control")]
    public string ExceptKey = "IsAttacking";

    protected EnemyAttackData _data;
    protected float _nodeEntryTime;
    protected int _entryFrame; 
    protected bool _isActionFinishedInternally;
    protected bool _hasTriggeredLoop;
    private bool _hasSeenTag;
    private float _hitConfirmTime = -1f;
    private bool _didSetLock = false; 
    private bool _wasInTransition = false; 
    private bool _hasHaltedMovement = false; 

    #region Lifecycle (Sealed)
    
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

        if (!brain.blackboard.HasKey(attackKey) || !brain.blackboard.GetValue<EnemyAttackData>(attackKey, out _data))
        {
            Log("[Error] '" + attackKey + "' 키를 블랙보드에서 찾을 수 없습니다.", true);
            _isActionFinishedInternally = true;
            return;
        }

        bool isAlreadyInAttackState = runner.CurrentState == EnemyStateController.EnemyState.Attack || runner._animationBridge.IsAttacking;
        bool isStunnedState = runner.CurrentState == EnemyStateController.EnemyState.Stunned;
        bool isRecovering = runner._stateController != null && runner._stateController.IsRecoveringFromStun;
        
        if (runner._stateController.IsStateLocked || isAlreadyInAttackState || isStunnedState || isRecovering)
        {
            Log(string.Format("진입 거부: 공격 중, 잠겨 있음, 스턴 상태 혹은 회복 중. (상태: {0}, Lock: {1}, AnimAtk: {2}, Recovering: {3})", 
                runner.CurrentState, runner._stateController.IsStateLocked, runner._animationBridge.IsAttacking, isRecovering));
            
            _isActionFinishedInternally = true;
            return;
        }

        if (!CanExecuteInternal())
        {
            _isActionFinishedInternally = true;
            return;
        }

        Log("공격 노드 진입 확정: " + _data.AttackName);

        if (Handler != null) Handler.ResetAllFlags();
        
        brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, false);
        brain.blackboard.SetValue(EnemyBlackboardKeys.OnTakeHit, false);
        brain.blackboard.SetValue(ExceptKey, true);

        runner.AnimationBool("IsRushing", false);

        _data.damageData.AttackerTransform = runner.transform;
        runner.AnimationEvent(_data.AttackName);
        runner.SetState(EnemyStateController.EnemyState.Attack);
        runner.SetCurrentAttackData(_data);
        
        runner._stateController.SetLock(true);
        _didSetLock = true;

        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.canMove = false;
            ai.isStopped = true;
            ai.destination = runner.transform.position;
        }

        for (int i = 0; i < SO.Length; i++)
        {
            if (SO[i] != null)
            {
                SO[i].Reset(runner);
                SO[i].OnEnter(runner);
                SO[i].OnActionTriggered(runner);
            }
        }

        InitialMovementSetup();
    }

    protected sealed override NodeState OnUpdate()
    {
        if (_isActionFinishedInternally) return NodeState.FAILURE;
        if (_data == null) return NodeState.FAILURE;
        if (runner.animator == null) return NodeState.FAILURE; 

        if (runner.CurrentState != EnemyStateController.EnemyState.Attack)
        {
            Log("상태 변경 감지 (Attack -> " + runner.CurrentState + "): 공격 중단");
            return NodeState.FAILURE;
        }

        var stateInfo = runner.animator.GetCurrentAnimatorStateInfo(0);
        var nextStateInfo = runner.animator.GetNextAnimatorStateInfo(0);
        float elapsedTime = Time.time - _nodeEntryTime;

        bool isTagActive = stateInfo.IsTag(_data.AttackName) || nextStateInfo.IsTag(_data.AttackName);
        
        if (isTagActive && !_hasSeenTag)
        {
            _hasSeenTag = true;
            if (Handler != null) Handler.ResetAllFlags(); 
            Log("애니메이션 태그 확인됨 - 신호 대기 시작");
        }

        if (LoopAttack && escapeOnHitConfirm && _hitConfirmTime > 0)
        {
            if (Time.time - _hitConfirmTime >= hitEscapeDelay)
            {
                if (!brain.blackboard.GetValueOrDefault<bool>(LoopAction.EndKey, false))
                {
                    Log("타격 성공 후 지연시간 경과: 루프 탈출 신호 발생");
                    runner.AnimationBool("IsRushing", true);
                    brain.blackboard.SetValue(LoopAction.EndKey, true);
                }
            }
        }

        if (Time.frameCount > _entryFrame + 1)
        {
            NodeState finishState = CheckActionFinished(elapsedTime);
            if (finishState != NodeState.RUNNING) 
            {
                Log("CheckActionFinished에 의해 노드 종료: " + finishState);
                return finishState;
            }
        }

        if (!isTagActive)
        {
            if (elapsedTime > transitionBuffer)
            {
                Log("애니메이션 태그 불일치 종료 (Tag: " + _data.AttackName + ")");
                return NodeState.FAILURE;
            }
            return NodeState.RUNNING;
        }

        bool isInTransition = runner.animator.IsInTransition(0);
        if (isInTransition && !_wasInTransition)
        {
            if (Handler != null) Handler.ResetAllFlags();
        }
        _wasInTransition = isInTransition;

        HandleCommonSystems(stateInfo, nextStateInfo);

        // 이동 제어 로직 개선
        bool isLoopEnded = (LoopAttack && _hasTriggeredLoop && brain.blackboard.GetValueOrDefault<bool>(LoopAction.EndKey, false));
        bool movementActive = !IsMovementFinished && !isLoopEnded;
        
        if (movementActive)
        {
            _hasHaltedMovement = false; // 이동 중에는 정지 플래그 초기화
            UpdateMovement();
        }
        else
        {
            if (!_hasHaltedMovement)
            {
                _hasHaltedMovement = true;
                HaltMovement();
            }
        }

        return NodeState.RUNNING;
    }

    private void HaltMovement()
    {
        Log("물리 이동 중단 (Halt)");
        Rigidbody rb = runner.GetComponent<Rigidbody>();
        if (rb != null) 
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null) 
        {
            ai.isStopped = true;
            ai.destination = runner.transform.position;
            ai.maxSpeed = 0; 
            ai.Teleport(runner.transform.position); 
            ai.SetPath(null); 
        }

        // Momentum 제거를 위해 CharacterController가 있다면 Move(zero) 수행
        CharacterController cc = runner.GetComponent<CharacterController>();
        if (cc != null) cc.Move(Vector3.zero);
    }

    public sealed override void OnExit()
    {
        Log("공격 노드 정상 종료 (OnExit): " + (_data != null ? _data.AttackName : "Unknown"));
        CleanupAllStates();
        brain.StartSkillCooldown(attackKey);
    }

    public sealed override void Abort()
    {
        if (isEntered)
        {
            Log("공격 노드 중단 (Abort)");
            CleanupAllStates();
            isEntered = false; 
        }
    }

    private void CleanupAllStates()
    {
        SpecificCleanup();

        if (_didSetLock && runner._stateController != null)
        {
            runner._stateController.SetLock(false);
            _didSetLock = false;
        }

        if (runner._animationBridge != null)
        {
            runner._animationBridge.ClearIsAttacking();
        }

        runner.ParrySystem.StateNormal();
        brain.blackboard.SetValue(ExceptKey, false);
        brain.blackboard.SetValue(EnemyBlackboardKeys.OnTakeHit, false);
        if (Handler != null) Handler.ResetAllFlags();
        
        if (runner.CurrentState == EnemyStateController.EnemyState.Attack)
        {
            runner.SetState(EnemyStateController.EnemyState.Idle);
        }
        
        runner.AnimationBool("Walk", false);
        runner.AnimationBool("IsRushing", true); 
        
        runner.aIPath.enableRotation = true;
        runner.SetStiffness(0);

        StopMovementInternal();

        for (int i = 0; i < SO.Length; i++)
        {
            if (SO[i] != null) SO[i].OnExit(runner);
        }
        if (Handler != null) Handler.EndSO();

        LogStatus("정리 완료 (Cleanup)");
    }

    #endregion

    #region Execution Gate Logic

    private bool CanExecuteInternal()
    {
        if (!allowOutOfCombat && !brain._isCombat) return false;
        if (!brain.IsSkillReady(attackKey, _data.Cooltime)) return false;
        
        if (runner.CurrentState == EnemyStateController.EnemyState.Stunned)
        {
            Log("진입 거부: 현재 논리적 스턴 상태임.");
            return false;
        }

        if (runner._stateController != null && runner._stateController.IsRecoveringFromStun)
        {
            Log("진입 거부: 스턴 후 회복 중 (0.5초 대기)");
            return false;
        }

        var stateInfo = runner.animator.GetCurrentAnimatorStateInfo(0);
        {
            float dist = CalculateDistance();
            float range = GetRequiredRange();
            if (dist > range + rangeThreshold) return false;
        }
        return CheckCustomPreconditions();
    }

    private float CalculateDistance()
    {
        Vector3 myPos = runner.transform.position;
        Vector3 targetPos = runner.player.transform.position;
        if (ignoreYDistance) { myPos.y = 0; targetPos.y = 0; }
        return Vector3.Distance(myPos, targetPos);
    }

    protected virtual float GetRequiredRange() => _data != null ? _data.damageRadius : 2.0f;
    protected virtual bool CheckCustomPreconditions() => true;

    #endregion

    #region Abstract Hooks

    protected abstract void InitialMovementSetup();
    protected abstract void UpdateMovement();
    protected abstract bool IsMovementFinished { get; }
    protected virtual void SpecificCleanup() { }

    #endregion

    #region Internal Attack Engine

    private void HandleCommonSystems(AnimatorStateInfo stateInfo, AnimatorStateInfo nextStateInfo)
    {
        if (Handler != null && Handler.IsSound) Handler.EndSound();

        if (Handler != null && Handler.IsActionSO)
        {
            if (stateInfo.IsTag(_data.AttackName) || nextStateInfo.IsTag(_data.AttackName))
            {
                Log("ActionSO 트리거 감지");
                _hasHaltedMovement = false; // 행동 시작 시 플래그 초기화
                OnActionSOTriggered();
                for (int i = 0; i < SO.Length; i++)
                {
                    if (SO[i] != null)
                    {
                        SO[i].OnEnter(runner);
                    }
                }
                Handler.EndSO();
            }
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
            HandleLoopAttackLogic();
        }

        HandleRotation();
        HandleHitDetection();
    }

    protected virtual void OnActionSOTriggered() { }

    private void HandleRotation()
    {
        if (!Handler.IsActive)
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
        Collider[] hits = GetHitColliders(origin);
        foreach (var col in hits)
        {
            if (col.gameObject == runner.gameObject) continue;
            if (col.TryGetComponent<PlayerHealth>(out PlayerHealth Character))
            {
                Log("플레이어 타격 성공: " + col.name);
                Character.TakeDamage(_data.damageData);
                brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
                
                if (LoopAttack && _hitConfirmTime < 0)
                {
                    _hitConfirmTime = Time.time;
                    Log("히트 확정: 지연 후 루프 탈출 예정");
                }

                if (!maintainAtk) Handler.CloseHitWindow();
            }
        }
    }

    private Collider[] GetHitColliders(Vector3 origin)
    {
        List<Collider> validHits = new List<Collider>();
        switch (_data.shape)
        {
            case AttackShape.Sphere: return Physics.OverlapSphere(origin, _data.damageRadius);
            case AttackShape.Box: return Physics.OverlapBox(origin, _data.boxSize * 0.5f, runner.transform.rotation);
            case AttackShape.Fan:
                Collider[] raw = Physics.OverlapSphere(origin, _data.damageRadius);
                foreach (var c in raw)
                {
                    if (Vector3.Angle(runner.transform.forward, (c.transform.position - origin).normalized) <= _data.fanAngle * 0.5f) validHits.Add(c);
                }
                return validHits.ToArray();
            default: return new Collider[0];
        }
    }

    private void HandleLoopAttackLogic()
    {
        bool hasHit = brain.blackboard.GetValueOrDefault<bool>(EnemyBlackboardKeys.DidLastAttackHit, false);
        if (hasHit && LoopAttack && !_hasTriggeredLoop)
        {
            _hasTriggeredLoop = true;
            for (int i = 0; i < SO.Length; i++)
            {
                if (SO[i] != null) SO[i].UseSomeThing(runner);
            }
        }
    }

    private NodeState CheckActionFinished(float elapsedTime)
    {
        if (!_hasSeenTag && elapsedTime < transitionBuffer + 0.3f)
        {
            return NodeState.RUNNING;
        }

        bool isTimedOut = elapsedTime >= maxNodeDuration; 
        
        bool isLoopOngoing = false;
        if (LoopAttack && _hasTriggeredLoop)
        {
             isLoopOngoing = !brain.blackboard.GetValueOrDefault<bool>(LoopAction.EndKey, false);
        }

        if (isLoopOngoing && !isTimedOut)
        {
            return NodeState.RUNNING;
        }

        // [수정] 오직 애니메이션 종료 신호와 절대 타임아웃만 체크합니다.
        // 이동 완료(IsMovementFinished)는 OnUpdate에서 물리 정지만 수행할 뿐 노드를 종료시키지 않습니다.
        if ((Handler != null && Handler.IsActionFinished) || isTimedOut)
        {
            if (isTimedOut) Log("시간 만료 종료", true);
            else if (Handler != null && Handler.IsActionFinished) Log("애니메이션 신호로 종료");

            if (NextBT) return NodeState.SUCCESS;
            bool hit = brain.blackboard.GetValueOrDefault<bool>(EnemyBlackboardKeys.DidLastAttackHit, false);
            return hit ? NodeState.SUCCESS : NodeState.FAILURE;
        }

        return NodeState.RUNNING;
    }

    protected void StopMovementInternal()
    {
        Rigidbody rb = runner.GetComponent<Rigidbody>();
        if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.canMove = true;
            ai.isStopped = false;
            ai.maxSpeed = runner.Movement._normalSpeed;
            
            ai.destination = runner.transform.position;
            
            if (!ai.pathPending) ai.SearchPath();
        }
    }

    protected void Log(string message, bool isError = false)
    {
        if (!debugMode) return;
        string msg = string.Format("[{0} : {1}] {2}", this.GetType().Name, runner.name, message);
        if (isError) Debug.LogError(msg); else Debug.Log(msg);
    }

    private void LogStatus(string context)
    {
        if (!debugMode) return;
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        string status = string.Format("[{0}] State: {1}, Lock: {2}, AnimAtk: {3}, ai.canMove: {4}, ai.isStopped: {5}, Speed: {6}",
            context, runner.CurrentState, runner._stateController.IsStateLocked, runner._animationBridge.IsAttacking, 
            ai != null ? ai.canMove.ToString() : "N/A", ai != null ? ai.isStopped.ToString() : "N/A",
            ai != null ? ai.maxSpeed.ToString() : "N/A");
        Log(status);
    }
    #endregion
}
