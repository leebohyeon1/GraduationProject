using UnityEngine;
using BehaviorTree;
using MoreMountains.Feedbacks;
using Pathfinding;
using andywiecko.BurstTriangulator;

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
    bool tracking = false;
    AIPath aIPath;
    public override void OnEnter()
    {
        aIPath = runner.GetComponent<AIPath>();
        // 1. Enemy의 범용 플래그들을 리셋합니다.
        Handler.ResetAllFlags();
        _didHitPlayer = false;
        // runner.Movement.StartOrUpdateChase(runner.player.transform.position);
        runner.SetState(Enemy.EnemyState.Attack);
        runner.Movement.StopMovement();
        // aIPath.enableRotation = false;
        
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

        if (directionToPlayer != Vector3.zero && !tracking)
        {
            runner.transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
        if (Handler.IsSound)
        {
            Handler.EndSound();
        }
        if (Handler.IsHitWindowOpen)
        {
            tracking = true;
        }
        
        if (Handler.IsHitWindowOpen && !runner.ParrySystem.IsParry)
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
                    Debug.Log($"{damage} damage {stat.FinalDamage} finalDmg,  Tier{runner.heatSystem.GetTier()}");
                }

               if (col.TryGetComponent<IDamageable>(out IDamageable Character))
                {
                    Character.TakeDamage(stat.FinalDamage, 0,new DamageData(StiffenessAmount, runner.transform));
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
        tracking = false;
        // 노드가 중단될 경우를 대비해 플래그를 다시 한번 리셋
        Handler.ResetAllFlags();
        runner.SetState(Enemy.EnemyState.Idle);
        // runner.Movement.StopMovement();
    }
    public override void Abort()
    {
        tracking = false;
        // 노드가 중단될 경우를 대비해 플래그를 다시 한번 리셋
        Handler.ResetAllFlags();
        runner.SetState(Enemy.EnemyState.Idle);
        // runner.Movement.StopMovement();
        // Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        // directionToPlayer.y = 0;
        
        // runner.transform.rotation = Quaternion.LookRotation(directionToPlayer);
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