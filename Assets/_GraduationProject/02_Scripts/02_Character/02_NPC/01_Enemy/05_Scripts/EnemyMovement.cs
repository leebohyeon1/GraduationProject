using BehaviorTree;
using Pathfinding;
using UnityEngine;
using static Enemy;

public class EnemyMovement
{
    private Enemy _runner;
    AIPath aIPath;
    Rigidbody rb;
    private EnemyStateController.EnemyState CurrentState => _runner.CurrentState;
    
    public float _normalSpeed {get; private set; } = 2f;
    public EnemyMovement(Enemy enemy)
    {
        _runner = enemy;
        _normalSpeed = _runner.NormalSpeed;
        aIPath = _runner.GetComponent<AIPath>();
        aIPath.maxSpeed = _normalSpeed;
        rb = _runner.GetComponent<Rigidbody>();
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

        aIPath.destination = newTarget;
        
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
    
    public void UpdateStrafeAnim()
    {
        if(!_runner.AnimationBasedMovement) return;
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
        aIPath.canMove = false;
        aIPath.isStopped = true;
        _runner.AnimationBool("Walk", false);

    }
}
