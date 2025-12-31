using UnityEngine;
using BehaviorTree;
using Pathfinding; // [필수] A* Pathfinding Project 네임스페이스

public class Service_PressureMove : ServiceNode
{
    public float Distance = 5.5f;       // 타겟과 유지할 압박 거리 (m)
    public float Change_Dir_Time = 2.5f;// 방향(좌/우)을 바꾸는 시간 간격 (초)
    public float BlockedWaitTime = 0.5f;// 벽에 막혔을 때 대기하는 시간 (초)
    public string Pos_Key = "PressurePos"; // 계산된 좌표를 저장할 블랙보드 키
    public string Dir_Key = "StrafeDir";   // 현재 방향(-1, 1)을 저장할 블랙보드 키
    
    private float _timer;
    private int _currentDir = 1; // 1: 오른쪽, -1: 왼쪽
    private float _waitTimer = 0f;

   public override void OnEnter()
    {
        base.OnEnter(); // 타이머 초기화 등을 위해 부모 호출
        _timer = Change_Dir_Time;
        _currentDir = Random.Range(0, 2) == 0 ? -1 : 1; 
        _waitTimer = 0f;
    }

    protected override void OnServiceLogic()
    {
        if (runner.player == null) return;

        // 1. 대기 타이머
        if (_waitTimer > 0)
        {
            _waitTimer -= UpdateInterval; // ServiceNode의 UpdateInterval 활용
            brain.blackboard.SetValue(Pos_Key, runner.transform.position);
            return;
        }

        // 2. 방향 전환 타이머
        _timer -= UpdateInterval;
        if (_timer <= 0)
        {
            FlipDirection();
        }

        // 3. 좌표 계산
        Vector3 targetPos = CalculatePressurePosition();

        // 4. 유효성 검사 (A* Node & Raycast)
        if (IsValidPosition(targetPos))
        {
            brain.blackboard.SetValue(Pos_Key, targetPos);
            brain.blackboard.SetValue(Dir_Key, _currentDir);
        }
        else
        {
            FlipDirection();
            _waitTimer = BlockedWaitTime;
            brain.blackboard.SetValue(Pos_Key, runner.transform.position);
        }
    }

    private void FlipDirection()
    {
        _currentDir *= -1;
        _timer = Change_Dir_Time + Random.Range(-0.5f, 0.5f);
    }

    private Vector3 CalculatePressurePosition()
    {
        Transform target = runner.player.transform;
        Vector3 myPos = runner.transform.position;

        Vector3 dirToMe = (myPos - target.position).normalized;
        Vector3 rightDir = Vector3.Cross(Vector3.up, (target.position - myPos).normalized);
        Vector3 moveDir = (rightDir * _currentDir);

        float currentDist = Vector3.Distance(myPos, target.position);
        if (currentDist < Distance - 0.5f) moveDir += dirToMe;
        else if (currentDist > Distance + 0.5f) moveDir -= dirToMe;

        return myPos + (moveDir.normalized * 2.0f);
    }

    private bool IsValidPosition(Vector3 pos)
    {
        // A* 그래프 상의 가장 가까운 노드 찾기
        NNInfo info = AstarPath.active.GetNearest(pos, NNConstraint.Default);

        if (info.node == null || !info.node.Walkable) return false;
        if (Vector3.Distance(pos, info.position) > 1.0f) return false;

        // 벽 체크 (Raycast)
        Vector3 start = runner.transform.position + Vector3.up * 0.5f;
        Vector3 dir = (pos - runner.transform.position).normalized;
        float dist = Vector3.Distance(runner.transform.position, pos);
        
        // 레이어 마스크는 프로젝트 설정에 맞게 수정하세요 ("Wall", "Default" 등)
        int layerMask = LayerMask.GetMask("Wall", "Default"); 
        
        if (Physics.Raycast(start, dir, out RaycastHit hit, dist, layerMask))
        {
            if (hit.transform != runner.transform) return false; 
        }

        return true;
    }
    
    public override Node Clone()
    {
        return Instantiate(this);
    }
}