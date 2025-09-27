// --- FILE: Action_LookAtPlayer.cs ---

using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "LookAtPlayer", menuName = "BehaviorTree/Action/LookAtPlayer")]
public class Action_LookAtPlayer : Node
{
    [Tooltip("회전 속도를 조절합니다.")]
    public float rotationSpeed = 5f;

    [Tooltip("이 각도(degree) 이내로 들어오면 회전을 멈추고 성공으로 처리합니다.")]
    public float acceptanceAngle = 5.0f;

    private Transform playerTransform;

    public override void initNode()
    {
        base.initNode();
        playerTransform = runner.player.transform;
    }

    protected override NodeState OnUpdate()
    {
        if (playerTransform == null)
        {
            // 플레이어가 없으면 즉시 실패 처리
            return NodeState.FAILURE;
        }

        // 플레이어를 향하는 방향 벡터 계산 (Y축은 무시하여 수평으로만 회전)
        Vector3 directionToPlayer = playerTransform.position - runner.transform.position;
        directionToPlayer.y = 0;

        // 방향 벡터가 거의 0이라면 (바로 위나 아래에 있다면) 회전할 필요 없음
        if (directionToPlayer.sqrMagnitude < 0.001f)
        {
            runner.SetState(Enemy.EnemyState.Idle);
            Debug.Log(brain.CurrentState);  
            return NodeState.SUCCESS;

        }

        // 목표 회전값(Quaternion) 계산
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

        // 현재 각도와 목표 각도의 차이 계산
        float angleDifference = Quaternion.Angle(runner.transform.rotation, targetRotation);

        // 허용 오차 각도 이내라면 즉시 성공 처리
        if (angleDifference <= acceptanceAngle)
        {
            return NodeState.SUCCESS;
        }

        // 부드러운 회전을 위해 Slerp(구면 선형 보간) 사용
        runner.transform.rotation = Quaternion.Slerp(
            runner.transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        // 아직 회전 중이므로 RUNNING 상태 반환
        return NodeState.RUNNING;
    }

    public override Node Clone()
    {
        Action_LookAtPlayer newNode = Instantiate(this);
        return newNode;
    }
}
