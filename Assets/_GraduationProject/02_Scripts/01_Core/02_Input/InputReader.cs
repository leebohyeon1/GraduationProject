using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using System;
using UnityEngine.SceneManagement;

// Input Actions 에셋에서 C# 클래스를 생성(Generate C# Class)해야 합니다.
// 클래스 이름은 에셋 이름과 동일한 InputSystem_Actions 라고 가정합니다.
[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/Input/Input Reader")]
public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions, InputSystem_Actions.IUIActions, InputSystem_Actions.IDeveloperActions
{
    // Player Actions
    public event Action<Vector2> MoveEvent = delegate { };
    public event Action<Vector2> LookEvent = delegate { };
    public event Action<Vector2> MousePositionEvent = delegate { };

    public event Action AttackEvent = delegate { };
    public event Action AttackHoldEvent = delegate { };
    public event Action AttackCancelledEvent = delegate { };

    public event Action DodgeEvent = delegate { };
    public event Action ParryEvent = delegate { };
    public event Action ToggleLockOnEvent = delegate { };
    public event Action LockOnTargetChangeEvent = delegate { };
    public event Action<Vector2> LockOnTargetChangeVector2Event = delegate { };

    public event Action InteractEvent = delegate { };
    public event Action InteractHoldEvent = delegate { };
    public event Action InteractCancelEvent = delegate { };

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

    public event Action<InputDeviceType> InputDeviceChangedEvent = delegate { };

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

    public void OnMousePosition(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = context.ReadValue<Vector2>();
        MousePositionEvent.Invoke(mousePosition);
    }

    public void OnMeleeAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            AttackEvent.Invoke();
        }
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

    public void OnParry(InputAction.CallbackContext context)
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
                    ParryEvent.Invoke();
                }
                break;
            case InputActionPhase.Canceled:
                AttackCancelledEvent.Invoke();
                break;
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
                    LockOnTargetChangeEvent.Invoke();
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
        LockOnTargetChangeVector2Event.Invoke(lockOnInput);
    }

    public void OnSceneLoad1(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(0);
    }
    public void OnSceneLoad2(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(1);
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