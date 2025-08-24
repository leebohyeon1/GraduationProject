using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using BH_Lib.DI;

// Input Actions 에셋에서 C# 클래스를 생성(Generate C# Class)해야 합니다.
// 클래스 이름은 에셋 이름과 동일한 InputSystem_Actions 라고 가정합니다.
[CreateAssetMenu(fileName = "InputReader", menuName = "System/Input Reader")]
public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions
{
    // 이동 이벤트
    public event UnityAction<Vector2> MoveEvent = delegate { };
    // 공격 이벤트 (시작)
    public event UnityAction AttackEvent = delegate { };
    // 공격 이벤트 (종료)
    public event UnityAction AttackCancelledEvent = delegate { };
    // 회피 이벤트
    public event UnityAction DodgeEvent = delegate { };
    // 시선/조준 이벤트
    public event UnityAction<Vector2> LookEvent = delegate { };
    // 마우스 위치 이벤트
    public event UnityAction<Vector2> MousePositionEvent = delegate { };
    // 입력 기기 변경 이벤트
    public event UnityAction<InputDeviceType> InputDeviceChangedEvent = delegate { };

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

    public void EnablePlayerActions()
    {
        _inputActions.Player.Enable();
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

    public void OnAttack(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                AttackEvent.Invoke();
                break;
            case InputActionPhase.Canceled:
                AttackCancelledEvent.Invoke();
                break;
        }
    }

    public void OnInteract(InputAction.CallbackContext context) { }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            DodgeEvent.Invoke();
        }
    }
    public void OnMousePosition(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = context.ReadValue<Vector2>();
        MousePositionEvent.Invoke(mousePosition);
    }

    /// <summary>
    /// 외부에서 입력 기기 변경을 알릴 때 사용하는 함수
    /// </summary>
    /// <param name="deviceType">변경된 입력 기기 타입</param>
    public void NotifyInputDeviceChanged(InputDeviceType deviceType)
    {
        InputDeviceChangedEvent?.Invoke(deviceType);
    }
}
