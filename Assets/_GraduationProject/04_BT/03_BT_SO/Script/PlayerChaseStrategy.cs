using UnityEngine;
using Pathfinding;
using UnityEngine.XR;

[CreateAssetMenu(fileName = "PlayerChaseStrategy", menuName = "Enemy/Strategy/PlayerChaseStrategy")]
public class PlayerChaseStrategy : EnemyUseAnything
{
    [Header("Rush Settings")]
    public float maxRushSpeed = 20f;      // 최대 돌진 속도
    public float hitRadius = 1.5f;        // 플레이어 접촉 판정 범위
    public LayerMask obstacleMask;        // 벽 레이어

    [Header("Movement & Inertia")]
    public float rushDuration = 3.0f;     // 돌진 지속 시간 (추적 시간)
    public float turnSpeed = 5.0f;        // 회전 속도 (낮을수록 회전이 둔해져서 관성이 느껴짐)
    
    // X축: 시간(0~1), Y축: 속도 배율 (가속->유지->감속 곡선 추천)
    public AnimationCurve speedCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.2f, 1f), new Keyframe(0.8f, 1f), new Keyframe(1, 0));

    // 블랙보드 키
    private const string KEY_RUSHBOOL = "RushBool";
    private const string KEY_RUSH_START_TIME = "RushStartTime";
    private const string KEY_RUSH_VELOCITY_DIR = "RushVelocityDir"; // [추가] 현재 이동 방향 저장용

    public override T OnEnter<T>(T runner)
    {
        var blackboard = runner._aiController._aiBrain.blackboard;

        if(!blackboard.GetValueOrDefault<bool>(KEY_RUSHBOOL, true))
        {
            return runner;
        }
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;

        IAstarAI ai = enemy.GetComponent<IAstarAI>();

        // 1. A* 네비게이션 끄기 (수동 이동을 위해)
        if (ai != null)
        {
            ai.canMove = false;
            ai.isStopped = true;
        }

        // 2. 초기 이동 방향 설정 (현재 적이 바라보는 방향)
        Vector3 initialDir = enemy.transform.forward;
        blackboard.SetValue(KEY_RUSH_VELOCITY_DIR, initialDir);
        
        blackboard.SetValue(KEY_RUSHBOOL, false);
        blackboard.SetValue(KEY_RUSH_START_TIME, Time.time);

        runner.aIPath.enableRotation = false; // A* 회전 비활성화
        runner.Movement.StopMovement();

        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        if (!runner._aiController._aiBrain.blackboard.HasKey(KEY_RUSH_START_TIME))
        {
            // 아직 세팅이 안 된 상태이므로 로직을 수행하지 않고 리턴
            return runner;
        }
        // 종료 조건 체크 (외부에서 불리언을 켰을 경우)
        if (runner._aiController._aiBrain.blackboard.GetValue<bool>(KEY_RUSHBOOL))
        {
            return runner;
        }
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;

        // 1. 시간 계산 및 종료 체크
        float startTime = enemy._aiController._aiBrain.blackboard.GetValue<float>(KEY_RUSH_START_TIME);
        float elapsedTime = Time.time - startTime;
        float normalizedTime = elapsedTime / rushDuration;

        if (normalizedTime >= 1.0f)
        {
            Debug.Log("[Rush] 시간 종료");
            StopRush(enemy);
            return runner;
        }

        // 2. 현재 속도 배율 계산 (가속도 곡선 적용)
        float speedMultiplier = speedCurve.Evaluate(normalizedTime);
        float currentSpeed = maxRushSpeed * speedMultiplier;

        // 3. 방향 계산 (Steering Behavior)
        // 저장된 현재 이동 방향을 가져옴
        Vector3 currentDir = enemy._aiController._aiBrain.blackboard.GetValue<Vector3>(KEY_RUSH_VELOCITY_DIR);
        
        Vector3 playerPos = enemy.player.transform.position;
        Vector3 myPos = enemy.transform.position;

        // 플레이어를 향한 방향
        Vector3 targetDir = (playerPos - myPos).normalized;
        targetDir.y = 0; // 평지 이동 가정

        // 현재 방향에서 타겟 방향으로 부드럽게 회전 (turnSpeed가 관성을 결정)
        // Vector3.RotateTowards(현재방향, 목표방향, 회전각도제한, 크기제한)
        Vector3 newDir = Vector3.RotateTowards(currentDir, targetDir, turnSpeed * Time.deltaTime, 0.0f);
        newDir.Normalize();

        // 4. 회전 적용 (모델이 이동 방향을 보게 함)
        if (newDir != Vector3.zero)
        {
            enemy.transform.rotation = Quaternion.LookRotation(newDir);
        }

        // 5. 이동 처리 (Raycast 충돌 체크 포함)
        Vector3 moveStep = newDir * currentSpeed * Time.deltaTime;
        float moveDist = moveStep.magnitude;

        if (moveDist > 0.0001f)
        {
            // 벽 체크
            if (!Physics.Raycast(myPos + Vector3.up * 0.5f, newDir, moveDist, obstacleMask))
            {
                enemy.transform.position += moveStep;
            }
            else
            {
                // 벽에 박으면 멈춤
                // Debug.Log("[Rush] 벽 충돌");
                StopRush(enemy);
                return runner;
            }
        }

        // 6. 플레이어 충돌 체크
        float distToPlayer = Vector3.Distance(myPos, playerPos);
        if (distToPlayer <= hitRadius)
        {
            // Debug.Log("[Rush] 플레이어 충돌 성공");
            StopRush(enemy);
            return runner;
        }

        // 7. 변경된 방향을 블랙보드에 저장 (다음 프레임을 위해)
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_RUSH_VELOCITY_DIR, newDir);

        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        
        if (enemy != null)
        {
            StopRush(enemy);
        }
        return runner;
    }

    private void StopRush(Enemy enemy)
    {
        // 상태 초기화
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_RUSHBOOL, true);
        
        // 물리 관성 제거
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // A* 네비게이션 복구
        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.Teleport(enemy.transform.position); // 현재 위치로 A* 에이전트 동기화
            ai.canMove = true;
            ai.isStopped = false;
            ai.maxSpeed = enemy.Movement._normalSpeed;
            ai.destination = enemy.transform.position; // 도착 지점을 현재 위치로 하여 즉시 이동 방지
            
            if (ai is AIPath aiPath) aiPath.enableRotation = true;
        }

        var Rvo = enemy.GetComponent<Pathfinding.RVO.RVOController>();
        if (Rvo != null)
        {
            Rvo.locked = false;
            Rvo.lockWhenNotMoving = true;
            Rvo.velocity = Vector3.zero;
        }
    }
}