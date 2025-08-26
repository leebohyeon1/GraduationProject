using System.Collections;
using BH_Lib.DI;
using UnityEngine;

/// <summary>
/// 플레이어의 이동을 담당하는 클래스
/// </summary>
public class PlayerMovement : MonoBehaviour, IPlayerMovement
{
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _transform;
    private Camera _mainCamera;

    [Header("Physics")]
    [SerializeField] private LayerMask _groundLayerMask = 1 << 3;

    private Vector3 _velocity;
    private bool _isGrounded;

    // 회피 쿨다운
    private float _lastDodgeTime = -999f;
    private float _dodgeCooldown => _context.Stats.DodgeCooldown;

    private PlayerContext _context;

    public void Tick()
    {
        CheckGrounded();
        ApplyGravity();
    }

    public void Initialize(PlayerContext context)
    {
        _context = context;

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

    public void Dodge(Vector3 direction, bool hasInput)
    {
        if (hasInput)
        {
            Move(direction, DodgeSpeed);
        }
        else
        {
            // 캐릭터 전진 이동
            Vector3 moveVector = transform.forward;
            Vector3 movement = moveVector * DodgeSpeed * Time.deltaTime;
            movement.y = _velocity.y * Time.deltaTime;
            _characterController.Move(movement);
        }

        _lastDodgeTime = Time.time;

    }

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

        _transform.rotation = Quaternion.LookRotation(cameraForward * direction.z + cameraRight * direction.x);
    }

    public bool CanDodge()
    {
        if (Time.time - _lastDodgeTime >= _dodgeCooldown)
        {

            return true;
        }

        return false;
    }

    private void CheckGrounded()
    {
        // CharacterController의 아래쪽 경계에서 체크
        Vector3 rayOrigin = _transform.position - new Vector3(0, _characterController.height / 2f, 0);
        _isGrounded = Physics.Raycast(rayOrigin, Vector3.down, _context.Stats.GroundCheckDistance, _groundLayerMask);
    }

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

    public bool IsGrounded => _isGrounded;
    public Vector3 Velocity => _velocity;
    public float MoveSpeed => _context.Stats.MoveSpeed;
    public float DodgeSpeed => _context.Stats.DodgeSpeed;

}

