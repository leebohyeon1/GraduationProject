using BehaviorTree;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;
using static Enemy;

public class EnemyMovement : MonoBehaviour
{
    private const float MinimumSafeMoveDistance = 0.05f;

    private Enemy _runner;
    AIPath aIPath;
    public float _normalSpeed{get; private set;}
    private EnemyStateController.EnemyState CurrentState => _runner.CurrentState;

    private RVOController _rvo; 

    [Header("Safety Settings")]
    public LayerMask obstacleMask;         
    public float wallBuffer = 0.5f;        
    public bool AnimationBasedMovement ;

    public float CharacterRadius
    {
        get
        {
            return _rvo != null ? _rvo.radius : 0.5f;
        }
    }   


    public void Initialize(Enemy enemy)
    {
        _runner = enemy;
        _normalSpeed = _runner.enemyStat.MoveSpeed;
        aIPath = _runner.GetComponent<AIPath>();
        aIPath.maxSpeed = _normalSpeed;

        _rvo = _runner.GetComponent<RVOController>();
    }
    public void StartRush(Vector3 targetPosition, float rushSpeed)
    {
        if (aIPath == null)
        {
            return;
        }

        aIPath.enabled = true;
        aIPath.maxSpeed = rushSpeed;
        aIPath.destination = GetNearestSafePosition(targetPosition);
        aIPath.isStopped = false;

    }
    public void StartOrUpdateChase(Vector3 newTarget, EnemyStateController.EnemyState ChaseState = EnemyStateController.EnemyState.Chase, float chaseSpeed = 4)
    {
        bool isRecovering = _runner._stateController != null && _runner._stateController.IsRecoveringFromStun;
        
        if (CurrentState == EnemyStateController.EnemyState.Hit || CurrentState == EnemyStateController.EnemyState.Stunned || CurrentState == EnemyStateController.EnemyState.Die || isRecovering)
        {
            StopMovement();
            return;
        }
        if (aIPath == null) return;

        // aIPath.enabled = true;
        aIPath.canMove = true;       
        aIPath.isStopped = false;    
        aIPath.maxSpeed = chaseSpeed;
        // Debug.Log($"enemy destiniation{newTarget}");
        Vector3 correctedPos = GetVolumeCorrectedPosition(newTarget);
        aIPath.destination = correctedPos;
        
        if (_runner.CurrentState != EnemyStateController.EnemyState.Hit && _runner.CurrentState != EnemyStateController.EnemyState.Attack)
        {
            _runner.SetState(ChaseState);
            _runner.AnimationBool("Walk", true);
        }
        
        if (!aIPath.pathPending) 
        {
            aIPath.SearchPath(); 
        }
    }
    private Vector3 GetVolumeCorrectedPosition(Vector3 targetPos)
    {
        Vector3 myPos = _runner.transform.position;
        Vector3 dir = (targetPos - myPos);
        float dist = dir.magnitude;
        
        if (dist < 0.01f) return targetPos;

        dir.Normalize();
        if (IsPathBlocked(dir, dist, out RaycastHit hit))
        {
            float safeDist = Mathf.Max(0, hit.distance - wallBuffer);
            Vector3 result = myPos + (dir * safeDist);
            // Debug.Log(string.Format("[EnemyMovement : {0}] 경로 막힘 감지. 보정됨: {1} -> {2}", _runner.name, targetPos, result));
            return result;
        }
        return targetPos;
    }
    public bool IsPathBlocked(Vector3 direction, float distance, out RaycastHit hit)
    {
        Vector3 castOrigin = _runner.transform.position + Vector3.up * 0.5f;
        
        if (Physics.SphereCast(castOrigin, CharacterRadius, direction, out hit, distance, obstacleMask))
        {
            return true; 
        }
        
        hit = new RaycastHit(); 
        return false; 
    }

    public Vector3 GetSafeKnockbackPosition(Vector3 startPosition, Vector3 moveDirection, float moveDistance, out bool wasClamped)
    {
        wasClamped = false;

        Vector3 flatDirection = moveDirection;
        flatDirection.y = 0f;
        if (flatDirection.sqrMagnitude <= 0.0001f || moveDistance <= 0f)
        {
            return startPosition;
        }

        flatDirection.Normalize();
        Vector3 desiredTarget = startPosition + flatDirection * moveDistance;
        return GetStraightSafeDestination(startPosition, desiredTarget, out wasClamped);
    }

    public float GetHorizontalDistance(Vector3 from, Vector3 to)
    {
        from.y = 0f;
        to.y = 0f;
        return Vector3.Distance(from, to);
    }

    public bool IsMeaningfulSafeMove(Vector3 from, Vector3 to)
    {
        return GetHorizontalDistance(from, to) > MinimumSafeMoveDistance;
    }

