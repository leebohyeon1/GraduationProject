using UnityEngine;
using BehaviorTree;

public class Task_SetRecovery : Node
{
    
    public override Node Clone()
    {
         return Instantiate(this);
    }

    public override void OnEnter()
    {
        // 몬스터의 상태를 'Recovery'로 변경 (Enemy 스크립트에 해당 기능이 있다고 가정)
        // 예: runner.SetRecoveryMode(true); 
        // 또는 runner.isRecovering = true;
        
        if (runner != null)
        {
            runner.EnemyHealth.SetRecovery(true); // 회복 ON
            // runner.SetInvincible(true); // <-- 무적은 삭제함!
        }
    }

    protected override NodeState OnUpdate()
    {
        // 상태만 켜고 바로 다음 노드(이동)로 넘어가야 하므로 Success 반환
        return NodeState.SUCCESS;
    }
}