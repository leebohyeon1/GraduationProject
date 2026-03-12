using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;
using System;
using System.Collections;

public class AudioSettingUI : SettingPageUI
{
    [Serializable]
    public class AudioSettingRow
    {
        public string Name;
        public MMSoundManager.MMSoundManagerTracks Track;
        public Image FillImage;      // 슬라이더 대신 사용할 Image (Type: Filled)
        public RectTransform HandleRect; // 핸들 아이콘/이미지의 RectTransform
        public GameObject Selector;  // 선택 시 하이라이트 오브젝트

        [HideInInspector] public RectTransform SelectorRect; // 캐싱된 RectTransform
        [HideInInspector] public Vector2 OriginalSize;       // 초기 크기 저장
        public Coroutine SizeCoroutine;                      // 개별 애니메이션 관리
    }

    [Header("Settings")]
    [SerializeField] private InputReaderSO _inputReader;
    [SerializeField] private AudioSettingRow[] _rows;
    [SerializeField] private float _changeSpeed = 1.2f; // 기본 음량 변화 속도

    [Header("Volume Acceleration")]
    [SerializeField] private float _accelerationRate = 2.0f; // 가속도 증가 속도 (초당 배율 증가량)
    [SerializeField] private float _maxMultiplier = 5.0f;    // 최대 가속 배율
    private float _currentMultiplier = 1.0f;                 // 현재 가속 배율

    [Header("Selector Animation")]
    [SerializeField] private Vector2 _selectedSize = new Vector2(370, 28);
    [SerializeField] private float _animationSpeed = 15f; // 애니메이션 속도

    private int _currentRowIndex = 0;
    private Vector2 _navigationInput;
    private bool _isVerticalInputCaptured = false;

    private void Awake()
    {
        // 초기 설정 및 데이터 캐싱
        foreach (var row in _rows)
        {
            if (row.Selector != null)
            {
                row.SelectorRect = row.Selector.GetComponent<RectTransform>();
                row.OriginalSize = row.SelectorRect.sizeDelta;
                
                // 모든 Selector를 항상 켜둠
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

        // 실행 중인 모든 애니메이션 정지
        foreach (var row in _rows)
        {
            if (row.SizeCoroutine != null) StopCoroutine(row.SizeCoroutine);
        }
    }

    private void InitializeUI()
    {
        if (!MMSoundManager.HasInstance) return;

        foreach (var row in _rows)
        {
            float currentVol = MMSoundManager.Instance.GetTrackVolume(row.Track, false);
            row.FillImage.fillAmount = Mathf.Clamp01(currentVol);
            UpdateHandlePosition(row); // 초기 위치 설정
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
        // 좌우 입력 처리 (음량 조절)
        if (Mathf.Abs(_navigationInput.x) > 0.2f)
        {
            // 누르고 있는 동안 배율을 서서히 높입니다.
            _currentMultiplier = Mathf.Min(_currentMultiplier + _accelerationRate * Time.unscaledDeltaTime, _maxMultiplier);
            
            // 기존 속도에 가속 배율을 곱해 적용합니다.
            AdjustVolume(_navigationInput.x * _changeSpeed * _currentMultiplier * Time.unscaledDeltaTime);
        }
        else
        {
            // 입력을 떼면 가속 배율을 1.0(기본값)으로 초기화합니다.
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
        
        UpdateHandlePosition(currentRow); // 음량 변경 시 핸들 위치 업데이트

        if (MMSoundManager.HasInstance)
        {
            MMSoundManager.Instance.SetTrackVolume(currentRow.Track, newFillAmount);
            MMSoundManager.Instance.SaveSettings();
        }
    }

    /// <summary>
    /// FillImage의 fillAmount에 따라 핸들의 위치를 이동시킵니다.
    /// </summary>
    private void UpdateHandlePosition(AudioSettingRow row)
    {
        if (row.HandleRect == null || row.FillImage == null) return;

        // FillImage의 Rect 크기를 기준으로 fillAmount를 곱해 X 좌표를 계산합니다.
        float width = row.FillImage.rectTransform.rect.width;
        float xPos = width * row.FillImage.fillAmount;

        // 핸들의 anchoredPosition X값을 업데이트합니다. (Y값은 기존 유지)
        row.HandleRect.anchoredPosition = new Vector2(xPos, row.HandleRect.anchoredPosition.y);
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < _rows.Length; i++)
        {
            var row = _rows[i];
            if (row.SelectorRect == null) continue;

            // 이전 애니메이션 중지 후 새로운 목표 크기로 애니메이션 시작
            if (row.SizeCoroutine != null) StopCoroutine(row.SizeCoroutine);

            Vector2 targetSize = (i == _currentRowIndex) ? _selectedSize : row.OriginalSize;
            row.SizeCoroutine = StartCoroutine(AnimateSize(row.SelectorRect, targetSize));
        }
    }

    private IEnumerator AnimateSize(RectTransform target, Vector2 targetSize)
    {
        // 목표 크기에 도달할 때까지 부드럽게 보간(Lerp)
        while (Vector2.Distance(target.sizeDelta, targetSize) > 0.1f)
        {
            target.sizeDelta = Vector2.Lerp(target.sizeDelta, targetSize, Time.unscaledDeltaTime * _animationSpeed);
            yield return null;
        }
        target.sizeDelta = targetSize;
    }
}
