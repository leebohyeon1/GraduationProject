using UnityEngine;
using Pathfinding; // A*는 끄기 위해 네임스페이스 필요

[CreateAssetMenu(fileName = "RushToFixedLocation", menuName = "Enemy/Strategy/Rush To Fixed Location")]
public class RushToFixedLocationStrategy : EnemyUseAnything
{
    [Header("Rush Settings")]
    public float rushSpeed = 20f;       // 돌진 속도 (직접 이동이므로 좀 더 빨라도 됨)
    public float hitRadius = 1.5f;      // 플레이어 접촉 판정 범위
    public float overshootDist = 3.0f;  // 목표 오버슈트 거리
    public LayerMask obstacleMask;      // 벽 레이어 (이동 중 벽 체크용)

    // 블랙보드 키 (목적지 저장용)
    private const string KEY_RUSH_DEST = "RushDestination";
    private const string KEY_RUSHBOOL = "RushBool";

    public override T OnEnter<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;

        IAstarAI ai = enemy.GetComponent<IAstarAI>();

        // 1. [중요] A* 네비게이션 잠시 끄기
        // (직접 이동할 것이므로 A*가 간섭하지 못하게 함)
        if (ai != null)
        {
            ai.canMove = false; // 이동 연산 중지
            ai.isStopped = true; 
            
        }

        // 2. 목표 지점 계산
        Vector3 playerPos = enemy.player.transform.position;
        Vector3 myPos = enemy.transform.position;

        Vector3 dir = (playerPos - myPos);
        dir.y = 0; 
        if (dir == Vector3.zero) dir = enemy.transform.forward;
        dir.Normalize();

        // 3. 최종 목적지 계산 (랜덤 오프셋 살짝 추가)
        Vector3 offset = Quaternion.Euler(0, Random.Range(0, 360), 0) * new Vector3(0.5f, 0, 0);
        Vector3 finalDestination = playerPos + (dir * overshootDist) + offset;

        // 4. 블랙보드에 목적지 저장 (SO는 공유되므로 상태를 저장하면 안됨)
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_RUSH_DEST, finalDestination);
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_RUSHBOOL, false);

        // 5. 방향 회전 (시작 시 딱 한번)
        enemy.transform.rotation = Quaternion.LookRotation(dir);


        Debug.Log($"[Rush] 직접 이동 시작! 목표: {finalDestination}");
        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        if(runner._aiController._aiBrain.blackboard.GetValue<bool>(KEY_RUSHBOOL))
        {
            return runner; // 이미 멈춤 처리됨
        }
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;

        // 블랙보드에서 목표지점 가져오기
        if (!enemy._aiController._aiBrain.blackboard.GetValue<Vector3>(KEY_RUSH_DEST, out Vector3 targetPos))
        {
            return runner; // 목표가 없으면 중단
        }

        // 1. [직접 이동] Transform 직접 수정
        float step = rushSpeed * Time.deltaTime;
        Vector3 currentPos = enemy.transform.position;
        
        // 목표 방향으로 이동
        Vector3 nextPos = Vector3.MoveTowards(currentPos, targetPos, step);
        
        // [벽 체크] 다음 위치로 가는데 벽이 있다면?
        Vector3 moveDir = (nextPos - currentPos).normalized;
        float moveDist = Vector3.Distance(currentPos, nextPos);

        // 벽이 없으면 이동 적용
        if (!Physics.Raycast(currentPos + Vector3.up * 0.5f, moveDir, moveDist, obstacleMask))
        {
            enemy.transform.position = nextPos;
        }
        else
        {
            Debug.Log("[Rush] 벽에 부딪힘!");
            StopRush(enemy); // 벽에 박으면 정지
            return runner;
        }

        // 2. [접촉 체크] 플레이어 충돌
        float distToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.transform.position);
        if (distToPlayer <= hitRadius)
        {
            Debug.Log("[Rush] 플레이어 명중!");
            StopRush(enemy);
            return runner;
        }

        // 3. [도착 체크] 목표지점 도달 (거리가 아주 가까우면)
        if (Vector3.Distance(enemy.transform.position, targetPos) < 0.1f)
        {
            Debug.Log("[Rush] 빗나감 (목표 도착)");
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

        // 2. [추가] Rigidbody 관성(속도) 강제 제거 (가장 중요!)
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;        // 남은 이동 가속도 제거
            rb.angularVelocity = Vector3.zero; // 회전 가속도 제거
            
            // (선택) 확실하게 멈추기 위해 잠시 키네마틱을 켰다 끌 수도 있음
            // rb.isKinematic = true; 
        }

        // 3. [추가] RVO(회피) 속도 강제 제거

        // 4. A* AI 복구 (다음 행동을 위해)
        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.canMove = true;      
            ai.isStopped = false;    
            ai.maxSpeed = enemy.Movement._normalSpeed; 
            
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