using UnityEngine;

/// <summary>
/// UI 관련 입력을 처리하고 상태를 관리하는 클래스입니다.
/// </summary>
public class UIInputHandler : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader _inputReader;

    // UI input state variables
    private bool _cancelInput;
    private Vector2 _navigateInput;
    private bool _submitInput;
    private bool _clickInput;
    private Vector2 _pointInput;
    private bool _rightClickInput;
    private bool _middleClickInput;
    private Vector2 _scrollWheelInput;

    #region Properties
    public bool CancelInput => _cancelInput;
    public Vector2 NavigateInput => _navigateInput;
    public bool SubmitInput => _submitInput;
    public bool ClickInput => _clickInput;
    public Vector2 PointInput => _pointInput;
    public bool RightClickInput => _rightClickInput;
    public bool MiddleClickInput => _middleClickInput;
    public Vector2 ScrollWheelInput => _scrollWheelInput;
    #endregion

    private void OnEnable()
    {
        if (_inputReader == null)
        {
            return;
        }

        // Subscribe to UI events
        _inputReader.CancelEvent += OnCancel;
        _inputReader.NavigateEvent += OnNavigate;
        _inputReader.SubmitEvent += OnSubmit;
        _inputReader.ClickEvent += OnClick;
        _inputReader.PointEvent += OnPoint;
        _inputReader.RightClickEvent += OnRightClick;
        _inputReader.MiddleClickEvent += OnMiddleClick;
        _inputReader.ScrollWheelEvent += OnScrollWheel;
    }

    private void OnDisable()
    {
        if (_inputReader == null)
        {
            return;
        }

        // Unsubscribe from UI events
        _inputReader.CancelEvent -= OnCancel;
        _inputReader.NavigateEvent -= OnNavigate;
        _inputReader.SubmitEvent -= OnSubmit;
        _inputReader.ClickEvent -= OnClick;
        _inputReader.PointEvent -= OnPoint;
        _inputReader.RightClickEvent -= OnRightClick;
        _inputReader.MiddleClickEvent -= OnMiddleClick;
        _inputReader.ScrollWheelEvent -= OnScrollWheel;
    }

    // Callback methods for UI inputs
    private void OnCancel() => _cancelInput = true;
    private void OnNavigate(Vector2 navigateInput) => _navigateInput = navigateInput;
    private void OnSubmit() => _submitInput = true;
    private void OnClick() => _clickInput = true;
    private void OnPoint(Vector2 pointInput) => _pointInput = pointInput;
    private void OnRightClick() => _rightClickInput = true;
    private void OnMiddleClick() => _middleClickInput = true;
    private void OnScrollWheel(Vector2 scrollInput) => _scrollWheelInput = scrollInput;

    /// <summary>
    /// Resets one-shot input flags at the end of the frame.
    /// Should be called by a manager class.
    /// </summary>
    public void LateTick()
    {
        _cancelInput = false;
        _submitInput = false;
        _clickInput = false;
        _rightClickInput = false;
        _middleClickInput = false;
    }
}