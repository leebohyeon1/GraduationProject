using UnityEngine;
using BehaviorTree;
using Pathfinding;

/// <summary>
/// 특정 지점을 기준으로 좌우를 왕복하며 이동하는 노드입니다.
/// 방향을 바꿀 때마다 최소/최대 노이즈 값을 적용하며, 벽과의 거리를 계산하여 도달 가능한 지점으로 자동 보정합니다.
/// </summary>
[CreateAssetMenu(fileName = "Task_SlideMove", menuName = "BehaviorTree/Action/Task_SlideMove")]
public class Task_SlideMove : Node
{
    [Header("Slide Settings")]
    [Tooltip("기본 이동 거리(m)")]
    public float slideDistance = 3.0f;
    
    [Tooltip("거리 변동 최소값 (m)")]
    public float minNoise = -1.0f;
    
    [Tooltip("거리 변동 최대값 (m)")]
    public float maxNoise = 1.0f;

    [Tooltip("이동 속도")]
    public float moveSpeed = 5.0f;
    
    [Tooltip("목표 도달 판정 거리")]
    public float arrivalThreshold = 0.3f;

    private Vector3 _centerPos;
    private Vector3 _currentTarget;
    private bool _movingRight = true;

    public override void OnEnter()
    {
        base.OnEnter();
        _centerPos = runner.transform.position;
        
        // 초기 방향 랜덤 결정
        _movingRight = Random.value > 0.5f;
        
        SetNextTarget();

        AIPath ai = runner.aIPath;
        if (ai != null)
        {
            ai.canMove = true;
            ai.isStopped = false;
            ai.maxSpeed = moveSpeed;
            ai.destination = _currentTarget;
            ai.endReachedDistance = arrivalThreshold;
            ai.enableRotation = false; 
        }

        Debug.Log($"<color=cyan>[Task_SlideMove]</color> 슬라이드 시작. 기준점: {_centerPos}, 첫 목표: {(_movingRight ? "Right" : "Left")}");
    }

    protected override NodeState OnUpdate()
    {
        AIPath ai = runner.aIPath;
        if (runner == null || ai == null) return NodeState.FAILURE;

        float distToTarget = Vector3.Distance(runner.transform.position, _currentTarget);
        bool reached = distToTarget <= arrivalThreshold || ai.reachedDestination;

        if (reached)
        {
            _movingRight = !_movingRight;
            SetNextTarget();
            
            ai.destination = _currentTarget;
            ai.endReachedDistance = arrivalThreshold;
            
            float actualDist = Vector3.Distance(_centerPos, _currentTarget);
            Debug.Log($"<color=cyan>[Task_SlideMove]</color> 목표 도달. 방향 전환 -> {(_movingRight ? "Right" : "Left")}, 실제 거리: {actualDist:F2}m");
        }
        else
        {
            ai.destination = _currentTarget;
        }

        if (runner.Movement != null)
        {
            runner.Movement.UpdateStrafeAnim();
        }

        return NodeState.RUNNING;
    }

    private void SetNextTarget()
    {
        Vector3 toPlayer = (runner.player.transform.position - _centerPos).normalized;
        Vector3 rightDir = Vector3.Cross(Vector3.up, toPlayer).normalized;
        if (rightDir == Vector3.zero) rightDir = runner.transform.right;

        float noise = Random.Range(minNoise, maxNoise);
        float finalDist = slideDistance + noise;
        finalDist = Mathf.Max(0.1f, finalDist);

        Vector3 rawTarget = _movingRight ? (_centerPos + rightDir * finalDist) : (_centerPos - rightDir * finalDist);
        
        // [사용자 요청] 몬스터의 부피와 벽 사이의 거리를 계산하여 도달 가능한 지점으로 보정
        _currentTarget = GetAdjustedTarget(_centerPos, rawTarget);
    }

    private Vector3 GetAdjustedTarget(Vector3 start, Vector3 end)
    {
        Vector3 dir = (end - start);
        float dist = dir.magnitude;
        if (dist < 0.01f) return end;
        dir.Normalize();

        float radius = 0.5f;
        float wallBuffer = 0.5f;
        LayerMask mask = LayerMask.GetMask("Environment", "Wall"); // 기본값

        if (runner.Movement != null)
        {
            radius = runner.Movement.CharacterRadius;
            wallBuffer = runner.Movement.wallBuffer;
            mask = runner.Movement.obstacleMask;
        }

        // SphereCast를 사용하여 캐릭터 부피(Radius)를 고려한 충돌 체크
        // 목표 방향으로 캐릭터가 끼지 않고 갈 수 있는 최대 지점을 찾습니다.
        Vector3 rayOrigin = start + Vector3.up * 0.5f;
        if (Physics.SphereCast(rayOrigin, radius, dir, out RaycastHit hit, dist + wallBuffer, mask))
        {
            // 벽에 부딪혔다면 안전 거리(wallBuffer)만큼 뒤로 뺍니다.
            float safeDist = Mathf.Max(0, hit.distance - wallBuffer);
            return start + (dir * safeDist);
        }

        return GetValidPoint(end);
    }

    private Vector3 GetValidPoint(Vector3 rawPoint)
    {
        if (AstarPath.active == null) return rawPoint;
        NNInfo info = AstarPath.active.GetNearest(rawPoint, NNConstraint.Walkable);
        return info.node != null ? (Vector3)info.position : rawPoint;
    }

    public override void OnExit()
    {
        base.OnExit();
        AIPath ai = runner.aIPath;
        if (runner != null && ai != null)
        {
            ai.isStopped = true;
            ai.maxSpeed = runner.Movement._normalSpeed;
            ai.enableRotation = true;
            ai.endReachedDistance = 0.2f;
        }
    }

    public override Node Clone()
    {
        Task_SlideMove node = Instantiate(this);
        node.slideDistance = slideDistance;
        node.minNoise = minNoise;
        node.maxNoise = maxNoise;
        node.moveSpeed = moveSpeed;
        node.arrivalThreshold = arrivalThreshold;
        return node;
    }
}
