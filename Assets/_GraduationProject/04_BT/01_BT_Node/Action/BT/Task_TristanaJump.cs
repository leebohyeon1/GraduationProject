using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "Task_TristanaJump", menuName = "BehaviorTree/Action/Task_TristanaJump")]
public class Task_TristanaJump : BaseAttackNode
{
    [Header("Jump Settings")]
    public float jumpRange = 8.0f;
    public float jumpDuration = 0.8f;
    public float jumpHeight = 5.0f;
    public AnimationCurve heightCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
    public float maxTriggerRange = 10f;

    [Header("Landing Settings")]
    public float impactRadius = 2.5f;
    public DamageData impactDamage;

    private Vector3 _startPos;
    private Vector3 _targetPos;
    private bool _isJumping;

    protected override float GetRequiredRange() => maxTriggerRange;

    protected override void InitialMovementSetup()
    {
        _isJumping = false;

        _startPos = runner.transform.position;
        Vector3 playerPos = runner.player.transform.position;
        Vector3 direction = (playerPos - _startPos);
        direction.y = 0;
        float distance = direction.magnitude;
        direction.Normalize();

        float jumpDist = Mathf.Min(distance, jumpRange);
        _targetPos = _startPos + (direction * jumpDist);

        NNInfo info = AstarPath.active.GetNearest(_targetPos, NNConstraint.Walkable);
        if (info.node != null) _targetPos = info.position;
        Log("점프 목표 계산 완료: " + _targetPos);
    }

    protected override void OnActionSOTriggered()
    {
        Log("점프 시작 (OnActionSOTriggered)");
        _isJumping = true;
        _nodeEntryTime = Time.time;
        
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.canMove = false;
            ai.isStopped = true;
        }
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
        landPos.y = AstarPath.active.GetNearest(landPos).position.y;
        runner.transform.position = landPos;

        Collider[] hitColliders = Physics.OverlapSphere(landPos, impactRadius, LayerMask.GetMask("Player"));
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                impactDamage.AttackerTransform = runner.transform;
                playerHealth.TakeDamage(impactDamage);
                brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
            }
        }

        runner.animator.SetBool("IsRushing", true);
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
        node.continuousRotation = this.continuousRotation;
        node.maintainAtk = this.maintainAtk;
        node.SO = this.SO;
        node.LoopAttack = this.LoopAttack;
        node.NextBT = this.NextBT;
        node.debugMode = this.debugMode;
        node.checkRangeOnEnter = this.checkRangeOnEnter;
        node.rangeThreshold = this.rangeThreshold;
        node.jumpRange = this.jumpRange;
        node.jumpDuration = this.jumpDuration;
        node.jumpHeight = this.jumpHeight;
        node.heightCurve = this.heightCurve;
        node.impactRadius = this.impactRadius;
        node.impactDamage = this.impactDamage;
        node.maxTriggerRange = this.maxTriggerRange;
        node.ExceptKey = this.ExceptKey;
        return node;
    }
}
