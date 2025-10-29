using BehaviorTree;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;
using static Enemy;

public class EnemyMovement
{
    private Enemy _runner;
    AIPath aIPath;
    Rigidbody rb;
    Animator animator;
    private EnemyState CurrentState => _runner.CurrentState;
    public EnemyMovement(Enemy enemy)
    {
        _runner = enemy;
        _normalSpeed = _runner.NormalSpeed;
        aIPath = _runner.GetComponent<AIPath>();
        animator = _runner.animator;
        rb = _runner.GetComponent<Rigidbody>();
        _normalSpeed = _runner.NormalSpeed;
    }

    float _normalSpeed = 2f;
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
    public void StartWallRush(float rushSpeed)
    {
        if (aIPath != null)
        {
            aIPath.enabled = false; // A* Pathfinding 비활성화
        }
        Vector3 lookAtPosition = _runner.player.transform.position;
        lookAtPosition.y = _runner.transform.position.y;
        // _runner.transform.LookAt(lookAtPosition);
        // _runner.SetLastRushHitObject(null);
        if (rb != null)
        {
            rb.isKinematic = false; // Rigidbody 물리 효과 활성화
            rb.linearVelocity = _runner.transform.forward * rushSpeed; // 현재 바라보는 방향으로 속도 적용
        }
    }
    public Node.NodeState Patrols()
    {
        if (aIPath == null || _runner.wayPoints == null || _runner.wayPoints.Length == 0)
        {
            return Node.NodeState.FAILURE;
        }

        if (aIPath.reachedDestination)
        {
            _runner.wayPointIndex = (_runner.wayPointIndex + 1) % _runner.wayPoints.Length;
            StartOrUpdateChase(_runner.wayPoints[_runner.wayPointIndex], _normalSpeed);
        }

        return Node.NodeState.RUNNING;
    }

    public void StartPatrol()
    {
        if (_runner.wayPoints == null || _runner.wayPoints.Length == 0) return;

        StartOrUpdateChase(_runner.wayPoints[_runner.wayPointIndex], _normalSpeed);
    }
    public void StartOrUpdateChase(Vector3 newTarget,float speed = 2, string animationBool = "Walk")
    {
        if (CurrentState == EnemyState.Stunned || CurrentState == EnemyState.Attack || CurrentState == EnemyState.Die || CurrentState == EnemyState.Noise )
        {
            StopMovement();
            return;
        }
        if (aIPath == null) return;
        if(_runner.CurrentState != EnemyState.Hit)
        _runner.SetState(EnemyState.Chase);
        _runner.AnimationBool(animationBool, true);
        aIPath.enabled = true;
        CalculationResult stat = _runner.heatSystem.CalculationHeat("Test", ActorType.Monster, _runner.heatSystem.GetTier(), 0);
        aIPath.maxSpeed = speed * stat.FinalSpeed; // _normalSpeed 변수가 Enemy.cs에 선언되어 있어야 합니다.
        aIPath.destination = newTarget;
        aIPath.isStopped = false;
    }

    // Transform을 받는 오버로딩 버전도 유지
    public void StartOrUpdateChase(Transform target,string animationBool = "Walk")
    {
        StartOrUpdateChase(target.position, _normalSpeed, animationBool);
    }
    public void StopMovement()
    {
        aIPath.SetPath(null);
        _runner.AnimationBool("Walk", false);
        _runner.AnimationBool("Run", false);
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