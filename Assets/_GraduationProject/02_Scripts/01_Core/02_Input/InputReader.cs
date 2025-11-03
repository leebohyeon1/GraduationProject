using UnityEngine;
using UnityEngine.InputSystem;
using BH_Lib.DI;
using UnityEngine.InputSystem.Interactions;
using BH_Lib.Log;
using System;
using Unity.AppUI.UI;

// Input Actions 에셋에서 C# 클래스를 생성(Generate C# Class)해야 합니다.
// 클래스 이름은 에셋 이름과 동일한 InputSystem_Actions 라고 가정합니다.
[CreateAssetMenu(fileName = "InputReader", menuName = "System/Input Reader")]
public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions, InputSystem_Actions.IUIActions, InputSystem_Actions.IDeveloperActions
{
    // Player Actions
    public event Action<Vector2> MoveEvent = delegate { };
    public event Action AttackEvent = delegate { };
    public event Action AttackHoldEvent = delegate { };
    public event Action AttackCancelledEvent = delegate { };
    public event Action RangedAttackEvent = delegate { };
    public event Action RangedAttackCancelledEvent = delegate { };
    public event Action DodgeEvent = delegate { };
    public event Action DefendEvent = delegate { };
    public event Action DefendCancelledEvent = delegate { };
    public event Action<Vector2> LookEvent = delegate { };
    public event Action<Vector2> MousePositionEvent = delegate { };
    public event Action<InputDeviceType> InputDeviceChangedEvent = delegate { };
    public event Action SkillEvent = delegate { };
    public event Action SkillChangeEvent = delegate { };
    public event Action SkillChangeCancelEvent = delegate { };
    public event Action InteractEvent = delegate { };
    public event Action PotionEvent = delegate { };

    // UI Actions
    public event Action CancelEvent = delegate { };
    public event Action<Vector2> NavigateEvent = delegate { };
    public event Action SubmitEvent = delegate { };
    public event Action ClickEvent = delegate { };
    public event Action<Vector2> PointEvent = delegate { };
    public event Action RightClickEvent = delegate { };
    public event Action MiddleClickEvent = delegate { };
    public event Action<Vector2> ScrollWheelEvent = delegate { };

    // Developer Actions;
    public event Action ToggleConsoleEvent = delegate { };  
    public event Action EnterEvent = delegate { };

    private InputSystem_Actions _inputActions;

    private void OnEnable()
    {
        if (_inputActions == null)
        {
            _inputActions = new InputSystem_Actions();
            _inputActions.Player.SetCallbacks(this);
            _inputActions.UI.SetCallbacks(this);
            _inputActions.Developer.SetCallbacks(this); 
        }
        EnablePlayerActions();
        EnableDeveloperActions();
    }

    private void OnDisable()
    {
        DisablePlayerActions();
        DisableUIActions();
        DisableDeveloperActions();
    }

    public void EnablePlayerActions()
    {
        _inputActions.Player.Enable();
    }

    public void DisablePlayerActions()
    {
        _inputActions?.Player.Disable();
    }

    public void EnableUIActions()
    {
        _inputActions.UI.Enable();
    }

    public void DisableUIActions()
    {
        _inputActions?.UI.Disable();
    }

    public void EnableDeveloperActions()
    {
        _inputActions.Developer.Enable();
    }

    public void DisableDeveloperActions()
    {
        _inputActions.Developer.Disable();
    }

    // Player Action Implementations
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

    // UI Action Implementations
    public void OnCancel(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            CancelEvent.Invoke();
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            ClickEvent.Invoke();
        }
    }

    public void OnMiddleClick(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            MiddleClickEvent.Invoke();
        }
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        NavigateEvent.Invoke(context.ReadValue<Vector2>());
    }

    public void OnPoint(InputAction.CallbackContext context)
    {
        PointEvent.Invoke(context.ReadValue<Vector2>());
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            RightClickEvent.Invoke();
        }
    }

    public void OnScrollWheel(InputAction.CallbackContext context)
    {
        ScrollWheelEvent.Invoke(context.ReadValue<Vector2>());
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            SubmitEvent.Invoke();
        }
    }

    // Developer Action Implementations
    public void OnToggleConsole(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            ToggleConsoleEvent.Invoke();
        }
    }

    public void OnEnter(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            EnterEvent.Invoke();
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
        DisableUIActions();
        _inputActions?.Dispose();
        _inputActions = null;
    }

}