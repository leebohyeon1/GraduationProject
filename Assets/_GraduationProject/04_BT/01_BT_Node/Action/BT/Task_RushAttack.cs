using UnityEngine;
using BehaviorTree;
using Pathfinding;

/// <summary>
/// 돌진 공격을 수행하고 종료 직전 짧은 관성 감속을 적용하는 공격 노드입니다.
/// </summary>
[CreateAssetMenu(fileName = "Task_RushAttack", menuName = "BehaviorTree/Action/Task_RushAttack")]
public class Task_RushAttack : BaseAttackNode
{
    private enum RushState
    {
        Tracking,
        Charging,
        Decelerating
    }

    [Header("Rush Settings")]
    [Tooltip("돌진 중 초당 이동 속도입니다. RushAttack 본체 이동량 계산에 사용합니다.")]
    public float rushSpeed = 15f;

    [Tooltip("목표 지점에 얼마나 가까워지면 감속을 시작할지 설정합니다. 플레이어 근처 도착 판정에 사용합니다.")]
    public float lockDistance = 5.0f;

    [Tooltip("돌진 상태를 최대 몇 초까지 유지할지 설정합니다. 시간이 지나면 관성 감속으로 전환됩니다.")]
    public float maxChargeDuration = 3.0f;

    [Tooltip("이 공격을 시작할 수 있는 최대 거리입니다. BT 진입 가능 범위 판정에 사용합니다.")]
    public float maxTriggerRange = 15f;

    [Tooltip("돌진 중 목표 방향을 얼마나 빠르게 따라갈지 설정합니다. 0이면 처음 방향을 유지합니다.")]
    public float trackingTurnSpeed = 2.0f;

    [Tooltip("목표 지점 도착 후 몇 초 동안 감속해 0 속도로 멈출지 설정합니다. 기본값은 0.2초이며 RushAttack 관성 정지에 사용합니다.")]
    public float decelerationDuration = 0.2f;

    [Header("Phase 2 Trail Settings")]
    [SerializeField] private string trailFeedbackName = "RushTrail";

    [Tooltip("2페이즈에서 잔상 폭발을 얼마나 자주 생성할지 설정합니다. 이동 중 피드백 간격으로 사용합니다.")]
    public float trailSpawnInterval = 1.5f;

    private RushState _rushState;
    private bool _endStrategy;
    private bool _isRushing;
    private Vector3 _chargeDirection;
    private float _chargeStartTime;
    private Vector3 _lastTrailPos;
    private int _currentPhase;
    private Vector3 _rushTargetPos;
    private float _decelerationStartTime;
    private float _decelerationStartSpeed;
    private float _trackingTurnSpeed;

    protected override float GetRequiredRange() => maxTriggerRange;

