using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "LoopAction", menuName = "Enemy/Strategy/LoopAction")]
public class LoopAction : EnemyUseAnything
{
    [Header("Settings")]
    public string AnimationBool = "IsRushing";
    public float ActionDuration = 5.0f;
    public float StartDelay = 0.5f;

    public const string KEY_START_TIME = "Shared_StartTime";
    public const string KEY_DURATION = "Shared_Duration";
    public const string EndKey = "Shared_Ended";

    public override T OnEnter<T>(T runner)
    {
        var blackboard = runner._aiController._aiBrain.blackboard;

        // OnEnter 시점에 즉시 타이머 시작
        blackboard.SetValue(KEY_START_TIME, Time.time);
        blackboard.SetValue(KEY_DURATION, ActionDuration);
        blackboard.SetValue(EndKey, false);

        // [추가] 시작 시에는 루프 탈출용 변수를 false로 초기화
        if (runner is Enemy enemy)
        {
            enemy.AnimationBool(AnimationBool, false);
        }

        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        var blackboard = runner._aiController._aiBrain.blackboard;
        if (!blackboard.HasKey(KEY_START_TIME)) return runner;

        float startTime = blackboard.GetValue<float>(KEY_START_TIME);
        float elapsedTime = Time.time - startTime;

        // [수정] 지속 시간이 다 되면 루프 탈출을 위해 AnimationBool을 TRUE로 설정
if (elapsedTime >= ActionDuration)
{
if (runner is Enemy enemy)
{
                enemy.AnimationBool(AnimationBool, true);
                enemy.Movement?.StopMovement(); // [추가] 시간 만료 시 이동 즉시 정지
}
blackboard.SetValue(EndKey, true);
}

        return runner;
    }

    public override bool UseSomeThing<T>(T runner)
    {
        MonoBehaviour monoRunner = runner as MonoBehaviour;
        if (monoRunner != null)
        {
            var blackboard = (runner as Enemy)._aiController._aiBrain.blackboard;
            float currentStartTime = blackboard.GetValueOrDefault<float>(KEY_START_TIME, -1f);
            
            monoRunner.StartCoroutine(DelayAnimation(runner, StartDelay, currentStartTime));
            return true;
        }
        return false;
    }

    private IEnumerator DelayAnimation<T>(T runner, float delay, float sessionStartTime)
    {
        yield return new WaitForSeconds(delay);
        
        if (runner is Enemy enemy)
        {
            var blackboard = enemy._aiController._aiBrain.blackboard;
            if (blackboard.HasKey(KEY_START_TIME) && 
                Mathf.Approximately(blackboard.GetValue<float>(KEY_START_TIME), sessionStartTime))
            {
                // Action 발동 시점 (예: 돌진 시작 등)
                // 만약 이 시점에 이미 종료되었다면 수행하지 않음
                if (!blackboard.GetValueOrDefault<bool>(EndKey, false))
                {
                    // 루프 시작 시에는 false로 유지 (Animator가 시작 애니메이션을 틀도록)
                    // 또는 시스템 구성에 따라 다를 수 있으나, 탈출이 true라면 여기서는 건드리지 않거나 명시적 false
                    enemy.AnimationBool(AnimationBool, false);
                }
            }
        }
    }

    public override T OnExit<T>(T runner)
    {
        // OnExit 시에는 확실히 TRUE로 만들어 루프 탈출을 보장함
if (runner is Enemy enemy)
{
            enemy.AnimationBool(AnimationBool, true);
            enemy.Movement?.StopMovement(); // [추가] 종료 시 이동 정지
var blackboard = enemy._aiController._aiBrain.blackboard;
blackboard.RemoveKey(KEY_START_TIME);
blackboard.RemoveKey(KEY_DURATION);
blackboard.SetValue(EndKey, false);
}
        return runner;
    }

    public override void Reset<T>(T runner)
    {
        // Reset 시에도 동일하게 처리
if (runner is Enemy enemy)
{
            enemy.AnimationBool(AnimationBool, true);
            enemy.Movement?.StopMovement(); // [추가] 리셋 시 이동 정지
var blackboard = enemy._aiController._aiBrain.blackboard;
blackboard.RemoveKey(KEY_START_TIME);
blackboard.RemoveKey(KEY_DURATION);
blackboard.SetValue(EndKey, false);
}
    }
}
