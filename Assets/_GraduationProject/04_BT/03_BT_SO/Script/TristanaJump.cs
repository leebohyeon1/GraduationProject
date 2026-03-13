using Pathfinding;
using UnityEngine;

[CreateAssetMenu(fileName = "TristanaJump", menuName = "Enemy/Strategy/TristanaJump")]
public class TristanaJump : EnemyUseAnything
{
    [Header("Jump Settings")]
    public float jumpRange = 8.0f;          // 최대 점프 거리
    public float jumpDuration = 0.8f;       // 점프 체공 시간 (고정 시간)
    public float jumpHeight = 5.0f;         // 점프 최대 높이 (Y축)
    
    [Header("Landing Settings")]
    public float impactRadius = 2.5f;       // 착지 시 데미지 범위
    public DamageData impactDamage;         // 착지 데미지 데이터
    
    [Header("Trajectory")]
    // X축: 0~1 (시간), Y축: 0~1 (높이 비율). 
    // 모양을 (0,0) -> (0.5, 1) -> (1,0) 으로 설정하여 포물선을 만드세요.
    public AnimationCurve heightCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

    // 블랙보드 키
    private const string KEY_JUMP_START_POS = "JumpStartPos";
    private const string KEY_JUMP_END_POS = "JumpEndPos";
    private const string KEY_JUMP_START_TIME = "JumpStartTime";
    private const string KEY_IS_JUMPING = "IsJumping";

    public override T OnEnter<T>(T runner)
    {
        var blackboard = runner._aiController._aiBrain.blackboard;

        // 이미 점프 중이면 리턴
        if (blackboard.GetValueOrDefault<bool>(KEY_IS_JUMPING, false))
        {
            return runner;
        }

        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;

        // 1. A* 및 물리 정지 (공중 이동을 위해 직접 제어)
        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.canMove = false;
            ai.isStopped = true;
        }
        enemy.Movement.StopMovement();

        // 2. 목표 지점 계산
        Vector3 startPos = enemy.transform.position;
        Vector3 playerPos = enemy.player.transform.position;
        
        // 플레이어 방향으로 최대 사거리만큼 계산
        Vector3 direction = (playerPos - startPos);
        direction.y = 0; // 높이 무시
        float distance = direction.magnitude;
        direction.Normalize();

        // 사거리를 벗어나면 최대 사거리로 제한
        float jumpDist = Mathf.Min(distance, jumpRange);
        Vector3 targetPos = startPos + (direction * jumpDist);

        // [중요] 목표 지점이 이동 가능한 곳인지 확인 (A* NavMesh 기준)
        // 벽 속으로 들어가는 것을 방지하기 위해 가장 가까운 노드로 보정
        NNInfo info = AstarPath.active.GetNearest(targetPos, NNConstraint.Default);
        if (info.node != null)
        {
            targetPos = info.position;
        }

        // 3. 블랙보드 데이터 설정
        blackboard.SetValue(KEY_JUMP_START_POS, startPos);
        blackboard.SetValue(KEY_JUMP_END_POS, targetPos);
        blackboard.SetValue(KEY_JUMP_START_TIME, Time.time);
        blackboard.SetValue(KEY_IS_JUMPING, true);

        // 4. 점프 시작 애니메이션 트리거 (필요시)
        // enemy.animHandler.Play("JumpStart");

        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        var blackboard = runner._aiController._aiBrain.blackboard;
        if (!blackboard.GetValueOrDefault<bool>(KEY_IS_JUMPING, false)) return runner;

        Enemy enemy = runner as Enemy;
        
        // 1. 시간 계산
        float startTime = blackboard.GetValue<float>(KEY_JUMP_START_TIME);
        float elapsedTime = Time.time - startTime;
        float normalizedTime = elapsedTime / jumpDuration; // 0.0 ~ 1.0

        // 2. 이동 로직 (Parabolic Movement)
        if (normalizedTime < 1.0f)
        {
            Vector3 startPos = blackboard.GetValue<Vector3>(KEY_JUMP_START_POS);
            Vector3 endPos = blackboard.GetValue<Vector3>(KEY_JUMP_END_POS);

            // A. 수평 이동 (Lerp: 선형 보간)
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, normalizedTime);

            // B. 수직 이동 (Animation Curve 활용)
            // 커브 값(0~1) * 최대 높이
            float height = heightCurve.Evaluate(normalizedTime) * jumpHeight;
            currentPos.y += height;

            // 위치 적용
            enemy.transform.position = currentPos;
            
            // (선택) 진행 방향 바라보기
            Vector3 lookDir = (endPos - startPos).normalized;
            if(lookDir != Vector3.zero) 
                enemy.transform.rotation = Quaternion.LookRotation(lookDir);
        }
        else
        {
            // 시간 종료 -> 착지
            Landing(enemy);
        }

        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy != null)
        {
            // 강제 종료 시 안전하게 착지 처리
            if (runner._aiController._aiBrain.blackboard.GetValueOrDefault<bool>(KEY_IS_JUMPING, false))
            {
                Landing(enemy);
            }
        }
        return runner;
    }

    private void Landing(Enemy enemy)
    {
        var blackboard = enemy._aiController._aiBrain.blackboard;
        
        // 1. 상태 해제
        blackboard.SetValue(KEY_IS_JUMPING, false);

        // 2. 위치 보정 (최종 목표 지점으로 강제 이동 및 높이 초기화)
        Vector3 landPos = blackboard.GetValue<Vector3>(KEY_JUMP_END_POS);
        // 혹시 공중에 떠있을 수 있으므로 y값을 NavMesh 높이로 맞춤
        landPos.y = AstarPath.active.GetNearest(landPos).position.y;
        enemy.transform.position = landPos;

        // 3. 착지 데미지 및 이펙트 (광역 데미지)
        Collider[] hitColliders = Physics.OverlapSphere(landPos, impactRadius, LayerMask.GetMask("Player"));
        foreach (var hitCollider in hitColliders)
        {
            // 플레이어 데미지 처리
            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>(); // 혹은 적절한 컴포넌트
            if (playerHealth != null)
            {
                impactDamage.AttackerTransform = enemy.transform;
                playerHealth.TakeDamage(impactDamage);
                
                // (선택) 슬로우 효과 추가 가능
            }
        }
        // Debug.Log($"[TristanaJump] 쿵! {landPos} 착지 완료");
        enemy.animator.SetBool("IsRushing" , true);
        // 4. A* 및 물리 복구
       
    }
    
    public override void Reset<T>(T runner)
    {
        // 필요 시 초기화 로직
    }
}