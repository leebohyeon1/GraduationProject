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
    [SerializeField] private int StiffenessAmount = 10;
    CalculationResult stat;

    public override void OnEnter()
    {
        // 1. Enemy의 범용 플래그들을 리셋합니다.
        Handler.ResetAllFlags();
        _didHitPlayer = false;

        runner.SetState(Enemy.EnemyState.Attack);
        runner.Movement.StopMovement();
        runner.AnimationEvent(animationName);
        runner.SetCurrentAttackData(damageRadius, attackOffset);
        stat = runner.heatSystem.CalculationHeat("Test", ActorType.Monster, runner.heatSystem.GetTier(), damage);
    }

    protected override NodeState OnUpdate()
    {
        Vector3 attackOrigin = runner.transform.position + runner.transform.TransformDirection(attackOffset);
        if (Handler.IsSound)
        {
           // runner.PlayFeedback(animationName, attackOrigin);
            Handler.EndSound();
        }
        if (Handler.IsHitWindowOpen)
        {
            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, damageRadius * stat.FinalRange, LayerMask.GetMask("Player"));
            foreach (var col in hitColliders)
            {
                if (col.TryGetComponent<IDamageable>(out IDamageable player))
                {
                    // player.TakeDamage( stat.FinalDamage, StiffenessAmount, runner);
                    player.TakeDamage( stat.FinalDamage, runner);
                    _didHitPlayer = true;
                    if (!maintainAtk)
                    {
                        Handler.CloseHitWindow();
                    }
                }
            }
        }

        if (Handler.IsActionFinished)
        {
            return _didHitPlayer ? NodeState.SUCCESS : NodeState.FAILURE;
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        // 노드가 중단될 경우를 대비해 플래그를 다시 한번 리셋
        Handler.ResetAllFlags();
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