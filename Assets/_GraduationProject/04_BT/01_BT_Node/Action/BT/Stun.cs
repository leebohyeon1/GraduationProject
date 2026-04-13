using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "Stun", menuName = "BehaviorTree/Stun")]
public class Stun : Node
{
    public int Damage = 30;
    public DamageData damageData;
    public override void OnEnter()
    {
        damageData.AttackerTransform = runner.transform;
    }
    protected override NodeState OnUpdate()
    {
        // runner.player.ApplyStun(2);
        if (runner.CurrentState == EnemyStateController.EnemyState.Rush)
        {
            runner.GetComponent<Animator>().SetBool("Rush_Running", false);
            runner.SetState(EnemyStateController.EnemyState.Idle);
            runner.GetComponent<Rigidbody>().linearVelocity = Vector3.zero; 
        }
        // runner.player.GetComponent<IDamageable>().TakeDamage(Damage, runner.heatSystem.GetTier(), damageData);
        // // BTDebug.Log("플레이어 기절함");
        return NodeState.SUCCESS;
    }

    public override void Abort()
    {
        base.Abort();
        // 상태를 먼저 변경하여 StopMovement의 보호 로직을 통과하게 함
        if (runner.CurrentState == EnemyStateController.EnemyState.Rush)
        {
            runner.GetComponent<Animator>().SetBool("Rush_Running", false);
            runner.SetState(EnemyStateController.EnemyState.Idle);
            runner.GetComponent<Rigidbody>().linearVelocity = Vector3.zero; 

        }
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}