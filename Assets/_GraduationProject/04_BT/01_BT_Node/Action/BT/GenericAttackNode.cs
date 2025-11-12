using UnityEngine;
using BehaviorTree;
public class GenericAttackNode : Node
{
    [Header("Attack Properties")]
    public string AttackName;
    public int damage;
    public float damageRadius;
    public Vector3 attackOffset;
    public bool maintainAtk;

    private bool _didHitPlayer;
    bool tracking = false;
    bool parryEffectPlayed = false;
    public DamageData damageData;
    public float rotationSpeed = 15f; 
    public override void OnEnter()
    {
        Handler.ResetAllFlags();
        _didHitPlayer = false;
        parryEffectPlayed = false;  
        // runner.Movement.StartOrUpdateChase(runner.player.transform.position);
        runner.Movement.StopMovement();
        damageData.AttackerTransform = runner.transform;
        runner.AnimationEvent(AttackName);
        runner.SetCurrentAttackData(damageRadius, attackOffset);
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;
        initNode();
        runner.SetStiffness(damageData.StiffnessAmount);
        brain.blackboard.SetValue("IsAttacking", true);
    }

    protected override NodeState OnUpdate()
    {
        runner.aIPath.enableRotation = false;

        Vector3 attackOrigin = runner.transform.position + runner.transform.TransformDirection(attackOffset);

    if (Handler.IsSound)
    {
        Handler.EndSound();
    }
    if (Handler.IsActive)
    {
        tracking = true;
        runner.SetState(Enemy.EnemyState.Attack);
    }else
    if (brain.blackboard.GetValue<Vector3>("LastPlayerPos", out Vector3 lastPlayerPos))
    {
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            if (!tracking)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

                if (rotationSpeed > 0)
                {
                    runner.transform.rotation = Quaternion.Slerp(
                        runner.transform.rotation, 
                        targetRotation, 
                        Time.deltaTime * rotationSpeed
                    );
                }
                else
                {
                    runner.transform.rotation = targetRotation;
                }
            }
        }
    }
        
        if(Handler.IsHitWindowOpen && !parryEffectPlayed)
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
        // 노드가 중단될 경우를 대비해 플래그를 다시 한번 리셋
        Debug.Log("GenericAttackNode Exit");
        Handler.ResetAllFlags();
        runner.SetState(Enemy.EnemyState.Idle);
        brain.blackboard.SetValue("IsAttacking", false);
        tracking = false;
        // runner.Movement.StopMovement();
    }
    public override void Abort()
    {
        Debug.Log("GenericAttackNode Aborted");
        // 노드가 중단될 경우를 대비해 플래그를 다시 한번 리셋
        Handler.ResetAllFlags();
        runner.SetState(Enemy.EnemyState.Idle);
        
        if(!parryEffectPlayed)
        {
            parryEffectPlayed = true;   
        }
        tracking = false;
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