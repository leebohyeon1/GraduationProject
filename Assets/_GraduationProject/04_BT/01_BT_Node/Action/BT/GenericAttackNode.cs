using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using System.Collections;
public class GenericAttackNode : Node
{
    [Header("Attack Properties")]
    public string AttackName;
    public int damage;
    public float damageRadius;
    public Vector3 attackOffset;
    public bool maintainAtk;

    private bool _didHitPlayer;
    CalculationResult stat;
    bool tracking = false;
    bool parryEffectPlayed = false;
    public DamageData damageData;
    public override void OnEnter()
    {
        Handler.ResetAllFlags();
        _didHitPlayer = false;
        parryEffectPlayed = false;
        runner.Movement.StopMovement();
        runner.aIPath.enableRotation = false;
        damageData.AttackerTransform = runner.transform;
        runner.AnimationEvent(AttackName);
        runner.SetCurrentAttackData(damageRadius, attackOffset);
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;
        stat = runner.heatSystem.CalculationHeat("Test", runner.heatSystem.ActorType, runner.heatSystem.GetTier(), damage);
        initNode();
        runner.SetStiffness(damageData.StiffnessAmount);
    }

    protected override NodeState OnUpdate()
    {
        Vector3 attackOrigin = runner.transform.position + runner.transform.TransformDirection(attackOffset);

        if (brain.blackboard.GetValue<Vector3>("LastPlayerPos", out Vector3 lastPlayerPos))
        {
            Vector3 directionToPlayer = lastPlayerPos - runner.transform.position;
            directionToPlayer.y = 0;
            if (!tracking)
            {
                runner.transform.rotation = Quaternion.LookRotation(directionToPlayer);
            }
        }
        if (Handler.IsSound)
        {
            Handler.EndSound();
        }
        if (Handler.IsActive)
        {
            tracking = true;
            runner.SetState(Enemy.EnemyState.Attack);
        }

        if (Handler.IsHitWindowOpen)
        {
            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, damageRadius * stat.FinalRange);
            foreach (var col in hitColliders)
            {
                if (col.gameObject == runner.gameObject) continue; // 자기 자신은 무시
                if (col.TryGetComponent<IHeatable>(out IHeatable heatable))
                {
                    stat = runner.heatSystem.CalculationHeat(AttackName, heatable.ActorType, runner.heatSystem.GetTier(), damage);
                    SourceMap sourceMap = runner.heatSystem.SourceMapDataBase.GetSourceMap(AttackName, heatable.ActorType, runner.heatSystem.GetTier());
                    int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
                    heatable.ChangeHeat(deltaHeat);
                }

                if (col.TryGetComponent<IDamageable>(out IDamageable Character))
                {
                    Character.TakeDamage(damageData);

                    _didHitPlayer = true;
                    if (!maintainAtk)
                    {
                        Handler.CloseHitWindow();
                    }
                }
            }
        }

        if (Handler.IsHitWindowOpen && !parryEffectPlayed)
        {
            Handler.CloseHitWindow();
            parryEffectPlayed = true;
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

        if (!parryEffectPlayed)
        {
            parryEffectPlayed = true;
        }
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


