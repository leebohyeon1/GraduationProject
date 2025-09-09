using System.Collections;
using BH_Lib.DI;
using UnityEngine;

/// <summary>
/// 플레이어의 이동을 담당하는 클래스
/// CharacterController를 사용하여 카메라 기준 이동, 회피, 중력 처리 등을 수행합니다.
/// </summary>
public class PlayerMovement : MonoBehaviour, IPlayerMovement
{
    [Header("Components")]
    [Tooltip("플레이어 이동을 처리할 CharacterController")]
    [SerializeField] private CharacterController _characterController;
    [Tooltip("플레이어 Transform (캐싱용)")]
    [SerializeField] private Transform _transform;
    /// <summary>메인 카메라 참조 (카메라 기준 이동용)</summary>
    private Camera _mainCamera;

    [Header("Physics")]
    [Tooltip("지면 체크용 레이어 마스크")]
    [SerializeField] private LayerMask _groundLayerMask = 1 << 3;

    /// <summary>현재 속도 벡터 (중력 포함)</summary>
    private Vector3 _velocity;
    /// <summary>지면 접촉 상태</summary>
    private bool _isGrounded;

    /// <summary>마지막 회피 시간 (쿨다운 계산용)</summary>
    private float _lastDodgeTime = -999f;
    /// <summary>회피 쿨다운 시간</summary>
    private float _dodgeCooldown => _context.Stats.DodgeCooldown;

    /// <summary>플레이어 컨텍스트 참조</summary>
    private PlayerContext _context;

    /// <summary>
    /// 물리 업데이트 (매 프레임 호출)
    /// 지면 접촉 체크 및 중력 적용
    /// </summary>
    public void Tick()
    {
        CheckGrounded();
        ApplyGravity();
    }

    /// <summary>
    /// 플레이어 이동 시스템 초기화
    /// </summary>
    /// <param name="context">플레이어 컨텍스트</param>
    public void Initialize(PlayerContext context)
    {
        _context = context;

        // 컴포넌트 참조 설정
        if (_characterController == null)
        {
            _characterController = GetComponent<CharacterController>();
        }

        if (_transform == null)
        {
            _transform = transform;
        }

        _mainCamera = Camera.main;
    }

