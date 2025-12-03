using UnityEngine;
using Pathfinding;
using BehaviorTree;
using Pathfinding.RVO;
public class Task_StrafeAroundTarget : Node
{[Header("Orbit Settings")]
    public float radius = 5.0f;
    public float rotationSpeed = 20.0f;    

    private AIPath _ai;              
    private RVOController _rvo;      
    private float _currentAngle = 0f;

    public override void OnEnter()
    {
        base.OnEnter();
        // 1. 컴포넌트 캐싱 (한 번만 가져오도록 최적화 가능)
        if (_ai == null) _ai = runner.GetComponent<AIPath>();
        if (_rvo == null) _rvo = runner.GetComponent<RVOController>();

        // 2. 초기 각도 설정 (Blackboard의 SlotIndex 활용)
        if (brain.blackboard.GetValue<int>("SquadSlotIndex", out int myIndex) && 
            brain.blackboard.GetValue<int>("PeripheralColleagues", out int total))
        {
            float angleStep = 360f / (total > 0 ? total : 1);
            _currentAngle = myIndex * angleStep;
        }
        _ai.enableRotation = false;
        
    }

    protected override NodeState OnUpdate()
    {
        if (_ai == null) return NodeState.FAILURE;
        if(!brain.blackboard.GetValue<bool>("IsSurrounding"))
        {
            return NodeState.SUCCESS;
        }
        for(int i = 0; i < runner._aiController.enemyAttackDatas.Length; i++)
        {
            if(runner._aiController._aiBrain.IsSkillReady(runner._aiController.enemyAttackDatas[i].AttackName,
            runner._aiController.enemyAttackDatas[i].Cooltime))
            {
                return NodeState.SUCCESS;
            }
        }
        Vector3 targetPos = runner.player.transform.position; 
        
        _currentAngle += rotationSpeed * Time.deltaTime;
        if (_currentAngle >= 360f) _currentAngle -= 360f;

        float x = Mathf.Sin(_currentAngle * Mathf.Deg2Rad) * radius;
        float z = Mathf.Cos(_currentAngle * Mathf.Deg2Rad) * radius;

        Vector3 desiredPos = targetPos + new Vector3(x, 0, z);

        NNInfo info = AstarPath.active.GetNearest(desiredPos);
        if (info.node != null)
        {
            desiredPos = info.position; 
        }
        _ai.destination = desiredPos;
        Debug.Log($"Strafing to Position: {desiredPos} this object name: {runner.name}");
        runner.Movement.StartOrUpdateChase(desiredPos);
        RotateTowards(targetPos);

        return NodeState.RUNNING;
    }

    private void RotateTowards(Vector3 target)
    {
        Vector3 dir = (target - runner.transform.position).normalized;
        dir.y = 0; // 수평 회전만
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            runner.transform.rotation = Quaternion.Slerp(runner.transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }
    
    public override void OnExit()
    {
        runner.groupAi.SurroundToggle(false);
        brain.blackboard.SetValue("IsSurrounding", false);
        Debug.Log("Exiting Strafe Around Target");
        runner.aIPath.enableRotation = true;
        _rvo.velocity = Vector3.zero;
        runner.Movement.StopMovement();
        _ai.Teleport(runner.transform.position);
         // 필요 시 멈춤 처리
         // if(_ai != null) _ai.destination = runner.transform.position;
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}