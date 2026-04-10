using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "TripleRushEffect", menuName = "Enemy/AttackEffect/Triple Rush Movement")]
public class TripleRushEffectSO : EnemyUseAnything
{
    [Header("Rush Settings")]
    [Tooltip("1, 2타 돌진 최대 전진 거리")]
    public float MaxDashDist = 2.0f;

    [Tooltip("1, 2타 돌진 이동 속도")]
    public float dashSpeed = 10.0f;

    [Tooltip("3타(도약) 최종 이동 거리")]
    public float leapDistance = 4.0f;

    [Tooltip("3타(도약) 총 소요 시간")]
    public float leapDuration = 0.7f;

    [Tooltip("3타(도약) 이동 가속도 조절 커브")]
    public AnimationCurve leapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Blackboard Keys")]
    public string bb_Index = "TR_Index";
    public string bb_IsMoving = "TR_IsMoving";
    public string bb_StartPos = "TR_StartPos";
    public string bb_TargetPos = "TR_TargetPos";
    public string bb_StartTime = "TR_StartTime";
    public string bb_Duration = "TR_Duration";

    public override void Reset<T>(T runner)
    {
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

    public override T OnEnter<T>(T runner)
    {
        if (runner is Enemy enemy)
        {
            var bb = GetBlackboard(enemy);
            if (bb != null)
            {
                int currentIndex = bb.GetValue<int>(bb_Index) + 1;
                bb.SetValue(bb_Index, currentIndex);

                if (currentIndex <= 3)
                {
                    SetupNextRush(enemy, bb, currentIndex);
                }
                else
                {
                    bb.SetValue(bb_IsMoving, false);
                }
            }
        }

        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        if (runner is Enemy enemy)
        {
            var bb = GetBlackboard(enemy);
            if (bb != null && bb.GetValue<bool>(bb_IsMoving))
            {
                PerformMovement(enemy, bb);
            }
        }

        return runner;
    }

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

    private void SetupNextRush(Enemy runner, BlackBoard bb, int index)
    {
        Vector3 currentPos = runner.transform.position;
        Vector3 playerPos = runner.player != null
            ? runner.player.transform.position
            : (currentPos + runner.transform.forward);

        Vector3 dirToPlayer = playerPos - currentPos;
        dirToPlayer.y = 0;

        float distToPlayer = dirToPlayer.magnitude;
        if (distToPlayer > 0.001f)
        {
            dirToPlayer /= distToPlayer;
        }
        else
        {
            dirToPlayer = runner.transform.forward;
        }

        Vector3 targetPos = currentPos;
        float moveDuration = 0.1f;

        if (index == 1 || index == 2)
        {
            float moveDist = Mathf.Min(MaxDashDist, distToPlayer);
            targetPos = currentPos + (dirToPlayer * moveDist);
            moveDuration = Mathf.Max(0.1f, moveDist / Mathf.Max(0.01f, dashSpeed));
        }
        else if (index == 3)
        {
            targetPos = currentPos + (dirToPlayer * leapDistance);
            moveDuration = Mathf.Max(0.1f, leapDuration);
        }

        bb.SetValue(bb_StartPos, currentPos);
        bb.SetValue(bb_TargetPos, targetPos);
        bb.SetValue(bb_StartTime, Time.time);
        bb.SetValue(bb_Duration, moveDuration);
        bb.SetValue(bb_IsMoving, true);
    }

    private void PerformMovement(Enemy runner, BlackBoard bb)
    {
        float startTime = bb.GetValue<float>(bb_StartTime);
        float duration = Mathf.Max(0.001f, bb.GetValue<float>(bb_Duration));
        float progress = (Time.time - startTime) / duration;

        Vector3 startPos = bb.GetValue<Vector3>(bb_StartPos);
        Vector3 targetPos = bb.GetValue<Vector3>(bb_TargetPos);

        if (progress >= 1.0f)
        {
            runner.transform.position = targetPos;
            bb.SetValue(bb_IsMoving, false);
            return;
        }

        Vector3 dir = (targetPos - startPos).normalized;
        if (dir != Vector3.zero)
        {
            runner.transform.rotation = Quaternion.Slerp(
                runner.transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 15f);
        }

        int currentIndex = bb.GetValue<int>(bb_Index);
        if (currentIndex == 3)
        {
            float curveValue = leapCurve.Evaluate(Mathf.Clamp01(progress));
            runner.transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
        }
        else
        {
            runner.transform.position = Vector3.Lerp(startPos, targetPos, Mathf.Clamp01(progress));
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
