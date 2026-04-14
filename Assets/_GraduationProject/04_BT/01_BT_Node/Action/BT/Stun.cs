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
        return NodeState.SUCCESS;
    }

    public override void Abort()
    {
        base.Abort();
        // ?곹깭瑜?癒쇱? 蹂寃쏀븯??StopMovement??蹂댄샇 濡쒖쭅???듦낵?섍쾶 ??
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
