using Pathfinding;
using UnityEngine;

[CreateAssetMenu(fileName = "KSante", menuName = "Enemy/Strategy/KSante")]
public class KSante : EnemyUseAnything
{
    [Header("Rush Settings")]
    public float rushSpeed = 20f;       // 기본 돌진 속도 (곡선의 Y값이 1일 때의 속도)
    public float hitRadius = 1.5f;      // 플레이어 접촉 판정 범위
    public float overshootDist = 3.0f;  // 목표 오버슈트 거리
    public LayerMask obstacleMask;      // 벽 레이어

    [Header("Speed Curve Settings")]
    public float rushDuration = 1.0f;   // 돌진이 지속될 총 시간 (초)
    public float turnSpeed = 10f;      // 회전 속도 (도/초)
    // 블랙보드 키
     public float PushDistance = 5.0f;
    public DamageData AttackDataKnockback;
    private const string KEY_RUSH_DEST = "RushDestination";
    private const string KEY_RUSHBOOL = "RushBool";
    private const string KEY_RUSH_START_TIME = "RushStartTime"; // [추가] 시작 시간 저장용
    private const string KEY_HAS_HIT = "HasHitPlayer"; // [추가] 중복 충돌 방지용
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
        blackboard.SetValue(KEY_HAS_HIT, false); // [추가] 충돌 상태 초기화
        blackboard.SetValue(KEY_RUSH_START_TIME, Time.time);
        
        // [추가] 시작 시간 기록 (곡선 계산을 위해 필요)
        // [로그 1] 시작 데이터 (Cyan 색상)
        Debug.Log($"<color=cyan>[Rush Start] 시작위치: {myPos} -> 플레이어위치: {playerPos} -> 1차목표: {finalDestination}</color>");
        runner.aIPath.enableRotation = false;

