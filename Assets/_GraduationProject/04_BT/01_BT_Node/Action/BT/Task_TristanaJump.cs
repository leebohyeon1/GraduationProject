using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "Task_TristanaJump", menuName = "BehaviorTree/Action/Task_TristanaJump")]
public class Task_TristanaJump : BaseAttackNode
{
    [Header("Jump Settings")]
    /// <summary>
    /// 理쒕? ?먰봽 ?ш굅由ъ엯?덈떎.
    /// </summary>
    public float jumpRange = 8.0f;
    /// <summary>
    /// ?먰봽 吏???쒓컙?낅땲??
    /// </summary>
    public float jumpDuration = 0.8f;
    /// <summary>
    /// ?먰봽 ?믪씠?낅땲??
    /// </summary>
    public float jumpHeight = 5.0f;
    /// <summary>
    /// ?먰봽 ?믪씠 而ㅻ툕?낅땲??
    /// </summary>
    public AnimationCurve heightCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
    /// <summary>
    /// ?몃━嫄??ш굅由ъ엯?덈떎.
    /// </summary>
    public float maxTriggerRange = 10f;

    [Header("Landing Settings")]
    /// <summary>
    /// 李⑹? 異⑷꺽 諛섍꼍?낅땲??
    /// </summary>
    public float impactRadius = 2.5f;
    /// <summary>
    /// 李⑹? 異⑷꺽 ?쇳빐 ?곗씠?곗엯?덈떎.
    /// </summary>
    public DamageData impactDamage;
    /// <summary>
    /// 李⑹? 吏硫??덉씠?댁엯?덈떎.
    /// </summary>
    public LayerMask groundLayer;
    /// <summary>
    /// 吏硫?泥댄겕 ?쒖옉 ?믪씠?낅땲??
    /// </summary>
    public float groundCheckHeight = 2.0f;
    /// <summary>
    /// 吏硫?泥댄겕 嫄곕━?낅땲??
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
    }

    protected override void OnActionSOTriggered()
    {
        // [?섏젙] ?좊땲硫붿씠???대깽???쒖젏???ㅼ떆媛??뚮젅?댁뼱 ?꾩튂瑜?湲곕컲?쇰줈 紐⑺몴 吏??怨꾩궛
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
        _isJumping = true;
        _nodeEntryTime = Time.time; // ?먰봽 ?쒖옉 ?쒖젏 由ъ뀑
        
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.canMove = false;
            ai.isStopped = true;
        }
        Debug.Log($"Starting jump attack towards {_targetPos}. Distance: {distance}, Jump Distance: {jumpDist} , EnemyName : {runner.name}");
    }

    protected override void UpdateMovement()
    {
        if (!_isJumping && runner.CurrentState == EnemyStateController.EnemyState.Stunned)
        {
            Vector3 landPos = _targetPos;
            Vector3 rayOrigin = landPos + Vector3.up * groundCheckHeight;
            float rayDistance = groundCheckHeight + groundCheckDistance;
            LayerMask rayMask = groundLayer;
            if (rayMask.value == 0)
            {
                rayMask = LayerMask.GetMask("Ground");
            }

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, rayMask, QueryTriggerInteraction.Ignore))
            {
                landPos.y = hit.point.y;
            }
            runner.transform.position = landPos;
            return;
        }
        if(!_isJumping) return;

        float jumpTime = Time.time - _nodeEntryTime;
        float normalizedTime = jumpTime / jumpDuration;

        if (normalizedTime < 1.0f)
        {
            Vector3 currentPos = Vector3.Lerp(_startPos, _targetPos, normalizedTime);
            float height = heightCurve.Evaluate(normalizedTime) * jumpHeight;
            currentPos.y += height;

            runner.transform.position = currentPos;

        }
        else
        {
            Landing();
        }
    }

    protected override bool IsMovementFinished => !_isJumping;

    private void Landing()
    {
        Debug.Log("Landing from jump attack.");
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
        node.transitionBuffer = this.transitionBuffer;
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
