using BH_Lib.DI;
using UnityEngine;

/// <summary>
/// 플레이어의 입력을 받아 캐릭터를 제어하는 컨트롤러 (InputSystem 사용)
/// InputReader로부터 입력을 받아 PlayerMovement와 Player에게 전달
/// </summary>
public class PlayerController : DIMonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader _inputReader;
    
    [Header("Player Components")]
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private Player _player;
    
    private Vector2 _moveInput;
    private bool _attackInput;

    protected override void Awake()
    {
        // 컴포넌트 자동 할당
        if (_playerMovement == null)
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        if (_player == null)
        {
            _player = GetComponent<Player>();
        }
    }
    
    protected override void OnEnable()
    {
        if (_inputReader != null)
        {
            _inputReader.MoveEvent += OnMove;
            _inputReader.AttackEvent += OnAttack;
            _inputReader.AttackCancelledEvent += OnAttackCancelled;
        }
    }
    
    private void OnDisable()
    {
        if (_inputReader != null)
        {
            _inputReader.MoveEvent -= OnMove;
            _inputReader.AttackEvent -= OnAttack;
            _inputReader.AttackCancelledEvent -= OnAttackCancelled;
        }
    }
    
    private void Update()
    {
        HandleMovement();
    }
    
    private void OnMove(Vector2 moveInput)
    {
        _moveInput = moveInput;
    }
    
    private void OnAttack()
    {
        _attackInput = true;
        if (_player != null)
        {
            _player.TryAttack();
        }
    }
    
    private void OnAttackCancelled()
    {
        _attackInput = false;
    }
    
    private void HandleMovement()
    {
        if (_playerMovement != null && _moveInput != Vector2.zero)
        {
            // 2D 입력을 3D 월드 좌표로 변환 (Y축은 0으로 고정)
            Vector3 moveDirection = new Vector3(_moveInput.x, 0, _moveInput.y);
            _playerMovement.Move(moveDirection);
        }
    }
    
    // 디버깅용 프로퍼티
    public Vector2 MoveInput => _moveInput;
    public bool AttackInput => _attackInput;
}

