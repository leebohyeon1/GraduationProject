using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "Action_BackStep", menuName = "BehaviorTree/Action/BackStep")]
public class Action_BackStep : Node
{
    [Header("Settings")]
    [Tooltip("백스텝 속도 (빠르게 설정)")]
    public float backStepSpeed = 15f;
    
    [Tooltip("백스텝 지속 시간 (짧게 설정)")]
    public float duration = 0.4f;
    
    [Tooltip("실행할 애니메이션 트리거 이름")]
    public string animationTrigger = "Do_BackStep";
    
    [Tooltip("벽 감지 거리 (뒤쪽에 벽이 있으면 멈춤)")]
    public float wallCheckDist = 1.0f;

    // 내부 변수
    private float _startTime;
    private Vector3 _dashDirection;
    private AIPath _aiPath;
    private CharacterController _cc;
    private Vector3 _verticalVelocity; // 중력 처리용

    public override void OnEnter()
    {
        _aiPath = runner.GetComponent<AIPath>();
        _cc = runner.GetComponent<CharacterController>();
        _startTime = Time.time;
        _verticalVelocity = Vector3.zero;

        // 1. 네비게이션 잠시 비활성화 (직접 이동 제어 위함)
        if (_aiPath != null) _aiPath.enabled = false;

        // 2. 방향 설정 (현재 바라보는 방향의 반대)
        _dashDirection = -runner.transform.forward;

        // 3. 상태 및 애니메이션 설정
        runner.SetState(Enemy.EnemyState.Rush); // 혹은 Evasion 등 적절한 상태
        runner.AnimationEvent(animationTrigger);
        
        // (선택) 시작 시 순간적인 힘을 원한다면 여기서 처리 가능하지만, 
        // CharacterController.Move를 매 프레임 호출하는 방식이 더 제어가 쉽습니다.
    }

    protected override NodeState OnUpdate()
    {
        // 1. 시간 체크
        if (Time.time - _startTime > duration)
        {
            return NodeState.SUCCESS;
        }

        // 2. 뒤쪽 벽 체크 (막히면 즉시 종료)
        // 캐릭터의 허리 높이 정도에서 뒤로 레이를 쏩니다.
        Vector3 rayOrigin = runner.transform.position + Vector3.up * 0.8f;
        if (Physics.Raycast(rayOrigin, _dashDirection, wallCheckDist, LayerMask.GetMask("Wall", "Default")))
        {
            return NodeState.SUCCESS;
        }

        // 3. 이동 처리
        if (_cc != null)
        {
            // 중력 적용
            if (_cc.isGrounded && _verticalVelocity.y < 0)
            {
                _verticalVelocity.y = -2f;
            }
            _verticalVelocity.y += Physics.gravity.y * Time.deltaTime;

            // 최종 이동 벡터 계산
            Vector3 move = (_dashDirection * backStepSpeed * Time.deltaTime) + (_verticalVelocity * Time.deltaTime);
            _cc.Move(move);
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        // 1. 네비게이션 재활성화 및 위치 동기화
        if (_aiPath != null)
        {
            // 현재 물리 위치로 AIPath 위치를 강제 이동시켜 텔레포트 현상 방지
            _aiPath.Teleport(runner.transform.position, false);
            _aiPath.enabled = true;
        }

        // 2. 상태 초기화
        runner.Movement.StopMovement(); // 잔여 속도 제거
        runner.SetState(Enemy.EnemyState.Idle);
    }

    public override Node Clone()
    {
        Action_BackStep node = Instantiate(this);
        node.backStepSpeed = this.backStepSpeed;
        node.duration = this.duration;
        node.animationTrigger = this.animationTrigger;
        node.wallCheckDist = this.wallCheckDist;
        return node;
    }
}