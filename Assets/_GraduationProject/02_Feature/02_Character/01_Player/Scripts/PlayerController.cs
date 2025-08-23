using BH_Lib.DI;
using BH_Lib.FSM;
using UnityEngine;

/// <summary>
/// 플레이어의 입력을 받아 캐릭터를 제어하는 컨트롤러 (InputSystem 사용)
/// InputReader로부터 입력을 받아 PlayerMovement와 Player에게 전달
/// </summary>
public class PlayerController : PlayerComponent
{
    [Header("Input")]
    [SerializeField] private InputReader _inputReader;
    
    private Vector2 _moveInput;
    private bool _attackInput;
    private bool _dodgeInput;
    
    private void OnEnable()
    {   
        if (_inputReader != null)
        {
            _inputReader.MoveEvent += OnMove;
            _inputReader.AttackEvent += OnAttack;
            _inputReader.AttackCancelledEvent += OnAttackCancelled;
            _inputReader.DodgeEvent += OnDodge;
        }
    }
    
    private void OnDisable()
    {
        if (_inputReader != null)
        {
            _inputReader.MoveEvent -= OnMove;
            _inputReader.AttackEvent -= OnAttack;
            _inputReader.AttackCancelledEvent -= OnAttackCancelled;
            _inputReader.DodgeEvent -= OnDodge;
        }
    }
    
    private void OnMove(Vector2 moveInput)
    {
        _moveInput = moveInput;
    }
    
    private void OnAttack()
    {
        _attackInput = true;
    }
    
    private void OnAttackCancelled()
    {
        _attackInput = false;
    }
    
    private void OnDodge()
    {
        _dodgeInput = true;
    }
    
    // 입력 상태 리셋 (한 프레임 후)
    public void LateTick()
    {
        _attackInput = false;
        _dodgeInput = false;
    }
    
    // 퍼블릭 프로퍼티 (상태에서 접근용)
    public Vector2 MoveInput => _moveInput;
    public bool AttackInput => _attackInput;
    public bool DodgeInput => _dodgeInput;

}

