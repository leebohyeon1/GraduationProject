using UnityEngine;
using BehaviorTree;
using Pathfinding;

/// <summary>
/// ActionSO ?몃━嫄??쒖젏???꾩튂濡??뚯쭊?섎뒗 怨듦꺽 ?몃뱶?낅땲??
/// </summary>
[CreateAssetMenu(fileName = "Task_RushAttack", menuName = "BehaviorTree/Action/Task_RushAttack")]
public class Task_RushAttack : BaseAttackNode
{
    private enum RushState { Tracking, Charging }

    [Header("Rush Settings")]
    /// <summary>
    /// 돌진 속도입니다. (즉, 이 속도로 돌진이 진행됩니다.)
    /// </summary>
    public float rushSpeed = 15f;
    /// <summary>
    /// 이건 돌진이 플레이어와 이 거리 이하로 가까워지면 자동으로 멈추는 설정입니다. (즉, 플레이어와 너무 가까워지는 것을 방지하여 돌진이 계속되는 상황을 방지)
    /// </summary>
    public float lockDistance = 5.0f;
    /// <summary>
    /// 이건 돌진 시작 후 최대 지속 시간으로, 이 시간이 지나면 돌진이 자동으로 종료됩니다. (즉, 이 시간 이상 돌진이 지속되지 않도록 하는 안전장치 역할)
    /// </summary>
    public float maxChargeDuration = 3.0f; 
    /// <summary>
    /// 이건 ActionSO 트리거 시점에서 플레이어와의 최대 거리로, 이 범위를 벗어나면 트리거 자체가 안 됩니다. (즉, 이 범위 내에서만 돌진 공격이 시작될 수 있습니다.)
    /// </summary>
    public float maxTriggerRange = 15f;
    /// <summary>
    /// 이건 돌진 중 플레이어를 추적할 때의 회전 속도로, 값이 높을수록 플레이어를 더 빠르게 추적합니다. (0이면 처음 설정된 방향으로만 돌진)
    /// </summary>
    public float trackingTurnSpeed = 2.0f;

    [Header("Phase 2 Trail Settings")]
    [SerializeField] private string trailFeedbackName = "RushTrail";
    /// <summary>
    /// 이건 돌진 중 플레이어와의 거리가 이 값 이상일 때마다 돌진 궤적 이펙트를 생성하는 간격입니다. (즉, 플레이어와 멀어질수록 더 자주 궤적이 생성됩니다.)
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
            // 吏곸꽑 ?뚯쭊 ?섑뻾
            float moveStep = rushSpeed * Time.deltaTime;

            Vector3 desiredDir = playerPos - myPos;
            desiredDir.y = 0;
            if (desiredDir.sqrMagnitude > 0.001f)
            {
                float turnFactor = Mathf.Clamp01(trackingTurnSpeed * Time.deltaTime);
                _chargeDirection = Vector3.Slerp(_chargeDirection, desiredDir.normalized, turnFactor);
            }
            
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

            Vector3 safeNextPos = GetSafeStepDestination(myPos, _chargeDirection, moveStep, out bool blockedByWall);
            Vector3 actualMove = safeNextPos - myPos;

            if (GetHorizontalDistance(myPos, safeNextPos) <= minimumMovementDistance)
            {
                StopRush();
                return;
            }

            runner.transform.position = safeNextPos;

            if (actualMove.sqrMagnitude > 0.0001f)
            {
                Vector3 moveDir = actualMove.normalized;
                moveDir.y = 0f;
                if (moveDir.sqrMagnitude > 0.0001f)
                {
                    runner.transform.rotation = Quaternion.LookRotation(moveDir);
                    _chargeDirection = moveDir;
                }
            }

            if (blockedByWall)
            {
                StopRush();
                return;
            }

            // Phase 2: 吏?섏삩 ?먮━????컻 ?앹꽦
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
        var service = brain.getService<Service_UpdateBossVars>();
        if (service != null)
        {
            service.initNode();
        }
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
    /// ?몃뱶 蹂듭젣蹂몄쓣 ?앹꽦?⑸땲??
    /// </summary>
    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = this.attackKey;
        node.transitionBuffer = this.transitionBuffer;
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
