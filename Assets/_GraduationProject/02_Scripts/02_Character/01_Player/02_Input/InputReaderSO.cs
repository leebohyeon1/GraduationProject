using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using System;
using UnityEngine.SceneManagement;

// Input Actions 에셋에서 C# 클래스를 생성(Generate C# Class)해야 합니다.
// 클래스 이름은 에셋 이름과 동일한 InputSystem_Actions 라고 가정합니다.
[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/Input/Input Reader")]
public class InputReaderSO : ScriptableObject, InputSystem_Actions.IPlayerActions, InputSystem_Actions.IUIActions, InputSystem_Actions.IDeveloperActions
{
    // Player Actions
    public event Action<Vector2> MoveEvent = delegate { };
    public event Action<Vector2> MousePositionEvent = delegate { };

    public event Action NormalAttackEvent = delegate { };
    public event Action NormalAttackCancelEvent = delegate { };
    public event Action NormalCounterEvent = delegate { };
    public event Action ChargeStartEvent = delegate { };
    public event Action ChargeCancelEvent = delegate { };

    public event Action DodgeEvent = delegate { };
    public event Action ToggleLockOnEvent = delegate { };
    public event Action LockOnTargetChangeForKeyboard = delegate { };
    public event Action<Vector2> LockOnTargetChangeForGamepadEvent = delegate { };

    public event Action InteractEvent = delegate { };
    public event Action InteractHoldEvent = delegate { };
    public event Action InteractCancelEvent = delegate { };

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

    public void Initialize()
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

    /// <summary>
    /// 입력 시스템 리소스를 정리합니다
    /// </summary>
    public void Dispose()
    {
        DisablePlayerActions();
        DisableUIActions();
        DisableDeveloperActions();
        _inputActions?.Dispose();
        _inputActions = null;

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

    public void OnNormalAttack(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                    NormalAttackEvent.Invoke();
                break;
            case InputActionPhase.Canceled:
                    NormalAttackCancelEvent.Invoke();
                break;
        }
    }

    public void OnCounterAndCharge(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if (context.interaction is HoldInteraction)
                {
                    ChargeStartEvent.Invoke();
                }
                else
                {
                    NormalCounterEvent.Invoke();
                }
                break;
            case InputActionPhase.Canceled:
                ChargeCancelEvent.Invoke();
                break;
        }
    }

    public void OnMousePosition(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = context.ReadValue<Vector2>();
        MousePositionEvent.Invoke(mousePosition);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if (context.interaction is HoldInteraction)
                {
                    InteractHoldEvent.Invoke();
                }
                else
                {
                    InteractEvent.Invoke();
                }
                break;
            case InputActionPhase.Canceled:
                InteractCancelEvent.Invoke();
                break;
        }
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            DodgeEvent.Invoke();
        }
    }

    public void OnLockOnForKeyboard(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if (context.interaction is HoldInteraction)
                {
                    ToggleLockOnEvent.Invoke();
                }
                else
                {
                    LockOnTargetChangeForKeyboard.Invoke();
                }
                break;
        }
    }

    public void OnToggleLockOnForGamepad(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            ToggleLockOnEvent.Invoke();
        }
    }

    public void OnLockOnTargetChangeForGamepad(InputAction.CallbackContext context)
    {
        Vector2 lockOnInput = context.ReadValue<Vector2>();
        LockOnTargetChangeForGamepadEvent.Invoke(lockOnInput);
    }

    public void OnPotion(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
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

}