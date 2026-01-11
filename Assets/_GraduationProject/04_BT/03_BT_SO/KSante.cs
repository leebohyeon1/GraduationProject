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
    public DamageData AttackDataKnockback;
    private const string KEY_RUSH_DEST = "RushDestination";
    private const string KEY_RUSHBOOL = "RushBool";
    private const string KEY_RUSH_START_TIME = "RushStartTime"; // [추가] 시작 시간 저장용
    private const string KEY_HAS_HIT = "HasHitPlayer"; // [추가] 중복 충돌 방지용
        public override T OnEnter<T>(T runner)
    {
        if(!runner._aiController._aiBrain.blackboard.GetValue<bool>(KEY_RUSHBOOL))
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
        var board = enemy._aiController._aiBrain.blackboard;
        // 3. 블랙보드 데이터 설정
        board.SetValue(KEY_RUSH_DEST, finalDestination);
        board.SetValue(KEY_RUSHBOOL, false);
        board.SetValue(KEY_HAS_HIT, false); // [추가] 충돌 상태 초기화
        board.SetValue(KEY_RUSH_START_TIME, Time.time);
        
        // [추가] 시작 시간 기록 (곡선 계산을 위해 필요)
        
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
            enemy.player.GetComponent<EnemyHealth>().TakeDamage(AttackDataKnockback);
            board.SetValue(KEY_HAS_HIT, false);
        }
        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        var board = runner._aiController._aiBrain.blackboard;
        if(board.GetValue<bool>(KEY_RUSHBOOL))
        {
            return runner; 
        }
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;
        if (!board.GetValue<Vector3>(KEY_RUSH_DEST, out Vector3 targetPos))
        {
            return runner; 
        }
        if(enemy.animHandler.IsActionSO)
        {
        Debug.Log(this.name + " is running SO ");
        }
        // [추가] 시간 경과에 따른 속도 계산
        float startTime = board.GetValue<float>(KEY_RUSH_START_TIME);
        float elapsedTime = Time.time - startTime;      // 경과 시간
        float normalizedTime = elapsedTime / rushDuration; // 0.0 ~ 1.0 사이 값으로 정규화

        // 시간이 다 되면 종료
        if (normalizedTime >= 1.0f)
        {
            Debug.Log("[Rush] 지속 시간 종료");
            StopRush(enemy);
            return runner;
        }


        float step = rushSpeed * Time.deltaTime;
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
        bool hashit = board.GetValue<bool>(KEY_HAS_HIT);
        if (!hashit)
        {
            float distToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.transform.position);
            if (distToPlayer <= hitRadius)
            {
                // Debug.Log("[Rush] 플레이어 명중!");
                PlayerTORush(enemy);
                return runner;
            }
            
        }
        // 2. [접촉 체크]

        // 3. [도착 체크]
        if (Vector3.Distance(enemy.transform.position, targetPos) < 0.1f)
        {
            // Debug.Log("[Rush] 목표 도착");
            StopRush(enemy);
        }

        return runner;
    }
    private void PlayerTORush(Enemy enemy)
    {
        var board = enemy._aiController._aiBrain.blackboard;

        // 1. 중복 호출 방지 플래그 설정
        board.SetValue(KEY_HAS_HIT, true);

        // 2. 새로운 목표 지점 계산: 현재 위치에서 바라보는 방향(Forward)으로 5m
        Vector3 currentPos = enemy.transform.position;
        Vector3 pushDir = enemy.transform.forward; // 혹은 (playerPos - myPos).normalized
        Vector3 newDestination = currentPos + (pushDir * 5.0f);
        enemy.player.transform.parent = enemy.transform;
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
        
        Debug.Log("플레이어 접촉! 5m 추가 돌진 시작");
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
}
