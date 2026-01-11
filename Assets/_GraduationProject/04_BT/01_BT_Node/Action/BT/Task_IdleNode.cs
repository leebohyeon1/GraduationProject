using UnityEngine;
using BehaviorTree;


[CreateAssetMenu(fileName = "IdleNode", menuName = "BehaviorTree/IdleNode")]
public class IdleNode : Node
{
    [Header("Inverter Settings")]
    [SerializeField] private float idleTime = 2f;
    private float idleTimer;

    public override void OnEnter()
    {
        runner.SetState(EnemyStateController.EnemyState.Idle);
        runner.Movement.StopMovement();
    }

    protected override NodeState OnUpdate()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            return NodeState.SUCCESS;
        }
        return NodeState.RUNNING;
    }
    public override void Abort()
    {
        base.Abort();
        idleTimer = 0f;

    }
    public override Node Clone()
    {
        return Instantiate(this);
    }
}