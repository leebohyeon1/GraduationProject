using BH_Lib.Log;
using System;
using UnityEngine;

/// <summary>
/// 플레이어의 이동, 회전, 중력 등 물리적인 움직임을 담당하는 컴포넌트입니다.
/// </summary>
public class PlayerMovement : MonoBehaviour, IDisposable
{
    #region Private Fields
    private CharacterController _characterController; // 캐릭터 컨트롤러
    private Camera _mainCamera; // 메인 카메라
    private PlayerEvents _events;

    private Vector3 _velocity; // 현재 속도 (중력 포함)
    private bool _isGrounded; // 지면 접촉 여부
    private float _lastDodgeTime = -999f; // 마지막 회피 시간
    private Quaternion _targetRotation; // 목표 회전값
    private bool _hasTargetRotation; // 목표 회전값 존재 여부
    #endregion

    #region Properties
    public float LastDodgeTime => _lastDodgeTime;
    public Quaternion TargetRotation => _targetRotation;
    public bool HasTargetRotation => _hasTargetRotation;
    #endregion

    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize(CharacterController characterController, PlayerEvents events)
    {
        _characterController = characterController;
        _mainCamera = Camera.main;

        _events = events;
    }

    public void Dispose()
    {
    }

    /// <summary>
    /// 지면 접촉 여부를 확인합니다.
    /// </summary>
    public void CheckGrounded(float groundCheckDistance, LayerMask groundLayerMask)
    {
        Vector3 rayOrigin = transform.position - new Vector3(0, _characterController.height / 2f, 0);
        _isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundLayerMask);
    }

    /// <summary>
    /// 중력을 적용합니다.
    /// </summary>
    public void ApplyGravity(float gravityScale)
    {
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // 지면에 붙어있도록 약간의 하향력 유지
        }
        else
        {
            _velocity.y += gravityScale * Time.fixedDeltaTime;
        }

        // 최대 낙하 속도 제한
        if (_velocity.y < -30f) _velocity.y = -30f;
    }


    #region Move
    /// <summary>
    /// 입력 방향으로 이동합니다.
    /// </summary>
    public void Move(Vector3 direction, float moveSpeed, float rotateSpeed)
    {
        if (_characterController == null || _mainCamera == null) return;

        // 카메라 기준 방향 벡터 계산
        Vector3 cameraForward = Vector3.Scale(_mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 moveVector = (cameraForward * direction.z + _mainCamera.transform.right * direction.x).normalized;

        // 이동 및 중력 적용
        Vector3 movement = moveVector * moveSpeed * Time.fixedDeltaTime;
        movement.y = _velocity.y * Time.fixedDeltaTime;
        _characterController.Move(movement);

        // 이동 방향으로 회전
        if (moveVector.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveVector);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 외부에서 계산된 변위만큼 캐릭터를 강제로 이동시킵니다. 중력이 함께 적용됩니다.
    /// </summary>
    /// <param name="displacement">프레임당 이동할 변위</param>
    public void ForceMove(Vector3 displacement)
    {
        displacement.y = _velocity.y * Time.fixedDeltaTime;
        _characterController.Move(displacement);
    }
    #endregion

    #region Dodge
    /// <summary>
    /// 회피를 실행합니다.
    /// </summary>
    public void Dodge(Vector3 direction, float dodgeSpeed, float rotateSpeed)
    {
        if (direction.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.Scale(_mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized * direction.z + _mainCamera.transform.right * direction.x);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
        }

        Vector3 movement = transform.forward * dodgeSpeed * Time.fixedDeltaTime;
        movement.y = _velocity.y * Time.fixedDeltaTime;
        _characterController.Move(movement);

        _lastDodgeTime = Time.time;
    }
    #endregion

    #region Rotate
    /// <summary>
    /// 입력 장치에 따라 목표 방향으로 회전합니다.
    /// </summary>
    public void RotateToDirection(InputDeviceType deviceType, Vector2 moveInput, Vector2 mousePosition)
    {
        SetRotation(GetTargetRotation(deviceType, moveInput, mousePosition));
    }

    /// <summary>
    /// 지정된 방향으로 즉시 회전합니다.
    /// </summary>
    public void RotateToDirection(Vector3 direction)
    {
        if (transform == null) return;
        Vector3 cameraForward = Vector3.Scale(_mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        SetRotation(Quaternion.LookRotation(cameraForward * direction.z + _mainCamera.transform.right * direction.x));
    }

    /// <summary>
    /// 설정된 목표 회전값으로 회전합니다.
    /// </summary>
    public void RotateToTargetRotation()
    {
        SetRotation(TargetRotation);
        ClearTargetRotation();
    }

    /// <summary>
    /// 입력에 따른 목표 회전값을 계산합니다.
    /// </summary>
    public Quaternion GetTargetRotation(InputDeviceType deviceType, Vector2 moveInput, Vector2 mousePosition)
    {
        if (deviceType == InputDeviceType.KeyboardMouse)
        {
            return Quaternion.LookRotation(GetTargetDirection(deviceType, moveInput, mousePosition), Vector3.up);
        }
        else // Gamepad
        {
            if (moveInput.sqrMagnitude < 0.1f || _mainCamera == null)
            {
                return transform.rotation;
            }

            Vector3 lookDirection = GetTargetDirection(deviceType, moveInput, mousePosition);
            if (lookDirection.sqrMagnitude > 0.1f)
            {
                return Quaternion.LookRotation(lookDirection, Vector3.up);
            }
        }

        return transform.rotation;
    }

    /// <summary>
    /// 회전값을 설정합니다.
    /// </summary>
    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    /// <summary>
    /// 목표 회전값을 설정합니다.
    /// </summary>
    public void SetTargetRotation(Quaternion targetRotation)
    {
        _targetRotation = targetRotation;
        _hasTargetRotation = true;
    }

    /// <summary>
    /// 목표 회전값을 초기화합니다.
    /// </summary>
    public void ClearTargetRotation()
    {
        _targetRotation = Quaternion.Euler(Vector3.zero);
        _hasTargetRotation = false;
    }

    public Vector3 GetTargetDirection(InputDeviceType deviceType, Vector2 moveInput, Vector2 mousePosition)
    {
        if (deviceType == InputDeviceType.KeyboardMouse)
        {
            float distance = Vector3.Distance(transform.position, _mainCamera.transform.position);
            Vector3 point = _mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distance));

            Vector3 direction = point - transform.position;
            direction.y = 0;
            direction.Normalize();

            return direction;
        }
        else
        {
            Vector3 lookDirection = (Vector3.Scale(_mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized * moveInput.y + Camera.main.transform.right * moveInput.x).normalized;
            return lookDirection;
        }
    }
    #endregion

    #region Event
    #endregion
}