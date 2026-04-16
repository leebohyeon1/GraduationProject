using UnityEngine;
using BehaviorTree;
using Pathfinding;

public class Service_PressureMove : ServiceNode
{
    [Header("Distance Settings")]
    public float MinDistance = 5.0f;        // 최소 유지 거리
    public float MaxDistance = 6.0f;        // 최대 유지 거리

    [Header("Timing Settings")]
    public float Change_Dir_MinTime = 2.0f; // 방향 전환 최소 시간
    public float Change_Dir_MaxTime = 3.0f; // 방향 전환 최대 시간
    public float BlockedWaitTime = 0.5f;    // 벽 막힘 대기 시간

    [Header("Angle Settings (Degree)")]
    [Tooltip("이 각도 범위를 유지하려고 노력합니다. 벗어나면 돌아옵니다.")]
    [Range(0f, 180f)]
    public float FrontAngle = 45.0f;       // 정면 사수 범위 (넓게 잡는 것이 좋습니다)

    [Header("Blackboard Keys")]
    public string Pos_Key = "PressurePos";
    public string Dir_Key = "StrafeDir";

    private float _timer;
    private int _currentDir = 1; // 1: 오른쪽, -1: 왼쪽
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

        // 1. 대기 타이머 (벽에 막혔을 때 잠깐 멈춤)
        if (_waitTimer > 0)
        {
            _waitTimer -= UpdateInterval;
            brain.blackboard.SetValue(Pos_Key, runner.transform.position);
            return;
        }

        Transform targetTF = runner.player.transform;
        Vector3 myPos = runner.transform.position;

        // 2. 각도(Degree) 계산
        Vector3 dirToMe = (myPos - targetTF.position).normalized;
        // 타겟의 정면 벡터와 나에게 오는 벡터 사이의 각도 (-180 ~ 180)
        // 양수(+)면 타겟 기준 오른쪽, 음수(-)면 타겟 기준 왼쪽
        float angle = Vector3.SignedAngle(targetTF.forward, dirToMe, Vector3.up);

        // 현재 내가 정면 범위 안에 있는가?
        bool isInFront = Mathf.Abs(angle) < FrontAngle;

        if (isInFront)
        {
            // [상황 A: 정면 유지 중] -> 여기서 "지그재그" 무빙
            // 플레이어 앞에서 알짱거리며 위협하거나 간을 봅니다.
            _timer -= UpdateInterval;
            if (_timer <= 0)
            {
                FlipDirection();
            }
        }
        else
        {
            // [상황 B: 정면 이탈] -> "강제 복귀"
            // 지그재그를 하지 않고, 무조건 정면(0도) 방향으로 이동합니다.
            
            // angle이 양수(오른쪽)라면 -> 왼쪽(-1)으로 이동해야 정면
            // angle이 음수(왼쪽)라면 -> 오른쪽(1)으로 이동해야 정면
            _currentDir = angle > 0 ? -1 : 1;

            // 정면으로 돌아가는 동안에는 랜덤 타이머를 리셋하여 방향이 튀지 않게 함
            ResetDirectionTimer();
        }

        // 3. 최종 좌표 계산 (원형 이동 + 거리 조절)
        Vector3 targetPos = CalculatePressurePosition(targetTF, myPos);

        // 4. 유효성 검사 (갈 수 있는 곳인지)
        if (IsValidPosition(targetPos))
        {
            brain.blackboard.SetValue(Pos_Key, targetPos);
            brain.blackboard.SetValue(Dir_Key, _currentDir);
        }
        else
        {
            // 가려는 곳이 벽이다 -> 반대 방향으로 전환하고 잠시 대기
            FlipDirection();
            _waitTimer = BlockedWaitTime;
            brain.blackboard.SetValue(Pos_Key, myPos);
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
    
    // 플레이어 주위를 도는 방향 계산
    Vector3 rightDir = Vector3.Cross(Vector3.up, dirFromTarget); 
    Vector3 strafeDir = rightDir * _currentDir;

    // 원하는 유지 거리 결정 (Min과 Max 사이)
    float desiredDist = (MinDistance + MaxDistance) * 0.5f;

    // [핵심] 플레이어 위치에서부터의 오프셋으로 계산
    // 플레이어로부터 dirFromTarget 방향으로 desiredDist만큼 떨어진 곳에 횡이동 방향을 더함
    Vector3 finalPos = target.position + (dirFromTarget * desiredDist) + (strafeDir * 1.5f);
    
    return finalPos; 
}

    private bool IsValidPosition(Vector3 pos)
    {
        NNInfo info = AstarPath.active.GetNearest(pos, NNConstraint.Default);
        if (info.node == null || !info.node.Walkable) return false;
        // A* 노드와 너무 멀리 떨어져 있으면(절벽 등) 갈 수 없음
        if (Vector3.Distance(pos, info.position) > 1.0f) return false;

        // 벽 체크 (Raycast)
        Vector3 start = runner.transform.position + Vector3.up * 0.5f;
        Vector3 dir = (pos - runner.transform.position).normalized;
        float dist = Vector3.Distance(runner.transform.position, pos);
        
        // 장애물 레이어 (Wall 등) 설정 필요
        int layerMask = LayerMask.GetMask("Wall", "Default"); 
        
        if (Physics.Raycast(start, dir, out RaycastHit hit, dist, layerMask))
        {
            // 내 자신이 아니면 벽으로 간주
            if (hit.transform != runner.transform) return false; 
        }

        return true;
    }
    
    public override Node Clone()
    {
        // 1. 현재 이 노드(this)를 기반으로 새로운 인스턴스 생성
        Service_PressureMove newNode = Instantiate(this);
        
        // 2. 부모(Node/ServiceNode)로부터 상속받은 필수 데이터 명시적 복사
        newNode.UpdateInterval = this.UpdateInterval;
        
        // 3. 자식 클래스 전용 데이터 복사
        Debug.Log($"Cloning Service_PressureMove: MinDistance={this.MinDistance}, MaxDistance={this.MaxDistance}");
        newNode.MinDistance = this.MinDistance;
        newNode.MaxDistance = this.MaxDistance;
        newNode.Change_Dir_MinTime = this.Change_Dir_MinTime;
        newNode.Change_Dir_MaxTime = this.Change_Dir_MaxTime;
        newNode.BlockedWaitTime = this.BlockedWaitTime;
        newNode.FrontAngle = this.FrontAngle;
        newNode.Pos_Key = this.Pos_Key;
        newNode.Dir_Key = this.Dir_Key;

        // 4. 중요: 런타임에 계산되는 내부 상태 변수는 초기화
        newNode._timer = 0f;
        newNode._currentDir = 1;
        newNode._waitTimer = 0f;
        newNode.lastExecutionTime = 0f;

        return newNode;
    }
}