    protected override void InitialMovementSetup()
    {
        _endStrategy = false;
        _isRushing = false;
        _rushState = RushState.Tracking;
        _decelerationStartTime = 0f;
        _decelerationStartSpeed = 0f;
        runner.AnimationBool("IsRushing", false);
        _currentPhase = brain.blackboard.GetValueOrDefault<int>(EnemyBlackboardKeys.Phase, 1);
        _trackingTurnSpeed = trackingTurnSpeed;
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
        Debug.Log($"[Task_RushAttack] {runner.name} rush started.");

        Vector3 myPos = runner.transform.position;
        _rushTargetPos = runner.player != null ? runner.player.transform.position : myPos + runner.transform.forward;
        _chargeDirection = (_rushTargetPos - myPos).normalized;
        _chargeDirection.y = 0f;

        if (_chargeDirection.sqrMagnitude <= 0.0001f)
        {
            _chargeDirection = runner.transform.forward;
            _chargeDirection.y = 0f;
            _chargeDirection.Normalize();
        }

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
        if (!_isRushing || runner.player == null)
        {
            return;
        }

        Vector3 myPos = runner.transform.position;
        Vector3 playerPos = runner.player.transform.position;

        if (_rushState == RushState.Charging)
        {
            _rushTargetPos = playerPos;
            UpdateChargeDirection(myPos, playerPos);

            if (runner.Movement.IsPathBlocked(_chargeDirection, 0.5f, out RaycastHit hit))
            {
                BeginDeceleration(rushSpeed);
                return;
            }

            if (Time.time - _chargeStartTime >= maxChargeDuration)
            {
                BeginDeceleration(rushSpeed);
                return;
            }

            if (Vector3.Distance(myPos, _rushTargetPos) <= lockDistance)
            {
                BeginDeceleration(rushSpeed);
                return;
            }

            float moveStep = rushSpeed * Time.deltaTime;
            runner.transform.position += _chargeDirection * moveStep;
            runner.transform.rotation = Quaternion.LookRotation(_chargeDirection);
            TrySpawnRushTrail();
            return;
        }

        if (_rushState == RushState.Decelerating)
        {
            _trackingTurnSpeed = 0;
            float duration = Mathf.Max(0.01f, decelerationDuration);
            float elapsed = Time.time - _decelerationStartTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);
            float currentSpeed = Mathf.Lerp(_decelerationStartSpeed, 0f, normalizedTime);

            if (currentSpeed > 0.001f)
            {
                runner.transform.position += _chargeDirection * currentSpeed * Time.deltaTime;
                if (_chargeDirection.sqrMagnitude > 0.001f)
                {
                    runner.transform.rotation = Quaternion.LookRotation(_chargeDirection);
                }

                TrySpawnRushTrail();
                return;
            }

            CompleteRush();
        }
    }

    /// <summary>
    /// 돌진 중 플레이어의 최신 위치를 반영해 돌진 방향을 부드럽게 갱신합니다.
    /// </summary>
    private void UpdateChargeDirection(Vector3 myPos, Vector3 playerPos)
    {
        Vector3 desiredDir = playerPos - myPos;
        desiredDir.y = 0f;
        if (desiredDir.sqrMagnitude <= 0.001f)
        {
            return;
        }

        float turnFactor = Mathf.Clamp01(_trackingTurnSpeed * Time.deltaTime);
        _chargeDirection = Vector3.Slerp(_chargeDirection, desiredDir.normalized, turnFactor);
        _chargeDirection.y = 0f;
        _chargeDirection.Normalize();
    }

    /// <summary>
    /// 돌진 종료 직전에 관성 감속 단계로 전환합니다.
    /// </summary>
    private void BeginDeceleration(float startSpeed)
    {
        if (_rushState == RushState.Decelerating || _endStrategy)
        {
            return;
        }

        _rushState = RushState.Decelerating;
        _decelerationStartTime = Time.time;
        _decelerationStartSpeed = Mathf.Max(0f, startSpeed);
        Debug.Log($"[Task_RushAttack] {runner.name} entered deceleration for {Mathf.Max(0.01f, decelerationDuration):0.00}s.");
    }

    /// <summary>
    /// 감속이 끝난 뒤 러시 종료 플래그와 후속 서비스 갱신을 수행합니다.
    /// </summary>
    private void CompleteRush()
    {
        if (_endStrategy)
        {
            return;
        }

        runner.Movement.StopMovement();
        _rushState = RushState.Tracking;
        _endStrategy = true;
        brain.blackboard.SetValue(LoopAction.EndKey, true);
        runner.AnimationBool("IsRushing", true);
        Debug.Log($"[Task_RushAttack] {runner.name} rush completed.");

        var service = brain.getService<Service_UpdateBossVars>();
        if (service != null)
        {
            service.initNode();
        }
    }

    private void TrySpawnRushTrail()
    {
        if (_currentPhase < 2)
        {
            return;
        }

        if (Vector3.Distance(_lastTrailPos, runner.transform.position) >= trailSpawnInterval)
        {
            SpawnTrailExplosion(_lastTrailPos);
            _lastTrailPos = runner.transform.position;
        }
    }

    private void SpawnTrailExplosion(Vector3 pos)
    {
        if (runner.animHandler != null && !string.IsNullOrWhiteSpace(trailFeedbackName))
        {
            runner.animHandler.PlayFeedbackAtPosition(trailFeedbackName, pos);
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
    /// 런타임 인스턴스에서도 동일한 Rush 설정을 유지하도록 복제합니다.
    /// </summary>
    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = attackKey;
        node.transitionBuffer = transitionBuffer;
        node.SO = SO;
        node.LoopAttack = LoopAttack;
        node.NextBT = NextBT;
        node.debugMode = debugMode;
        node.checkRangeOnEnter = checkRangeOnEnter;
        node.rangeThreshold = rangeThreshold;
        node.ignoreYDistance = ignoreYDistance;
        node.allowOutOfCombat = allowOutOfCombat;
        node.rushSpeed = rushSpeed;
        node.lockDistance = lockDistance;
        node.maxChargeDuration = maxChargeDuration;
        node.maxTriggerRange = maxTriggerRange;
        node.trackingTurnSpeed = trackingTurnSpeed;
        node.decelerationDuration = decelerationDuration;
        node.trailSpawnInterval = trailSpawnInterval;
        node.ExceptKey = ExceptKey;
        node.escapeOnHitConfirm = escapeOnHitConfirm;
        node.hitEscapeDelay = hitEscapeDelay;
        return node;
    }
}
