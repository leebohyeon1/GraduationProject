using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "LoopAction", menuName = "Enemy/Strategy/LoopAction")]
public class LoopAction : EnemyUseAnything
{
    [Header("Settings")]
    public string AnimationBool = "IsRushing";
    public float ExitTime = 5;
    public float ExitAnimation = 0.5f;

    // 블랙보드 키 정의
    private const string KEY_TIMER = "LoopAction_Timer";
    private const string KEY_IS_TRIGGERED = "LoopAction_IsTriggered";

    public override T OnEnter<T>(T runner)
    {
        var blackboard = runner._aiController._aiBrain.blackboard;
        if(blackboard.HasKey(KEY_TIMER))
        {
            // 이미 실행 중인 상태라면 초기화하지 않고 바로 리턴
            return runner;
        }

        // 1. 초기화: 타이머 0, 트리거 false로 설정
        // 이 코드가 실행되어야 비로소 OnUpdate가 작동 자격을 얻습니다.
        blackboard.SetValue(KEY_TIMER, 0f);
        blackboard.SetValue(KEY_IS_TRIGGERED, false);

        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        var blackboard = runner._aiController._aiBrain.blackboard;

        // [안전장치 1] 타이머 키가 아예 없다면? -> OnEnter가 아직 안 돌았거나 초기화 실패
        // 즉시 리턴하여 아래 로직이 실행되지 않도록 차단
        if (!blackboard.HasKey(KEY_TIMER))
        {
            return runner;
        }


        // 2. 타이머 계산
        float currentTimer = blackboard.GetValue<float>(KEY_TIMER);
        currentTimer += Time.deltaTime;
        blackboard.SetValue(KEY_TIMER, currentTimer);

        // 3. 시간 체크
        if (currentTimer >= ExitTime)
        {
            runner.AnimationBool(AnimationBool, true);
            
            // 실행 완료 표시 (이제 [안전장치 2]에 의해 다음 프레임부터는 Update 로직이 스킵됨)
            blackboard.SetValue(KEY_IS_TRIGGERED, true);
        }
        
        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy != null)
        {
            enemy.AnimationBool(AnimationBool, false);
        }

        // [정리] 상태 종료 시 키를 삭제하거나 초기화하여 다음 실행 대비
        var blackboard = runner._aiController._aiBrain.blackboard;
        
        // 블랙보드 구현에 RemoveKey가 있다면 사용하는 것이 가장 좋습니다.
        // 없다면 null이나 초기값으로 돌려놓습니다.
        if (blackboard.HasKey(KEY_TIMER)) 
            blackboard.RemoveKey(KEY_TIMER); 

        if (blackboard.HasKey(KEY_IS_TRIGGERED))
            blackboard.RemoveKey(KEY_IS_TRIGGERED);

        return runner;
    }

    public override bool UseSomeThing<T>(T runner)
    {
        MonoBehaviour monoRunner = runner as MonoBehaviour;
        if (monoRunner != null)
        {
            monoRunner.StartCoroutine(DelayAnimation(runner, ExitAnimation));
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
}