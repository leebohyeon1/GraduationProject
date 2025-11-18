using BH_Lib.DI;
using BH_Lib.FSM;
using BH_Lib.Log;
using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 플레이어의 입력을 처리하고 관련 이벤트를 발생시키는 클래스입니다. (Unity Input System 사용)
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader _inputReader; // 입력 처리기
    private IInputDeviceDetector _inputDeviceDetector; // 입력 장치 감지기

    // 입력 상태 변수
    private Vector2 _moveInput; // 이동 입력
    private Vector2 _lookInput; // 시선 입력 (게임패드)
    private Vector2 _mousePosition; // 마우스 위치 (키보드/마우스)
    private bool _attackInput; // 공격 입력
    private bool _attackHeldInput; // 공격 홀드 입력
    private bool _rangedAttackInput; // 원거리 공격 입력
    private bool _dodgeInput; // 회피 입력
    private bool _defendInput; // 방어 입력
    private bool _skillInput; // 스킬 입력
    private bool _skilChangeInput; // 스킬 변경 입렵
    private bool _InteractInput; // 상호작용 입력
    private bool _potionInput; // 포션 사용 입력
    private bool _parryInput; // 패리 입력
    private bool _toggleLockOnInput; // 락온 토글 입력

    private bool _canAttackHeldInput = true; // 차징 중복 막기 위함

    #region Properties
    public Vector2 MoveInput => _moveInput;
    public Vector2 LookInput => _lookInput;
    public Vector2 MousePosition => _mousePosition;
    public bool AttackInput => _attackInput;
    public bool AttackHeldInput => _attackHeldInput;
    public bool RangedAttackInput => _rangedAttackInput;
    public bool DodgeInput => _dodgeInput;
    public bool DefendInput => _defendInput;
    public bool SkillInput => _skillInput;
    public bool SkillChangeInput => _skilChangeInput;
    public bool InteractInput => _InteractInput;
    public bool PotionInput => _potionInput;
    public bool ParryInput => _parryInput;  
    public bool ToggleLockOnInput => _toggleLockOnInput;
    #endregion

    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize(IInputDeviceDetector inputDeviceDetector)
    {
        _inputDeviceDetector = inputDeviceDetector;
        if (_inputDeviceDetector != null)
        {
            _inputDeviceDetector.OnInputDeviceChanged.AddListener(OnInputDeviceDetectorChanged);
        }

        _canAttackHeldInput = true;
    }
    
    /// <summary>
    /// 입력 장치 변경 시 호출됩니다.
    /// </summary>
    private void OnInputDeviceDetectorChanged(InputDeviceType deviceType)
    {
        if (_inputReader != null)
        {
            _inputReader.NotifyInputDeviceChanged(deviceType);
        }
    }

    private void OnEnable()
    {
        if (_inputReader == null)
        {
            return;
        }

        // 이벤트 구독
        _inputReader.MoveEvent += OnMove;
        _inputReader.LookEvent += OnLook;
        _inputReader.MousePositionEvent += OnMousePosition;

        _inputReader.AttackEvent += OnAttack;
        _inputReader.AttackHoldEvent += OnAttackHold;
        _inputReader.AttackCancelledEvent += OnAttackCancelled;

        _inputReader.DodgeEvent += OnDodge;
        _inputReader.DefendEvent += OnDefend;
        _inputReader.DefendCancelledEvent += OnDefendCancelled;
        _inputReader.ParryEvent += OnParry;
        _inputReader.ToggleLockOnEvent += ToggleLockOnEvent;

        _inputReader.InteractEvent += OnInteract;

    }
    
    private void OnDisable()
    {
        if (_inputReader == null)
        {
            return;
        }

        // 이벤트 구독 해제
        _inputReader.MoveEvent -= OnMove;
        _inputReader.LookEvent -= OnLook;
        _inputReader.MousePositionEvent -= OnMousePosition;

        _inputReader.AttackEvent -= OnAttack;
        _inputReader.AttackHoldEvent -= OnAttackHold;
        _inputReader.AttackCancelledEvent -= OnAttackCancelled;

        _inputReader.DodgeEvent -= OnDodge;
        _inputReader.DefendEvent -= OnDefend;
        _inputReader.DefendCancelledEvent -= OnDefendCancelled;
        _inputReader.ParryEvent -= OnParry;
        _inputReader.ToggleLockOnEvent -= ToggleLockOnEvent;

        _inputReader.InteractEvent -= OnInteract;
    }
    
    private void OnDestroy()
    {
        if (_inputDeviceDetector != null)
        {
            _inputDeviceDetector.OnInputDeviceChanged.RemoveListener(OnInputDeviceDetectorChanged);
        }
    }

    // 각 입력 이벤트에 대한 콜백 함수들
    private void OnMove(Vector2 moveInput) => _moveInput = moveInput;
    private void OnLook(Vector2 lookInput) => _lookInput = lookInput;
    private void OnMousePosition(Vector2 mousePosition) => _mousePosition = mousePosition;

    private void OnAttack()
    {
        _attackInput = true;
    }

    private void OnAttackHold()
    {
        if (_canAttackHeldInput)
        {
            _attackHeldInput = true;
            _canAttackHeldInput = false;
        }
    }

    private void OnAttackCancelled() 
    {
        _attackInput = false;

        if (!_canAttackHeldInput)
        {
            _attackHeldInput = false;
            _canAttackHeldInput = true;
        }
    }
    
    private void OnDodge() => _dodgeInput = true;
    private void OnDefend() => _defendInput = true;
    private void OnDefendCancelled() => _defendInput = false;
    private void OnParry() => _parryInput = true;
    private void ToggleLockOnEvent() => _toggleLockOnInput = true;

    private void OnInteract() => _InteractInput = true;

    

    /// <summary>
    /// 매 프레임 마지막에 호출되어 일회성 입력 상태를 초기화합니다.
    /// </summary>
    public void LateTick()
    {
        _attackInput = false;
        _skillInput = false;
        _attackInput = false;
        _dodgeInput = false;
        _InteractInput = false;
        _potionInput = false;
        _parryInput = false;
        _toggleLockOnInput = false;
    }

    public void SetAttackHeldInput(bool isAttackHold)
    {
        _attackHeldInput = isAttackHold;
    }
}