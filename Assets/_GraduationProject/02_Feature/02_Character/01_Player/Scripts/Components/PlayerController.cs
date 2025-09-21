using BH_Lib.DI;
using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 입력을 받아 캐릭터를 제어하는 컨트롤러입니다. (Unity의 새로운 Input System 사용)
/// InputReader로부터 입력을 받아 각종 이벤트를 발생시키고, 현재 입력 상태를 저장합니다.
/// </summary>
public class PlayerController : MonoBehaviour, IPlayerController
{
    [Header("Input")]
    [SerializeField] private InputReader _inputReader;
    private IInputDeviceDetector _inputDeviceDetector;

    /// <summary>
    /// 현재 이동 입력 값입니다. (x, y)
    /// </summary>
    private Vector2 _moveInput;
    /// <summary>
    /// 현재 공격 입력 상태입니다.
    /// </summary>
    private bool _attackInput;
    /// <summary>
    /// 현재 공격 홀드 입력 상태입니다.
    /// </summary>
    private bool _attackHeldInput;
    /// <summary>
    /// 현재 원거리 공격 입력 상태입니다.
    /// </summary>
    private bool _rangedAttackInput;
    /// <summary>
    /// 현재 회피 입력 상태입니다.
    /// </summary>
    private bool _dodgeInput;    
    /// <summary>
    /// 현재 방어 입력 상태입니다.
    /// </summary>
    private bool _defendInput;   
    /// <summary>
    /// 현재 조준/시선 입력 값입니다.
    /// </summary>
    private Vector2 _lookInput;
    private Vector2 _mousePosition;
    /// <summary>
    /// 스킬 입력 상태입니다.
    /// </summary>
    private bool _skillInput;


    #region Properties
    public Vector2 MoveInput => _moveInput;
    public bool AttackInput => _attackInput;
    public bool AttackHeldInput => _attackHeldInput;
    public bool RangedAttackInput => _rangedAttackInput;
    public bool DodgeInput => _dodgeInput;
    public bool DefendInput => _defendInput;
    public bool SkillInput => _skillInput;
    public Vector2 LookInput => _lookInput;
    public Vector2 MousePosition => _mousePosition;
    #endregion

    public void Initialize(IInputDeviceDetector inputDeviceDetector)
    {
        _inputDeviceDetector = inputDeviceDetector;

        // InputDeviceDetector 이벤트 구독
        if (_inputDeviceDetector != null)
        {
            _inputDeviceDetector.OnInputDeviceChanged.AddListener(OnInputDeviceDetectorChanged);
        }
    }
    
    /// <summary>
    /// InputDeviceDetector에서 입력 기기가 변경되었을 때 호출되는 콜백 함수
    /// </summary>
    /// <param name="deviceType">변경된 입력 기기 타입</param>
    private void OnInputDeviceDetectorChanged(InputDeviceType deviceType)
    {
        // InputReader에 입력 기기 변경 알림
        if (_inputReader != null)
        {
            _inputReader.NotifyInputDeviceChanged(deviceType);
        }
    }

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
            _inputReader.AttackHoldEvent += OnAttackHold;
            _inputReader.AttackCancelledEvent += OnAttackCancelled;
            _inputReader.RangedAttackEvent += OnRangedAttack;
            _inputReader.RangedAttackCancelledEvent += OnRangedAttackCancelled;
            _inputReader.DodgeEvent += OnDodge;
            _inputReader.DefendEvent += OnDefend;
            _inputReader.DefendCancelledEvent += OnDefendCancelled;
            _inputReader.LookEvent += OnLook;
            _inputReader.MousePositionEvent += OnMousePosition;
            _inputReader.SkillEvent += OnSkill;
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
            _inputReader.AttackHoldEvent -= OnAttackHold;
            _inputReader.AttackCancelledEvent -= OnAttackCancelled;
            _inputReader.RangedAttackEvent -= OnRangedAttack;
            _inputReader.RangedAttackCancelledEvent -= OnRangedAttackCancelled;
            _inputReader.DodgeEvent -= OnDodge;
            _inputReader.DefendEvent -= OnDefend;
            _inputReader.DefendCancelledEvent -= OnDefendCancelled;
            _inputReader.LookEvent -= OnLook;
            _inputReader.MousePositionEvent -= OnMousePosition;
            _inputReader.SkillEvent -= OnSkill;
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
    /// 공격 홀드 이벤트가 발생했을 때 호출되는 콜백 함수입니다.
    /// </summary>
    private void OnAttackHold()
    {
        _attackHeldInput = true;
    }

    /// <summary>
    /// 공격 취소 이벤트가 발생했을 때 호출되는 콜백 함수입니다.
    /// </summary>
    private void OnAttackCancelled()
    {
        _attackInput = false;
        _attackHeldInput = false;
    }
    
    /// <summary>
    /// 원거리 공격 이벤트가 발생했을 때 호출되는 콜백 함수입니다.
    /// </summary>
    private void OnRangedAttack()
    {
        _rangedAttackInput = true;
    }
    
    /// <summary>
    /// 원거리 공격 취소 이벤트가 발생했을 때 호출되는 콜백 함수입니다.
    /// </summary>
    private void OnRangedAttackCancelled()
    {
        _rangedAttackInput = false;
    }
    
    /// <summary>
    /// 회피 이벤트가 발생했을 때 호출되는 콜백 함수입니다.
    /// </summary>
    private void OnDodge()
    {
        _dodgeInput = true;
    }
    
    /// <summary>
    /// 방어 이벤트가 발생했을 때 호출되는 콜백 함수입니다.
    /// </summary>
    private void OnDefend()
    {
        _defendInput = true;
    }
    
    /// <summary>
    /// 방어 취소 이벤트가 발생했을 때 호출되는 콜백 함수입니다.
    /// </summary>
    private void OnDefendCancelled()
    {
        _defendInput = false;
    }
    
    /// <summary>
    /// 조준/시선 이벤트가 발생했을 때 호출되는 콜백 함수입니다. (게임패드 전용)
    /// </summary>
    /// <param name="lookInput">조준/시선 입력 벡터입니다.</param>
    private void OnLook(Vector2 lookInput)
    {
        _lookInput = lookInput;
    }
    
    /// <summary>
    /// 마우스 위치 이벤트가 발생했을 때 호출되는 콜백 함수입니다.
    /// </summary>
    /// <param name="mousePosition">마우스 스크린 위치입니다.</param>
    private void OnMousePosition(Vector2 mousePosition)
    {
        _mousePosition = mousePosition;
    }

    /// <summary>
    /// 스킬 이벤트가 발생했을 때 호출되는 콜백 함수입니다.
    /// </summary>
    private void OnSkill()
    {
        _skillInput = true;
    }

    /// <summary>
    /// 매 프레임의 마지막에 호출되어, 한 번만 처리해야 하는 입력 상태를 리셋합니다.
    /// </summary>
    public void LateTick()
    {
        _skillInput = false;
        _attackInput = false;
        _dodgeInput = false;
    }
    
    private void OnDestroy()
    {
        // InputDeviceDetector 이벤트 해제
        if (_inputDeviceDetector != null)
        {
            _inputDeviceDetector.OnInputDeviceChanged.RemoveListener(OnInputDeviceDetectorChanged);
        }
    }

}