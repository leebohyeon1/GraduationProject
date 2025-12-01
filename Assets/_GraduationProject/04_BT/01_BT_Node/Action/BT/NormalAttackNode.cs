using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using System.Collections;
public class NormalAttackNode : Node
{
    [Header("Attack Properties")]
    public string attackKey;
    public bool maintainAtk;
    private bool _didHitPlayer;
    private EnemyAttackData _data;
    bool tracking = false;
    bool _parryEffectPlayed = false;

    bool _isCooldownDenied;
    bool _isStunned;
    public override void OnEnter()
    {
        Debug.Log("NormalAttackNode OnEnter called.");
        if(!brain.blackboard.GetValue<EnemyAttackData>(attackKey, out _data))
        {
            Debug.LogError("No Attack Data Found for key: " + attackKey);
            return;
        }
        Debug.Log(runner.ParrySystem._isStunned);
        if (runner.ParrySystem.CurrentState == ParrySystem.EnemyState.StunnedExit)
        {
            _isStunned = true;
            Debug.Log("Attack Aborted due to Stun: " + _isStunned);
            return;
        }
        _isCooldownDenied = false;
        if(!brain.IsSkillReady(attackKey, _data.Cooltime))
        {
            Debug.Log("Skill on Cooldown: " + attackKey);
            _isCooldownDenied = true;
            return;
        }
        Handler.ResetAllFlags();


        _didHitPlayer = false;
        _parryEffectPlayed = false;
        runner.Movement.StopMovement();
        runner.aIPath.enableRotation = false;
        _data.damageData.AttackerTransform = runner.transform;
        
        runner.AnimationEvent(_data.AttackName);
        runner.SetCurrentAttackData(_data.damageRadius, _data.attackOffset);
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;
        initNode();
        runner.SetStiffness(_data.damageData.StiffnessAmount);
    }

    protected override NodeState OnUpdate()
    {
        if(_data == null || _isCooldownDenied)
        {
            return NodeState.FAILURE;
        }
        if (runner.ParrySystem.CurrentState == ParrySystem.EnemyState.StunnedExit)
        {
            Debug.Log("Attack Aborted due to Stun: " + _isStunned);
            return NodeState.FAILURE;
        }
        Vector3 attackOrigin = runner.transform.position + runner.transform.TransformDirection(_data.attackOffset);

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
            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, _data.damageRadius );
            brain.blackboard.SetValue("IsAttacking", true);
            foreach (var col in hitColliders)
            {
                if (col.gameObject == runner.gameObject) continue; // 자기 자신은 무시


                if (col.TryGetComponent<IDamageable>(out IDamageable Character))
                {
                    Character.TakeDamage(_data.damageData);

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
        Debug.Log("NormalAttackNode OnExit called.");
        _isStunned = false;
        tracking = false;
        // 노드가 중단될 경우를 대비해 플래그를 다시 한번 리셋
        runner.ParrySystem.StateNormal();        
        brain.blackboard.SetValue("IsAttacking", false);
        Handler.ResetAllFlags();
        runner.SetState(Enemy.EnemyState.Idle);
        runner.SetStiffness(0);
        if(!_isCooldownDenied)
        brain.StartSkillCooldown(attackKey);
        // runner.Movement.StopMovement();
    }
    public override void Abort()
    {
        tracking = false;
        // 노드가 중단될 경우를 대비해 플래그를 다시 한번 리셋
        Handler.ResetAllFlags();
        runner.SetState(Enemy.EnemyState.Idle);

        if (!_parryEffectPlayed)
        {
            _parryEffectPlayed = true;
        }
        runner.SetStiffness(0);
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.attackKey = this.attackKey;
        node.maintainAtk = this.maintainAtk;
        return node;
    }
}


