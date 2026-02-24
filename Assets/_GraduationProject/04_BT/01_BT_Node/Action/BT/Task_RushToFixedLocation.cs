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
        Log("돌진 준비 (ActionSO 대기 중)");
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
        _targetPos = info.node != null ? info.position : rawTarget;

        Log("돌진 시작 (OnActionSOTriggered) - 실시간 목표 설정: " + _targetPos);
        
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
            Log("돌진 시간 종료");
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
            if (!Physics.Raycast(currentPos + Vector3.up * 0.5f, moveDir, moveDist + 1f, obstacleMask))
            {
                CharacterController cc = runner.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.Move(nextPos - currentPos);
                }
                else
                {
                    runner.transform.position = nextPos;
                }
                
                IAstarAI ai = runner.GetComponent<IAstarAI>();
                if (ai != null) ai.Teleport(runner.transform.position);
            }
            else
            {
                Log("벽 충돌로 돌진 중단");
                _isRushing = false;
                return;
            }
        }

        if (Vector3.Distance(runner.transform.position, runner.player.transform.position) <= hitRadius)
        {
            Log("플레이어 접촉으로 돌진 중단");
            _isRushing = false;
            brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
            return;
        }

        if (Vector3.Distance(runner.transform.position, _targetPos) < 0.1f)
        {
            Log("목표 지점 도달");
            _isRushing = false;
        }
    }

    protected override bool IsMovementFinished => !_isRushing;

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
