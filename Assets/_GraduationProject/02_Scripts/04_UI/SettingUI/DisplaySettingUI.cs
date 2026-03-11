using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DisplaySettingUI : SettingPageUI
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private TMP_Dropdown _screenModeDropdown;
    [SerializeField] private TMP_Dropdown _frameRateDropdown;
    [SerializeField] private Toggle _screenShakeToggle;

    private List<Resolution> _resolutions = new List<Resolution>();

    private void Start()
    {
        SetupResolutions();
        SetupScreenMode();
        SetupFrameRate();
        SetupScreenShake();
    }

    private void SetupResolutions()
    {
        if (_resolutionDropdown == null) return;

        _resolutions.Clear();
        _resolutionDropdown.ClearOptions();
        
        Resolution[] allResolutions = Screen.resolutions;
        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            string option = $"{allResolutions[i].width} x {allResolutions[i].height} @ {allResolutions[i].refreshRateRatio.value:F0}Hz";
            options.Add(option);

            if (allResolutions[i].width == Screen.currentResolution.width &&
                allResolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
            _resolutions.Add(allResolutions[i]);
        }

        _resolutionDropdown.AddOptions(options);
        _resolutionDropdown.value = currentResIndex;
        _resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void SetupScreenMode()
    {
        if (_screenModeDropdown == null) return;
        _screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
        _screenModeDropdown.value = Screen.fullScreen ? 0 : 1;
    }

    private void SetupFrameRate()
    {
        if (_frameRateDropdown == null) return;
        _frameRateDropdown.onValueChanged.AddListener(SetFrameRate);
        _frameRateDropdown.value = 1; // Default 60fps
    }

    private void SetupScreenShake()
    {
        if (_screenShakeToggle == null) return;
        _screenShakeToggle.isOn = PlayerPrefs.GetInt("ScreenShake", 1) == 1;
        _screenShakeToggle.onValueChanged.AddListener(SetScreenShake);
    }

    public void SetResolution(int index)
    {
        if (index < 0 || index >= _resolutions.Count) return;
        Resolution res = _resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
    }

    public void SetScreenMode(int index) => Screen.fullScreen = (index == 0);

    public void SetFrameRate(int index)
    {
        int[] rates = { 30, 60, 144, -1 };
        if (index >= 0 && index < rates.Length)
        {
            Application.targetFrameRate = rates[index];
        }
    }

    public void SetScreenShake(bool useShake)
    {
        PlayerPrefs.SetInt("ScreenShake", useShake ? 1 : 0);
        PlayerPrefs.Save();
    }
}
