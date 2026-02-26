using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using System;

// Input Actions 에셋에서 C# 클래스를 생성(Generate C# Class)해야 합니다.
// 클래스 이름은 에셋 이름과 동일한 InputSystem_Actions 라고 가정합니다.
[CreateAssetMenu(fileName = "InputReader", menuName = "Project/Input/Input Reader")]
public class InputReaderSO : ScriptableObject, InputSystem_Actions.IPlayerActions, 
    InputSystem_Actions.IUIActions, InputSystem_Actions.IDeveloperActions, InputSystem_Actions.IShareActions
{
    public enum InputMode
    {
        None,       // 모든 입력 차단
        Gameplay,   // 캐릭터 조작
        UI,         // 인벤토리, 메뉴 등
        CutScene     // 컷씬 전용 
    }

    // 현재 상태를 외부에서 읽을 수 있게 프로퍼티 제공
    public InputMode CurrentInputMode { get; private set; } = InputMode.None;
    // 상태가 바뀔 때 알림을 받고 싶다면 이벤트 추가
    public event Action<InputMode> InputModeChanged;

    // Player Actions
    public event Action<Vector2> MoveEvent = delegate { };
    public event Action<Vector2> MousePositionEvent = delegate { };

    public event Action NormalAttackEvent = delegate { };
    public event Action NormalAttackCancelEvent = delegate { };
    public event Action NormalCounterEvent = delegate { };
    public event Action NormalCounterInputEvent = delegate { };
    public event Action NormalCounterInputCancelEvent = delegate { };    
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
    public event Action EscapeEvent = delegate { }; 

    // UI Actions
    public event Action CancelEvent = delegate { };
    public event Action<Vector2> NavigateEvent = delegate { };
    public event Action SubmitEvent = delegate { };
    public event Action ClickEvent = delegate { };
    public event Action<Vector2> PointEvent = delegate { };
    public event Action RightClickEvent = delegate { };
    public event Action MiddleClickEvent = delegate { };
    public event Action<Vector2> ScrollWheelEvent = delegate { };
    public event Action AnyKeyEvent = delegate { };
    public event Action NextEvent = delegate { };
    public event Action PreviousEvent = delegate { };

    // Developer Actions;
    public event Action ToggleConsoleEvent = delegate { };  
    public event Action EnterEvent = delegate { };

    private InputSystem_Actions _inputActions;

    private void OnEnable()
    {
        if (_inputActions == null)
        {
            Debug.Log("InputReader 초기화");
            _inputActions = new InputSystem_Actions();
            _inputActions.Player.SetCallbacks(this);
            _inputActions.UI.SetCallbacks(this);
            _inputActions.Developer.SetCallbacks(this);
            _inputActions.Share.SetCallbacks(this);
        }

        _inputActions.Share.Enable(); // 공용 입력은 항상 활성화

        SetInputMode(InputMode.None);
    }

    private void OnDisable()
    {
        // ScriptableObject가 비활성화되거나 게임이 종료될 때 정리
        _inputActions.Share.Disable();
        DisableAllInput();

        // 이전에 말씀드린 안전한 해제 방식
        _inputActions.Player.RemoveCallbacks(this);
        _inputActions.UI.RemoveCallbacks(this);
        _inputActions.Developer.RemoveCallbacks(this);
        _inputActions.Share.RemoveCallbacks(this);

        _inputActions = null;
    }

    /// <summary>
    /// 입력 모드 설정
    /// </summary>
    /// <param name="newMode">새 모드</param>
    public void SetInputMode(InputMode newMode)
    {
        // 우선 모든 맵을 비활성화 (개발자 맵 같은 상시 맵 제외)
        DisableAllInput();

        switch (newMode)
        {
            case InputMode.Gameplay:
                _inputActions.Player.Enable();
                break;

            case InputMode.UI:
                _inputActions.UI.Enable();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case InputMode.CutScene:

                break;

            case InputMode.None:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }

        CurrentInputMode = newMode;
        InputModeChanged?.Invoke(newMode);

        Debug.Log($"입력 모드 변경: {newMode}");
    }

    /// <summary>
    /// 모든 입력을 차단합니다. (컷씬, 로딩 등)
    /// </summary>
    public void DisableAllInput()
    {
        _inputActions.Player.Disable();
        _inputActions.UI.Disable();
        // 개발자 콘솔조차 막으려면 이것도 Disable
        // _inputActions.Developer.Disable(); 
    }


    public void OnEscape(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            EscapeEvent.Invoke();
        }
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

    public void OnCounterInputCheck(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                NormalCounterInputEvent.Invoke();
                break;
            case InputActionPhase.Canceled:
                NormalCounterInputCancelEvent.Invoke();
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

    public void OnAnyKey(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            AnyKeyEvent.Invoke();
        }
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            NextEvent.Invoke();
        }
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            PreviousEvent.Invoke();
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