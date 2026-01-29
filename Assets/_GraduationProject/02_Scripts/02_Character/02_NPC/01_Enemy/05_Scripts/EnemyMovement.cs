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
        if (_runner.CurrentState != EnemyStateController.EnemyState.Hit && _runner.CurrentState != EnemyStateController.EnemyState.Attack)
        {
            _runner.SetState(ChaseState);
            _runner.AnimationBool("Walk", true);
        }
        aIPath.enabled = true;
        aIPath.canMove = true;
        aIPath.maxSpeed = chaseSpeed;
        aIPath.isStopped = false;
        aIPath.destination = newTarget;
        // Debug.Log($"[Action_Run] 목표: {newTarget} | 현재: {_runner.transform.position} | 남은거리: {Vector3.Distance(_runner.transform.position, newTarget)} aipathstopped: {aIPath.isStopped}");
        
        if (!aIPath.pathPending) 
        {
            aIPath.SearchPath();
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
        aIPath.SetPath(null);
        _runner.AnimationBool("Walk", false);
        aIPath.enableRotation = true;
        aIPath.isStopped = true;

        if (CurrentState == EnemyStateController.EnemyState.Rush ||
            CurrentState == EnemyStateController.EnemyState.Beam ||
            CurrentState == EnemyStateController.EnemyState.Stunned ||
            CurrentState == EnemyStateController.EnemyState.Die ||
            CurrentState == EnemyStateController.EnemyState.Attack)
        {

            aIPath.enabled = false;
            return;
        }
        else
        {
            aIPath.enabled = true;
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        if (aIPath == null)
        {
            return;
        }

    }
}
    // public void StartWallRush(float rushSpeed)
    // {
    //     if (aIPath != null)
    //     {
    //         aIPath.enabled = false; // A* Pathfinding 비활성화
    //     }
    //     Vector3 lookAtPosition = _runner.player.transform.position;
    //     lookAtPosition.y = _runner.transform.position.y;
    //     // _runner.transform.LookAt(lookAtPosition);
    //     // _runner.SetLastRushHitObject(null);
    //     if (rb != null)
    //     {
    //         rb.isKinematic = false; // Rigidbody 물리 효과 활성화
    //         rb.linearVelocity = _runner.transform.forward * rushSpeed; // 현재 바라보는 방향으로 속도 적용
    //     }
    // }