using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class CutSceneSkip : MonoBehaviour
{
    [SerializeField] private PlayableDirector _playableDirector;

    [Header("Input & Settings")]
    [SerializeField] private InputReaderSO _inputReaderSO;
    [SerializeField] private float _holdTime = 1.0f;
    [SerializeField] private float _fadeOutSpeed = 2.0f; // 게이지가 줄어드는 속도
    [SerializeField] private float _spareTime = 0.5f; // 컷씬 끝에서 남겨둘 시간 (초)

    [Header("UI References")]
    [SerializeField] private CanvasGroup _uiCanvasGroup;
    [SerializeField] private Image _progressBar;
    
    private Coroutine _skipCoroutine;
    private Coroutine _fadeOutCoroutine;
    private float _currentProgress = 0f; // 0 ~ 1 사이의 진행도

    private void Awake()
    {
        if (_playableDirector == null)
        {
            _playableDirector = GetComponent<PlayableDirector>();
        }

        ResetUIImmediate();
    }

    private void OnEnable()
    {
        _inputReaderSO.SkipStartEvent += OnSkipStart;
        _inputReaderSO.SkipEndEvent += OnSkipEnd;
    }

    private void OnDisable()
    {
        _inputReaderSO.SkipStartEvent -= OnSkipStart;
        _inputReaderSO.SkipEndEvent -= OnSkipEnd;
        StopAllCoroutines();
    }

    private void OnSkipStart()
    {
        if (_playableDirector != null && _playableDirector.state == PlayState.Playing)
        {
            StopFadeOutCoroutine();
            StopSkipCoroutine();
            _skipCoroutine = StartCoroutine(SkipRoutine());
        }
    }

    private void OnSkipEnd()
    {
        StopSkipCoroutine();
        if (_currentProgress > 0)
        {
            _fadeOutCoroutine = StartCoroutine(FadeOutRoutine());
        }
    }

    private IEnumerator SkipRoutine()
    {
        SetUIVisibility(true);
        float elapsedTime = _currentProgress * _holdTime; // 현재 게이지 위치에서 시작

        while (elapsedTime < _holdTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            _currentProgress = Mathf.Clamp01(elapsedTime / _holdTime);
            
            UpdateUI();
            yield return null;
        }
        
        PerformSkip();
        ResetUIImmediate();
        _skipCoroutine = null;
    }

    private IEnumerator FadeOutRoutine()
    {
        while (_currentProgress > 0)
        {
            _currentProgress -= Time.unscaledDeltaTime * _fadeOutSpeed;
            _currentProgress = Mathf.Max(0, _currentProgress);
            
            UpdateUI();
            
            // UI 투명도도 게이지와 함께 서서히 줄어들게 함
            if (_uiCanvasGroup != null)
            {
                _uiCanvasGroup.alpha = Mathf.Min(1, _currentProgress * 2f); 
            }
            
            yield return null;
        }

        ResetUIImmediate();
        _fadeOutCoroutine = null;
    }

    private void UpdateUI()
    {
        if (_progressBar != null)
        {
            _progressBar.fillAmount = _currentProgress;
        }
    }

    private void PerformSkip()
    {
        if (_playableDirector != null)
        {
            _playableDirector.time = _playableDirector.duration - _spareTime;
            _playableDirector.Evaluate();
        }
    }

    private void StopSkipCoroutine()
    {
        if (_skipCoroutine != null)
        {
            StopCoroutine(_skipCoroutine);
            _skipCoroutine = null;
        }
    }

    private void StopFadeOutCoroutine()
    {
        if (_fadeOutCoroutine != null)
        {
            StopCoroutine(_fadeOutCoroutine);
            _fadeOutCoroutine = null;
        }
    }

    private void SetUIVisibility(bool isVisible)
    {
        if (_uiCanvasGroup != null)
        {
            _uiCanvasGroup.alpha = isVisible ? 1 : 0;
        }
    }

    private void ResetUIImmediate()
    {
        _currentProgress = 0f;
        SetUIVisibility(false);
        if (_progressBar != null)
        {
            _progressBar.fillAmount = 0;
        }
    }
}
