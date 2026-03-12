using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;
using System;
using System.Collections;
using DG.Tweening; // DOTween 추가

public class AudioSettingUI : SettingPageUI
{
    [Serializable]
    public class AudioSettingRow
    {
        public string Name;
        public MMSoundManager.MMSoundManagerTracks Track;
        public Image FillImage;      
        public RectTransform HandleRect; 
        public GameObject Selector;  

        [HideInInspector] public RectTransform SelectorRect; 
        [HideInInspector] public Vector2 OriginalSize;       
    }

    [Header("Settings")]
    [SerializeField] private InputReaderSO _inputReader;
    [SerializeField] private AudioSettingRow[] _rows;
    [SerializeField] private float _changeSpeed = 1.2f; 

    [Header("Volume Acceleration")]
    [SerializeField] private float _accelerationRate = 2.0f; 
    [SerializeField] private float _maxMultiplier = 5.0f;    
    private float _currentMultiplier = 1.0f;                 

    [Header("Selector Animation")]
    [SerializeField] private Vector2 _selectedSize = new Vector2(370, 28);
    [SerializeField] private float _animationDuration = 0.2f; // 속도에서 시간(Duration)으로 변경

    private int _currentRowIndex = 0;
    private Vector2 _navigationInput;
    private bool _isVerticalInputCaptured = false;

    private void Awake()
    {
        foreach (var row in _rows)
        {
            if (row.Selector != null)
            {
                row.SelectorRect = row.Selector.GetComponent<RectTransform>();
                row.OriginalSize = row.SelectorRect.sizeDelta;
                row.Selector.SetActive(true);
            }
        }
    }

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

        foreach (var row in _rows)
        {
            row.SelectorRect?.DOKill();
        }
    }

    private void InitializeUI()
    {
        if (!MMSoundManager.HasInstance) return;

        foreach (var row in _rows)
        {
            float currentVol = MMSoundManager.Instance.GetTrackVolume(row.Track, false);
            row.FillImage.fillAmount = Mathf.Clamp01(currentVol);
            UpdateHandlePosition(row);
        }

        UpdateSelectionVisual();
    }

    private void OnNavigate(Vector2 input)
    {
        _navigationInput = input;

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
        if (Mathf.Abs(_navigationInput.x) > 0.2f)
        {
            _currentMultiplier = Mathf.Min(_currentMultiplier + _accelerationRate * Time.unscaledDeltaTime, _maxMultiplier);
            AdjustVolume(_navigationInput.x * _changeSpeed * _currentMultiplier * Time.unscaledDeltaTime);
        }
        else
        {
            _currentMultiplier = 1.0f;
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
        
        UpdateHandlePosition(currentRow);

        if (MMSoundManager.HasInstance)
        {
            MMSoundManager.Instance.SetTrackVolume(currentRow.Track, newFillAmount);
            MMSoundManager.Instance.SaveSettings();
        }
    }

    private void UpdateHandlePosition(AudioSettingRow row)
    {
        if (row.HandleRect == null || row.FillImage == null) return;

        float width = row.FillImage.rectTransform.rect.width;
        float xPos = width * row.FillImage.fillAmount;
        row.HandleRect.anchoredPosition = new Vector2(xPos, row.HandleRect.anchoredPosition.y);
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < _rows.Length; i++)
        {
            var row = _rows[i];
            if (row.SelectorRect == null) continue;

            Vector2 targetSize = (i == _currentRowIndex) ? _selectedSize : row.OriginalSize;
            
            // DOTween으로 크기 변경
            row.SelectorRect.DOKill();
            row.SelectorRect.DOSizeDelta(targetSize, _animationDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }
    }
}
