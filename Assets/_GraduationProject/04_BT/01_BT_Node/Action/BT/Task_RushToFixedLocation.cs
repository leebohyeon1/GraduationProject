using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "Task_RushToFixedLocation", menuName = "BehaviorTree/Action/Task_RushToFixedLocation")]
public class Task_RushToFixedLocation : BaseAttackNode
{
    [Header("Rush Settings")]
    public float rushSpeed = 20f;
    public float hitRadius = 1.5f;
    public float overshootDist = 3.0f;
    public LayerMask obstacleMask;
    public float rushDuration = 1.0f;
    public AnimationCurve rushCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.5f), new Keyframe(1, 0));
    public float turnSpeed = 10f;
    public float maxTriggerRange = 15f;

    private Vector3 _targetPos;
    private bool _isRushing;
    private float _rushStartTime;

    protected override float GetRequiredRange() => maxTriggerRange;

    protected override void InitialMovementSetup()
    {
        _rushStartTime = 0;
        _isRushing = false;
        runner.aIPath.enableRotation = false;
    }

    protected override void OnActionSOTriggered()
    {
        Vector3 playerPos = runner.player.transform.position;
        Vector3 myPos = runner.transform.position;
        Vector3 dir = (playerPos - myPos);
        dir.y = 0;
        if (dir == Vector3.zero) dir = runner.transform.forward;
        dir.Normalize();

        Vector3 offset = Quaternion.Euler(0, Random.Range(0, 360), 0) * new Vector3(0.5f, 0, 0);
        Vector3 rawTarget = playerPos + (dir * overshootDist) + offset;
        
        NNInfo info = AstarPath.active.GetNearest(rawTarget, NNConstraint.Walkable);
        Vector3 candidateTarget = info.node != null ? info.position : rawTarget;
        _targetPos = GetStraightSafeDestination(myPos, candidateTarget, out _);

        
        _isRushing = true;
        _rushStartTime = Time.time;
        
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.isStopped = true; 
            ai.canMove = false; 
            ai.destination = runner.transform.position;
        }
    }

    protected override void UpdateMovement()
    {
        if (!_isRushing) return;

        float rushElapsedTime = Time.time - _rushStartTime;
        float normalizedTime = rushElapsedTime / rushDuration;

        if (normalizedTime >= 1.0f)
        {
            _isRushing = false;
            return;
        }

        float speedMultiplier = rushCurve.Evaluate(normalizedTime);
        float currentSpeed = rushSpeed * speedMultiplier;

        float step = currentSpeed * Time.deltaTime;
        Vector3 currentPos = runner.transform.position;
        Vector3 nextPos = Vector3.MoveTowards(currentPos, _targetPos, step);
        Vector3 moveDir = (nextPos - currentPos).normalized;
        moveDir.y = 0;

        float moveDist = Vector3.Distance(currentPos, nextPos);

        if (moveDist > 0.0001f)
        {
            Vector3 safeNextPos = GetSafeStepDestination(currentPos, moveDir, moveDist, out bool blockedByWall);
            float actualMoveDistance = GetHorizontalDistance(currentPos, safeNextPos);

            if (actualMoveDistance > minimumMovementDistance)
            {
                CharacterController cc = runner.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.Move(safeNextPos - currentPos);
                }
                else
                {
                    runner.transform.position = safeNextPos;
                }
                
                IAstarAI ai = runner.GetComponent<IAstarAI>();
                if (ai != null) ai.Teleport(runner.transform.position);

                if (blockedByWall)
                {
                    _isRushing = false;
                    return;
                }
            }
            else
            {
                _isRushing = false;
                return;
            }
        }

        if (Vector3.Distance(runner.transform.position, runner.player.transform.position) <= hitRadius)
        {
            _isRushing = false;
                brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
                brain.blackboard.SetValue(EnemyBlackboardKeys.LastAttackSuccessTime, Time.time);
            return;
        }

        if (Vector3.Distance(runner.transform.position, _targetPos) < 0.1f)
        {
            _isRushing = false;
        }
    }

    protected override bool IsMovementFinished => !_isRushing;

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
        node.hitRadius = this.hitRadius;
        node.overshootDist = this.overshootDist;
        node.obstacleMask = this.obstacleMask;
        node.rushDuration = this.rushDuration;
        node.rushCurve = this.rushCurve;
        node.turnSpeed = this.turnSpeed;
        node.maxTriggerRange = this.maxTriggerRange;
        node.ExceptKey = this.ExceptKey;
        node.escapeOnHitConfirm = this.escapeOnHitConfirm;
        node.hitEscapeDelay = this.hitEscapeDelay;
        return node;
    }
}
