using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "TripleRushEffect", menuName = "Enemy/AttackEffect/Triple Rush Movement")]
public class TripleRushEffectSO : EnemyUseAnything
{
    [Header("Rush Settings")]
    [Tooltip("1, 2타 돌진 시 회당 최대 전진 거리 (기본 2.0)")]
    public float MaxDashDist = 2.0f;

    [Tooltip("1, 2타 돌진 이동 속도")]
    public float dashSpeed = 10.0f;

    [Tooltip("3타(도약) 최종 이동 거리 (기본 4.0)")]
    public float leapDistance = 4.0f;

    [Tooltip("3타(도약) 총 소요 시간 (기본 0.7)")]
    public float leapDuration = 0.7f;

    [Tooltip("3타(도약) 이동 완급 조절용 커브")]
    public AnimationCurve leapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Blackboard Keys")]
    public string bb_Index = "TR_Index";
    public string bb_IsMoving = "TR_IsMoving";
    public string bb_StartPos = "TR_StartPos";
    public string bb_TargetPos = "TR_TargetPos";
    public string bb_StartTime = "TR_StartTime";
    public string bb_Duration = "TR_Duration";

    // ---------------------------------------------------------
    // 1. Reset (노드 진입 시 초기화)
    // ---------------------------------------------------------
    public override void Reset<T>(T runner)
    {
        // T가 Enemy이거나 Enemy를 상속받은 경우에만 동작
        if (runner is Enemy enemy)
        {
            var bb = GetBlackboard(enemy);
            if (bb != null)
            {
                bb.SetValue(bb_Index, 0);
                bb.SetValue(bb_IsMoving, false);
            }
        }
    }

    // ---------------------------------------------------------
    // 2. OnEnter (Handler.IsActionSO가 True일 때, 즉 공격 타이밍에 호출됨)
    //    -> 다음 돌진 목표 설정
    // ---------------------------------------------------------
    public override T OnEnter<T>(T runner)
    {
        if (runner is Enemy enemy)
        {
            var bb = GetBlackboard(enemy);
            if (bb != null)
            {
                // 콤보 인덱스 증가 (0 -> 1 -> 2 -> 3)
                int currentIndex = bb.GetValue<int>(bb_Index) + 1;
                bb.SetValue(bb_Index, currentIndex);

                if (currentIndex <= 3)
                {
                    SetupNextRush(enemy, bb, currentIndex);
                }
            }
        }
        return runner;
    }

    // ---------------------------------------------------------
    // 3. OnUpdate (매 프레임 이동 로직 수행)
    // ---------------------------------------------------------
    public override T OnUpdate<T>(T runner)
    {
        if (runner is Enemy enemy)
        {
            var bb = GetBlackboard(enemy);
            // 이동 중(`bb_IsMoving`)일 때만 위치 업데이트 수행
            if (bb != null && bb.GetValue<bool>(bb_IsMoving))
            {
                PerformMovement(enemy, bb);
            }
        }
        return runner;
    }

    // ---------------------------------------------------------
    // 4. OnExit (노드 종료 시 정리)
    // ---------------------------------------------------------
    public override T OnExit<T>(T runner)
    {
        if (runner is Enemy enemy)
        {
            var bb = GetBlackboard(enemy);
            if (bb != null)
            {
                bb.SetValue(bb_IsMoving, false);
                bb.SetValue(bb_Index, 0);
            }
        }
        return runner;
    }

    // ---------------------------------------------------------
    // Helper Methods (내부 로직)
    // ---------------------------------------------------------
    private void SetupNextRush(Enemy runner, BlackBoard bb, int index)
    {
        Vector3 currentPos = runner.transform.position;
        // 플레이어가 없으면 정면으로 설정
        Vector3 playerPos = runner.player != null ? runner.player.transform.position : (currentPos + runner.transform.forward);
        
        Vector3 dirToPlayer = (playerPos - currentPos);
        dirToPlayer.y = 0;
        float distToPlayer = dirToPlayer.magnitude;
        dirToPlayer.Normalize();

        Vector3 targetPos = Vector3.zero;
        float moveDuration = 0f;

        if (index == 1 || index == 2)
        {
            // [1, 2타] 거리 제한: Min(2.0f, 현재거리)
            float moveDist = Mathf.Min(MaxDashDist, distToPlayer);
            targetPos = currentPos + (dirToPlayer * moveDist);

            // 속도 기반 시간 계산 (최소 시간 0.1초 보장)
            moveDuration = moveDist / dashSpeed;
            if (moveDuration < 0.1f) moveDuration = 0.1f;
        }
        else if (index == 3)
        {
            // [3타] 도약: 고정 거리
            targetPos = currentPos + (dirToPlayer * leapDistance);
            moveDuration = leapDuration;
        }

        // 블랙보드에 상태 저장
        bb.SetValue(bb_StartPos, currentPos);
        bb.SetValue(bb_TargetPos, targetPos);
        bb.SetValue(bb_StartTime, Time.time);
        bb.SetValue(bb_Duration, moveDuration);
        bb.SetValue(bb_IsMoving, true);
    }

    private void PerformMovement(Enemy runner, BlackBoard bb)
    {
        float startTime = bb.GetValue<float>(bb_StartTime);
        float duration = bb.GetValue<float>(bb_Duration);
        float timeElapsed = Time.time - startTime;
        float progress = timeElapsed / duration;

        Vector3 startPos = bb.GetValue<Vector3>(bb_StartPos);
        Vector3 targetPos = bb.GetValue<Vector3>(bb_TargetPos);

        // 이동 완료 체크
        if (progress >= 1.0f)
        {
            runner.transform.position = targetPos;
            bb.SetValue(bb_IsMoving, false);
            return;
        }

        // 회전 (플레이어 방향 보정)
        Vector3 dir = (targetPos - startPos).normalized;
        if (dir != Vector3.zero)
        {
            runner.transform.rotation = Quaternion.Slerp(runner.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 15f);
        }

        // 이동 방식 분기
        int currentIndex = bb.GetValue<int>(bb_Index);
        if (currentIndex == 3)
        {
            // [3타] Animation Curve 적용
            float curveValue = leapCurve.Evaluate(progress);
            runner.transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
        }
        else
        {
            // [1, 2타] 선형 이동
            runner.transform.position = Vector3.Lerp(startPos, targetPos, progress);
        }
    }

    private BlackBoard GetBlackboard(Enemy runner)
    {
        var controller = runner.GetComponent<AiController>();
        if (controller != null && controller._aiBrain != null)
        {
            return controller._aiBrain.blackboard;
        }
        return null;
    }
}