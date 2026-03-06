using System;
using UnityEngine;

/// <summary>
/// 플레이어의 기본 상태를 정의하는 클래스
/// </summary>
public abstract class PlayerBaseState : IState, IDisposable
{
    /// <summary>
    /// 플레이어 애니메이터 상태 enum
    /// </summary>
    public enum AnimatorState
    {
        Idle = 0,
        Move = 1,
        Dodge = 2,
        NormalAttack = 3,
        NormalCounterAttack = 4,
        Charge = 5,
        ChargeCounterCounterAttack = 6,
        SpecialAttack = 7,

        NormalDamaged = -1,
        HeavyDamaged = -2,
        Knockdown = -3,
    }

    protected StateMachine<PlayerController> p_stateMachine;            // 상태를 관리하는 상태 머신
    protected PlayerController p_owner => p_stateMachine.GetContext();  // 플레이어 기본 상태를 소유한 클래스 변수
    protected Animator p_animator;

    protected const string p_stateParamter = "State";

    public PlayerBaseState(StateMachine<PlayerController> stateMachine)
    { 
        p_stateMachine = stateMachine;
        p_animator = p_owner.GetComponent<Animator>();
    }

    // 상태가 시작할 때 호출
    public virtual void OnEnter()
    {
        SetupEvents();
        SetupStats();
        SetupAnimator();
    }

    // 매 프레임마다 호출
    public virtual void OnUpdate()
    {

    }

    // 물리 시간마다 호출
    public virtual void OnFixedUpdate()
    {

    }

    // 상태가 끝날 때 호출
    public virtual void OnExit()
    {
        ClearEvents();
        ClearStats();
        ClearAnimator();
    }

    #region Setup Function
    /// <summary>
    /// 이벤트 설정 함수
    /// </summary>
    protected virtual void SetupEvents()
    {
        p_owner.InputReader.MoveEvent += OnMove;
        p_owner.InputReader.MousePositionEvent += OnMousePosition;

        p_owner.InputReader.DodgeEvent += OnDodge;
        p_owner.InputReader.NormalAttackEvent += OnNormalAttack;
        p_owner.InputReader.NormalCounterEvent += OnNormalCounter;
        p_owner.InputReader.ChargeStartEvent += OnChargeStart;
        p_owner.InputReader.ChargeCancelEvent += OnChargeCancel;

        p_owner.InputReader.ToggleLockOnEvent += OnToggleLockOn;
        p_owner.InputReader.LockOnTargetChangeForKeyboard += OnLockOnTargetChangeForKeyboard;
        p_owner.InputReader.LockOnTargetChangeForGamepadEvent += OnLockOnTargetChangeForGamepadEvent;

    }
    /// <summary>
    /// 능력치 설정 함수
    /// </summary>
    protected virtual void SetupStats()
    {

    }
    /// <summary>
    /// 애니메이터 설정 함수
    /// </summary>
    protected virtual void SetupAnimator()
    {

    }
    #endregion

    #region Clear Function
    /// <summary>
    /// 이벤트 해제 함수
    /// </summary>
    protected virtual void ClearEvents()
    {
        p_owner.InputReader.MoveEvent -= OnMove;
        p_owner.InputReader.MousePositionEvent -= OnMousePosition;

        p_owner.InputReader.DodgeEvent -= OnDodge;
        p_owner.InputReader.NormalAttackEvent -= OnNormalAttack;
        p_owner.InputReader.NormalCounterEvent -= OnNormalCounter;
        p_owner.InputReader.ChargeStartEvent -= OnChargeStart;
        p_owner.InputReader.ChargeCancelEvent -= OnChargeCancel;

        p_owner.InputReader.ToggleLockOnEvent -= OnToggleLockOn;
        p_owner.InputReader.LockOnTargetChangeForKeyboard -= OnLockOnTargetChangeForKeyboard;
        p_owner.InputReader.LockOnTargetChangeForGamepadEvent -= OnLockOnTargetChangeForGamepadEvent;
    }
    /// <summary>
    /// 능력치 해제 함수
    /// </summary>
    protected virtual void ClearStats()
    {

    }
    /// <summary>
    /// 애니메이터 해제 함수
    /// </summary>
    protected virtual void ClearAnimator()
    {

    }

    #endregion

    #region InputEventHandle
    /// <summary>
    /// 이동 입력 처리
    /// </summary>
    /// <param name="vector2">이동 방향</param>
    protected virtual void OnMove(Vector2 vector2) { }

    /// <summary>
    /// 마우스 위치 입력 처리
    /// </summary>
    /// <param name="vector2">마우스 위치</param>
    protected virtual void OnMousePosition(Vector2 vector2) { }   

    /// <summary>
    /// 회피 입력 처리
    /// </summary>
    protected virtual void OnDodge() { }

    /// <summary>
    /// 공격 입력 처리
    /// </summary>
    protected virtual void OnNormalAttack() { }
    
    /// <summary>
    /// 일반 상쇄 입력 처리
    /// </summary>
    protected virtual void OnNormalCounter() { }

    /// <summary>
    /// 차지 시작 입력 처리
    /// </summary>
    protected virtual void OnChargeStart() { }

    /// <summary>
    /// 차지 종료 입력 처리
    /// </summary>
    protected virtual void OnChargeCancel() { }

    /// <summary>
    /// 락온 토글 입력 처리
    /// </summary>
    protected virtual void OnToggleLockOn()
    {
        if (p_owner.LockOn.IsLockOn)
        {
            p_owner.LockOn.LockOff();
        }
        else
        {
            InputDeviceType currentInputDevice = p_owner.InputHandler.CurrentInputDevice;
            Vector3 searchOrigin = p_owner.InputHandler.MousePosition;

            p_owner.LockOn.LockOn(currentInputDevice, searchOrigin);
        }
    }

    /// <summary>
    /// 키보드 락온 대상 변경 입력 처리
    /// </summary>
    protected virtual void OnLockOnTargetChangeForKeyboard()
    {
        InputDeviceType currentInputDevice = p_owner.InputHandler.CurrentInputDevice;
        Vector3 searchOrigin = p_owner.InputHandler.MousePosition;

        // 락온이 되어 있지 않으면 락온
        if (!p_owner.LockOn.IsLockOn)
        {
            p_owner.LockOn.LockOn(currentInputDevice, searchOrigin);
        }
        else
        {
            p_owner.LockOn.ChangeLockOnTargetByMouse(searchOrigin);
        }

    }

    /// <summary>
    /// 게임 패드 락온 대상 변경 입력 처리
    /// </summary>
    /// <param name="gamepadInput">대상 탐색 방향</param>
    protected virtual void OnLockOnTargetChangeForGamepadEvent(Vector2 gamepadInput)
    {
        // 락온이 되어 있지 않으면 리턴
        if (!p_owner.LockOn.IsLockOn)
        {
            return;
        }

        p_owner.LockOn.ChangeLockOnTargetByGamePad(gamepadInput);
    }
    #endregion

    // 객체가 완전히 파괴되거나 명시적으로 자원을 해제해야 할 때 호출
    public void Dispose()
    {
        ClearEvents();
        ClearStats();
        ClearAnimator();

        // 메모리 누수를 방지하기 위해 가비지 컬렉터에게 
        // "이 객체의 소멸(Finalize) 처리는 안 해도 돼" 라고 알려줌
        GC.SuppressFinalize(this);
    }
}

