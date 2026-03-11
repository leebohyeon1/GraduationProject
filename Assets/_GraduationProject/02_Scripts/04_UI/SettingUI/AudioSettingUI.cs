using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;
using System;

public class AudioSettingUI : SettingPageUI
{
    [Serializable]
    public class AudioSettingRow
    {
        public string Name;
        public MMSoundManager.MMSoundManagerTracks Track;
        public Image FillImage;      // 슬라이더 대신 사용할 Image (Type: Filled)
        public GameObject Selector;  // 선택 시 활성화될 하이라이트 오브젝트
    }

    [Header("Settings")]
    [SerializeField] private InputReaderSO _inputReader;
    [SerializeField] private AudioSettingRow[] _rows;
    [SerializeField] private float _changeSpeed = 1.2f; // 음량 변화 속도

    private int _currentRowIndex = 0;
    private Vector2 _navigationInput;
    private bool _isVerticalInputCaptured = false;

    private void OnEnable()
    {
        if (_inputReader != null)
        {
            _inputReader.NavigateEvent += OnNavigate;
        }

        InitializeUI();
    }

    private void OnDisable()
    {
        if (_inputReader != null)
        {
            _inputReader.NavigateEvent -= OnNavigate;
        }
    }

    private void InitializeUI()
    {
        if (!MMSoundManager.HasInstance) return;

        foreach (var row in _rows)
        {
            float currentVol = MMSoundManager.Instance.GetTrackVolume(row.Track, false);
            row.FillImage.fillAmount = Mathf.Clamp01(currentVol);
        }

        UpdateSelectionVisual();
    }

    private void OnNavigate(Vector2 input)
    {
        _navigationInput = input;

        // 상하 입력 처리 (항목 이동)
        if (Mathf.Abs(input.y) > 0.6f)
        {
            if (!_isVerticalInputCaptured)
            {
                int dir = input.y > 0 ? -1 : 1;
                ChangeSelectedRow(dir);
                _isVerticalInputCaptured = true;
            }
        }
        else
        {
            _isVerticalInputCaptured = false;
        }
    }

    private void Update()
    {
        // 좌우 입력 처리 (음량 조절) - 누르고 있으면 연속 변화
        if (Mathf.Abs(_navigationInput.x) > 0.2f)
        {
            AdjustVolume(_navigationInput.x * _changeSpeed * Time.unscaledDeltaTime);
        }
    }

    private void ChangeSelectedRow(int direction)
    {
        _currentRowIndex = (_currentRowIndex + direction + _rows.Length) % _rows.Length;
        UpdateSelectionVisual();
    }

    private void AdjustVolume(float delta)
    {
        AudioSettingRow currentRow = _rows[_currentRowIndex];
        
        float newFillAmount = Mathf.Clamp01(currentRow.FillImage.fillAmount + delta);
        currentRow.FillImage.fillAmount = newFillAmount;

        if (MMSoundManager.HasInstance)
        {
            MMSoundManager.Instance.SetTrackVolume(currentRow.Track, newFillAmount);
            MMSoundManager.Instance.SaveSettings();
        }
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < _rows.Length; i++)
        {
            if (_rows[i].Selector != null)
            {
                _rows[i].Selector.SetActive(i == _currentRowIndex);
            }
        }
    }
}
