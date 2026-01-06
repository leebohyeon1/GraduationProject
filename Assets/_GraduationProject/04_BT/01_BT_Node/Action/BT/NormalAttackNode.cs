using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using System.Collections;
using Pathfinding;
public class NormalAttackNode : Node
{
    [Header("Attack Properties")]
    public string attackKey;
    public bool maintainAtk;
    private bool _didHitPlayer;
    private EnemyAttackData _data;
    bool tracking = false;
    bool _parryEffectPlayed = false;
    public EnemyUseAnything SO = null;

    bool _isCooldownDenied = false;

    public override void OnEnter()
    {
        if (!brain.blackboard.GetValue<EnemyAttackData>(attackKey, out _data))
        {
            Debug.LogError("No Attack Data Found for key: " + attackKey);
            return;
        }
        if (runner.ParrySystem.CurrentState == ParrySystem.EnemyState.StunnedExit)
        {
            return;
        }
        if (!brain.IsSkillReady(attackKey, _data.Cooltime))
        {
            _isCooldownDenied = true;
            return;
        }
        Handler.ResetAllFlags();
        _didHitPlayer = false;
        _parryEffectPlayed = false;

        _data.damageData.AttackerTransform = runner.transform;
        runner.AnimationEvent(_data.AttackName);
        runner.SetState(Enemy.EnemyState.Attack);
        Debug.Log($"state {runner.CurrentState}");
        runner.SetCurrentAttackData(_data.damageRadius, _data.attackOffset);

        runner.SetStiffness(_data.damageData.StiffnessAmount);

        runner.Movement.StopMovement();
        if(SO != null)
        {
            tracking = true;
        }
    }

    protected override NodeState OnUpdate()
    {
        if (_data == null || _isCooldownDenied)
        {
            return NodeState.FAILURE;
        }
        if (runner.ParrySystem.CurrentState == ParrySystem.EnemyState.StunnedExit)
        {
            return NodeState.FAILURE;
        }
        if (SO != null)
        {
            SO.OnUpdate(runner);
        }
        Vector3 attackOrigin = runner.transform.position + runner.transform.TransformDirection(_data.attackOffset);

        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;
        if (!tracking && SO == null)
        {
            runner.transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
        if (Handler.IsActionSO)
        {
            if (SO != null)
            {
                SO.OnEnter(runner);
            }
            Handler.EndSO();
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
            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, _data.damageRadius);
            brain.blackboard.SetValue("IsAttacking", true);
            foreach (var col in hitColliders)
            {
                if (col.gameObject == runner.gameObject) continue; // 자기 자신은 무시
                Debug.Log("[NormalAttackNode] Checking collision with " + col.gameObject.name);
                if (col.TryGetComponent<IDamageable>(out IDamageable Character))
                {
                    Character.TakeDamage(_data.damageData);
                    Debug.Log("[NormalAttackNode] Hit " + Character);

                    _didHitPlayer = true;
                    if (!maintainAtk)
                    {
                        Handler.CloseHitWindow();
                    }
                }
            }
        }

        if (Handler.IsHitWindowOpen && !_parryEffectPlayed)
        {
            Handler.CloseHitWindow();
            _parryEffectPlayed = true;
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
        runner.ParrySystem.StateNormal();
        brain.blackboard.SetValue("IsAttacking", false);
        Handler.ResetAllFlags();
        runner.SetState(Enemy.EnemyState.Idle);
        runner.aIPath.enableRotation = true;
        runner.SetStiffness(0);
        if (SO != null)
        {
            SO.OnExit(runner);
        }
        if (!_isCooldownDenied)
        {
            brain.StartSkillCooldown(attackKey);
        }

    }
    public override void Abort()
    {
        Debug.Log("[NormalAttackNode] Abort called.");
        tracking = false;
        // 노드가 중단될 경우를 대비해 플래그를 다시 한번 리셋
        Handler.ResetAllFlags();
        runner.SetState(Enemy.EnemyState.Idle);
        runner.aIPath.enableRotation = true;

        if (!_parryEffectPlayed)
        {
            _parryEffectPlayed = true;
        }
        runner.SetStiffness(0);
        if (SO != null)
        {
            SO.OnExit(runner);
        }
        var RVO = runner.GetComponent<Pathfinding.RVO.RVOController>();
        if (RVO != null)
        {
            RVO.locked = false;
            RVO.lockWhenNotMoving = true;
        }
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = this.attackKey;
        node.maintainAtk = this.maintainAtk;
        return node;
    }
}


