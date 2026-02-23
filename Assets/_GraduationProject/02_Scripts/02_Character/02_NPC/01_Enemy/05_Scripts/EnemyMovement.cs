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

    private RVOController _rvo; // RVO 컴포넌트 참조

    [Header("Safety Settings")]
    // characterRadius 변수 삭제 -> RVOController.radius 사용
    public LayerMask obstacleMask;         // 벽 레이어
    public float wallBuffer = 0.5f;        // 벽 여유 거리
    public bool AnimationBasedMovement ;

    public float CharacterRadius
    {
        get
        {
            // RVOController가 있으면 그 반지름을 사용, 없으면 기본값 0.5f
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
        aIPath.destination = targetPosition;
        aIPath.isStopped = false;

    }
    public void StartOrUpdateChase(Vector3 newTarget, EnemyStateController.EnemyState ChaseState = EnemyStateController.EnemyState.Chase, float chaseSpeed = 4)
    {
        if (CurrentState == EnemyStateController.EnemyState.Stunned || CurrentState == EnemyStateController.EnemyState.Die)
        {
            StopMovement();
            return;
        }
        if (aIPath == null) return;

        aIPath.enabled = true;
        aIPath.canMove = true;       // 이동 권한 부여
        aIPath.isStopped = false;    // 정지 해제
        aIPath.maxSpeed = chaseSpeed;

        aIPath.destination = GetVolumeCorrectedPosition(newTarget);
        if (_runner.CurrentState != EnemyStateController.EnemyState.Hit && _runner.CurrentState != EnemyStateController.EnemyState.Attack)
        {
            _runner.SetState(ChaseState);
            _runner.AnimationBool("Walk", true);
        }
        // Debug.Log($"[Action_Run] 목표: {newTarget} | 현재: {_runner.transform.position} | 남은거리: {Vector3.Distance(_runner.transform.position, newTarget)} aipathstopped: {aIPath.isStopped}");
        
        if (!aIPath.pathPending) 
    {
        aIPath.SearchPath(); // 경로 재계산 강제
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
            // 벽 바로 앞까지만 이동하도록 보정
            float safeDist = Mathf.Max(0, hit.distance - wallBuffer);
            return myPos + (dir * safeDist);
        }
        return targetPos;
    }
    public bool IsPathBlocked(Vector3 direction, float distance, out RaycastHit hit)
    {
        Vector3 castOrigin = _runner.transform.position + Vector3.up * 0.5f;
        
        // SphereCast로 부피 체크
        if (Physics.SphereCast(castOrigin, CharacterRadius, direction, out hit, distance, obstacleMask))
        {
            return true; // 막힘
        }
        
        hit = new RaycastHit(); // 빈 값
        return false; // 뚫림
    }
    public void UpdateStrafeAnim()
    {
        if(!AnimationBasedMovement) return;
        Vector3 worldVelocity = aIPath.velocity;
        Vector3 localVelocity = _runner.transform.InverseTransformDirection(worldVelocity);
        _runner.animator.SetFloat("InputX", localVelocity.x / aIPath.maxSpeed , 0.1f, Time.deltaTime);
        _runner.animator.SetFloat("InputZ", localVelocity.z / aIPath.maxSpeed , 0.1f, Time.deltaTime);
    }

    // Transform을 받는 오버로딩 버전도 유지
    public void StartOrUpdateChase(Vector3 target)
    {
        StartOrUpdateChase(target, EnemyStateController.EnemyState.Chase);
    }
    public void StopMovement()
    {
        // Debug.Log($"[EnemyMovement] StopMovement 호출 - 현재 상태: {CurrentState}");
        aIPath.canMove = false;
        aIPath.isStopped = true;
        aIPath.destination = _runner.transform.position;
        _runner.AnimationBool("Walk", false);

    }
}
