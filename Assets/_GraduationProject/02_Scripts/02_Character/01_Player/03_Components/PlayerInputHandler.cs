using System;
using UnityEngine;

/// <summary>
/// 플레이어의 입력을 처리하고 관련 이벤트를 발생시키는 클래스입니다. (Unity Input System 사용)
/// </summary>
public class PlayerInputHandler : MonoBehaviour, IDisposable
{
    [Header("Input")]
    [SerializeField] private InputReaderSO _inputReader; // 입력 처리기
    private PlayerEvents _events;

    // 입력 상태 변수
    private Vector3 _moveInput; // 이동 입력
    private Vector3 _mousePosition; // 마우스 위치 (키보드/마우스)
    private bool _attackInput; // 공격 입력
    private bool _attackHeldInput; // 공격 홀드 입력
    private bool _rangedAttackInput; // 원거리 공격 입력
    private bool _dodgeInput; // 회피 입력
    private bool _skillInput; // 스킬 입력
    private bool _skilChangeInput; // 스킬 변경 입렵
    private bool _InteractInput; // 상호작용 입력
    private bool _potionInput; // 포션 사용 입력
    private bool _parryInput; // 패리 입력
    private bool _toggleLockOnInput; // 락온 토글 입력
    private bool _lockOnTargetChangeInput; // 락온 타겟 변경 입력
    private Vector2 _lockOnTargetChangeVector2Input; // 락온 타겟 변경 벡터 입력

    private bool _canAttackHeldInput = true; // 차징 중복 막기 위함
    private InputDeviceType _currentInputDevice;    // 현재 입력 디바이스
    private bool _canBufferInput = false;           // 선입력 가능 여부

    #region Properties
    public Vector3 MoveInput => _moveInput;
    public Vector3 MousePosition => _mousePosition;
    public bool AttackInput => _attackInput;
    public bool AttackHeldInput => _attackHeldInput;
    public bool RangedAttackInput => _rangedAttackInput;
    public bool DodgeInput => _dodgeInput;
    public bool SkillInput => _skillInput;
    public bool SkillChangeInput => _skilChangeInput;
    public bool InteractInput => _InteractInput;
    public bool PotionInput => _potionInput;
    public bool ParryInput => _parryInput;  
    public bool ToggleLockOnInput => _toggleLockOnInput;
    public bool LockOnTargetChangeInput => _lockOnTargetChangeInput;
    public Vector2 LockOnTargetChangeVector2Input => _lockOnTargetChangeVector2Input;

    public InputDeviceType CurrentInputDevice => _currentInputDevice;
    public bool CanBufferInput => _canBufferInput;  
    #endregion

    /// <summary>
    /// 클래스 초기화
    /// </summary>
    /// <param name="inputReader">InputReader 스크립터블 오브젝트</param>
    public void Initialize(InputReaderSO inputReader, PlayerEvents events)
    {
        _inputReader = inputReader;
        _events = events;

        // 이벤트 등록
        InputDeviceDetector.Instance.InputDeviceChanged.AddListener(OnInputDeviceChanged);

        // 이벤트 구독
        _inputReader.MoveEvent += OnMove;
        _inputReader.MousePositionEvent += OnMousePosition;

        _inputReader.NormalAttackEvent += OnNormalAttack;
        _inputReader.ChargeStartEvent += OnChargeStart;
        _inputReader.ChargeCancelEvent += OnAttackCancel;

        _inputReader.DodgeEvent += OnDodge;
        _inputReader.NormalCounterEvent += OnParry;
        _inputReader.ToggleLockOnEvent += ToggleLockOnEvent;
        _inputReader.LockOnTargetChangeEvent += LockOnTargetChangeEvent;
        _inputReader.LockOnTargetChangeVector2Event += LockOnTargetChangeVector2Event;

        _inputReader.InteractEvent += OnInteract;
        _inputReader.PotionEvent += OnPotion;

        _events.BufferInputStarted += OnBufferInputStarted;
        _events.BufferInputEnded += OnBufferInputEnded;
    }

    /// <summary>
    /// 클래스 해제
    /// </summary>
    public void Dispose()
    {
        InputDeviceDetector.Instance.InputDeviceChanged.RemoveListener(OnInputDeviceChanged);

        // 이벤트 구독 해제
        _inputReader.MoveEvent -= OnMove;
        _inputReader.MousePositionEvent -= OnMousePosition;

        _inputReader.NormalAttackEvent -= OnNormalAttack;
        _inputReader.ChargeStartEvent -= OnChargeStart;
        _inputReader.ChargeCancelEvent -= OnAttackCancel;

        _inputReader.DodgeEvent -= OnDodge;
        _inputReader.NormalCounterEvent -= OnParry;
        _inputReader.ToggleLockOnEvent -= ToggleLockOnEvent;
        _inputReader.LockOnTargetChangeEvent -= LockOnTargetChangeEvent;
        _inputReader.LockOnTargetChangeVector2Event -= LockOnTargetChangeVector2Event;

        _inputReader.InteractEvent -= OnInteract;
        _inputReader.PotionEvent -= OnPotion;

        _inputReader.Dispose();

        _events.BufferInputStarted -= OnBufferInputStarted;
        _events.BufferInputEnded -= OnBufferInputEnded;
    }

    /// <summary>
    /// 입력 장치 변경 시 호출됩니다.
    /// </summary>
    private void OnInputDeviceChanged(InputDeviceType deviceType)
    {
        _currentInputDevice = deviceType;
    }

    // 각 입력 이벤트에 대한 콜백 함수들
    private void OnMove(Vector2 moveInput)
    {
        Debug.Log(moveInput);
        _moveInput = new Vector3(moveInput.x, 0, moveInput.y);
    }

    private void OnMousePosition(Vector2 mousePosition)
    {
        _mousePosition = new Vector3(mousePosition.x, 0, mousePosition.y);
    }

    private void OnNormalAttack()
    {
        _attackInput = true;
    }

    private void OnChargeStart()
    {
        if (_canAttackHeldInput)
        {
            _attackHeldInput = true;
            _canAttackHeldInput = false;
        }
    }

    private void OnAttackCancel() 
    {
        _parryInput = false;

        if (!_canAttackHeldInput)
        {
            _attackHeldInput = false;
            _canAttackHeldInput = true;
        }
    }
    
    private void OnDodge() => _dodgeInput = true;
    private void OnParry() => _parryInput = true;
    private void ToggleLockOnEvent() => _toggleLockOnInput = true;
    private void LockOnTargetChangeEvent() => _lockOnTargetChangeInput = true;
    private void LockOnTargetChangeVector2Event(Vector2 vector2) => _lockOnTargetChangeVector2Input = vector2;

    private void OnInteract() => _InteractInput = true;
    private void OnPotion() => _potionInput = true;

    /// <summary>
    /// 선입력 시작
    /// </summary>
    private void OnBufferInputStarted()
    {
        _canBufferInput = true;
    }

    /// <summary>
    /// 선입력 종료
    /// </summary>
    private void OnBufferInputEnded()
    {
        _canBufferInput = false;
    }

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
        _lockOnTargetChangeInput = false;
    }

    public void SetAttackHeldInput(bool isAttackHold)
    {
        _attackHeldInput = isAttackHold;
    }

}