using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "LoopAction", menuName = "Enemy/Strategy/LoopAction")]
public class LoopAction : EnemyUseAnything
{
    [Header("Settings")]
    public string AnimationBool = "IsRushing";
    public float ActionDuration = 5.0f; // 전체 지속 시간 (이동 시간)
    public float StartDelay = 0.5f;     // 애니메이션 발동 전 딜레이

    // [핵심] 두 스크립트가 공유할 시작 시간 키
    public const string KEY_START_TIME = "Shared_StartTime";
    public const string KEY_DURATION = "Shared_Duration";
    public const string EndKey = "Shared_Ended";

    public override T OnEnter<T>(T runner)
    {
        var blackboard = runner._aiController._aiBrain.blackboard;

        // 1. 시작 시간 도장 찍기 (이 시간을 기준으로 모든 이동이 계산됨)
        if (!blackboard.HasKey(KEY_START_TIME))
        {
            blackboard.SetValue(KEY_START_TIME, Time.time);
            blackboard.SetValue(KEY_DURATION, ActionDuration);
        }

        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        var blackboard = runner._aiController._aiBrain.blackboard;
        if (!blackboard.HasKey(KEY_START_TIME)) return runner;

        float startTime = blackboard.GetValue<float>(KEY_START_TIME);
        float elapsedTime = Time.time - startTime;

        // 2. 시간이 다 되면 종료 (이동도 같이 멈추게 됨)
        if (elapsedTime >= ActionDuration)
        {
            runner.AnimationBool(AnimationBool, true);
            blackboard.SetValue(EndKey, true);
            // 종료 로직 (상위 노드로 종료 신호 보냄)
            // 보통 BT에서는 여기서 return runner가 아니라 종료 상태를 반환하거나
            // 블랙보드 키를 지워서 OnExit을 유도합니다.
        }

        return runner;
    }

    public override bool UseSomeThing<T>(T runner)
    {
        MonoBehaviour monoRunner = runner as MonoBehaviour;
        if (monoRunner != null)
        {
            // 딜레이 후 애니메이션 실행 (이동 로직과는 별개로 돔)
            monoRunner.StartCoroutine(DelayAnimation(runner, StartDelay));
            return true;
        }
        return false;
    }

    private IEnumerator DelayAnimation<T>(T runner, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (runner is Enemy enemy)
        {
            enemy.AnimationBool(AnimationBool, true);
        }
    }

    public override T OnExit<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy != null) enemy.AnimationBool(AnimationBool, false);

        var blackboard = runner._aiController._aiBrain.blackboard;
        blackboard.RemoveKey(KEY_START_TIME); // 키 삭제로 상태 초기화
        blackboard.RemoveKey(KEY_DURATION);
        blackboard.SetValue(EndKey, false);
 
        return runner;
    }
}