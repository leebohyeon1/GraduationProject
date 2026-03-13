using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening; // DOTween 네임스페이스 추가

public class DisplaySettingUI : SettingPageUI
{
    // --- 설정 항목을 위한 추상 베이스 클래스 ---
    [System.Serializable]
    public abstract class SettingItem
    {
        [Header("UI References")]
        public TextMeshProUGUI valueText;
        public RectTransform leftArrow;
        public RectTransform rightArrow;
        public RectTransform focusTarget;

        [Header("Animation Settings")]
        protected float bumpDistance = 15f;
        protected float shakeDistance = 8f;
        protected float duration = 0.15f;

        protected MonoBehaviour owner;
        private Vector2 leftArrowInitialPos;
        private Vector2 rightArrowInitialPos;
        private bool initialized = false;

        public virtual void Initialize(MonoBehaviour owner)
        {
            if (initialized)
            {
                return;
            }

            this.owner = owner;
            if (leftArrow != null)
            {
                leftArrowInitialPos = leftArrow.anchoredPosition;
            }
            if (rightArrow != null)
            {
                rightArrowInitialPos = rightArrow.anchoredPosition;
            }
            initialized = true;
        }

        public abstract bool ChangeValue(int direction);
        public abstract void UpdateUI();

        public void PlayFeedback(bool isBoundary, int direction)
        {
            RectTransform arrow = (direction > 0) ? rightArrow : leftArrow;
            Vector2 initialPos = (direction > 0) ? rightArrowInitialPos : leftArrowInitialPos;
            if (arrow == null) return;

            // 기존 애니메이션 즉시 종료 및 초기 위치로 복구
            arrow.DOKill(true);
            arrow.anchoredPosition = initialPos;

            if (isBoundary)
            {
                // 초기 위치 기준 흔들림
                arrow.DOShakeAnchorPos(0.3f, new Vector2(shakeDistance, 0), 15, 90, false, true)
                    .SetUpdate(true);
            }
            else
            {
                // 초기 위치 기준으로 톡 튀어나옴 (Yoyo 루프로 초기 위치 복귀)
                arrow.DOAnchorPosX(initialPos.x + (direction * bumpDistance), duration)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);
            }
        }
    }

    // --- 구체적인 설정 항목 구현체들 ---

    [System.Serializable]
    public class ResolutionSetting : SettingItem {
        private List<Resolution> resolutions = new List<Resolution>();
        private int resIndex;

        public override void Initialize(MonoBehaviour owner) {
            base.Initialize(owner);
            resolutions.Clear();
            Resolution[] allRes = Screen.resolutions;
            
            // 저장된 해상도 값 로드 (없으면 현재 화면 해상도)
            int savedWidth = PlayerPrefs.GetInt("ResWidth", Screen.width);
            int savedHeight = PlayerPrefs.GetInt("ResHeight", Screen.height);

            for (int i = 0; i < allRes.Length; i++) {
                resolutions.Add(allRes[i]);
                if (allRes[i].width == savedWidth && allRes[i].height == savedHeight) resIndex = i;
            }
            UpdateUI();
        }

        public override bool ChangeValue(int dir) {
            int prev = resIndex;
            resIndex = Mathf.Clamp(resIndex + dir, 0, resolutions.Count - 1);
            if (prev == resIndex) return true;
            
            Resolution res = resolutions[resIndex];
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);

            // 설정값 저장
            PlayerPrefs.SetInt("ResWidth", res.width);
            PlayerPrefs.SetInt("ResHeight", res.height);
            PlayerPrefs.Save();

            UpdateUI();
            return false;
        }

        public override void UpdateUI() {
            if (valueText != null && resolutions.Count > 0) {
                Resolution res = resolutions[resIndex];
                valueText.text = $"{res.width} x {res.height}";
            }
        }
    }

    [System.Serializable]
    public class ScreenModeSetting : SettingItem {
        private readonly string[] modes = { "Full Screen", "Windowed" };
        private int modeIndex;

        public override void Initialize(MonoBehaviour owner) {
            base.Initialize(owner);
            // 저장된 화면 모드 로드 (0: Full Screen, 1: Windowed, 기본값 0)
            modeIndex = PlayerPrefs.GetInt("ScreenMode", Screen.fullScreen ? 0 : 1);
            UpdateUI();
        }

        public override bool ChangeValue(int dir) {
            int prev = modeIndex;
            modeIndex = Mathf.Clamp(modeIndex + dir, 0, modes.Length - 1);
            if (prev == modeIndex) return true;
            
            Screen.fullScreen = (modeIndex == 0);

            // 설정값 저장
            PlayerPrefs.SetInt("ScreenMode", modeIndex);
            PlayerPrefs.Save();

            UpdateUI();
            return false;
        }

        public override void UpdateUI() {
            if (valueText != null) valueText.text = modes[modeIndex];
        }
    }

    [System.Serializable]
    public class FrameRateSetting : SettingItem {
        private readonly string[] labels = { "30 FPS", "60 FPS", "144 FPS", "Unlimited" };
        private readonly int[] values = { 30, 60, 144, -1 };
        private int frameIndex;

        public override void Initialize(MonoBehaviour owner) {
            base.Initialize(owner);
            // 저장된 프레임 제한 로드 (기본값 60)
            int savedFrameRate = PlayerPrefs.GetInt("TargetFrameRate", 60);
            Application.targetFrameRate = savedFrameRate;

            frameIndex = 1; // Default 60
            for (int i = 0; i < values.Length; i++) if (values[i] == savedFrameRate) { frameIndex = i; break; }
            UpdateUI();
        }

        public override bool ChangeValue(int dir) {
            int prev = frameIndex;
            frameIndex = Mathf.Clamp(frameIndex + dir, 0, labels.Length - 1);
            if (prev == frameIndex) return true;
            
            int targetVal = values[frameIndex];
            Application.targetFrameRate = targetVal;

            // 설정값 저장
            PlayerPrefs.SetInt("TargetFrameRate", targetVal);
            PlayerPrefs.Save();

            UpdateUI();
            return false;
        }

        public override void UpdateUI() {
            if (valueText != null) valueText.text = labels[frameIndex];
        }
    }

    [System.Serializable]
    public class ScreenShakeSetting : SettingItem {
        private bool isShake;
        public override void Initialize(MonoBehaviour owner) {
            base.Initialize(owner);
            isShake = PlayerPrefs.GetInt("ScreenShake", 1) == 1;
            UpdateUI();
        }

        public override bool ChangeValue(int dir) {
            bool prev = isShake;
            if (dir > 0) isShake = false; else isShake = true;
            
            if (prev == isShake) return true;
            
            PlayerPrefs.SetInt("ScreenShake", isShake ? 1 : 0);
            PlayerPrefs.Save();
            UpdateUI();
            return false;
        }

        public override void UpdateUI() {
            if (valueText != null) valueText.text = isShake ? "On" : "Off";
        }
    }

    // --- 메인 DisplaySettingUI 클래스 로직 ---

    [Header("Input")]
    [SerializeField] private InputReaderSO _inputReader;

    [Header("Settings Items")]
    [SerializeField] private ResolutionSetting _resSetting;
    [SerializeField] private ScreenModeSetting _modeSetting;
    [SerializeField] private FrameRateSetting _frameSetting;
    [SerializeField] private ScreenShakeSetting _shakeSetting;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject _selectionIndicator;
    [SerializeField] private float _indicatorSpeed = 0.2f; // 초 단위로 변경 (DOTween용)

    private SettingItem[] _items;
    private SettingItem[] Items
    {
        get
        {
            if (_items == null)
            {
                _items = new SettingItem[] { _resSetting, _modeSetting, _frameSetting, _shakeSetting };
            }
            return _items;
        }
    }

    private int _currentIndex = 0;

    private void Awake() { }

    private void Start()
    {
        foreach (var item in Items) item.Initialize(this);
    }

    private void OnEnable()
    {
        if (_inputReader != null) _inputReader.NavigateEvent += OnNavigate;
        _currentIndex = 0;
        UpdateSelectionVisuals();
    }

    private void OnDisable()
    {
        if (_inputReader != null) _inputReader.NavigateEvent -= OnNavigate;
        _selectionIndicator?.transform.DOKill();
    }

    public override void OnPageOpen()
    {
        base.OnPageOpen();
        _currentIndex = 0;
        UpdateSelectionVisuals();
    }

    private void OnNavigate(Vector2 axis)
    {
        if (Mathf.Abs(axis.y) > 0.5f)
        {
            int prev = _currentIndex;
            _currentIndex = Mathf.Clamp(_currentIndex + (axis.y > 0 ? -1 : 1), 0, Items.Length - 1);
            if (prev != _currentIndex) UpdateSelectionVisuals();
        }
        else if (Mathf.Abs(axis.x) > 0.5f)
        {
            int dir = axis.x > 0 ? 1 : -1;
            bool isBoundary = Items[_currentIndex].ChangeValue(dir);
            Items[_currentIndex].PlayFeedback(isBoundary, dir);
        }
    }

    private void UpdateSelectionVisuals()
    {
        if (_selectionIndicator != null && Items[_currentIndex].focusTarget != null)
        {
            Transform target = Items[_currentIndex].focusTarget;
            
            _selectionIndicator.transform.DOKill();
            _selectionIndicator.transform.SetParent(target, true);

            // 이동 및 스케일 애니메이션
            _selectionIndicator.transform.localScale = Vector3.one * 1.15f;
            _selectionIndicator.transform.DOScale(Vector3.one, _indicatorSpeed)
                .SetUpdate(true);
                
            _selectionIndicator.transform.DOLocalMove(Vector3.zero, _indicatorSpeed)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);

            if (!_selectionIndicator.activeSelf) _selectionIndicator.SetActive(true);
        }
    }
}