    private Vector3 GetStraightSafeDestination(Vector3 startPosition, Vector3 desiredTarget, out bool wasClamped)
    {
        wasClamped = false;

        Vector3 start = FlattenToGroundPlane(startPosition, startPosition.y);
        Vector3 target = FlattenToGroundPlane(desiredTarget, startPosition.y);
        Vector3 delta = target - start;
        float totalDistance = delta.magnitude;

        if (totalDistance <= MinimumSafeMoveDistance)
        {
            return ResolveBufferedDestination(target, startPosition.y);
        }

        Vector3 direction = delta / totalDistance;
        Vector3 castOrigin = start + Vector3.up * 0.5f;
        float castRadius = CharacterRadius;
        float wallPadding = GetWallPadding();

        if (Physics.SphereCast(castOrigin, castRadius, direction, out RaycastHit hit, totalDistance + wallPadding, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            float safeDistance = Mathf.Max(0f, hit.distance - wallPadding);
            Vector3 clamped = start + direction * safeDistance;
            wasClamped = true;
            return ResolveBufferedDestination(clamped, startPosition.y);
        }

        if (!HasWallClearance(target))
        {
            wasClamped = true;
            return FindLastClearPointOnLine(start, direction, totalDistance, startPosition.y);
        }

        return target;
    }

    private Vector3 FindLastClearPointOnLine(Vector3 start, Vector3 direction, float totalDistance, float yLevel)
    {
        Vector3 best = start;
        float low = 0f;
        float high = totalDistance;

        for (int i = 0; i < 10; i++)
        {
            float mid = (low + high) * 0.5f;
            Vector3 probe = start + direction * mid;
            if (HasWallClearance(probe))
            {
                best = probe;
                low = mid;
            }
            else
            {
                high = mid;
            }
        }

        return ResolveBufferedDestination(best, yLevel);
    }

    private Vector3 ResolveBufferedDestination(Vector3 desiredTarget, float yLevel)
    {
        Vector3 candidate = FlattenToGroundPlane(desiredTarget, yLevel);
        if (HasWallClearance(candidate))
        {
            return candidate;
        }

        if (AstarPath.active != null)
        {
            float step = Mathf.Max(0.25f, CharacterRadius * 0.5f);
            float maxRadius = Mathf.Max(3f, CharacterRadius + GetWallPadding() + 2f);

            for (float radius = step; radius <= maxRadius; radius += step)
            {
                for (int i = 0; i < 16; i++)
                {
                    float angle = i * 22.5f * Mathf.Deg2Rad;
                    Vector3 sample = candidate + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                    NNInfo info = AstarPath.active.GetNearest(sample, NNConstraint.Walkable);
                    if (info.node != null && info.node.Walkable)
                    {
                        Vector3 safePos = FlattenToGroundPlane(info.position, yLevel);
                        if (HasWallClearance(safePos))
                        {
                            return safePos;
                        }
                    }
                }
            }
        }

        return candidate;
    }

    private bool HasWallClearance(Vector3 position)
    {
        float clearanceRadius = CharacterRadius + GetWallPadding();
        Vector3 checkPos = position + Vector3.up * 0.5f;
        return !Physics.CheckSphere(checkPos, clearanceRadius, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private float GetWallPadding()
    {
        return Mathf.Max(0f, wallBuffer);
    }

    private Vector3 FlattenToGroundPlane(Vector3 position, float yLevel)
    {
        position.y = yLevel;
        return position;
    }

    public Vector3 GetNearestSafePosition(Vector3 target)
    {
        float checkRadius = CharacterRadius;
        if (!Physics.CheckSphere(target + Vector3.up * 0.5f, checkRadius, obstacleMask)) return target;

        float step = 0.5f;
        for (float r = step; r <= 3.0f; r += step)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * r;
                NNInfo info = AstarPath.active.GetNearest(target + offset, NNConstraint.Walkable);
                if (info.node != null && info.node.Walkable)
                {
                    if (!Physics.CheckSphere(info.position + Vector3.up * 0.5f, checkRadius, obstacleMask)) return info.position;
                }
            }
        }
        return target;
    }
    public void UpdateStrafeAnim()
    {
        if(!AnimationBasedMovement) return;
        Vector3 worldVelocity = aIPath.velocity;
        Vector3 localVelocity = _runner.transform.InverseTransformDirection(worldVelocity);
        _runner.animator.SetFloat("InputX", localVelocity.x / aIPath.maxSpeed , 0.1f, Time.deltaTime);
        _runner.animator.SetFloat("InputZ", localVelocity.z / aIPath.maxSpeed , 0.1f, Time.deltaTime);
    }

    public void StartOrUpdateChase(Vector3 target)
    {
        StartOrUpdateChase(target, EnemyStateController.EnemyState.Chase);
    }
    public void StopMovement()
    {
        if (aIPath != null)
        {
            aIPath.canMove = false;
            aIPath.isStopped = true;
            aIPath.destination = _runner.transform.position;
            _runner.AnimationBool("Walk", false);
        }
    }
}
