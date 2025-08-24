using BH_Lib.DI;
using BH_Lib.FSM;
using UnityEngine;

/// <summary>
/// 플레이어의 입력을 받아 캐릭터를 제어하는 컨트롤러입니다. (Unity의 새로운 Input System 사용)
/// InputReader로부터 입력을 받아 각종 이벤트를 발생시키고, 현재 입력 상태를 저장합니다.
/// </summary>
public class PlayerController : PlayerComponent
{
    [Header("Input")]
    [Tooltip("입력 이벤트를 제공하는 InputReader ScriptableObject입니다.")]
    [SerializeField] private InputReader _inputReader;
    
    /// <summary>
    /// 현재 이동 입력 값입니다. (x, y)
    /// </summary>
    private Vector2 _moveInput;
    
    /// <summary>
    /// 현재 공격 입력 상태입니다.
    /// </summary>
    private bool _attackInput;
    
    /// <summary>
    /// 현재 회피 입력 상태입니다.
    /// </summary>
    private bool _dodgeInput;
    
    /// <summary>
    /// 컴포넌트가 활성화될 때 호출됩니다.
    /// InputReader의 이벤트에 리스너를 등록합니다.
    /// </summary>
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
    
    /// <summary>
    /// 컴포넌트가 비활성화될 때 호출됩니다.
    /// 등록했던 이벤트 리스너를 해제합니다.
    /// </summary>
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
    
    /// <summary>
    /// 이동 이벤트가 발생했을 때 호출되는 콜백 함수입니다.
    /// </summary>
    /// <param name="moveInput">이동 입력 벡터입니다.</param>
    private void OnMove(Vector2 moveInput)
    {
        _moveInput = moveInput;
    }
    
    /// <summary>
    /// 공격 이벤트가 발생했을 때 호출되는 콜백 함수입니다.
    /// </summary>
    private void OnAttack()
    {
        _attackInput = true;
    }
    
    /// <summary>
    /// 공격 취소 이벤트가 발생했을 때 호출되는 콜백 함수입니다.
    /// </summary>
    private void OnAttackCancelled()
    {
        _attackInput = false;
    }
    
    /// <summary>
    /// 회피 이벤트가 발생했을 때 호출되는 콜백 함수입니다.
    /// </summary>
    private void OnDodge()
    {
        _dodgeInput = true;
    }
    
    /// <summary>
    /// 매 프레임의 마지막에 호출되어, 한 번만 처리해야 하는 입력 상태를 리셋합니다.
    /// </summary>
    public void LateTick()
    {
        _attackInput = false;
        _dodgeInput = false;
    }
    
    // 다른 스크립트(주로 상태 클래스)에서 현재 입력 값을 참조하기 위한 프로퍼티들입니다.
    public Vector2 MoveInput => _moveInput;
    public bool AttackInput => _attackInput;
    public bool DodgeInput => _dodgeInput;

}