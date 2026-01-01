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
    public float Radius = 15f;
    public float Delay = 2f;
    public override void OnEnter()
    {
        runner.SetState(Enemy.EnemyState.Patrol);
        _aiPath = runner.GetComponent<AIPath>();
        _hasTarget = false;
        _isWaiting = false;
        _waitTimer = 0f;
    }

    protected override NodeState OnUpdate()
    {
        if(brain.blackboard.GetValue<bool>("DetectPlayer", out bool DetectPlayer) && DetectPlayer)
        {
            return NodeState.FAILURE;
        }
        if (brain._isCombat)
        {
            return NodeState.FAILURE;
        }
        if( _isWaiting)
        {
            _waitTimer += Time.deltaTime;
            if(_waitTimer >= Delay)
            {
                _isWaiting = false;
                _hasTarget = false; // 대기 후 새로운 목표 지점 설정
            }
            else
            {
                return NodeState.RUNNING;
            }
        }
        if (!_hasTarget || (_aiPath != null && _aiPath.reachedDestination))
        {
            Vector3 randomDirection = runner.StartPos + (Random.insideUnitSphere * Radius);

            GraphNode graphNode = AstarPath.active.GetNearest(randomDirection).node;

            if (graphNode != null && !graphNode.Destroyed)
            {
                Vector3 nodePos = (Vector3)graphNode.position;
                Vector3 targetPos = nodePos;
                if (Physics.Raycast(nodePos + Vector3.up * 10f, Vector3.down, out RaycastHit hitInfo, 20f, LayerMask.GetMask("Ground")))
                {
                    targetPos = hitInfo.point;
                }
                runner.Movement.StartOrUpdateChase(targetPos);
                _hasTarget = true;
            }
            if (_hasTarget)
            {
                RaycastHit hit;
                if (Physics.Raycast(runner.transform.position + Vector3.up * 0.25f, runner.transform.forward, out hit, 1f, LayerMask.GetMask("Ground")))
                {
                    _hasTarget = false; // 장애물이 감지되면 새로운 목표 지점을 설정하도록 플래그를 재설정
                }
            }
            if(_hasTarget && !_isWaiting && _aiPath != null && _aiPath.reachedDestination)
            {
                _isWaiting = true; // 도착 후 대기 상태로 전환
                _waitTimer = 0f;
                runner.Movement.StopMovement();
            }
        }
        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        runner.Movement.StopMovement();
    }

    
    public override Node Clone()
    {
        return Instantiate(this);
    }
}