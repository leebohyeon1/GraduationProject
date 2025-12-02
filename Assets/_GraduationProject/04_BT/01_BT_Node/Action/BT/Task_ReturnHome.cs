using BehaviorTree;
using Pathfinding;
using UnityEngine;

public class Task_ReturnHome : Node
{
    AIPath  _aiPath;
    bool _hasDestination = false;
    public override Node Clone()
    {
        Task_ReturnHome newNode = Instantiate(this);
        return newNode;
    }
    public override void OnEnter()
    {
        brain.CombatEnter(false);
        _aiPath = runner.GetComponent<AIPath>();
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
        // if(brain.blackboard.GetValue<bool>())
        runner.Movement.StartOrUpdateChase(brain.blackboard.GetValue<Vector3>("HomePosition"), Enemy.EnemyState.Patrol);
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