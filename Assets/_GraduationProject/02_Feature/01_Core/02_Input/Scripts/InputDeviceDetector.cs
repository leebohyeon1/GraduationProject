using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using BH_Lib.DI;

/// <summary>
/// Unity Input System을 활용한 입력 기기 감지 클래스
/// </summary>
[Register(typeof(IInputDeviceDetector), LifetimeScope.Singleton)]
public class InputDeviceDetector : DIMonoBehaviour, IInputDeviceDetector
{
    private InputDeviceType _currentInputDevice = InputDeviceType.KeyboardMouse;
    private bool _isDetectionActive = false;

    /// <summary>
    /// 현재 활성화된 입력 기기 타입
    /// </summary>
    public InputDeviceType CurrentInputDevice => _currentInputDevice;

    /// <summary>
    /// 입력 기기가 변경되었을 때 발생하는 이벤트
    /// </summary>
    public UnityEvent<InputDeviceType> OnInputDeviceChanged { get; private set; } = new UnityEvent<InputDeviceType>();

    protected override void Awake()
    {
        base.Awake();

        // 초기 입력 기기 설정
        DetectInitialInputDevice();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        StartDetection();
    }

    private void OnDisable()
    {
        StopDetection();
    }

    #region IInputDeviceDetector Implementation
    /// <summary>
    /// 입력 기기 감지를 시작하는 함수
    /// </summary>
    public void StartDetection()
    {
        if (_isDetectionActive) return;

        _isDetectionActive = true;

        // Input System 이벤트 등록
        InputSystem.onActionChange += OnActionChange;

    }

    /// <summary>
    /// 입력 기기 감지를 중지하는 함수
    /// </summary>
    public void StopDetection()
    {
        if (!_isDetectionActive) return;

        _isDetectionActive = false;

        // Input System 이벤트 해제
        InputSystem.onActionChange -= OnActionChange;
    }

    /// <summary>
    /// 현재 입력 기기가 키보드와 마우스인지 확인하는 함수
    /// </summary>
    /// <returns>키보드와 마우스 사용 여부</returns>
    public bool IsKeyboardMouse()
    {
        return _currentInputDevice == InputDeviceType.KeyboardMouse;
    }

    /// <summary>
    /// 현재 입력 기기가 게임패드인지 확인하는 함수
    /// </summary>
    /// <returns>게임패드 사용 여부</returns>
    public bool IsGamepad()
    {
        return _currentInputDevice == InputDeviceType.Gamepad;
    }

    /// <summary>
    /// 초기 입력 기기를 감지하는 함수
    /// </summary>
    private void DetectInitialInputDevice()
    {
        // 연결된 게임패드가 있는지 확인
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            SetCurrentInputDevice(InputDeviceType.Gamepad);
        }
        else
        {
            SetCurrentInputDevice(InputDeviceType.KeyboardMouse);
        }
    }

    /// <summary>
    /// Input System 액션 변경 이벤트 처리
    /// </summary>
    /// <param name="obj">액션 변경 객체</param>
    /// <param name="change">변경 타입</param>
    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            if (obj is InputAction action && action.activeControl != null)
            {
                InputDeviceType detectedDevice = DetectInputDeviceFromControl(action.activeControl);

                if (detectedDevice != _currentInputDevice)
                {
                    SetCurrentInputDevice(detectedDevice);
                }
            }
        }
    }

    /// <summary>
    /// 입력 컨트롤로부터 입력 기기 타입을 감지하는 함수
    /// </summary>
    /// <param name="control">입력 컨트롤</param>
    /// <returns>감지된 입력 기기 타입</returns>
    private InputDeviceType DetectInputDeviceFromControl(InputControl control)
    {
        // 게임패드 관련 입력인지 확인
        if (control.device is Gamepad)
        {
            return InputDeviceType.Gamepad;
        }

        // 키보드나 마우스 관련 입력인지 확인
        if (control.device is Keyboard || control.device is Mouse)
        {
            return InputDeviceType.KeyboardMouse;
        }

        // 기본값은 키보드&마우스
        return InputDeviceType.KeyboardMouse;
    }

    /// <summary>
    /// 현재 입력 기기를 설정하고 이벤트를 발생시키는 함수
    /// </summary>
    /// <param name="deviceType">설정할 입력 기기 타입</param>
    private void SetCurrentInputDevice(InputDeviceType deviceType)
    {
        if (_currentInputDevice != deviceType)
        {
            _currentInputDevice = deviceType;
            OnInputDeviceChanged?.Invoke(_currentInputDevice);
        }
    }
    
    #endregion
}