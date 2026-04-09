using BehaviorTree;
using Pathfinding;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomPatrol", menuName = "BehaviorTree/RandomPatrol")]
public class RandomPatrol : Node
{
    AIPath _aiPath;
    bool _hasTarget = false;
    bool _isWaiting = false;
    float _waitTimer = 0f;
    
    [Header("Patrol Settings")]
    public float Radius = 15f;
    public float Delay = 2f;
    public float MoveSpeed = 6.0f;
    
    [Header("Stuck Detection")]
    public float StuckThreshold = 0.2f; 
    public float StuckTimeout = 2.0f;    
    private Vector3 _lastPosition;
    private float _stuckTimer = 0f;

    public override void OnEnter()
    {
        runner.SetState(EnemyStateController.EnemyState.Patrol);
        _aiPath = runner.GetComponent<AIPath>();
        if (_aiPath != null) { _aiPath.canMove = true; _aiPath.isStopped = false; }
        _hasTarget = false;
        _isWaiting = false;
        _waitTimer = 0f;
        _stuckTimer = 0f;
        _lastPosition = runner.transform.position;
    }

    protected override NodeState OnUpdate()
    {
        if (brain == null || brain.blackboard == null) return NodeState.FAILURE;
        if (_aiPath == null) _aiPath = runner.GetComponent<AIPath>();
        if (brain.blackboard.GetValue<bool>("DetectPlayer", out bool detectPlayer) && detectPlayer) return NodeState.FAILURE;
        if (brain._isCombat) return NodeState.FAILURE;

        if (_aiPath != null)
        {
            if (!_aiPath.canMove) _aiPath.canMove = true;
            if (_aiPath.isStopped && !_isWaiting) _aiPath.isStopped = false;
        }

        if (_isWaiting)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= Delay) { _isWaiting = false; _hasTarget = false; }
            else return NodeState.RUNNING;
        }

        if (!_hasTarget || (_aiPath != null && _aiPath.reachedDestination))
        {
            if (_hasTarget && _aiPath != null && _aiPath.reachedDestination)
            {
                _isWaiting = true; _waitTimer = 0f;
                runner.Movement.StopMovement();
                return NodeState.RUNNING;
            }

            Vector3 targetPos = FindValidRandomPoint(); if (targetPos != Vector3.zero) targetPos = runner.Movement.GetNearestSafePosition(targetPos);
            if (targetPos != Vector3.zero)
            {
                runner.Movement.StartOrUpdateChase(targetPos, EnemyStateController.EnemyState.Patrol, MoveSpeed);
                _hasTarget = true;
                _stuckTimer = 0f;
                _lastPosition = runner.transform.position;
                if (_aiPath != null && !_aiPath.pathPending) _aiPath.SearchPath();
            }
        }
        else
        {
            float distMoved = Vector3.Distance(runner.transform.position, _lastPosition);
            if (distMoved < StuckThreshold * Time.deltaTime * MoveSpeed)
            {
                _stuckTimer += Time.deltaTime;
                if (_stuckTimer >= StuckTimeout) { _hasTarget = false; _stuckTimer = 0f; }
            }
            else { _stuckTimer = 0f; _lastPosition = runner.transform.position; }
        }
        return NodeState.RUNNING;
    }

    private Vector3 FindValidRandomPoint()
    {
        GraphNode currentNode = AstarPath.active.GetNearest(runner.transform.position, NNConstraint.Default).node;
        if (currentNode == null) return Vector3.zero;

        float agentRadius = runner.Movement.CharacterRadius;

        for (int i = 0; i < 15; i++)
        {
            Vector3 randomPos = runner.StartPos + (Random.insideUnitSphere * Radius);
            NNInfo info = AstarPath.active.GetNearest(randomPos, NNConstraint.Default);
            GraphNode targetNode = info.node;

            if (targetNode != null && targetNode.Walkable && !targetNode.Destroyed)
            {
                // [Fix] 인접 노드 체크: Physics.OverlapSphere를 사용하여 더 확실하게 벽과의 거리 확보
                if (Physics.CheckSphere(info.position + Vector3.up * 0.5f, agentRadius * 1.5f, LayerMask.GetMask("Wall", "Default")))
                    continue;

                if (PathUtilities.IsPathPossible(currentNode, targetNode))
                {
                    Vector3 nodePos = (Vector3)targetNode.position;
                    if (Physics.Raycast(nodePos + Vector3.up * 5f, Vector3.down, out RaycastHit hitInfo, 10f, LayerMask.GetMask("Ground")))
                        return hitInfo.point;
                    return nodePos;
                }
            }
        }
        return Vector3.zero;
    }

    public override void OnExit() { runner.Movement.StopMovement(); }
    public override Node Clone() => Instantiate(this);
}
