using UnityEngine;
using BehaviorTree;
using Pathfinding;

public class Task_FleeFromTarget : Node
{
    [Header("Settings")]
    public float fleeDistance = 5f;
    public float fleeSpeed = 5f;

    public override void OnEnter()
    {
        base.OnEnter();
        if (runner.player == null) return;

        Vector3 myPos = runner.transform.position;
        Vector3 playerPos = runner.player.transform.position;

        // 1. 1순위: 플레이어 반대 방향
        Vector3 fleeDir = (myPos - playerPos).normalized;
        fleeDir.y = 0;

        Vector3 finalDirection = fleeDir;
        float finalDistance = fleeDistance;

        // 2. Movement에게 "뒤쪽 막혔어?" 물어보기
        if (runner.Movement.IsPathBlocked(fleeDir, fleeDistance, out RaycastHit hit))
        {
            // [전술적 판단] 뒤가 막혔으니 좌/우(벽 타기) 방향을 계산하자
            Debug.Log("<color=yellow>[Task] 뒤가 막혀서 옆길을 찾습니다.</color>");

            // 벽의 법선(Normal)을 이용해 벽을 타고 흐르는 방향(Tangent) 계산
            Vector3 slideLeft = Vector3.Cross(hit.normal, Vector3.up).normalized;
            Vector3 slideRight = -slideLeft;

            // 3. 좌/우 중 어디가 뚫렸는지 Movement에게 다시 물어보기
            bool isLeftBlocked = runner.Movement.IsPathBlocked(slideLeft, fleeDistance, out RaycastHit leftHit);
            bool isRightBlocked = runner.Movement.IsPathBlocked(slideRight, fleeDistance, out RaycastHit rightHit);

            if (!isLeftBlocked && !isRightBlocked)
            {
                // 둘 다 뚫렸으면 원래 도망가려던 방향과 더 가까운 쪽(내적) 선택
                float dotLeft = Vector3.Dot(fleeDir, slideLeft);
                float dotRight = Vector3.Dot(fleeDir, slideRight);
                finalDirection = (dotLeft > dotRight) ? slideLeft : slideRight;
            }
            else if (!isLeftBlocked)
            {
                finalDirection = slideLeft;
            }
            else if (!isRightBlocked)
            {
                finalDirection = slideRight;
            }
            else
            {
                // [구석에 몰림] 양쪽 다 막혔으면 그냥 벽 바로 앞까지만 물러남
                finalDirection = fleeDir; 
                finalDistance = Mathf.Max(0, hit.distance - 0.5f); // 벽 버퍼 직접 적용 or Movement에 맡김
            }
        }

        // 4. 최종 결정된 좌표 계산
        Vector3 finalDestination = myPos + (finalDirection * finalDistance);

        // 5. 이동 명령 하달
        runner.Movement.StartOrUpdateChase(finalDestination, EnemyStateController.EnemyState.Rush, fleeSpeed);
    }

    protected override NodeState OnUpdate()
    {
        if (runner.player == null) return NodeState.FAILURE;

        var ai = runner.GetComponent<IAstarAI>();
        if (ai == null) return NodeState.FAILURE;

        if (!ai.pathPending && (ai.reachedEndOfPath || ai.reachedDestination)) 
        {
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }
}