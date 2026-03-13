using UnityEngine;
using Pathfinding;
using UnityEngine.XR;

[CreateAssetMenu(fileName = "RushToFixedLocation", menuName = "Enemy/Strategy/Rush To Fixed Location")]
public class RushToFixedLocationStrategy : EnemyUseAnything
{
    [Header("Rush Settings")]
    public float rushSpeed = 20f;       // 기본 돌진 속도 (곡선의 Y값이 1일 때의 속도)
    public float hitRadius = 1.5f;      // 플레이어 접촉 판정 범위
    public float overshootDist = 3.0f;  // 목표 오버슈트 거리
    public LayerMask obstacleMask;      // 벽 레이어

    [Header("Speed Curve Settings")]
    public float rushDuration = 1.0f;   // 돌진이 지속될 총 시간 (초)
    // X축: 0~1 (시간 비율), Y축: 속도 배율 (예: 0에서 시작해서 1로 갔다가 0으로 떨어짐)
    public AnimationCurve rushCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.5f), new Keyframe(1, 0)); 
    public float turnSpeed = 10f;      // 회전 속도 (도/초)
    // 블랙보드 키
    private const string KEY_RUSH_DEST = "RushDestination";
    private const string KEY_RUSHBOOL = "RushBool";
    private const string KEY_RUSH_START_TIME = "RushStartTime"; // [추가] 시작 시간 저장용

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

        // 1. A* 네비게이션 끄기
        if (ai != null)
        {
            ai.canMove = false;
            ai.isStopped = true; 
        }

        // 2. 목표 지점 계산
        Vector3 playerPos = enemy.player.transform.position;
        Vector3 myPos = enemy.transform.position;

        Vector3 dir = (playerPos - myPos);
        dir.y = 0; 
        if (dir == Vector3.zero) dir = enemy.transform.forward;
        dir.Normalize();

        Vector3 offset = Quaternion.Euler(0, Random.Range(0, 360), 0) * new Vector3(0.5f, 0, 0);
        Vector3 finalDestination = playerPos + (dir * overshootDist) + offset;

        // 3. 블랙보드 데이터 설정
        blackboard.SetValue(KEY_RUSH_DEST, finalDestination);
        blackboard.SetValue(KEY_RUSHBOOL, false);
        
        // [추가] 시작 시간 기록 (곡선 계산을 위해 필요)
        blackboard.SetValue(KEY_RUSH_START_TIME, Time.time);
        
        runner.aIPath.enableRotation = false;

        runner.Movement.StopMovement();
        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        if(runner._aiController._aiBrain.blackboard.GetValue<bool>(KEY_RUSHBOOL))
        {
            return runner; 
        }
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;
        if (!enemy._aiController._aiBrain.blackboard.GetValue<Vector3>(KEY_RUSH_DEST, out Vector3 targetPos))
        {
            return runner; 
        }
        // [추가] 시간 경과에 따른 속도 계산
        float startTime = enemy._aiController._aiBrain.blackboard.GetValue<float>(KEY_RUSH_START_TIME);
        float elapsedTime = Time.time - startTime;      // 경과 시간
        float normalizedTime = elapsedTime / rushDuration; // 0.0 ~ 1.0 사이 값으로 정규화

        // 시간이 다 되면 종료
        if (normalizedTime >= 1.0f)
        {
            Debug.Log("[Rush] 지속 시간 종료");
            StopRush(enemy);
            return runner;
        }

        // AnimationCurve에서 현재 시간의 속도 배율을 가져옴
        float speedMultiplier = rushCurve.Evaluate(normalizedTime);
        float currentSpeed = rushSpeed * speedMultiplier; // 최종 속도 = 기본 속도 * 배율

        // 1. [직접 이동] 가변 속도 적용
        float step = currentSpeed * Time.deltaTime;
        Vector3 currentPos = enemy.transform.position;
        
        // 목표 방향으로 이동
        Vector3 nextPos = Vector3.MoveTowards(currentPos, targetPos, step);
        
        // [벽 체크] (기존 로직 유지)
        Vector3 moveDir = (nextPos - currentPos).normalized;
        moveDir.y = 0; // 높이 차이 무시 (평지 이동 시)

        // if (moveDir != Vector3.zero)
        // {
        //     Quaternion targetRot = Quaternion.LookRotation(moveDir);
        //     // 돌진 중에는 조금 더 빠르게 회전해서 방향을 잡도록 보정 (turnSpeed * 2f 등 조절 가능)
        //     enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRot, turnSpeed * Time.deltaTime * 5f);
        // }
        float moveDist = Vector3.Distance(currentPos, nextPos);

        // 이동 거리가 아주 작으면(속도가 0인 구간 등) 레이캐스트 생략 가능
        if (moveDist > 0.0001f)
        {
            if (!Physics.Raycast(currentPos + Vector3.up * 0.5f, moveDir, moveDist, obstacleMask))
            {
                enemy.transform.position = nextPos;
            }
            else
            {
                // Debug.Log("[Rush] 벽에 부딪힘!");
                StopRush(enemy);
                return runner;
            }
        }

        // 2. [접촉 체크]
        float distToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.transform.position);
        if (distToPlayer <= hitRadius)
        {
            // Debug.Log("[Rush] 플레이어 명중!");
            StopRush(enemy);
            return runner;
        }

        // 3. [도착 체크]
        if (Vector3.Distance(enemy.transform.position, targetPos) < 0.1f)
        {
            // Debug.Log("[Rush] 목표 도착");
            StopRush(enemy);
        }

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
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_RUSHBOOL, true);
                // 3. 블랙보드 데이터 설정
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_RUSH_DEST, enemy.transform.position);
        
        // [추가] 시작 시간 기록 (곡선 계산을 위해 필요)
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_RUSH_START_TIME, null);
        
    }

    public override void Reset<T>(T runner)
    {
        runner._aiController._aiBrain.blackboard.RemoveKey(KEY_RUSHBOOL);
        
    }
}