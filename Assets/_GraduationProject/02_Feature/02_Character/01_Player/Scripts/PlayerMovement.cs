using BH_Lib.DI;
using UnityEngine;

/// <summary>
/// 플레이어의 이동을 담당하는 클래스
/// IMovable 인터페이스를 구현하여 이동 기능을 제공
/// </summary>
public class PlayerMovement : DIMonoBehaviour, IMovable
{
    [Header("Movement Settings")]
    [SerializeField] private PlayerStats _playerStats;
    
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _transform;
    private Camera _mainCamera;
    
    [Header("Physics")]
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask _groundLayerMask = 1 << 3;
    
    private Vector3 _velocity;
    private bool _isGrounded;
    
    public float MoveSpeed => _playerStats != null ? _playerStats.moveSpeed : 5f;

    protected override void Awake()
    {
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
    
    private void Update()
    {
        CheckGrounded();
        ApplyGravity();
    }
    
    public void Move(Vector3 direction)
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

        Vector3 movement = moveVector * MoveSpeed * Time.deltaTime;
        
        // 중력 적용
        movement.y = _velocity.y * Time.deltaTime;
        
        // 실제 이동
        _characterController.Move(movement);
        
        // 회전 (이동 방향으로)
        if (moveVector.magnitude > 0.1f)
        {
            _transform.rotation = Quaternion.LookRotation(moveVector);
        }
    }
    
    private void CheckGrounded()
    {
        _isGrounded = Physics.Raycast(_transform.position, Vector3.down, 
            _characterController.height / 2f + _groundCheckDistance, _groundLayerMask);
    }
    
    private void ApplyGravity()
    {
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // 약간의 하향력 유지
        }
        else
        {
            _velocity.y += _gravity * Time.deltaTime;
        }
    }
    
    public bool IsGrounded => _isGrounded;
    public Vector3 Velocity => _velocity;
}
