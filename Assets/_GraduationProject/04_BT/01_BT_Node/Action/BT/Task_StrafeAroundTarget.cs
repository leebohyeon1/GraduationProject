using UnityEngine;
using Pathfinding;
using BehaviorTree;
using Pathfinding.RVO;

public class Task_StrafeAroundTarget : Node
{
    [Header("Orbit Settings")]
    public float radius = 5.0f;
    public float rotationSpeed = 20.0f;    

    private AIPath _ai;              
    private RVOController _rvo;      
    private float _currentAngle = 0f;

    public override void OnEnter()
    {
        base.OnEnter();
        if (_ai == null) _ai = runner.GetComponent<AIPath>();
        if (_rvo == null) _rvo = runner.GetComponent<RVOController>();

        int myIndex = 0;
        int total = 1;
        
        brain.blackboard.GetValue<int>("SquadSlotIndex", out myIndex);
        brain.blackboard.GetValue<int>("PeripheralColleagues", out total);

        float angleStep = 360f / (total > 0 ? total : 1);
        _currentAngle = myIndex * angleStep;
        
        if (_ai != null) _ai.enableRotation = false;
    }

    protected override NodeState OnUpdate()
    {
        if (_ai == null) return NodeState.FAILURE;
        if(!brain.blackboard.GetValue<bool>("IsSurrounding")) return NodeState.SUCCESS;

        foreach(var data in runner._aiController.inGameenemyAttackDatas)
        {
            if(brain.IsSkillReady(data.AttackName, data.Cooltime)) return NodeState.SUCCESS;
        }

        Vector3 targetPos = runner.player.transform.position; 
        _currentAngle += rotationSpeed * Time.deltaTime;
        if (_currentAngle >= 360f) _currentAngle -= 360f;

        float x = Mathf.Sin(_currentAngle * Mathf.Deg2Rad) * radius;
        float z = Mathf.Cos(_currentAngle * Mathf.Deg2Rad) * radius;
        Vector3 desiredPos = targetPos + new Vector3(x, 0, z);

        NNInfo info = AstarPath.active.GetNearest(desiredPos);
        if (info.node != null) desiredPos = info.position; 

        _ai.destination = desiredPos;
        runner.Movement.StartOrUpdateChase(desiredPos);
        RotateTowards(targetPos);

        return NodeState.RUNNING;
    }

    private void RotateTowards(Vector3 target)
    {
        Vector3 dir = (target - runner.transform.position).normalized;
        dir.y = 0; 
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            runner.transform.rotation = Quaternion.Slerp(runner.transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }
    
    public override void OnExit()
    {
        brain.blackboard.SetValue("IsSurrounding", false);
        if (runner.aIPath != null) runner.aIPath.enableRotation = true;
        if (_rvo != null) _rvo.velocity = Vector3.zero;
        runner.Movement.StopMovement();
        if (_ai != null) _ai.Teleport(runner.transform.position);
    }

    public override Node Clone() => Instantiate(this);
}
