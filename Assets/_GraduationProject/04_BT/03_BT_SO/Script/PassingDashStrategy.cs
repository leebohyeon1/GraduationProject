using UnityEngine;
using Pathfinding;

[CreateAssetMenu(fileName = "PassingDashStrategy", menuName = "Enemy/Strategy/Passing Dash (Debug)")]
public class PassingDashStrategy : EnemyUseAnything
{
    [Header("Data Inspector")]
    public float DashSpeed = 15.0f;
    public float ExtraDist = 10.0f;     // 관통 후 이동 거리

    [Header("Settings")]
    public LayerMask obstacleMask;      // 벽 레이어
    public float arrivalThreshold = 0.5f;

    // 블랙보드 키
    private const string KEY_DASH_TARGET_POS = "DashTargetPos";
    private const string KEY_DASH_START_POS = "DashStartPos"; // 디버깅용 시작 위치

    public override T OnEnter<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;

        // 1. AI 및 물리 정지
        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (ai != null) { ai.canMove = false; ai.isStopped = true; }
        
        runner.Movement.StopMovement();
        runner.aIPath.enableRotation = false;

        // 2. [핵심] 방향 및 목표 지점 계산 (딱 한 번만 실행!)
        Vector3 startPos = enemy.transform.position;
        Vector3 playerPos = enemy.player.transform.position;

        // Y축 높이 보정 (적의 높이 기준)
        float fixedY = startPos.y; 
        
        Vector3 direction = (playerPos - startPos);
        direction.y = 0; // 평면 방향만 사용

        // 거리가 너무 가까우면 적의 정면을 사용
        if (direction.sqrMagnitude < 0.1f) direction = enemy.transform.forward;
        else direction.Normalize();

        // **목표 지점 = 플레이어 위치 + (방향 * 추가 거리)**
        // 적 위치(startPos) 기준이 아니라, 플레이어(playerPos) 기준으로 뒤로 더 가야 함
        Vector3 finalTarget = playerPos + (direction * ExtraDist);
        finalTarget.y = fixedY; // 높이 고정

        // 블랙보드 저장
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_DASH_TARGET_POS, finalTarget);
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_DASH_START_POS, startPos);

        // 적을 목표 방향으로 회전
        enemy.transform.rotation = Quaternion.LookRotation(direction);

        // [디버그] 목표 지점 로그 찍기
        // // Debug.Log($"[Dash] Start: {startPos} -> Player: {playerPos} -> Final: {finalTarget}");

        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null) return runner;
        if(enemy._aiController._aiBrain.blackboard.HasKey(KEY_DASH_TARGET_POS) == false) return runner;

        // 1. 목표 지점 가져오기
        Vector3 targetPos = enemy._aiController._aiBrain.blackboard.GetValue<Vector3>(KEY_DASH_TARGET_POS);
        Vector3 startPos = enemy._aiController._aiBrain.blackboard.GetValue<Vector3>(KEY_DASH_START_POS);
        Vector3 currentPos = enemy.transform.position;

        // ---------------------------------------------------------
        // [시각적 디버깅] Scene 뷰에서 확인하세요!
        // 빨간 선: 시작점 -> 목표점 (전체 경로)
        Debug.DrawLine(startPos, targetPos, Color.red);
        // 초록 선: 내 위치 -> 목표점 (남은 경로)
        Debug.DrawLine(currentPos, targetPos, Color.green);
        // ---------------------------------------------------------

        // 2. 이동 방향 벡터 (목표 지점 - 내 위치)
        Vector3 moveDir = (targetPos - currentPos);
        moveDir.y = 0; // 높이 무시
        float distToTarget = moveDir.magnitude;

        // 3. 도착 체크
        if (distToTarget <= arrivalThreshold)
        {
            // 목표 도달
            enemy.transform.position = targetPos; // 깔끔하게 위치 보정
            StopDash(enemy);
            return runner;
        }

        // 4. 이동 실행
        moveDir.Normalize();
        float moveDistance = DashSpeed * Time.deltaTime;

        // 벽 체크 (몸체 높이 1.0f 가정)
        if (Physics.Raycast(currentPos + Vector3.up * 1.0f, moveDir, moveDistance, obstacleMask))
        {
            // // Debug.Log("[Dash] 벽 충돌!");
            StopDash(enemy);
            return runner;
        }

        // 실제 이동
        enemy.transform.position += moveDir * moveDistance;

        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        // StopDash(runner as Enemy);
        return runner;
    }

    private void StopDash(Enemy enemy)
    {
        if (enemy == null) return;

        // 물리 초기화
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;
        enemy._aiController._aiBrain.blackboard.RemoveKey(KEY_DASH_TARGET_POS);
        enemy._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);

        // AI 복구
        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.Teleport(enemy.transform.position); // 현재 위치를 AI에게 알림
            ai.canMove = true;
            ai.isStopped = false;
            ai.maxSpeed = enemy.Movement._normalSpeed;
            if (ai is AIPath aiPath) aiPath.enableRotation = true;
        }
    }

    public override void Reset<T>(T runner)
    {
        
    }
}