        runner.Movement.StopMovement();
        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy != null)
        {
            StopRush(enemy);
        }
        var board = runner._aiController._aiBrain.blackboard;
        if(board.GetValue<bool>(KEY_HAS_HIT) )
        {
            enemy.player.transform.parent = null;
            AttackDataKnockback.AttackerTransform = enemy.transform;
            enemy.player.GetComponent<PlayerHealth>().TakeDamage(AttackDataKnockback);
            board.SetValue(KEY_HAS_HIT, false);
            enemy.player.GetComponent<IDragable>().Drop();
        }
        return runner;
    }

    public override T OnUpdate<T>(T runner)
{
    var board = runner._aiController._aiBrain.blackboard;
    
    // 1. 이미 Rush가 끝났는지 체크
    if(board.GetValue<bool>(KEY_RUSHBOOL))
    {
        return runner; 
    }

    Enemy enemy = runner as Enemy;
    if (enemy == null || enemy.player == null) return runner;

    // 2. 목표 지점 가져오기 (이번 프레임의 목표)
    if (!board.GetValue<Vector3>(KEY_RUSH_DEST, out Vector3 targetPos))
    {
        return runner; 
    }

    // [생략했던 부분] 애니메이션 상태 체크
    if(enemy.animHandler.IsActionSO)
    {
        Debug.Log(this.name + " is running SO ");
    }

    // [생략했던 부분] 시간 경과에 따른 속도 계산 및 종료 체크
    float startTime = board.GetValue<float>(KEY_RUSH_START_TIME);
    float elapsedTime = Time.time - startTime;      // 경과 시간
    float normalizedTime = elapsedTime / rushDuration; // 0.0 ~ 1.0 사이 값으로 정규화
    // 3. 이동 계산
    float step = rushSpeed * Time.deltaTime;
    Vector3 currentPos = enemy.transform.position;
    
    // 목표 방향으로 이동
    Vector3 nextPos = Vector3.MoveTowards(currentPos, targetPos, step);
    
    Vector3 moveDir = (nextPos - currentPos).normalized;
    moveDir.y = 0; // 높이 차이 무시 (평지 이동 시)

    float moveDist = Vector3.Distance(currentPos, nextPos);

    // 4. 이동 중 벽 체크 (이동하려는 거리가 아주 작으면 생략)
    if (moveDist > 0.0001f)
    {
        // 주의: obstacleMask에 플레이어가 포함되어 있으면 안 됩니다.
        if (!Physics.Raycast(currentPos + Vector3.up * 0.5f, moveDir, moveDist + 2, obstacleMask))
        {
            enemy.transform.position = nextPos;
        }
        else
        {
            Debug.Log($"<color=red>[Rush Stop] 벽 충돌! 현재위치: {currentPos}</color>");
            StopRush(enemy);
            return runner;
        }
    }
    bool hashit = board.GetValue<bool>(KEY_HAS_HIT);
    if (!hashit)
    {
        float distToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.transform.position);
        if (distToPlayer <= hitRadius)
        {
            Debug.Log("충돌 ");
            PlayerTORush(enemy);
            
            return runner; 
        }
    }
    // 시간이 다 되면 종료
    if (normalizedTime >= 1.0f)
    {
        Debug.Log("[Rush] 지속 시간 종료");
        StopRush(enemy);
        return runner;
    }


    

    if (Vector3.Distance(enemy.transform.position, targetPos) < 0.1f)
    {
        Debug.Log($"<color=green>[Rush Arrived] 목표 도착! 현재위치: {enemy.transform.position} / 목표: {targetPos}</color>");
        StopRush(enemy);
    }

    return runner;
}
    private void PlayerTORush(Enemy enemy)
    {
        var board = enemy._aiController._aiBrain.blackboard;

        // 1. 중복 호출 방지 플래그 설정
        board.SetValue(KEY_HAS_HIT, true);
        enemy.player.GetComponent<IDragable>().Drag();

        // 2. 새로운 목표 지점 계산: 현재 위치에서 바라보는 방향(Forward)으로 5m
        Vector3 currentPos = enemy.transform.position;
        Vector3 pushDir = enemy.transform.forward; // 혹은 (playerPos - myPos).normalized
        Vector3 newDestination = currentPos + (pushDir * PushDistance);
        enemy.player.transform.parent = enemy.transform;

        Vector3 rayOrigin = currentPos + Vector3.up * 0.5f;

        RaycastHit hit;
        // maxPushDistance 만큼 앞을 확인
        if (Physics.Raycast(rayOrigin, pushDir, out hit, PushDistance, obstacleMask))
        {
            // [벽 발견]
            // 벽 위치(hit.point)에서 wallBuffer만큼 뒤로 뺀 위치를 목표로 설정
            float distanceToWall = hit.distance;
            
            // 벽이 너무 가까우면(buffer보다 가까우면) 제자리 혹은 아주 조금만 이동
            float targetDist = Mathf.Max(0, distanceToWall - 3);
            
            newDestination = currentPos + (pushDir * targetDist);
            
            Debug.Log($"[KSante] 벽 감지됨! {hit.collider.name}. 거리: {distanceToWall:F2}, 목표이동거리: {targetDist:F2}");
        }
        else
        {
            // [벽 없음] 최대 거리로 이동
            newDestination = currentPos + (pushDir * PushDistance);
        }

        // 3. 블랙보드 목표 업데이트
        board.SetValue(KEY_RUSH_DEST, newDestination);

        // 4. [중요] 돌진 시간 리셋 (새로운 5m를 이동할 시간을 벌어줌)
        // 시간을 리셋하면 curve(0)부터 다시 시작하므로 멈칫할 수 있습니다.
        // 자연스럽게 이어지길 원한다면 별도의 'PushDuration' 변수를 쓰거나 로직 조정이 필요하지만,
        // 가장 간단한 방법은 시간을 리셋하되 curve 시작점이 0이 아니도록 하거나 그냥 다시 가속하는 것입니다.
        board.SetValue(KEY_RUSH_START_TIME, Time.time); 

        // (선택) 밀고 나갈 때는 조금 더 오래 밀고 싶다면 rushDuration을 여기서 늘려줘도 됩니다.
        // rushDuration = 1.5f; 

        // (선택) 플레이어에게 충격/넉백을 주고 싶다면 여기서 플레이어 스크립트 호출
        // enemy.player.GetComponent<Rigidbody>().AddForce(pushDir * 10f, ForceMode.Impulse);
        
        Debug.Log($"<color=yellow>[Push Start] 접촉성공! 현재위치: {currentPos} -> {hit.point} -> 2차목표(밀치기): {newDestination}</color>");
    }
    private void StopRush(Enemy enemy)
    {
        var board = enemy._aiController._aiBrain.blackboard;
        board.SetValue(KEY_RUSHBOOL, true);
                // 3. 블랙보드 데이터 설정
        board.SetValue(KEY_RUSH_DEST, enemy.transform.position);
        
        // [추가] 시작 시간 기록 (곡선 계산을 위해 필요)
        board.SetValue(KEY_RUSH_START_TIME, null);
        
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.Teleport(enemy.transform.position);
            ai.canMove = true;      
            ai.isStopped = false;    
            ai.maxSpeed = enemy.Movement._normalSpeed; 
            ai.destination = enemy.transform.position;
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

    public override void Reset<T>(T runner)
    {
        
    }
}
