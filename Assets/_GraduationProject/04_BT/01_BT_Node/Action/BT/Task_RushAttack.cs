using UnityEngine;
using BehaviorTree;
using Pathfinding;

/// <summary>
/// ActionSO 트리거 시점의 위치로 돌진하는 공격 노드입니다.
/// </summary>
[CreateAssetMenu(fileName = "Task_RushAttack", menuName = "BehaviorTree/Action/Task_RushAttack")]
public class Task_RushAttack : BaseAttackNode
{
    private enum RushState { Tracking, Charging }

    [Header("Rush Settings")]
    /// <summary>
    /// 돌진 속도입니다.
    /// </summary>
    public float rushSpeed = 15f;
    /// <summary>
    /// 플레이어 접촉 판정 거리입니다.
    /// </summary>
    public float lockDistance = 5.0f; // 이 거리 안에 오면 방향 고정 돌진
    /// <summary>
    /// 최대 돌진 시간입니다.
    /// </summary>
    public float maxChargeDuration = 3.0f; // 최대 돌진 시간
    /// <summary>
    /// 트리거 사거리입니다.
    /// </summary>
    public float maxTriggerRange = 15f;
    /// <summary>
    /// 추적 방향 보정 속도입니다.
    /// </summary>
    public float trackingTurnSpeed = 2.0f;

    [Header("Phase 2 Trail Settings")]
    [SerializeField] private string trailFeedbackName = "RushTrail";
    /// <summary>
    /// 트레일 생성 간격입니다.
    /// </summary>
    public float trailSpawnInterval = 1.5f;


    private RushState _rushState;
    private bool _endStrategy;
    private bool _isRushing;
    private Vector3 _chargeDirection;
    private float _chargeStartTime;
    private Vector3 _lastTrailPos;
    private int _currentPhase;
    private Vector3 _rushTargetPos;

    protected override float GetRequiredRange() => maxTriggerRange;

    protected override void InitialMovementSetup()
    {
        _endStrategy = false;
        _isRushing = false;
        _rushState = RushState.Tracking;
        runner.AnimationBool("IsRushing", false);
        _currentPhase = brain.blackboard.GetValueOrDefault<int>(EnemyBlackboardKeys.Phase, 1);
        Log("돌진 공격 준비 (Tracking 시작)");
        Debug.Log("[Task_RushAttack] TrackingTurnSpeed: " + trackingTurnSpeed);
    }

    protected override void OnActionSOTriggered()
    {
        if (_isRushing)
        {
            return;
        }
        _isRushing = true;
        _rushState = RushState.Charging;
        _chargeStartTime = Time.time;
        runner.AnimationBool("IsRushing", true);

        Vector3 myPos = runner.transform.position;
        _rushTargetPos = runner.player != null ? runner.player.transform.position : myPos + runner.transform.forward;
        _chargeDirection = (_rushTargetPos - myPos).normalized;
        _chargeDirection.y = 0;

        _lastTrailPos = myPos;
        
        if (runner.aIPath != null)
        {
            runner.aIPath.isStopped = true;
            runner.aIPath.canMove = false;
            runner.aIPath.enableRotation = false;
        }
    }

    protected override void UpdateMovement()
    {
        if (!_isRushing || runner.player == null) return;

        Vector3 playerPos = runner.player.transform.position;
        Vector3 myPos = runner.transform.position;
        float distToPlayer = Vector3.Distance(myPos, playerPos);

        if (_rushState == RushState.Charging)
        {
            // 직선 돌진 수행
            float moveStep = rushSpeed * Time.deltaTime;

            Vector3 desiredDir = playerPos - myPos;
            desiredDir.y = 0;
            if (desiredDir.sqrMagnitude > 0.001f)
            {
                float turnFactor = Mathf.Clamp01(trackingTurnSpeed * Time.deltaTime);
                _chargeDirection = Vector3.Slerp(_chargeDirection, desiredDir.normalized, turnFactor);
            }
            
            // 벽 충돌 체크
            if (runner.Movement.IsPathBlocked(_chargeDirection, 0.5f, out RaycastHit hit))
            {
                StopRush();
                return;
            }

            // 시간 초과 체크
            if (Time.time - _chargeStartTime >= maxChargeDuration)
            {
                StopRush();
                return;
            }

            if (distToPlayer <= lockDistance)
            {
                StopRush();
                return;
            }

            // 이동 적용
            runner.transform.position += _chargeDirection * moveStep;
            runner.transform.rotation = Quaternion.LookRotation(_chargeDirection);

            // Phase 2: 지나온 자리에 폭발 생성
            if (_currentPhase >= 2)
            {
                if (Vector3.Distance(_lastTrailPos, runner.transform.position) >= trailSpawnInterval)
                {
                    SpawnTrailExplosion(_lastTrailPos);
                    _lastTrailPos = runner.transform.position;
                }
            }
        }
    }

    private void SpawnTrailExplosion(Vector3 pos)
    {
        if (runner.animHandler != null && !string.IsNullOrWhiteSpace(trailFeedbackName))
        {
            runner.animHandler.PlayFeedbackAtPosition(trailFeedbackName, pos);
        }
    }

    private void StopRush()
    {
        runner.Movement.StopMovement();
        _endStrategy = true;
        brain.blackboard.SetValue(LoopAction.EndKey, true);
        runner.AnimationBool("IsRushing", true);
        runner._aiController._aiBrain.blackboard.SetValue("WalkingTime",false);
    }

    protected override bool IsMovementFinished => _endStrategy;

    protected override void SpecificCleanup()
    {
        _isRushing = false;
        runner.AnimationBool("IsRushing", false);
        if (runner.aIPath != null)
        {
            runner.aIPath.enableRotation = true;
        }
    }

    /// <summary>
    /// 노드 복제본을 생성합니다.
    /// </summary>
    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = this.attackKey;
        node.animationStateName = this.animationStateName;
        node.transitionBuffer = this.transitionBuffer;
        node.maxNodeDuration = this.maxNodeDuration;
        node.maintainAtk = this.maintainAtk;
        node.SO = this.SO;
        node.LoopAttack = this.LoopAttack;
        node.NextBT = this.NextBT;
        node.debugMode = this.debugMode;
        node.checkRangeOnEnter = this.checkRangeOnEnter;
        node.rangeThreshold = this.rangeThreshold;
        node.ignoreYDistance = this.ignoreYDistance;
        node.allowOutOfCombat = this.allowOutOfCombat;
        
        node.rushSpeed = this.rushSpeed;
        node.lockDistance = this.lockDistance;
        node.maxChargeDuration = this.maxChargeDuration;
        node.maxTriggerRange = this.maxTriggerRange;
        node.trackingTurnSpeed = this.trackingTurnSpeed;
        
        node.trailSpawnInterval = this.trailSpawnInterval;


        node.ExceptKey = this.ExceptKey;
        node.escapeOnHitConfirm = this.escapeOnHitConfirm;
        node.hitEscapeDelay = this.hitEscapeDelay;
        return node;
    }
}
