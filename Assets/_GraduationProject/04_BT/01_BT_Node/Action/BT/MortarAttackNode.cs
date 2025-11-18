using UnityEngine;
using BehaviorTree;
using System.Net;

public class MortarAttackNode : Node
{
    public MortarProjectile projectilePrefab;
    public Transform launchPoint;

    public float launchAngle = 45;
    public string AttackName = "MortarAttack";
    bool hasLaunched;
    public override void OnEnter()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Mortar Prefab이 할당되지 않았습니다!");
            return;
        }

        // 1. 공격 시작 시 초기화
        hasLaunched = false;
        runner.SetState(Enemy.EnemyState.Attack); // 상태를 공격으로 변경

        runner.AnimationEvent(AttackName);
        launchPoint = runner.LaunchPoint;
    }
    protected override NodeState OnUpdate()
    {
        if (!hasLaunched && Handler.IsHitWindowOpen)
        {
            LaunchProjectile();
            Debug.Log("laucnhed");
            hasLaunched = true;
        }
        if (hasLaunched && Handler.IsActionFinished)
        {
            Debug.Log("Mortar Attack Node Success");
            return NodeState.SUCCESS;
        }
        return NodeState.RUNNING;
    }
    private void LaunchProjectile()
    {
        Vector3 targetPositon = runner.player.transform.position;
        Vector3 startPos = (launchPoint == null) ? runner.transform.position + (runner.transform.up * 3f) : launchPoint.position;

        Vector3? launchVelocity = CalculateLaunchVelocity(startPos, targetPositon);

        if (launchVelocity.HasValue)
        {
            MortarProjectile projectile = Instantiate(projectilePrefab, startPos, Quaternion.identity);

            projectile.Launch(launchVelocity.Value, runner);
        }
    }
    private Vector3? CalculateLaunchVelocity(Vector3 start, Vector3 target)
    {
        float gravity = Physics.gravity.magnitude;
        float angle = launchAngle * Mathf.Deg2Rad;

        Vector3 displacementXZ = new Vector3(target.x - start.x, 0, target.z - start.z);
        float distance = displacementXZ.magnitude;
        float heightDifference = start.y - target.y;

        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);
        float tanAngle = Mathf.Tan(angle);

        float speedSquared = (gravity * distance * distance) / (2 * cosAngle * cosAngle * (distance * tanAngle + heightDifference));

        if (speedSquared <= 0)
        {
            Debug.LogWarning("목표 지점에 도달할 수 없습니다.");
            return null; // 목표 지점에 도달할 수 없음
        }


        float speed = Mathf.Sqrt(speedSquared);
        Vector3 velocity = new Vector3(0, speed * sinAngle, speed * cosAngle);
        Quaternion rotation = Quaternion.LookRotation(displacementXZ);
        return rotation * velocity;
    }

    public override Node Clone()
    {
        MortarAttackNode node = Instantiate(this);
        node.projectilePrefab = this.projectilePrefab;
        node.launchPoint = this.launchPoint;
        node.launchAngle = this.launchAngle;
        node.AttackName = this.AttackName;
        return node;
    }
    public override void OnExit()
    {
        hasLaunched = false;
        Handler.ResetAllFlags();
        runner.SetState(Enemy.EnemyState.Idle);
    }

}