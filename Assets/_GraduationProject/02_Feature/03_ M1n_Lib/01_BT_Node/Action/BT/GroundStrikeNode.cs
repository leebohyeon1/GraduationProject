using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using MoreMountains.Feedbacks; // List를 사용하기 위해 추가

[CreateAssetMenu(fileName = "GroundStrikeNode", menuName = "BehaviorTree/Action/GroundStrikeNode")]
public class GroundStrikeNode : Node
{
    [Header("Attack Properties")]
    public int damage = 15;
    public float damageRadius = 3f;
    public Vector3 attackOriginOffset = new Vector3(0, 0.5f, 1.5f);

    private List<Player> _hitPlayers;

    public override void OnEnter()
    {
        _hitPlayers = new List<Player>();

        Handler.ResetAllFlags();
        runner.SetState(Enemy.EnemyState.Attack);
        runner.Movement.StopMovement();
        runner.AnimationEvent("Do_GroundStrike");
        runner.SetCurrentAttackData(this.damageRadius, this.attackOriginOffset);
    }

    protected override NodeState OnUpdate()
    {
        Vector3 attackOrigin = runner.transform.position + runner.transform.TransformDirection(attackOriginOffset);

        if (Handler.IsSound)
        {
            runner.PlayFeedback("GroundStrike", attackOrigin);
            Handler.EndSound();

        }
        // "공격 창 열림!" 신호가 켜져 있는 동안 매 프레임 실행
        if (Handler.IsHitWindowOpen)
        {

            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, damageRadius, LayerMask.GetMask("Player"));
            foreach (var col in hitColliders)
            {
                if (col.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    Player player = damageable as Player;
                    IKnockbackable knockbackable = player.GetComponent<IKnockbackable>();
                    
                    if (!_hitPlayers.Contains(player))
                    {
                        damageable.TakeDamage(damage, runner);
                        knockbackable.ApplyKnockback(10f, (player.transform.position - attackOriginOffset).normalized);
                        _hitPlayers.Add(player); // 리스트에 추가하여 중복 데미지 방지
                    }
                }
            }
        }

        // "행동 끝!" 신호를 받으면 노드를 종료합니다.
        if (Handler.IsActionFinished)
        {
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        Handler.ResetAllFlags();
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.damage = this.damage;
        node.damageRadius = this.damageRadius;
        node.attackOriginOffset = this.attackOriginOffset;
        return node;
    }
}