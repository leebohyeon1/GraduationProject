using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "RushUntilWall", menuName = "BehaviorTree/Action/RushUntilWall")]
public class RushUntilWall : Node
{
    [Header("돌진 설정")]
    [Tooltip("돌진 속도입니다.")]
    public float rushSpeed = 25f;
    [Tooltip("벽을 감지할 정면 레이캐스트 거리입니다.")]
    public float wallDetectionDistance = 1.5f;
    [Tooltip("벽으로 인식할 레이어를 설정하세요.")]
    public LayerMask wallLayer;

    [Header("안전 장치")]
    [Tooltip("최대 돌진 시간입니다.")]
    public float timeout = 4.0f;
    [Tooltip("돌진 시작 후 충돌 검사를 시작하기까지의 유예 시간입니다.")]
    private float _collisionGracePeriod = 0.1f;


    private float _startTime;
    private bool _isRushing;
    private Vector3 _fixedRushDirection;
    private AIPath _aiPath;
    private CharacterController _characterController;
    
    private Vector3 _verticalVelocity;

    public override void OnEnter()
    {
        _isRushing = false;
        
        _aiPath = runner.GetComponent<AIPath>();
        _characterController = runner.GetComponent<CharacterController>();

        if (_characterController == null)
        {
            Debug.LogError("CharacterController component not found on runner!", runner);
            return;
        }

        if (_aiPath != null)
        {
            _aiPath.enabled = false;
        }
        
        // 중력 변수 초기화
        _verticalVelocity.y = 0;

        runner.SetState(EnemyStateController.EnemyState.Rush);
        runner.AnimationEvent("Do_Rush");

        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;
        runner.transform.rotation = Quaternion.LookRotation(directionToPlayer);
        _fixedRushDirection = runner.transform.forward;
    }

    protected override NodeState OnUpdate()
    {
        if (!_isRushing && Handler.IsHitWindowOpen)
        {
            _isRushing = true;
            _startTime = Time.time;
            runner.GetComponent<Animator>().SetBool("Rush_Running", true);
            Handler.CloseHitWindow();
        }

        if (!_isRushing)
        {
            return NodeState.RUNNING;
        }

        // --- 실제 돌진 로직 ---

        if (Time.time > _startTime + _collisionGracePeriod)
        {
            RaycastHit hit;
            Vector3 origin = runner.transform.position +runner.transform.forward + Vector3.up * 0.5f;
            if (Physics.Raycast(origin, _fixedRushDirection, out hit, wallDetectionDistance, wallLayer))
            {
                _characterController.Move(Vector3.zero);
                return NodeState.SUCCESS;
            }
        }


        if (Time.time - _startTime > timeout)
        {
            // // Debug.LogWarning("RushUntilWall: Timeout reached, stopping rush.");
            return NodeState.SUCCESS;
        }

        
        // 1. 캐릭터가 땅에 붙어있는지 확인합니다.
        if (_characterController.isGrounded && _verticalVelocity.y < 0)
        {
            // 땅에 있다면 수직 속도를 리셋하여 계속 아래로 파고들지 않도록 합니다.
            _verticalVelocity.y = -2f; 
        }
        
        Vector3 horizontalMovement = _fixedRushDirection * rushSpeed;
        _verticalVelocity.y += Physics.gravity.y * Time.deltaTime;
        Vector3 finalMovement = (horizontalMovement + _verticalVelocity) * Time.deltaTime;
        _characterController.Move(finalMovement);
        Debug.DrawRay(runner.transform.position + Vector3.up * 0.5f, _fixedRushDirection * wallDetectionDistance, Color.red);

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        runner.GetComponent<Animator>().SetBool("Rush_Running", false);
        runner.SetState(EnemyStateController.EnemyState.Idle);

        if (_aiPath != null)
        {
            _aiPath.enabled = true;
            _aiPath.Teleport(runner.transform.position, true);
        }
        _verticalVelocity = Vector3.zero;
        _characterController.Move(_verticalVelocity);

    }
    
    public override Node Clone()
    {
        RushUntilWall node = Instantiate(this);
        node.rushSpeed = this.rushSpeed;
        node.wallDetectionDistance = this.wallDetectionDistance;
        node.wallLayer = this.wallLayer;
        node.timeout = this.timeout;
        return node;
    }
}