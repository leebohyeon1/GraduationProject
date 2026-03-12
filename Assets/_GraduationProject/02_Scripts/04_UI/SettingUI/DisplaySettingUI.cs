using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class DisplaySettingUI : SettingPageUI
{
    [Header("Input")]
    [SerializeField] private InputReaderSO _inputReader;

    [Header("Visual Elements (Texts)")]
    [SerializeField] private TextMeshProUGUI _resolutionText;
    [SerializeField] private TextMeshProUGUI _screenModeText;
    [SerializeField] private TextMeshProUGUI _frameRateText;
    [SerializeField] private TextMeshProUGUI _screenShakeText;

    [Header("Selectable Buttons")]
    [SerializeField] private Button _resolutionButton;
    [SerializeField] private Button _screenModeButton;
    [SerializeField] private Button _frameRateButton;
    [SerializeField] private Button _screenShakeButton;

    private List<Resolution> _resolutions = new List<Resolution>();
    private int _resIndex;
    private int _modeIndex;
    private int _frameIndex;
    private bool _isScreenShake;

    private readonly string[] _screenModes = { "Full Screen", "Windowed" };
    private readonly string[] _frameRates = { "30 FPS", "60 FPS", "144 FPS", "Unlimited" };
    private readonly int[] _frameRateValues = { 30, 60, 144, -1 };

    private void Start()
    {
        InitializeSettings();
    }

    private void OnEnable()
    {
        if (_inputReader != null)
        {
            _inputReader.NavigateEvent += OnNavigate;
        }
        FocusFirstButton();
    }

    private void OnDisable()
    {
        if (_inputReader != null)
        {
            _inputReader.NavigateEvent -= OnNavigate;
        }
    }

    public override void OnPageOpen()
    {
        base.OnPageOpen();
        FocusFirstButton();
    }

    private void FocusFirstButton()
    {
        if (_resolutionButton != null)
        {
            _resolutionButton.Select();
        }
    }

    private void InitializeSettings()
    {
        // 1. 해상도 목록 초기화 및 현재 설정 찾기
        _resolutions.Clear();
        // Screen.resolutions는 모든 가능한 해상도를 가져옵니다.
        Resolution[] allRes = Screen.resolutions;
        
        if (allRes.Length > 0)
        {
            for (int i = 0; i < allRes.Length; i++)
            {
                _resolutions.Add(allRes[i]);
                if (allRes[i].width == Screen.width && allRes[i].height == Screen.height)
                {
                    _resIndex = i;
                }
            }
        }

        // 2. 화면 모드 초기화
        _modeIndex = Screen.fullScreen ? 0 : 1;

        // 3. 프레임 제한 초기화 (저장된 값이 없으면 60 FPS)
        int currentRate = Application.targetFrameRate;
        _frameIndex = 1; // Default 60
        for (int i = 0; i < _frameRateValues.Length; i++)
        {
            if (_frameRateValues[i] == currentRate)
            {
                _frameIndex = i;
                break;
            }
        }

        // 4. 화면 흔들림 초기화
        _isScreenShake = PlayerPrefs.GetInt("ScreenShake", 1) == 1;

        UpdateAllUI();
    }

    private void OnNavigate(Vector2 axis)
    {
        // 상하 이동은 EventSystem이 처리하므로, 여기서는 좌우 입력만 체크합니다.
        if (Mathf.Abs(axis.x) < 0.5f) return;

        GameObject current = EventSystem.current.currentSelectedGameObject;
        if (current == null) return;

        int direction = axis.x > 0 ? 1 : -1;

        if (current == _resolutionButton.gameObject)
            ChangeResolution(direction);
        else if (current == _screenModeButton.gameObject)
            ChangeScreenMode(direction);
        else if (current == _frameRateButton.gameObject)
            ChangeFrameRate(direction);
        else if (current == _screenShakeButton.gameObject)
            ChangeScreenShake(direction);
    }

    private void ChangeResolution(int dir)
    {
        if (_resolutions.Count == 0) return;
        _resIndex = (_resIndex + dir + _resolutions.Count) % _resolutions.Count;
        
        Resolution res = _resolutions[_resIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
        
        UpdateResolutionUI();
    }

    private void ChangeScreenMode(int dir)
    {
        _modeIndex = (_modeIndex + dir + _screenModes.Length) % _screenModes.Length;
        Screen.fullScreen = (_modeIndex == 0);
        
        UpdateScreenModeUI();
    }

    private void ChangeFrameRate(int dir)
    {
        _frameIndex = (_frameIndex + dir + _frameRates.Length) % _frameRates.Length;
        Application.targetFrameRate = _frameRateValues[_frameIndex];
        
        UpdateFrameRateUI();
    }

    private void ChangeScreenShake(int dir)
    {
        // 토글 방식이지만 좌우 입력 모두 반전 처리
        _isScreenShake = !_isScreenShake;
        PlayerPrefs.SetInt("ScreenShake", _isScreenShake ? 1 : 0);
        PlayerPrefs.Save();
        
        UpdateScreenShakeUI();
    }

    private void UpdateAllUI()
    {
        UpdateResolutionUI();
        UpdateScreenModeUI();
        UpdateFrameRateUI();
        UpdateScreenShakeUI();
    }

    private void UpdateResolutionUI()
    {
        if (_resolutionText != null && _resolutions.Count > 0)
        {
            Resolution res = _resolutions[_resIndex];
            _resolutionText.text = $"{res.width} x {res.height} @ {res.refreshRateRatio.value:F0}Hz";
        }
    }

    private void UpdateScreenModeUI()
    {
        if (_screenModeText != null)
            _screenModeText.text = _screenModes[_modeIndex];
    }

    private void UpdateFrameRateUI()
    {
        if (_frameRateText != null)
            _frameRateText.text = _frameRates[_frameIndex];
    }

    private void UpdateScreenShakeUI()
    {
        if (_screenShakeText != null)
            _screenShakeText.text = _isScreenShake ? "On" : "Off";
    }
}
