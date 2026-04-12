using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "Task_TristanaJump", menuName = "BehaviorTree/Action/Task_TristanaJump")]
public class Task_TristanaJump : BaseAttackNode
{
    [Header("Jump Settings")]
    /// <summary>
    /// 최대 점프 사거리입니다.
    /// </summary>
    public float jumpRange = 8.0f;
    /// <summary>
    /// 점프 지속 시간입니다.
    /// </summary>
    public float jumpDuration = 0.8f;
    /// <summary>
    /// 점프 높이입니다.
    /// </summary>
    public float jumpHeight = 5.0f;
    /// <summary>
    /// 점프 높이 커브입니다.
    /// </summary>
    public AnimationCurve heightCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
    /// <summary>
    /// 트리거 사거리입니다.
    /// </summary>
    public float maxTriggerRange = 10f;

    [Header("Landing Settings")]
    /// <summary>
    /// 착지 충격 반경입니다.
    /// </summary>
    public float impactRadius = 2.5f;
    /// <summary>
    /// 착지 충격 피해 데이터입니다.
    /// </summary>
    public DamageData impactDamage;
    /// <summary>
    /// 착지 지면 레이어입니다.
    /// </summary>
    public LayerMask groundLayer;
    /// <summary>
    /// 지면 체크 시작 높이입니다.
    /// </summary>
    public float groundCheckHeight = 2.0f;
    /// <summary>
    /// 지면 체크 거리입니다.
    /// </summary>
    public float groundCheckDistance = 6.0f;

    private Vector3 _startPos;
    private Vector3 _targetPos;
    [SerializeField] float TargetOffset = 0f;
    private bool _isJumping;

    protected override float GetRequiredRange() => maxTriggerRange;

    protected override void InitialMovementSetup()
    {
        _isJumping = false;
        Log("점프 준비 (ActionSO 대기 중)");
        Debug.Log("[Task_TristanaJump] 점프 준비");
    }

    protected override void OnActionSOTriggered()
    {
        // [수정] 애니메이션 이벤트 시점에 실시간 플레이어 위치를 기반으로 목표 지점 계산
        _startPos = runner.transform.position;
        Vector3 playerPos = runner.player.transform.position;
        Vector3 direction = (playerPos - _startPos);
        direction.y = 0;
        float distance = direction.magnitude - TargetOffset;
        direction.Normalize();

        float jumpDist = Mathf.Min(distance, jumpRange);
        Vector3 rawTarget = _startPos + (direction * jumpDist);

        NNInfo info = AstarPath.active.GetNearest(rawTarget, NNConstraint.Walkable);
        _targetPos = info.node != null ? info.position : rawTarget;
        Log("점프 시작 (OnActionSOTriggered) - 목표 설정: " + _targetPos);
        _isJumping = true;
        _nodeEntryTime = Time.time; // 점프 시작 시점 리셋
        
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.canMove = false;
            ai.isStopped = true;
        }
        Debug.Log("[Task_TristanaJump] 점프 시작 - 목표 위치: " + _targetPos);
    }

    protected override void UpdateMovement()
    {
        if (!_isJumping) return;

        float jumpTime = Time.time - _nodeEntryTime;
        float normalizedTime = jumpTime / jumpDuration;

        if (normalizedTime < 1.0f)
        {
            Vector3 currentPos = Vector3.Lerp(_startPos, _targetPos, normalizedTime);
            float height = heightCurve.Evaluate(normalizedTime) * jumpHeight;
            currentPos.y += height;

            runner.transform.position = currentPos;

            Vector3 lookDir = (_targetPos - _startPos).normalized;
            if (lookDir != Vector3.zero)
                runner.transform.rotation = Quaternion.LookRotation(lookDir);
        }
        else
        {
            Landing();
        }
    }

    protected override bool IsMovementFinished => !_isJumping;

    private void Landing()
    {
        Log("점프 착지 수행");
        _isJumping = false;

        Vector3 landPos = _targetPos;
        Vector3 rayOrigin = landPos + Vector3.up * groundCheckHeight;
        float rayDistance = groundCheckHeight + groundCheckDistance;
        LayerMask rayMask = groundLayer;
        if (rayMask.value == 0)
        {
            rayMask = LayerMask.GetMask("Ground");
        }
        rayMask &= ~LayerMask.GetMask("Player");

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, rayMask, QueryTriggerInteraction.Ignore))
        {
            landPos.y = hit.point.y;
        }
        runner.transform.position = landPos;

        Collider[] hitColliders = Physics.OverlapSphere(landPos, impactRadius, LayerMask.GetMask("Player"));
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                impactDamage.AttackerTransform = runner.transform;
                playerHealth.TakeDamage(impactDamage);
                brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
                brain.blackboard.SetValue(EnemyBlackboardKeys.LastAttackSuccessTime, Time.time);
            }
        }

        runner.AnimationBool("IsRushing", true);
    }

    protected override void SpecificCleanup()
    {
        if (_isJumping) Landing();
    }

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
        node.jumpRange = this.jumpRange;
        node.jumpDuration = this.jumpDuration;
        node.jumpHeight = this.jumpHeight;
        node.heightCurve = this.heightCurve;
        node.impactRadius = this.impactRadius;
        node.impactDamage = this.impactDamage;
        node.groundLayer = this.groundLayer;
        node.groundCheckHeight = this.groundCheckHeight;
        node.groundCheckDistance = this.groundCheckDistance;
        node.maxTriggerRange = this.maxTriggerRange;
        node.ExceptKey = this.ExceptKey;
        node.escapeOnHitConfirm = this.escapeOnHitConfirm;
        node.hitEscapeDelay = this.hitEscapeDelay;
        return node;
    }
}
