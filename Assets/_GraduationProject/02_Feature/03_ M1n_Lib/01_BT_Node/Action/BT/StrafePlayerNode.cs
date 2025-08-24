using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "StrafePlayerNode", menuName = "BehaviorTree/Action/StrafePlayerNode")]
public class StrafePlayerNode : Node
{
    [Tooltip("플레이어와 유지하려는 이상적인 거리입니다.")]
    public float idealDistance = 8f;
    [Tooltip("이 행동을 지속할 시간입니다.")]
    public float duration = 3f;
    [Tooltip("측면 이동 속도입니다.")]
    public float strafeSpeed = 4f;
    [Tooltip("뒤로 물러날 때의 속도입니다.")]
    public float backwardSpeed = 6f; // 보통 후진은 더 빠르게 설정

    private float _startTime;
    private int _strafeDirection; // 1: 오른쪽, -1: 왼쪽

    public override void OnEnter()
    {
        _startTime = Time.time;
        // 시작할 때 랜덤하게 왼쪽 또는 오른쪽으로 방향을 정합니다.
        _strafeDirection = (Random.value < 0.5f) ? 1 : -1;
        runner.SetState(Enemy.EnemyState.Chase); // 상태는 이동 가능한 상태로 설정
    }

    protected override NodeState OnUpdate()
    {
        // 정해진 시간이 지나면 성공으로 종료
        if (Time.time - _startTime > duration)
        {
            return NodeState.SUCCESS;
        }

        if (runner.player == null) return NodeState.FAILURE;

        // 플레이어 방향 및 거리 계산
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;
        float distanceToPlayer = directionToPlayer.magnitude;

        // 항상 플레이어를 바라보도록 강제
        runner.transform.rotation = Quaternion.LookRotation(directionToPlayer);

        Vector3 targetPosition;
        float currentSpeed;

        // ★★★★★ 핵심 로직 ★★★★★
        // 플레이어가 이상적인 거리보다 가까이 다가왔을 때: 뒤로 물러나기
        if (distanceToPlayer < idealDistance)
        {
            // 목표 지점: 플레이어로부터 정반대 방향
            targetPosition = runner.transform.position - directionToPlayer.normalized * 5f; // 현재 위치에서 뒤로
            currentSpeed = backwardSpeed;
        }
        // 적정 거리를 유지하며 옆으로 돌 때
        else
        {
            // 목표 지점: 현재 위치에서 측면 방향
            Vector3 sideDirection = runner.transform.right * _strafeDirection;
            targetPosition = runner.transform.position + sideDirection;
            currentSpeed = strafeSpeed;
        }

        // Enemy의 업그레이드된 이동 함수를 호출하여 목표 지점으로 이동
        runner.Movement.StartOrUpdateChase(targetPosition, currentSpeed);

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        runner.Movement.StopMovement();
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.idealDistance = this.idealDistance;
        node.duration = this.duration;
        node.strafeSpeed = this.strafeSpeed;
        node.backwardSpeed = this.backwardSpeed;
        return node;
    }
}