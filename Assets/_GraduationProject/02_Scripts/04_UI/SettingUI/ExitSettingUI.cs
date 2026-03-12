using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ExitSettingUI : SettingPageUI
{
    [SerializeField] private InputReaderSO _inputReader; // 패드 입력을 위한 InputReader
    [SerializeField] private Button _quitToTitleButton;
    [SerializeField] private Button _quitToDesktopButton;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject _selectionIndicator; // 버튼 선택 시 따라다닐 오브젝트
    [SerializeField] private float _indicatorSpeed = 18f;    // 표시기 이동 속도

    private MenuSettingUI _parentUI;
    private Coroutine _indicatorCoroutine;

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

        // 페이지가 열릴 때 첫 번째 버튼을 자동으로 선택
        FocusFirstButton();
    }

    private void OnDisable()
    {
        if (_inputReader != null)
        {
            _inputReader.NavigateEvent -= OnNavigate;
        }

        if (_indicatorCoroutine != null) 
        {
            StopCoroutine(_indicatorCoroutine);
        }
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

        // 현재 이벤트 시스템에서 선택된 게임 오브젝트 가져오기
        GameObject current = EventSystem.current.currentSelectedGameObject;

        // 선택된 게 없거나 이 페이지의 버튼이 아니면 리턴
        if (current == null)
        { 
            return; 
        }
        if (current != _quitToTitleButton.gameObject && current != _quitToDesktopButton.gameObject)
        {
            return;
        }

        // 선택된 버튼이 이전에 표시기가 붙어있던 버튼과 다를 때만 애니메이션 실행
        if (_selectionIndicator.transform.parent != current.transform)
        {
            if (_indicatorCoroutine != null)
            {
                StopCoroutine(_indicatorCoroutine);
            }
            _indicatorCoroutine = StartCoroutine(AnimateIndicator(current.transform));
        }
    }

    private IEnumerator AnimateIndicator(Transform targetParent)
    {
        // 1. 부모를 변경하되 월드 좌표 유지 (부드러운 출발을 위해)
        _selectionIndicator.transform.SetParent(targetParent, true);
        
        RectTransform rect = _selectionIndicator.GetComponent<RectTransform>();
        Vector2 targetPos = Vector2.zero;

        // 시작 시 살짝 커지는 펀치 효과 (시각적 강조)
        _selectionIndicator.transform.localScale = Vector3.one * 1.15f;
        if (!_selectionIndicator.activeSelf)
        {
            _selectionIndicator.SetActive(true);
        }

        // 2. 목표 지점(중앙 0,0)까지 Lerp 이동
        if (rect != null)
        {
            while (Vector2.Distance(rect.anchoredPosition, targetPos) > 0.1f)
            {
                rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.unscaledDeltaTime * _indicatorSpeed);
                _selectionIndicator.transform.localScale = Vector3.Lerp(_selectionIndicator.transform.localScale, Vector3.one, Time.unscaledDeltaTime * _indicatorSpeed);
                yield return null;
            }
            rect.anchoredPosition = targetPos;
        }
        else
        {
            while (Vector3.Distance(_selectionIndicator.transform.localPosition, Vector3.zero) > 0.01f)
            {
                _selectionIndicator.transform.localPosition = Vector3.Lerp(_selectionIndicator.transform.localPosition, Vector3.zero, Time.unscaledDeltaTime * _indicatorSpeed);
                yield return null;
            }
            _selectionIndicator.transform.localPosition = Vector3.zero;
        }

        _selectionIndicator.transform.localScale = Vector3.one;
        _indicatorCoroutine = null;
    }

    private void OnNavigate(Vector2 input)
    {
        // 현재 선택된 오브젝트가 없을 경우 다시 포커스를 잡아줍니다 (패드 조작 안정성)
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            FocusFirstButton();
        }
    }

    private void OnQuitToTitle()
    {
        _parentUI.OnQuitToTitle();
    }

    private void OnQuitToDesktop()
    {
        _parentUI.OnQuitToDesktop();
    }
}
