using UnityEngine;
using BehaviorTree;
using Pathfinding;

/// <summary>
/// ?뱀젙 吏?먯쓣 湲곗??쇰줈 醫뚯슦瑜??뺣났?섎ŉ ?대룞?섎뒗 ?몃뱶?낅땲??
/// 諛⑺뼢??諛붽? ?뚮쭏??理쒖냼/理쒕? ?몄씠利?媛믪쓣 ?곸슜?섎ŉ, 踰쎄낵??嫄곕━瑜?怨꾩궛?섏뿬 ?꾨떖 媛?ν븳 吏?먯쑝濡??먮룞 蹂댁젙?⑸땲??
/// </summary>
[CreateAssetMenu(fileName = "Task_SlideMove", menuName = "BehaviorTree/Action/Task_SlideMove")]
public class Task_SlideMove : Node
{
    [Header("Slide Settings")]
    [Tooltip("湲곕낯 ?대룞 嫄곕━(m)")]
    public float slideDistance = 3.0f;
    
    [Tooltip("嫄곕━ 蹂??理쒖냼媛?(m)")]
    public float minNoise = -1.0f;
    
    [Tooltip("嫄곕━ 蹂??理쒕?媛?(m)")]
    public float maxNoise = 1.0f;

    [Tooltip("?대룞 ?띾룄")]
    public float moveSpeed = 5.0f;
    
    [Tooltip("紐⑺몴 ?꾨떖 ?먯젙 嫄곕━")]
    public float arrivalThreshold = 0.3f;

    private Vector3 _centerPos;
    private Vector3 _currentTarget;
    private bool _movingRight = true;

    public override void OnEnter()
    {
        base.OnEnter();
        _centerPos = runner.transform.position;
        
        // 珥덇린 諛⑺뼢 ?쒕뜡 寃곗젙
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
        
        // [?ъ슜???붿껌] 紐ъ뒪?곗쓽 遺?쇱? 踰??ъ씠??嫄곕━瑜?怨꾩궛?섏뿬 ?꾨떖 媛?ν븳 吏?먯쑝濡?蹂댁젙
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
        LayerMask mask = LayerMask.GetMask("Environment", "Wall"); // 湲곕낯媛?

        if (runner.Movement != null)
        {
            radius = runner.Movement.CharacterRadius;
            wallBuffer = runner.Movement.wallBuffer;
            mask = runner.Movement.obstacleMask;
        }

        // SphereCast瑜??ъ슜?섏뿬 罹먮┃??遺??Radius)瑜?怨좊젮??異⑸룎 泥댄겕
        // 紐⑺몴 諛⑺뼢?쇰줈 罹먮┃?곌? ?쇱? ?딄퀬 媛????덈뒗 理쒕? 吏?먯쓣 李얠뒿?덈떎.
        Vector3 rayOrigin = start + Vector3.up * 0.5f;
        if (Physics.SphereCast(rayOrigin, radius, dir, out RaycastHit hit, dist + wallBuffer, mask))
        {
            // 踰쎌뿉 遺?ろ삍?ㅻ㈃ ?덉쟾 嫄곕━(wallBuffer)留뚰겮 ?ㅻ줈 類띾땲??
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
