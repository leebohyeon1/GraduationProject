using UnityEngine;
using Pathfinding;

[CreateAssetMenu(fileName = "PlayerChaseStrategy", menuName = "Enemy/Strategy/PlayerChaseStrategy")]
public class PlayerChaseStrategy : EnemyUseAnything
{
    [Header("Movement Settings")]
    public float maxRushSpeed = 20f;
    public float hitRadius = 1.5f;
    public float turnSpeed = 300f;

    // LoopAction의 ActionDuration과 맞춰야 합니다. (혹은 블랙보드 변수로 받아도 됨)
    public float expectedDuration = 5.0f; 

    public AnimationCurve speedCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.2f, 1f), new Keyframe(1, 0));

    private float _originalAcceleration;

    public override T OnEnter<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy == null || enemy.player == null) return runner;
        runner.Movement.StartOrUpdateChase(enemy.player.transform.position);

        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (ai != null && ai is AIPath aiPath)
        {
            _originalAcceleration = aiPath.maxAcceleration;
            aiPath.maxAcceleration = 10000f; 
            aiPath.rotationSpeed = turnSpeed; 
            aiPath.enableRotation = true;
        }

        return runner;
    }

    public override T OnUpdate<T>(T runner)
    {
        var blackboard = runner._aiController._aiBrain.blackboard;
        if(blackboard.GetValue<bool>(EnemyBlackboardKeys.DidLastAttackHit))
        {
            StopRush(runner );
            return runner; // 마지막 공격이 적중했으면 추격 중지
        }
        if(blackboard.GetValue<bool>(LoopAction.EndKey))
        {
            StopRush(runner);
            return runner; // LoopAction이 종료 신호를 보냈으면 추격 중지
        }
        // [핵심 변경점] LoopAction의 진행도가 아니라, '절대 시작 시간'을 가져옴
        // LoopAction.OnEnter가 실행되었다면 이 키가 반드시 존재함
        if (!blackboard.HasKey(LoopAction.KEY_START_TIME))
        {
            return runner; // 아직 시작 시간이 안 찍혔으면 대기
        }

        Enemy enemy = runner as Enemy;
        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (enemy == null || ai == null) return runner;

        // 1. 절대 시간 기반 진행도 계산 (오차 없음)
        float startTime = blackboard.GetValue<float>(LoopAction.KEY_START_TIME);
        float duration = blackboard.GetValue<float>(LoopAction.KEY_DURATION);

        float elapsedTime = Time.time - startTime;
        float normalizedTime = Mathf.Clamp01(elapsedTime / duration);

        // 2. 속도 적용
        float speedMultiplier = speedCurve.Evaluate(normalizedTime);
        ai.maxSpeed = maxRushSpeed * speedMultiplier;
        ai.destination = enemy.player.transform.position;

        // 3. 충돌 체크
        if (Vector3.Distance(enemy.transform.position, enemy.player.transform.position) <= hitRadius)
        {
            StopRush(enemy);
        }

        // 시간 종료 체크는 LoopAction이 해서 OnExit을 부를 것이므로 여기선 생략 가능
        // 하지만 안전장치로 넣어둬도 무방함

        return runner;
    }

    public override T OnExit<T>(T runner)
    {
        Enemy enemy = runner as Enemy;
        if (enemy != null) StopRush(enemy);
        return runner;
    }

    private void StopRush(Enemy enemy)
    {
        enemy.Movement.StopMovement();
        IAstarAI ai = enemy.GetComponent<IAstarAI>();
        if (ai != null && ai is AIPath aiPath)
        {
            aiPath.maxAcceleration = _originalAcceleration;
        }
    }

    public override void Reset<T>(T runner)
    {

    }
}