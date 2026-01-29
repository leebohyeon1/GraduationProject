using UnityEngine;
using Pathfinding;

[CreateAssetMenu(fileName = "RushAttackStrategy", menuName = "Enemy/Strategy/Rush Attack")]
public class RushAttackStrategy : EnemyUseAnything
{
    // [설정값] 이런 건 SO에 저장해도 됩니다. (모든 몬스터 공통)
    public float rushSpeed = 15f;    
    public float stopDistance = 1.0f;
    bool endStrategy = false;
    public float speed = 6;

    // [상태값] 돌진이 멈췄는지 여부 등은 '인스턴스'가 필요하지만, 
    // 간단하게 하기 위해 여기서는 runner를 통해 제어하거나, 
    // 복잡하면 Node에서 관리해야 합니다.
    // 일단 여기서는 로직만 처리합니다.



    public override T OnEnter<T>(T runner)
    {
         // runner를 통해 씬에 있는 컴포넌트에 접근합니다.
        var aiPath = runner.GetComponent<AIPath>(); 
        if (aiPath != null)
        {
            aiPath.maxSpeed = rushSpeed;
            aiPath.enableRotation = false;
            endStrategy = false;
        }
        return runner;
         // Debug.Log($"{runner.name}가 돌진 전략을 시작함");
        Debug.Log($"{runner.name}가 돌진 전략을 시작함");
    }


    public override T OnUpdate<T>(T runner)
    {
        // 씬에 있는 플레이어 찾기: runner.player
        if (runner.player == null) return null;

        // 로직 수행
        float dist = Vector3.Distance(runner.transform.position, runner.player.transform.position);
        
        if (dist > stopDistance && !runner.animHandler.IsHitWindowOpen && !endStrategy)
        {
             runner.Movement.StartRush(runner.player.transform.position, speed);
             // 회전 로직 등...
        }
        else
        {
             runner.Movement.StopMovement();
             endStrategy = true;
        }
        return runner;
    }

    public override T OnExit<T>(T runner) // <--- 종료 시 정리
    {
        // runner를 원래대로 돌려놓기
        var aiPath = runner.GetComponent<AIPath>();
        if (aiPath != null)
        {
            aiPath.maxSpeed = runner.Movement._normalSpeed; // 원래 속도로 복구 (혹은 Enemy 스탯 참조)
            aiPath.enableRotation = true;
        }
        return runner;
    }

    public override void Reset<T>(T runner)
    {
        
    }
}