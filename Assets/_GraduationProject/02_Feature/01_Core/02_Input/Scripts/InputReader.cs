using UnityEngine;
using UnityEngine.InputSystem;
using BH_Lib.DI;
using UnityEngine.InputSystem.Interactions;
using BH_Lib.Log;
using System;

// Input Actions 에셋에서 C# 클래스를 생성(Generate C# Class)해야 합니다.
// 클래스 이름은 에셋 이름과 동일한 InputSystem_Actions 라고 가정합니다.
[CreateAssetMenu(fileName = "InputReader", menuName = "System/Input Reader")]
public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions
{
    // 이동 이벤트
    public event Action<Vector2> MoveEvent = delegate { };
    // 공격 이벤트 (시작)
    public event Action AttackEvent = delegate { };
    // 공격 홀드 이벤트 
    public event Action AttackHoldEvent = delegate { };
    // 공격 이벤트 (종료)
    public event Action AttackCancelledEvent = delegate { };
    // 원거리 공격 이벤트 (시작)
    public event Action RangedAttackEvent = delegate { };
    // 원거리 공격 이벤트 (종료)
    public event Action RangedAttackCancelledEvent = delegate { };
    // 회피 이벤트
    public event Action DodgeEvent = delegate { };
    // 방어 이벤트 (시작)
    public event Action DefendEvent = delegate { };
    // 방어 이벤트 (종료)
    public event Action DefendCancelledEvent = delegate { };
    // 시선/조준 이벤트
    public event Action<Vector2> LookEvent = delegate { };
    // 마우스 위치 이벤트
    public event Action<Vector2> MousePositionEvent = delegate { };
    // 입력 기기 변경 이벤트
    public event Action<InputDeviceType> InputDeviceChangedEvent = delegate { };
    // 스킬 사용 이벤트
    public event Action SkillEvent = delegate { };
    // 스킬 변경 이벤트 (시작)
    public event Action SkillChangeEvent = delegate { };
    // 스킬 변경 이벤트 (종료)
    public event Action SkillChangeCancelEvent = delegate { };
    // 상호작용 이벤트
    public event Action InteractEvent = delegate { };
    // 포션 사용 이벤트
    public event Action PotionEvent = delegate { };

    private InputSystem_Actions _inputActions;

    private void OnEnable()
    {
        if (_inputActions == null)
        {
            _inputActions = new InputSystem_Actions();
            _inputActions.Player.SetCallbacks(this);
        }
        EnablePlayerActions();
    }

    private void OnDisable()
    {
        DisablePlayerActions();
    }

    public void EnablePlayerActions()
    {
        _inputActions.Player.Enable();
    }

    public void DisablePlayerActions()
    {
        _inputActions?.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 lookValue = context.ReadValue<Vector2>();
        LookEvent.Invoke(lookValue);
    }

    public void OnMeleeAttack(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if (context.interaction is HoldInteraction)
                {
                    AttackHoldEvent.Invoke();
                }
                else
                {
                    AttackEvent.Invoke();
                }
                break;
            case InputActionPhase.Canceled:
                AttackCancelledEvent.Invoke();
                break;
        }
    }

    public void OnRangedAttack(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                RangedAttackEvent.Invoke();
                break;
            case InputActionPhase.Canceled:
                RangedAttackCancelledEvent.Invoke();
                break;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            InteractEvent.Invoke();
        }
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            DodgeEvent.Invoke();
        }
    }

    public void OnDefend(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                DefendEvent.Invoke();
                break;
            case InputActionPhase.Canceled:
                DefendCancelledEvent.Invoke();
                break;
        }
    }
    public void OnMousePosition(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = context.ReadValue<Vector2>();
        MousePositionEvent.Invoke(mousePosition);
    }

    public void OnSkill(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            SkillEvent.Invoke();
        }
    }

    public void OnSkillChange(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                SkillChangeEvent.Invoke();
                break;
            case InputActionPhase.Canceled:
                SkillChangeCancelEvent.Invoke();
                break;
        }
    }

    public void OnPotion(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            PotionEvent.Invoke();
        }
    }

    /// <summary>
    /// 외부에서 입력 기기 변경을 알릴 때 사용하는 함수
    /// </summary>
    /// <param name="deviceType">변경된 입력 기기 타입</param>
    public void NotifyInputDeviceChanged(InputDeviceType deviceType)
    {
        InputDeviceChangedEvent?.Invoke(deviceType);
    }

    /// <summary>
    /// 입력 시스템 리소스를 정리합니다
    /// </summary>
    public void Dispose()
    {
        DisablePlayerActions();
        _inputActions?.Dispose();
        _inputActions = null;
    }

}
