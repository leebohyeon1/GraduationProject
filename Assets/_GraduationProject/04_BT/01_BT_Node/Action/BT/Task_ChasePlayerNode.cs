using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "ChasePlayerNode", menuName = "BehaviorTree/ChasePlayerNode")]
public class Task_ChasePlayerNode : Node
{
    AIPath aIPath;
    private bool _chaseStarted;
    private CharacterController _controller;
    private CharacterController _targetController;
    public float Distance = 3;
    public float speed = 4;
    public override void OnEnter()
    {
        _chaseStarted = false;
        if (!CanControlChase()) return;
        aIPath = runner.GetComponent<AIPath>();
        _controller = runner.GetComponent<CharacterController>();
        _targetController = runner.player ? runner.player.GetComponent<CharacterController>() : null;

        if (aIPath != null)
        {
            aIPath.canMove = true;
            aIPath.isStopped = false;
            aIPath.maxSpeed = speed;
        }

        runner.SetState(EnemyStateController.EnemyState.Chase);
        if (aIPath != null) aIPath.enableRotation = true;
        _chaseStarted = true;
    }
    protected override NodeState OnUpdate()
    {
        if (!_chaseStarted || !CanControlChase()) {
            return NodeState.FAILURE;
        }
        if (runner.player == null)
        {
            return NodeState.FAILURE;
        }
        
        if (HasReachedChaseDistance(runner.transform.position, _controller,
            runner.player.transform.position, _targetController, Distance))
        {
            runner.Movement.StopMovement();
            // Reaching the chase destination ends chasing. Keeping Chase here
            // excludes follow-up attacks whose existing condition requires Idle.
            if (runner.CurrentState == EnemyStateController.EnemyState.Chase &&
                runner._stateController != null && !runner._stateController.IsStateLocked)
                runner.SetState(EnemyStateController.EnemyState.Idle);
            return NodeState.SUCCESS;
        }
        runner.Movement.StartOrUpdateChase(runner.player.transform.position, EnemyStateController.EnemyState.Chase, speed);
        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        if (!_chaseStarted) return;
        _chaseStarted = false;
        // A higher-priority attack/stun may already own movement when this
        // previous chase is aborted. Do not stop that newly selected action.
        if (CanControlChase()) runner.Movement.StopMovement();
    }
    public override void Abort()
    {
        base.Abort();
    }

    private bool CanControlChase()
    {
        if (!runner || runner._stateController == null || runner._stateController.IsStateLocked ||
            (runner._animationBridge != null && runner._animationBridge.IsAttacking)) return false;
        switch (runner.CurrentState)
        {
            case EnemyStateController.EnemyState.Attack:
            case EnemyStateController.EnemyState.Rush:
            case EnemyStateController.EnemyState.Beam:
            case EnemyStateController.EnemyState.Hit:
            case EnemyStateController.EnemyState.Stunned:
            case EnemyStateController.EnemyState.Die:
                return false;
            default:
                return true;
        }
    }

    // Keep the authored root-to-root distance. Also accept physical capsule
    // contact when body size makes that distance impossible to reach.
    // Explicit positions let geometry audits exercise the exact production rule
    // with source-prefab controllers without moving or editing those prefabs.
    public static bool HasReachedChaseDistance(Vector3 chaserPosition, CharacterController chaser,
        Vector3 targetPosition, CharacterController target, float requestedDistance)
    {
        if ((targetPosition - chaserPosition).sqrMagnitude <= requestedDistance * requestedDistance) return true;
        if (!chaser || !target || !chaser.enabled || !target.enabled) return false;

        WorldCapsule(chaserPosition, chaser, out Vector3 a, out float aHalfSegment, out float aRadius);
        WorldCapsule(targetPosition, target, out Vector3 b, out float bHalfSegment, out float bRadius);
        float verticalGap = Mathf.Max(0f, Mathf.Abs(a.y - b.y) - aHalfSegment - bHalfSegment);
        const float contactTolerance = .05f;
        float contactRadius = aRadius + bRadius + contactTolerance;
        if (verticalGap > contactRadius) return false;
        float allowedHorizontalSquared = contactRadius * contactRadius - verticalGap * verticalGap;
        float dx = a.x - b.x, dz = a.z - b.z;
        return dx * dx + dz * dz <= allowedHorizontalSquared;
    }

    private static void WorldCapsule(Vector3 rootPosition, CharacterController controller,
        out Vector3 center, out float halfSegment, out float radius)
    {
        Vector3 scale = controller.transform.lossyScale;
        radius = controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        halfSegment = Mathf.Max(0f, controller.height * Mathf.Abs(scale.y) * .5f - radius);
        center = rootPosition + controller.transform.TransformVector(controller.center);
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}
