using BehaviorTree;
using Pathfinding;
using UnityEngine;

public class Task_ReturnHome : Node
{
    AIPath  _aiPath;
    bool _hasDestination = false;
    Vector3 HomePosition;
    public float MoveSpeed = 6.0f;
    
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
        _aiPath.enableRotation = true;
        
        brain.blackboard.SetValue("GoHome", true);
    }
    protected override NodeState OnUpdate()
    {
        if (brain._isCombat)
        {
            return NodeState.FAILURE;
        }
        _aiPath.enableRotation = true;

        runner.Movement.StartOrUpdateChase(HomePosition, EnemyStateController.EnemyState.Patrol, MoveSpeed);
        if (_aiPath != null && _aiPath.reachedDestination)
        {
            _hasDestination = true;
            return NodeState.SUCCESS;
        }
        return NodeState.RUNNING;
    }
    public override void OnExit()
    {
        if(_aiPath != null && _hasDestination)
        {
            brain.blackboard.SetValue("GoHome", false);
            runner.Movement.StopMovement();
            // _aiPath.;
        }
        _hasDestination = false;
    }
}