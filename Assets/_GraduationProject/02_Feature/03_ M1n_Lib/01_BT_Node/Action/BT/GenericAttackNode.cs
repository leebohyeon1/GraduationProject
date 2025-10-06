using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "GenericAttackNode", menuName = "BehaviorTree/Action/GenericAttackNode")]
public class GenericAttackNode : Node
{
    [Header("Attack Properties")]
    public string AttackName;
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
        runner.Movement.StartOrUpdateChase(runner.player.transform.position);
        runner.SetState(Enemy.EnemyState.Attack);
        runner.Movement.StopMovement();
        runner.AnimationEvent(AttackName);
        runner.SetCurrentAttackData(damageRadius, attackOffset);
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;
        stat = runner.heatSystem.CalculationHeat("Test", runner.heatSystem.ActorType, runner.heatSystem.GetTier(), damage);
        initNode();
        runner.SetStiffness(StiffenessAmount);
    }

    protected override NodeState OnUpdate()
    {
        Vector3 attackOrigin = runner.transform.position + runner.transform.TransformDirection(attackOffset);
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            runner.transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
        if (Handler.IsSound)
        {
            // runner.PlayFeedback(AttackName, attackOrigin);
            Handler.EndSound();
        }
        if (Handler.IsHitWindowOpen)
        {
            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, damageRadius * stat.FinalRange);
            foreach (var col in hitColliders)
            {
                if(col.gameObject == runner.gameObject) continue; // 자기 자신은 무시
                if (col.TryGetComponent<IHeatable>(out IHeatable heatable))
                {
                    stat = runner.heatSystem.CalculationHeat(AttackName, heatable.ActorType, runner.heatSystem.GetTier(), damage);
                    SourceMap sourceMap = runner.heatSystem.SourceMapDataBase.GetSourceMap(AttackName, heatable.ActorType, runner.heatSystem.GetTier());
                    int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
                    heatable.ChangeHeat(deltaHeat);
                }

               if (col.TryGetComponent<IDamageable>(out IDamageable Character))
                {
                    Character.TakeDamage( stat.FinalDamage, StiffenessAmount);
                    // Character.TakeDamage(stat.FinalDamage);
                    
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
        node.AttackName = this.AttackName;
        node.damage = this.damage;
        node.damageRadius = this.damageRadius;
        node.attackOffset = this.attackOffset;
        return node;
    }
}