    /// <summary>
    /// 카메라 기준 이동 처리
    /// 입력 방향을 카메라 기준으로 변환하여 이동 및 회전 수행
    /// </summary>
    /// <param name="direction">이동 방향 (로컬 입력 좌표)</param>
    /// <param name="speed">이동 속도</param>
    /// <param name="speedMultiplier">속도 배율 (기본값: 1f)</param>
    public void Move(Vector3 direction, float speed, float speedMultiplier = 1f)
    {
        if (_characterController == null || _mainCamera == null)
        {
            return;
        }

        // 카메라 기준으로 방향 변환
        Vector3 cameraForward = _mainCamera.transform.forward;
        Vector3 cameraRight = _mainCamera.transform.right;

        // Y축 제거 (수평면 이동만)
        cameraForward.y = 0;
        cameraRight.y = 0;

        // 정규화
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 입력에 따른 이동 방향 계산
        Vector3 moveVector = cameraForward * direction.z + cameraRight * direction.x;

        // 정규화 및 속도 적용
        if (moveVector.magnitude > 1f)
        {
            moveVector.Normalize();
        }

        Vector3 movement = moveVector * speed * speedMultiplier * Time.deltaTime;

        // 중력 적용
        movement.y = _velocity.y * Time.deltaTime;

        // 실제 이동
        _characterController.Move(movement);

        // 회전 (이동 방향으로)
        if (moveVector.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveVector);
            _transform.rotation = Quaternion.Slerp(
                _transform.rotation,
                targetRotation,
                _context.Stats.RotateSpeed * Time.fixedDeltaTime
            );
        }
    }

    /// <summary>
    /// 회피 이동 실행
    /// 입력 방향이 있으면 해당 방향으로, 없으면 전방으로 회피
    /// </summary>
    /// <param name="direction">회피 방향</param>
    /// <param name="hasInput">입력 방향 존재 여부</param>
    public void Dodge(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            // 입력 방향으로 회피 (카메라 기준 변환 포함)
            Move(direction, DodgeSpeed);
        }
        else
        {
            // 입력이 없으면 캐릭터 전진 방향으로 회피
            Vector3 moveVector = transform.forward;
            Vector3 movement = moveVector * DodgeSpeed * Time.deltaTime;
            movement.y = _velocity.y * Time.deltaTime;
            _characterController.Move(movement);
        }

        // 회피 시간 기록 (쿨다운 계산용)
        _lastDodgeTime = Time.time;
    }

    /// <summary>
    /// 지정된 방향으로 즉시 회전
    /// 회피 시작 시 방향 설정 등에 사용
    /// </summary>
    /// <param name="direction">회전할 방향</param>
    public void RotateImmediately(Vector3 direction)
    {
        if (_transform == null) return;

        // 카메라 기준으로 방향 변환
        Vector3 cameraForward = _mainCamera.transform.forward;
        Vector3 cameraRight = _mainCamera.transform.right;

        // Y축 제거 (수평면 이동만)
        cameraForward.y = 0;
        cameraRight.y = 0;

        // 정규화
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 즉시 회전 적용
        _transform.rotation = Quaternion.LookRotation(cameraForward * direction.z + cameraRight * direction.x);
    }

    /// <summary>
    /// 회피 가능 여부 체크 (쿨다운 확인)
    /// </summary>
    /// <returns>회피 가능하면 true</returns>
    public bool CanDodge()
    {
        if (Time.time - _lastDodgeTime >= _dodgeCooldown)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 지면 접촉 상태 체크
    /// CharacterController 하단에서 레이캐스트로 확인
    /// </summary>
    private void CheckGrounded()
    {
        // CharacterController의 아래쪽 경계에서 체크
        Vector3 rayOrigin = _transform.position - new Vector3(0, _characterController.height / 2f, 0);
        _isGrounded = Physics.Raycast(rayOrigin, Vector3.down, _context.Stats.GroundCheckDistance, _groundLayerMask);
    }

    /// <summary>
    /// 중력 적용
    /// 지면 접촉 시 미세한 하향력 유지, 공중에서는 중력 가속도 적용
    /// </summary>
    private void ApplyGravity()
    {
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // 약간의 하향력 유지하여 지면에 붙어있도록
        }
        else
        {
            _velocity.y += _context.Stats.Gravity * Time.deltaTime;
        }

        // 최대 낙하 속도 제한
        if (_velocity.y < -30f)
        {
            _velocity.y = -30f;
        }
    }

    /// <summary>
    /// 애니메이션 곡선을 사용한 전진 이동 코루틴
    /// 공격 시 전진 등에 사용됩니다.
    /// </summary>
    /// <param name="distance">이동 거리</param>
    /// <param name="duration">이동 지속 시간</param>
    /// <param name="curve">이동 애니메이션 곡선</param>
    /// <returns>코루틴 IEnumerator</returns>
    public IEnumerator CoMoveForwardWithCurve(float distance, float duration, AnimationCurve curve)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = _transform.position;
        Vector3 moveDirection = _transform.forward * distance;

        while (elapsedTime < duration)
        {
            float normalizedTime = elapsedTime / duration;
            float curveValue = curve.Evaluate(normalizedTime);

            Vector3 targetPosition = startPosition + moveDirection * curveValue;
            Vector3 movement = (targetPosition - _transform.position);

            _characterController.Move(movement);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>지면 접촉 여부</summary>
    public bool IsGrounded => _isGrounded;
    /// <summary>현재 속도 벡터</summary>
    public Vector3 Velocity => _velocity;
    /// <summary>기본 이동 속도</summary>
    public float MoveSpeed => _context.Stats.MoveSpeed;
    /// <summary>회피 이동 속도</summary>
    public float DodgeSpeed => _context.Stats.DodgeSpeed;

}

