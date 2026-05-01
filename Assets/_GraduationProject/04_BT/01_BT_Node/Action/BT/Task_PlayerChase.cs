using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "Task_PlayerChase", menuName = "BehaviorTree/Action/Task_PlayerChase")]
public class Task_PlayerChase : BaseAttackNode
{
    [Header("Chase Settings")]
    public float maxRushSpeed = 20f;
    public float turnSpeed = 300f;
    public float expectedDuration = 5.0f;
    public AnimationCurve speedCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.2f, 1f), new Keyframe(1, 0));
    public float maxTriggerRange = 20f;

    private float _originalAcceleration;
    private float _originalRotationSpeed;
    private bool _hasHit;
    private bool _isChasing;

    protected override float GetRequiredRange() => maxTriggerRange;

    protected override void InitialMovementSetup()
    {
        _hasHit = false;
        _isChasing = false;

        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null && ai is AIPath aiPath)
        {
            _originalAcceleration = aiPath.maxAcceleration;
            _originalRotationSpeed = aiPath.rotationSpeed;
        }

    }

    protected override void OnActionSOTriggered()
    {
        _isChasing = true;
        
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.isStopped = false;
            ai.canMove = true;
            if (ai is AIPath aiPath)
            {
                aiPath.maxAcceleration = 10000f;
                aiPath.rotationSpeed = turnSpeed;
                aiPath.enableRotation = true;
            }
        }
        runner.Movement.StartOrUpdateChase(runner.player.transform.position);
    }

    protected override void UpdateMovement()
    {
        if (!_isChasing) return; 

        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai == null) return;

        float elapsedTime = Time.time - _nodeEntryTime;
        float normalizedTime = Mathf.Clamp01(elapsedTime / expectedDuration);
        float speedMultiplier = speedCurve.Evaluate(normalizedTime);
        
        ai.maxSpeed = maxRushSpeed * speedMultiplier;
        ai.destination = runner.player.transform.position;

        float currentDist = Vector3.Distance(runner.transform.position, runner.player.transform.position);
        float stopThreshold = GetStoppingDistance();

        if (currentDist <= stopThreshold)
        {
            if (!_hasHit)
            {
                _hasHit = true;
                        brain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
                        brain.blackboard.SetValue(EnemyBlackboardKeys.LastAttackSuccessTime, Time.time);
            }
        }
    }

    private float GetStoppingDistance()
    {
        if (_data == null) return 1.5f;
        float range = 0f;
        switch (_data.shape)
        {
            case AttackShape.Box:
                range = (_data.boxSize.z * 0.5f) + _data.attackOffset.z;
                break;
            case AttackShape.Sphere:
            case AttackShape.Fan:
            default:
                range = _data.damageRadius + _data.attackOffset.z;
                break;
        }
        return Mathf.Max(range, 0.5f);
    }

    protected override bool IsMovementFinished => (Time.time - _nodeEntryTime >= expectedDuration);

    protected override void SpecificCleanup()
    {
        IAstarAI ai = runner.GetComponent<IAstarAI>();
        if (ai != null && ai is AIPath aiPath)
        {
            // 중앙 복구 시스템(BaseAttackNode)이 maxAcceleration을 처리하므로
            // 여기서는 회전 속도만 명시적으로 복구합니다.
            aiPath.rotationSpeed = _originalRotationSpeed;
        }
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
        node.maxRushSpeed = this.maxRushSpeed;
        node.turnSpeed = this.turnSpeed;
        node.expectedDuration = this.expectedDuration;
        node.speedCurve = this.speedCurve;
        node.maxTriggerRange = this.maxTriggerRange;
        node.ExceptKey = this.ExceptKey;
        node.escapeOnHitConfirm = this.escapeOnHitConfirm;
        node.hitEscapeDelay = this.hitEscapeDelay;
        return node;
    }
}
