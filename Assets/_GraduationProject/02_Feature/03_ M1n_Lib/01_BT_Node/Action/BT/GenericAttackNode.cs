using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "GenericAttackNode", menuName = "BehaviorTree/Action/GenericAttackNode")]
public class GenericAttackNode : Node
{
    [Header("Attack Properties")]
    public string animationName;
    public int damage;
    public float damageRadius;
    public Vector3 attackOffset;
    public bool maintainAtk;

    private bool _didHitPlayer;


    public override void OnEnter()
    {
        // 1. Enemy의 범용 플래그들을 리셋합니다.
        runner.ResetActionFlags();
        _didHitPlayer = false;

        runner.SetState(Enemy.EnemyState.Attack);
        runner.Movement.StopMovement();
        runner.AnimationEvent(animationName);
        runner.SetCurrentAttackData(damageRadius, attackOffset);
    }

    protected override NodeState OnUpdate()
    {
        Vector3 attackOrigin = runner.transform.position + runner.transform.TransformDirection(attackOffset);
        if (runner.IsSound)
        {
            runner.PlayFeedback(animationName, attackOrigin);
            runner.AnimationEvent_EndSound();
        }
        if (runner.IsHitWindowOpen)
        {
            Debug.Log(":");
            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, damageRadius, LayerMask.GetMask("Player"));
            foreach (var col in hitColliders)
            {
                if (col.TryGetComponent<IDamageable>(out IDamageable player))
                {
                    player.TakeDamage(damage, runner);
                    _didHitPlayer = true;
                    if (!maintainAtk)
                    {
                        runner.AnimationEvent_CloseHitWindow();
                    }
                }
            }
        }

        if (runner.IsActionFinished)
        {
            Debug.Log(this.animationName);
            return _didHitPlayer ? NodeState.SUCCESS : NodeState.FAILURE;
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        // 노드가 중단될 경우를 대비해 플래그를 다시 한번 리셋
        runner.ResetActionFlags();
        runner.SetState(Enemy.EnemyState.Idle);
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.animationName = this.animationName;
        node.damage = this.damage;
        node.damageRadius = this.damageRadius;
        node.attackOffset = this.attackOffset;
        return node;
    }
}