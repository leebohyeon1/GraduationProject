using BehaviorTree;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;

/// <summary>
/// 몬스터의 이동을 제어합니다. ITraversalProvider를 통해 몸집에 맞는 경로만 탐색합니다.
/// </summary>
public class EnemyMovement : MonoBehaviour
{
    private Enemy _runner;
    private AIPath _aIPath;
    private Seeker _seeker;
    public float _normalSpeed { get; private set; }
    private RVOController _rvo; 

    [Header("Safety Settings")]
    public LayerMask obstacleMask;         
    public float wallBuffer = 0.6f; 
    public float wallBufferMultiplier = 1.3f; 
    public bool AnimationBasedMovement;

    public float CharacterRadius => _rvo != null ? _rvo.radius : 0.5f;

    public void Initialize(Enemy enemy)
    {
        _runner = enemy;
        _normalSpeed = _runner.enemyStat.MoveSpeed;
        _aIPath = _runner.GetComponent<AIPath>();
        _seeker = _runner.GetComponent<Seeker>();
        _rvo = _runner.GetComponent<RVOController>();
        
        // [Fix] AIDestinationSetter가 있다면 비활성화 (BT에서 직접 목적지 제어)
        var ds = _runner.GetComponent<AIDestinationSetter>();
        if (ds != null) ds.enabled = false;

        if (_aIPath != null)
        {
            _aIPath.maxSpeed = _normalSpeed;
            _aIPath.autoRepath.mode = AutoRepathPolicy.Mode.EveryNSeconds;
            _aIPath.autoRepath.period = UnityEngine.Random.Range(0.4f, 0.6f);
            
            // [최적화] 몸집 대비 좁은 길 탐색 자체를 차단 (Return to sender if path is too narrow)
            float nodeSize = AstarPath.active.data.gridGraph.nodeSize;
            _seeker.traversalProvider = new RadiusTraversalProvider(CharacterRadius * wallBufferMultiplier, nodeSize, obstacleMask);
        }
    }
    public void StartRush(Vector3 targetPosition, float rushSpeed)
    {
        if (_aIPath == null) return;
        
        _aIPath.enabled = true;
        _aIPath.canMove = true;
        _aIPath.isStopped = false;
        _aIPath.maxSpeed = rushSpeed;
        
        Vector3 newDest = GetNearestSafePosition(targetPosition);
        Debug.Log($"[EnemyMovement] StartRush - From: {transform.position}, To: {newDest}, Target: {targetPosition}");
        
        if (Vector3.Distance(_aIPath.destination, newDest) > 0.1f)
        {
            _aIPath.destination = newDest;
            _aIPath.SearchPath();
        }
    }
    public void StartOrUpdateChase(Vector3 target) => StartOrUpdateChase(target, EnemyStateController.EnemyState.Chase);

    public void StartOrUpdateChase(Vector3 newTarget, EnemyStateController.EnemyState chaseState = EnemyStateController.EnemyState.Chase, float chaseSpeed = 4)
    {
        if (_runner.CurrentState == EnemyStateController.EnemyState.Stunned || _runner.CurrentState == EnemyStateController.EnemyState.Die)
        {
            StopMovement();
            return;
        }

        if (_aIPath == null) return;

        bool wasStopped = _aIPath.isStopped || !_aIPath.canMove || !_aIPath.enabled;
        _aIPath.enabled = true;
        _aIPath.canMove = true;       
        _aIPath.isStopped = false;    
        _aIPath.maxSpeed = chaseSpeed;
        
        _aIPath.destination = GetVolumeCorrectedPosition(newTarget);
        
        if (wasStopped && !_aIPath.pathPending) _aIPath.SearchPath();
        
        if (_runner.CurrentState != EnemyStateController.EnemyState.Hit && _runner.CurrentState != EnemyStateController.EnemyState.Attack)
        {
            _runner.SetState(chaseState);
            _runner.AnimationBool("Walk", true);
        }
    }

    public Vector3 GetNearestSafePosition(Vector3 target)
    {
        float checkRadius = CharacterRadius * wallBufferMultiplier;
        if (!Physics.CheckSphere(target + Vector3.up * 0.5f, checkRadius, obstacleMask))
        {
            Debug.Log($"[EnemyMovement] Target position {target} is safe for rush.");
          return target;  
        } 

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

    private Vector3 GetVolumeCorrectedPosition(Vector3 targetPos)
    {
        Vector3 myPos = _runner.transform.position;
        Vector3 dir = (targetPos - myPos);
        float dist = dir.magnitude;
        if (dist < 0.01f) return targetPos;

        dir.Normalize();
        float safeBuffer = Mathf.Max(wallBuffer, CharacterRadius * wallBufferMultiplier);
        if (IsPathBlocked(dir, dist, out RaycastHit hit))
        {
            return myPos + (dir * Mathf.Max(0, hit.distance - safeBuffer));
        }
        return targetPos;
    }

    public bool IsPathBlocked(Vector3 direction, float distance, out RaycastHit hit)
    {
        Vector3 castOrigin = _runner.transform.position + Vector3.up * 0.5f;
        return Physics.SphereCast(castOrigin, CharacterRadius * 0.9f, direction, out hit, distance, obstacleMask);
    }

    public void UpdateStrafeAnim()
    {
        if(!AnimationBasedMovement || _aIPath == null) return;
        Vector3 localVelocity = _runner.transform.InverseTransformDirection(_aIPath.velocity);
        _runner.animator.SetFloat("InputX", localVelocity.x / _aIPath.maxSpeed, 0.1f, Time.deltaTime);
        _runner.animator.SetFloat("InputZ", localVelocity.z / _aIPath.maxSpeed, 0.1f, Time.deltaTime);
    }

    public void StopMovement()
    {
        if (_aIPath != null)
        {
            _aIPath.isStopped = true;
            _aIPath.destination = _runner.transform.position;
            _runner.AnimationBool("Walk", false);
            Debug.Log($"[EnemyMovement] StopMovement called by: {System.Environment.StackTrace}");
        }
    }
}
