using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening; // DOTween 추가

public class ExitSettingUI : SettingPageUI
{
    [SerializeField] private InputReaderSO _inputReader;
    [SerializeField] private Button _quitToTitleButton;
    [SerializeField] private Button _quitToDesktopButton;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject _selectionIndicator;
    [SerializeField] private float _indicatorDuration = 0.2f; // 속도에서 시간으로 변경

    private MenuSettingUI _parentUI;

    private void Awake()
    {
        _parentUI = GetComponentInParent<MenuSettingUI>();
    }

    private void Start()
    {
        if (_parentUI == null) return;

        if (_quitToTitleButton != null)
        {
            _quitToTitleButton.onClick.AddListener(OnQuitToTitle);
        }

        if (_quitToDesktopButton != null)
        {
            _quitToDesktopButton.onClick.AddListener(OnQuitToDesktop);
        }
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

        _selectionIndicator?.transform.DOKill();
    }

    private void Update()
    {
        HandleIndicatorMovement();
    }

    public override void OnPageOpen()
    {
        base.OnPageOpen();
        FocusFirstButton();
    }

    private void FocusFirstButton()
    {
        if (_quitToTitleButton != null)
        {
            _quitToTitleButton.Select();
        }
    }

    private void HandleIndicatorMovement()
    {
        if (_selectionIndicator == null) return;

        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current == null) return;
        if (current != _quitToTitleButton.gameObject && current != _quitToDesktopButton.gameObject) return;

        // 부모가 다를 때만 새로운 애니메이션 실행
        if (_selectionIndicator.transform.parent != current.transform)
        {
            Transform target = current.transform;
            
            _selectionIndicator.transform.DOKill();
            _selectionIndicator.transform.SetParent(target, true);

            if (!_selectionIndicator.activeSelf) _selectionIndicator.SetActive(true);

            // 스케일 펀치 효과
            _selectionIndicator.transform.localScale = Vector3.one * 1.15f;
            _selectionIndicator.transform.DOScale(Vector3.one, _indicatorDuration)
                .SetUpdate(true);

            // 목표 지점(0,0)으로 이동
            _selectionIndicator.transform.DOLocalMove(Vector3.zero, _indicatorDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
    }

    private void OnNavigate(Vector2 input)
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            FocusFirstButton();
        }
    }

    private void OnQuitToTitle() => _parentUI.OnQuitToTitle();
    private void OnQuitToDesktop() => _parentUI.OnQuitToDesktop();
}
