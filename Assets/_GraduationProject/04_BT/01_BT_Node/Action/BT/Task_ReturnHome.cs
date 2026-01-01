using BehaviorTree;
using Pathfinding;
using UnityEngine;

public class Task_ReturnHome : Node
{
    AIPath  _aiPath;
    bool _hasDestination = false;
    Vector3 HomePosition;
    public override Node Clone()
    {
        Task_ReturnHome newNode = Instantiate(this);
        return newNode;
    }
    public override void OnEnter()
    {
        brain.CombatEnter(false);
        _aiPath = runner.GetComponent<AIPath>();
        runner.EnemyHealth.OnRecoveryHealth?.Invoke(true);

        HomePosition = brain.blackboard.GetValue<Vector3>("HomePosition");
    }
    protected override NodeState OnUpdate()
    {
        if (brain._isCombat)
        {
            return NodeState.FAILURE;
        }
        if (_aiPath != null && _aiPath.reachedDestination)
        {
            _hasDestination = true;
            return NodeState.SUCCESS;
        }
        runner.Movement.StartOrUpdateChase(HomePosition, Enemy.EnemyState.Patrol);
        return NodeState.RUNNING;
    }
    public override void OnExit()
    {
        if(_aiPath != null && _hasDestination)
        {
            // _aiPath.;
        }
        _hasDestination = false;
    }
}