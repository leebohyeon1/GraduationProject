using UnityEngine;

[CreateAssetMenu(fileName = "TeleportApproachStrategy", menuName = "Enemy/Strategy/Teleport Approach (Invisible)")]
public class TeleportApproachStrategy : EnemyUseAnything
{
    [Header("Data Inspector")]
    [Tooltip("사라져 있는 시간 (초)")]
    public float InvisibleDuration = 1.0f;

    [Tooltip("플레이어와 떨어져서 나타날 거리")]
    public float TeleportOffset = 5.0f; // 플레이어 등 뒤 5m

    [Tooltip("텔레포트 지점의 벽 충돌 검사 반경")]
    public float detectionRadius = 1.0f; // 벽에 끼임 방지용 여유 공간

    [Header("Settings")]
    public LayerMask wallLayerMask;

    // 블랙보드 키 (타이머 저장용)
    public string Animation_Ready = "Teleport_Ready";
    public string ANimation_End = "Teleport_End";
    private const string KEY_TELEPORT_START_TIME = "Teleport_StartTime";

    public override T OnEnter<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null) return runner;

        // 1. 시작 시간(Time.time)을 블랙보드에 기록
        //    (기존 멤버 변수 _timer 대체)
        enemy._aiController._aiBrain.blackboard.SetValue(KEY_TELEPORT_START_TIME, Time.time);

        // 2. 이동 정지
        enemy.Movement.StopMovement();

        // 3. 투명화 & 무적 ON
        SetEnemyInvisible(enemy, true);
        // enemy.AnimationEvent(Animation_Ready);
        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null) return default;
        var blackboard = enemy._aiController._aiBrain.blackboard;
        if(blackboard.HasKey(KEY_TELEPORT_START_TIME) == false)
            return null;
        // 1. 블랙보드에서 시작 시간 가져오기
        float startTime = blackboard.GetValue<float>(KEY_TELEPORT_START_TIME);
        
        // 2. 경과 시간 계산
        float elapsedTime = Time.time - startTime;

        // 3. 시간이 다 되었는지 확인
        if (elapsedTime >= InvisibleDuration)
        {
            // 위치 계산 및 이동
            MoveToBackOfPlayer(enemy);

            // 투명화 해제
            SetEnemyInvisible(enemy, false);
            enemy.AnimationEvent(ANimation_End);
            // 블랙보드 키 정리 (선택 사항)
            blackboard.RemoveKey(KEY_TELEPORT_START_TIME);

            // 전략 종료 -> 다음 노드(TaskNormalAttack) 실행
            return default(T); 
        }

        return runner; // 아직 시간 안 됨 -> 대기
    }

    public override T OnExit<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy != null)
        {
            // 강제 종료 시 안전 장치: 모습 드러내기
            SetEnemyInvisible(enemy, false);
            // 키 정리
            var blackboard = enemy._aiController._aiBrain.blackboard;
            blackboard.RemoveKey(KEY_TELEPORT_START_TIME);
        }
        return runner;
    }

    // --- 내부 로직 (이전과 동일) ---

    private void MoveToBackOfPlayer(Enemy enemy)
    {
        if (enemy.player == null) return;

        Transform playerTr = enemy.player.transform;
        Vector3 finalPos = CalculateSafePosition(playerTr);

        // 이동
        enemy.transform.position = finalPos;

        // 회전 (플레이어 바라보기)
        Vector3 dirToPlayer = (playerTr.position - finalPos).normalized;
        dirToPlayer.y = 0;
        if (dirToPlayer != Vector3.zero)
        {
            enemy.transform.rotation = Quaternion.LookRotation(dirToPlayer);
        }
    }

    private Vector3 CalculateSafePosition(Transform playerTr)
    {
        Vector3 playerPos = playerTr.position;
        Vector3 forward = playerTr.forward;
        Vector3 right = playerTr.right;
        
        // 4방향 체크: 뒤 -> 좌 -> 우 -> 앞
        Vector3[] checkDirs = { -forward, -right, right, forward };

        foreach (Vector3 dir in checkDirs)
        {
            Vector3 targetPos = playerPos + (dir * TeleportOffset);
            Vector3 checkPos = targetPos + Vector3.up * 1.0f; 

            if (!Physics.CheckSphere(checkPos, detectionRadius, wallLayerMask))
            {
                return targetPos;
            }
        }
        
        // 막혔으면 플레이어 위치 반환
        return playerPos; 
    }

    private void SetEnemyInvisible(Enemy enemy, bool invisible)
    {
        var renderers = enemy.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = !invisible;

        var col = enemy.GetComponent<Collider>();
        if (col != null) col.enabled = !invisible;
        
        // 무적 처리 추가 가능
        if (enemy.Shield != null) enemy.Shield.IsActive = invisible; // 예시
    }

    public override void Reset<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy != null)
        {
            var blackboard = enemy._aiController._aiBrain.blackboard;
            blackboard.RemoveKey(KEY_TELEPORT_START_TIME);
        }
    }
}