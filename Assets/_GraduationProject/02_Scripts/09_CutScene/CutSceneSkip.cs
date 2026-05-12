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
    
    [Header("UI Settings")]
    [SerializeField] private float _hintDuration = 2.0f; // UI가 떠있는 시간
    [SerializeField] private float _fadeTime = 0.5f; // 페이드 인/아웃에 걸리는 시간

    private Coroutine _skipCoroutine;
    private Coroutine _fadeOutCoroutine;
    private Coroutine _fadeInCoroutine;
    private Coroutine _hintCoroutine;
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
        _inputReaderSO.CutSceneAnyKeyEvent += OnAnyKey;
    }

    private void OnDisable()
    {
        _inputReaderSO.SkipStartEvent -= OnSkipStart;
        _inputReaderSO.SkipEndEvent -= OnSkipEnd;
        _inputReaderSO.CutSceneAnyKeyEvent -= OnAnyKey;
        StopAllCoroutines();
    }

    private void OnAnyKey()
    {
        if (_playableDirector != null && _playableDirector.state == PlayState.Playing)
        {
            // UI가 완전히 꺼져있을 때(알파가 0)이며, 실행 중인 루틴이 없을 때만 힌트 시작
            if (_uiCanvasGroup != null && _uiCanvasGroup.alpha <= 0f &&
                _skipCoroutine == null && _hintCoroutine == null && _fadeOutCoroutine == null && _fadeInCoroutine == null)
            {
                _hintCoroutine = StartCoroutine(HintRoutine());
            }
        }
    }

    private IEnumerator HintRoutine()
    {
        yield return StartCoroutine(FadeInRoutine());
        yield return new WaitForSecondsRealtime(_hintDuration);
        
        // 스킵 게이지가 차오르는 중이 아니라면 서서히 숨김
        if (_skipCoroutine == null)
        {
            _fadeOutCoroutine = StartCoroutine(FadeOutRoutine());
        }
        _hintCoroutine = null;
    }

    private void OnSkipStart()
    {
        if (_playableDirector != null && _playableDirector.state == PlayState.Playing)
        {
            StopFadeOutCoroutine();
            StopFadeInCoroutine();
            if (_hintCoroutine != null)
            {
                StopCoroutine(_hintCoroutine);
                _hintCoroutine = null;
            }
            StopSkipCoroutine();
            
            _fadeInCoroutine = StartCoroutine(FadeInRoutine());
            _skipCoroutine = StartCoroutine(SkipRoutine());
        }
    }

    private void OnSkipEnd()
    {
        StopSkipCoroutine();
        StopFadeInCoroutine();
        // 게이지가 있든 없든(힌트만 떠있든) 부드럽게 페이드 아웃 시작
        _fadeOutCoroutine = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator SkipRoutine()
    {
        float elapsedTime = _currentProgress * _holdTime; 

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

    private IEnumerator FadeInRoutine()
    {
        if (_uiCanvasGroup == null) yield break;

        float startAlpha = _uiCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < _fadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            _uiCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / _fadeTime);
            yield return null;
        }
        _uiCanvasGroup.alpha = 1f;
        _fadeInCoroutine = null;
    }

    private IEnumerator FadeOutRoutine()
    { 
        float startAlpha = (_uiCanvasGroup != null) ? _uiCanvasGroup.alpha : 0f;
        float elapsed = 0f;

        while (_currentProgress > 0 || (_uiCanvasGroup != null && _uiCanvasGroup.alpha > 0))
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;

            // 1. 게이지 감소
            if (_currentProgress > 0)
            {
                _currentProgress -= dt * _fadeOutSpeed;
                _currentProgress = Mathf.Max(0, _currentProgress);
                UpdateUI();
            }

            // 2. 투명도 감소
            if (_uiCanvasGroup != null)
            {
                _uiCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / _fadeTime);
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

    public void PerformSkip()
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

    private void StopFadeInCoroutine()
    {
        if (_fadeInCoroutine != null)
        {
            StopCoroutine(_fadeInCoroutine);
            _fadeInCoroutine = null;
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
