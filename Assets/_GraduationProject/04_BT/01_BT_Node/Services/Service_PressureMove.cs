using UnityEngine;
using BehaviorTree;
using Pathfinding;

public class Service_PressureMove : ServiceNode
{
    [Header("Distance Settings")]
    public float MinDistance = 5.0f;
    public float MaxDistance = 6.0f;

    [Header("Timing Settings")]
    public float Change_Dir_MinTime = 2.0f;
    public float Change_Dir_MaxTime = 3.0f;
    public float BlockedWaitTime = 0.5f;

    [Header("Angle Settings (Degree)")]
    [Range(0f, 180f)]
    public float FrontAngle = 45.0f;

    [Header("Blackboard Keys")]
    public string Pos_Key = "PressurePos";
    public string Dir_Key = "StrafeDir";

    private float _timer;
    private int _currentDir = 1;
    private float _waitTimer = 0f;

    public override void OnEnter()
    {
        base.OnEnter();
        ResetDirectionTimer();
        _waitTimer = 0f;
    }

    protected override void OnServiceLogic()
    {
        if (runner.player == null) return;

        if (_waitTimer > 0)
        {
            _waitTimer -= UpdateInterval;
            brain.blackboard.SetValue(Pos_Key, runner.transform.position);
            return;
        }

        Transform targetTF = runner.player.transform;
        Vector3 myPos = runner.transform.position;
        Vector3 dirToMe = (myPos - targetTF.position).normalized;
        float angle = Vector3.SignedAngle(targetTF.forward, dirToMe, Vector3.up);

        bool isInFront = Mathf.Abs(angle) < FrontAngle;

        if (isInFront)
        {
            _timer -= UpdateInterval;
            if (_timer <= 0) FlipDirection();
        }
        else
        {
            _currentDir = angle > 0 ? -1 : 1;
            ResetDirectionTimer();
        }

        Vector3 targetPos = CalculatePressurePosition(targetTF, myPos);

        // [Debug] 목표 좌표 계산 로그
        float distToPlayer = Vector3.Distance(myPos, targetTF.position);

        if (IsValidPosition(targetPos))
        {
            brain.blackboard.SetValue(Pos_Key, targetPos);
            brain.blackboard.SetValue(Dir_Key, _currentDir);
        }
        else
        {
            // [Debug] 실패 로그
            if (distToPlayer > MaxDistance + 2f)
            {
                // 멀리 있다면 물리 체크 실패를 무시하고 일단 접근하도록 강제 설정 (임시 해결책)
                brain.blackboard.SetValue(Pos_Key, targetPos);
            }
            else
            {
                FlipDirection();
                _waitTimer = BlockedWaitTime;
                brain.blackboard.SetValue(Pos_Key, myPos);
            }
        }
    }

    private void FlipDirection()
    {
        _currentDir *= -1;
        ResetDirectionTimer();
    }

    private void ResetDirectionTimer()
    {
        _timer = Random.Range(Change_Dir_MinTime, Change_Dir_MaxTime);
    }

    private Vector3 CalculatePressurePosition(Transform target, Vector3 myPos)
    {
        Vector3 dirFromTarget = (myPos - target.position).normalized;
        Vector3 rightDir = Vector3.Cross(Vector3.up, dirFromTarget); 
        Vector3 strafeDir = rightDir * _currentDir;
        float desiredDist = (MinDistance + MaxDistance) * 0.5f;

        // 플레이어 근처의 점을 계산
        Vector3 finalPos = target.position + (dirFromTarget * desiredDist) + (strafeDir * 1.5f);
        return finalPos; 
    }

    private bool IsValidPosition(Vector3 pos)
    {
        NNInfo info = AstarPath.active.GetNearest(pos, NNConstraint.Default);
        if (info.node == null || !info.node.Walkable) return false;
        if (Vector3.Distance(pos, info.position) > 1.2f) return false;

        Vector3 start = runner.transform.position + Vector3.up * 0.8f; // 약간 더 높게 시작
        Vector3 dir = (pos - runner.transform.position).normalized;
        float dist = Vector3.Distance(runner.transform.position, pos);
        
        int layerMask = LayerMask.GetMask("Wall", "Default"); 
        
        if (Physics.Raycast(start, dir, out RaycastHit hit, dist, layerMask))
        {
            if (hit.transform != runner.transform && !hit.transform.CompareTag("Player")) 
            {
                // [Debug] 무엇에 부딪혔는지 출력
                // Debug.Log($"[Service_PressureMove:{runner.name}] Raycast blocked by {hit.transform.name} at distance {hit.distance}");
                return false; 
            }
        }

        return true;
    }

    public override Node Clone() => Instantiate(this);
}
