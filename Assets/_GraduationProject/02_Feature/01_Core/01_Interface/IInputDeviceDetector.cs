using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 입력 기기 타입 열거형
/// </summary>
public enum InputDeviceType
{
    /// <summary>
    /// 키보드와 마우스
    /// </summary>
    KeyboardMouse,
    
    /// <summary>
    /// 게임패드/컨트롤러
    /// </summary>
    Gamepad
}

/// <summary>
/// 입력 기기 감지 인터페이스
/// </summary>
public interface IInputDeviceDetector
{
    /// <summary>
    /// 현재 활성화된 입력 기기 타입
    /// </summary>
    public InputDeviceType CurrentInputDevice { get; }
    
    /// <summary>
    /// 입력 기기가 변경되었을 때 발생하는 이벤트
    /// </summary>
    public UnityEvent<InputDeviceType> OnInputDeviceChanged { get; }
    
    /// <summary>
    /// 입력 기기 감지를 시작하는 함수
    /// </summary>
    public void StartDetection();
    
    /// <summary>
    /// 입력 기기 감지를 중지하는 함수
    /// </summary>
    public void StopDetection();
    
    /// <summary>
    /// 현재 입력 기기가 키보드와 마우스인지 확인하는 함수
    /// </summary>
    /// <returns>키보드와 마우스 사용 여부</returns>
    public bool IsKeyboardMouse();
    
    /// <summary>
    /// 현재 입력 기기가 게임패드인지 확인하는 함수
    /// </summary>
    /// <returns>게임패드 사용 여부</returns>
    public bool IsGamepad();
}