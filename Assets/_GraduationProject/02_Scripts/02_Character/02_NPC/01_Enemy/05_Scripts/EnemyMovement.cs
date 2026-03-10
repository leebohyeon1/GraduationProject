using BehaviorTree;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Enemy _runner;
    private AIPath _aIPath;
    public float _normalSpeed { get; private set; }
    private RVOController _rvo; 

    [Header("Safety Settings")]
    public LayerMask obstacleMask;         
    public float wallBuffer = 0.5f;        
    public bool AnimationBasedMovement;

    public float CharacterRadius => _rvo != null ? _rvo.radius : 0.5f;

    public void Initialize(Enemy enemy)
    {
        _runner = enemy;
        _normalSpeed = _runner.enemyStat.MoveSpeed;
        _aIPath = _runner.GetComponent<AIPath>();
        _rvo = _runner.GetComponent<RVOController>();
        
        if (_aIPath != null)
        {
            _aIPath.maxSpeed = _normalSpeed;
            _aIPath.autoRepath.mode = AutoRepathPolicy.Mode.EveryNSeconds;
            _aIPath.autoRepath.period = UnityEngine.Random.Range(0.4f, 0.6f);
        }
    }

    public void StartRush(Vector3 targetPosition, float rushSpeed)
    {
        if (_aIPath == null) return;
        _aIPath.enabled = true;
        _aIPath.canMove = true;
        _aIPath.isStopped = false;
        _aIPath.maxSpeed = rushSpeed;
        _aIPath.destination = targetPosition;
        _aIPath.SearchPath(); // Force immediate path calc
    }

    public void StartOrUpdateChase(Vector3 target)
    {
        StartOrUpdateChase(target, EnemyStateController.EnemyState.Chase);
    }

    public void StartOrUpdateChase(Vector3 newTarget, EnemyStateController.EnemyState chaseState = EnemyStateController.EnemyState.Chase, float chaseSpeed = 4)
    {
        bool isRecovering = _runner._stateController != null && _runner._stateController.IsRecoveringFromStun;
        if (_runner.CurrentState == EnemyStateController.EnemyState.Stunned || _runner.CurrentState == EnemyStateController.EnemyState.Die || isRecovering)
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
        
        // [Crucial Fix] 정지 상태에서 다시 움직일 때는 강제로 경로 재탐색을 트리거하여 엔진을 깨움
        if (wasStopped && !_aIPath.pathPending)
        {
            _aIPath.SearchPath();
        }
        
        if (_runner.CurrentState != EnemyStateController.EnemyState.Hit && _runner.CurrentState != EnemyStateController.EnemyState.Attack)
        {
            _runner.SetState(chaseState);
            _runner.AnimationBool("Walk", true);
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
            return myPos + (dir * safeDist);
        }
        return targetPos;
    }

    public bool IsPathBlocked(Vector3 direction, float distance, out RaycastHit hit)
    {
        Vector3 castOrigin = _runner.transform.position + Vector3.up * 0.5f;
        return Physics.SphereCast(castOrigin, CharacterRadius, direction, out hit, distance, obstacleMask);
    }

    public void UpdateStrafeAnim()
    {
        if(!AnimationBasedMovement || _aIPath == null) return;
        Vector3 localVelocity = _runner.transform.InverseTransformDirection(_aIPath.velocity);
        _runner.animator.SetFloat("InputX", localVelocity.x / _aIPath.maxSpeed , 0.1f, Time.deltaTime);
        _runner.animator.SetFloat("InputZ", localVelocity.z / _aIPath.maxSpeed , 0.1f, Time.deltaTime);
    }

    public void StopMovement()
    {
        if (_aIPath != null)
        {
            _aIPath.isStopped = true;
            _aIPath.destination = _runner.transform.position;
            _runner.AnimationBool("Walk", false);
        }
    }
}
