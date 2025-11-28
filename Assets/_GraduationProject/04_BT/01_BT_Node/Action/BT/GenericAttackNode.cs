using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using System.Collections;
public class GenericAttackNode : Node
{
    [Header("Attack Properties")]
    public EnemyAttackData AtkData;
    public bool maintainAtk;
    private bool _didHitPlayer;
    bool tracking = false;
    bool parryEffectPlayed = false;
    public override void OnEnter()
    {
        Handler.ResetAllFlags();
        _didHitPlayer = false;
        parryEffectPlayed = false;
        runner.Movement.StopMovement();
        runner.aIPath.enableRotation = false;
        AtkData.damageData.AttackerTransform = runner.transform;
        runner.AnimationEvent(AtkData.AttackName);
        runner.SetCurrentAttackData(AtkData.damageRadius, AtkData.attackOffset);
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;
        initNode();
        runner.SetStiffness(AtkData.damageData.StiffnessAmount);
    }

    protected override NodeState OnUpdate()
    {
        Vector3 attackOrigin = runner.transform.position + runner.transform.TransformDirection(AtkData.attackOffset);

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
            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, AtkData.damageRadius );
            foreach (var col in hitColliders)
            {
                if (col.gameObject == runner.gameObject) continue; // 자기 자신은 무시


                if (col.TryGetComponent<IDamageable>(out IDamageable Character))
                {
                    Character.TakeDamage(AtkData.damageData);

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
        return node;
    }
}


