using BehaviorTree;
using Pathfinding;
using UnityEngine;
using static Enemy;

public class EnemyMovement
{
    private Enemy _runner;
    AIPath aIPath;
    Rigidbody rb;
    Animator animator;
    private EnemyState CurrentState => _runner.CurrentState;
    float _normalSpeed = 2f;
    public EnemyMovement(Enemy enemy)
    {
        _runner = enemy;
        _normalSpeed = _runner.NormalSpeed;
        aIPath = _runner.GetComponent<AIPath>();
        aIPath.maxSpeed = _normalSpeed;
        animator = _runner.animator;
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


    public void StartOrUpdateChase(Vector3 newTarget, EnemyState ChaseState = EnemyState.Chase)
    {
        if (CurrentState == EnemyState.Stunned || CurrentState == EnemyState.Attack || CurrentState == EnemyState.Die || CurrentState == EnemyState.Noise)
        {
            StopMovement();
            return;
        }
        if (aIPath == null) return;
        if (_runner.CurrentState != EnemyState.Hit)
            _runner.SetState(ChaseState);
        _runner.AnimationBool("Walk", true);
        aIPath.enabled = true;
        aIPath.destination = newTarget;
        
        Debug.Log($"Chasing to Position: {newTarget} this object name: {_runner.name}");
        aIPath.isStopped = false;
    }

    // Transform을 받는 오버로딩 버전도 유지
    public void StartOrUpdateChase(Transform target)
    {
        StartOrUpdateChase(target.position, EnemyState.Chase);
    }
    public void StopMovement()
    {
        aIPath.SetPath(null);
        _runner.AnimationBool("Walk", false);
        aIPath.enableRotation = true;
        aIPath.isStopped = true;

        if (CurrentState == EnemyState.Rush ||
            CurrentState == EnemyState.Beam ||
            CurrentState == EnemyState.Stunned ||
            CurrentState == EnemyState.Die ||
            CurrentState == EnemyState.Attack)
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