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
    /// ?뚯쭊 ?띾룄?낅땲??
    /// </summary>
    public float rushSpeed = 15f;
    /// <summary>
    /// ?뚮젅?댁뼱 ?묒큺 ?먯젙 嫄곕━?낅땲??
    /// </summary>
    public float lockDistance = 5.0f; // ??嫄곕━ ?덉뿉 ?ㅻ㈃ 諛⑺뼢 怨좎젙 ?뚯쭊
    /// <summary>
    /// 理쒕? ?뚯쭊 ?쒓컙?낅땲??
    /// </summary>
    public float maxChargeDuration = 3.0f; // 理쒕? ?뚯쭊 ?쒓컙
    /// <summary>
    /// ?몃━嫄??ш굅由ъ엯?덈떎.
    /// </summary>
    public float maxTriggerRange = 15f;
    /// <summary>
    /// 異붿쟻 諛⑺뼢 蹂댁젙 ?띾룄?낅땲??
    /// </summary>
    public float trackingTurnSpeed = 2.0f;

    [Header("Phase 2 Trail Settings")]
    [SerializeField] private string trailFeedbackName = "RushTrail";
    /// <summary>
    /// ?몃젅???앹꽦 媛꾧꺽?낅땲??
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
            
            // 踰?異⑸룎 泥댄겕
            if (runner.Movement.IsPathBlocked(_chargeDirection, 0.5f, out RaycastHit hit))
            {
                StopRush();
                return;
            }

            // ?쒓컙 珥덇낵 泥댄겕
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

            // ?대룞 ?곸슜
            runner.transform.position += _chargeDirection * moveStep;
            runner.transform.rotation = Quaternion.LookRotation(_chargeDirection);

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
        node.animationStateName = this.animationStateName;
        node.transitionBuffer = this.transitionBuffer;
        node.maxNodeDuration = this.maxNodeDuration;
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
