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
    // 입력 기기 변경 이벤트
    public event UnityAction<InputDeviceType> InputDeviceChangedEvent = delegate { };

    private InputSystem_Actions _inputActions;

    private IInputDeviceDetector _inputDeviceDetector;

    private void OnEnable()
    {
        if (_inputActions == null)
        {
            _inputActions = new InputSystem_Actions();
            _inputActions.Player.SetCallbacks(this);
        }
        EnablePlayerActions();

        _inputDeviceDetector = DIContainer.Instance.Resolve<IInputDeviceDetector>();
        // 의존성 주입이 완료된 후 입력 기기 감지 이벤트 등록
        if (_inputDeviceDetector != null)
        {
            _inputDeviceDetector.OnInputDeviceChanged.AddListener(OnInputDeviceChanged);
        }
    }

    private void OnDisable()
    {
        // 입력 기기 감지 이벤트 해제
        if (_inputDeviceDetector != null)
        {
            _inputDeviceDetector.OnInputDeviceChanged.RemoveListener(OnInputDeviceChanged);
        }
    }

    /// <summary>
    /// 입력 기기 변경 이벤트 처리
    /// </summary>
    /// <param name="deviceType">변경된 입력 기기 타입</param>
    private void OnInputDeviceChanged(InputDeviceType deviceType)
    {
        InputDeviceChangedEvent?.Invoke(deviceType);
    }

    public void EnablePlayerActions()
    {
        _inputActions.Player.Enable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context) { /* 필요시 구현 */ }

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

    #region Input Device Checkers
    /// <summary>
    /// 현재 입력 기기 타입을 가져오는 함수
    /// </summary>
    /// <returns>현재 활성화된 입력 기기 타입</returns>
    public InputDeviceType GetCurrentInputDevice()
    {
        return _inputDeviceDetector?.CurrentInputDevice ?? InputDeviceType.KeyboardMouse;
    }

    /// <summary>
    /// 현재 입력 기기가 키보드와 마우스인지 확인하는 함수
    /// </summary>
    /// <returns>키보드와 마우스 사용 여부</returns>
    public bool IsKeyboardMouse()
    {
        return _inputDeviceDetector?.IsKeyboardMouse() ?? true;
    }

    /// <summary>
    /// 현재 입력 기기가 게임패드인지 확인하는 함수
    /// </summary>
    /// <returns>게임패드 사용 여부</returns>
    public bool IsGamepad()
    {
        return _inputDeviceDetector?.IsGamepad() ?? false;
    }
    
    #endregion
}
