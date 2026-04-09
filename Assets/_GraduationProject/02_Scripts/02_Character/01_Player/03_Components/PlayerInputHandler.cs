using System;
using UnityEngine;

/// <summary>
/// 플레이어의 입력을 처리하고 관련 이벤트를 발생시키는 클래스입니다. (Unity Input System 사용)
/// </summary>
public class PlayerInputHandler : MonoBehaviour, IDisposable
{
    [Header("Input")]
    [SerializeField] private InputReaderSO _inputReader; // 입력 처리기
    private PlayerEvents _events;

    // 입력 상태 변수
    private Vector3 _moveInput; // 이동 입력
    private Vector3 _mousePosition; // 마우스 위치 (키보드/마우스)

    private Vector2 _lockOnTargetChangeVector2Input; // 락온 타겟 변경 벡터 입력

    private InputDeviceType _currentInputDevice;    // 현재 입력 디바이스
    private bool _canBufferInput = false;           // 선입력 가능 여부

    #region Properties
    public Vector3 MoveInput => _moveInput;
    public Vector3 MousePosition => _mousePosition;

    public Vector2 LockOnTargetChangeVector2Input => _lockOnTargetChangeVector2Input;

    public InputDeviceType CurrentInputDevice => _currentInputDevice;
    public bool CanBufferInput => _canBufferInput;  
    #endregion

    /// <summary>
    /// 클래스 초기화
    /// </summary>
    /// <param name="player">플레이어</param>
    public void Initialize(PlayerController player)
    {
        _inputReader = player.InputReader;
        _events = player.Events;

        // 이벤트 등록
        InputDeviceDetector.Instance.InputDeviceChanged.AddListener(OnInputDeviceChanged);

        // 이벤트 구독
        _inputReader.MoveEvent += OnMove;
        _inputReader.MousePositionEvent += OnMousePosition;

        _events.BufferInputStarted += OnBufferInputStarted;
        _events.BufferInputEnded += OnBufferInputEnded;

        // 이벤트 해제 구독
        player.RegisterDisposable(this);

        _inputReader.SetInputMode(InputReaderSO.InputMode.Gameplay);
    }

    /// <summary>
    /// 클래스 해제
    /// </summary>
    public void Dispose()
    {
        InputDeviceDetector.Instance.InputDeviceChanged.RemoveListener(OnInputDeviceChanged);

        // 이벤트 구독 해제
        _inputReader.MoveEvent -= OnMove;
        _inputReader.MousePositionEvent -= OnMousePosition;

        _events.BufferInputStarted -= OnBufferInputStarted;
        _events.BufferInputEnded -= OnBufferInputEnded;
    }

    /// <summary>
    /// 입력 장치 변경 시 호출됩니다.
    /// </summary>
    private void OnInputDeviceChanged(InputDeviceType deviceType)
    {
        _currentInputDevice = deviceType;

        if(_currentInputDevice == InputDeviceType.KeyboardMouse)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if(_currentInputDevice == InputDeviceType.Gamepad)
        {
            Cursor.visible = false;
        }
    }

    // 각 입력 이벤트에 대한 콜백 함수들
    private void OnMove(Vector2 moveInput)
    {
        _moveInput = new Vector3(moveInput.x, 0, moveInput.y);
    }

    private void OnMousePosition(Vector2 mousePosition)
    {
        _mousePosition = new Vector3(mousePosition.x, 0, mousePosition.y);
    }

    /// <summary>
    /// 선입력 시작
    /// </summary>
    private void OnBufferInputStarted()
    {
        _canBufferInput = true;
    }

    /// <summary>
    /// 선입력 종료
    /// </summary>
    private void OnBufferInputEnded()
    {
        _canBufferInput = false;
    }
}