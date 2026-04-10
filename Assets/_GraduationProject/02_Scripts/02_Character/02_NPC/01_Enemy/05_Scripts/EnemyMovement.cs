using BehaviorTree;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;
using static Enemy;

public class EnemyMovement : MonoBehaviour
{
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
        
        if (CurrentState == EnemyStateController.EnemyState.Stunned || CurrentState == EnemyStateController.EnemyState.Die || isRecovering)
        {
            StopMovement();
            return;
        }
        if (aIPath == null) return;

        aIPath.enabled = true;
        aIPath.canMove = true;       
        aIPath.isStopped = false;    
        aIPath.maxSpeed = chaseSpeed;

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
