using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 돈 UI
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class PlayerMoneyUI : PlayerUIBase
{
    [Header("References")]
    [SerializeField] private Image _moneyBar;                // 돈 UI 이미지
    [SerializeField] private TMP_Text _moneyText;            // 현재 소지금 Text
    [SerializeField] private TMP_Text _changeText;           // 증감량 표시 Text (새로 추가됨)

    [SerializeField] private CanvasGroup _canvasGroup;
    private Coroutine _doneCoroutine;

    [Header("Settings")]
    [SerializeField] private float _fadeDuration = 0.5f;     // UI 전체 페이드 시간
    [SerializeField] private float _changeTextDuration = 1.0f; // 증감 텍스트 떠오르는 시간
    [SerializeField] private Vector2 _changeTextOffset = new Vector2(0, 50f); // 텍스트가 떠오를 높이

    private int _cachedMoney; // 실제 돈의 논리적 값을 기억할 변수
    private Vector3 _originChangeTextPos; // 증감 텍스트의 초기 위치 저장

    public override void Initialize(PlayerController player)
    {
        base.Initialize(player);
        p_player = player;

        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        // 증감 텍스트 초기 위치 기억
        if (_changeText != null)
        {
            _originChangeTextPos = _changeText.transform.localPosition;
            _changeText.alpha = 0f; // 시작할 땐 안 보이게
        }

        p_player.Money.MoneyChanged += OnMoneyChanged;

        // 초기 금액 설정 및 캐싱
        _cachedMoney = p_player.Money.CurrentMoney;
        SetMoneyText(_cachedMoney);

        // 초기 상태: 꺼짐
        _canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        _doneCoroutine = null;
    }

    public override void Dispose()
    {
        DOTween.Kill(this);
        // 증감 텍스트에 걸린 트윈도 확실히 제거하기 위해 ID로 킬 할수도 있지만, 
        // Kill(this)는 타겟이 'this'인 스크립트가 아니라 이 컴포넌트가 붙은 GameObject 관련 트윈을 끄는 경우가 많으므로
        // 안전하게 _changeText도 킬 해줍니다.
        if (_changeText != null)
        {
            DOTween.Kill(_changeText);
        }

        if (p_player != null)
        {
            p_player.Money.MoneyChanged -= OnMoneyChanged;
        }
    }

    private void OnMoneyChanged(int amount)
    {
        // 1. 기존 트윈/코루틴 정리
        DOTween.Kill(this);
        if (_changeText != null)
        {
            DOTween.Kill(_changeText); // 증감 텍스트 애니메이션 리셋
        }

        if (_doneCoroutine != null)
        {
            StopCoroutine(_doneCoroutine);
            _doneCoroutine = null;
        }

        // 2. 차액 계산 및 증감 텍스트 연출
        int difference = amount - _cachedMoney; // 변화량 계산
        _cachedMoney = amount; // 현재 금액 갱신

        ShowChangeText(difference);

        // 3. 메인 UI 활성화 및 페이드 인
        gameObject.SetActive(true);
        _canvasGroup.DOFade(1f, _fadeDuration).SetId(this).SetUpdate(true)
            .OnComplete(() =>
            {
                // 4. 숫자 카운팅 애니메이션
                int start = int.Parse(_moneyText.text);
                DOTween.To(
                    () => start,
                    x => SetMoneyText((int)x),
                    amount, 1f)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true)
                    .SetId(this)
                    .OnComplete(() =>
                    {
                        _doneCoroutine = StartCoroutine(MoneyChangeDone());
                    });
            });
    }

    /// <summary>
    /// 얼마가 변했는지 보여주는 텍스트 애니메이션
    /// </summary>
    private void ShowChangeText(int diff)
    {
        if (_changeText == null || diff == 0)
        {
            return;
        }

        // 1. 텍스트 내용 및 색상 설정
        if (diff > 0)
        {
            _changeText.text = $"+{diff}"; // +100 (천단위 콤마 포함)
        }
        else
        {
            _changeText.text = $"{diff}";  // -100
        }

        // 2. 위치 및 알파값 리셋 (연속 획득 시 제자리로 돌리기 위해)
        _changeText.transform.localPosition = _originChangeTextPos;
        _changeText.alpha = 1f;
        _changeText.gameObject.SetActive(true);

        // 3. 애니메이션: 위로 이동하면서 투명해짐
        // 이동
        _changeText.transform.DOLocalMove(_originChangeTextPos + (Vector3)_changeTextOffset, _changeTextDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .SetId(_changeText); // ID를 컴포넌트로 주어 개별 관리

        // 페이드 아웃
        _changeText.DOFade(0f, _changeTextDuration)
            .SetEase(Ease.InQuad)
            .SetUpdate(true)
            .SetId(_changeText);
    }

    private void SetMoneyText(int amount)
    {
        _moneyText.text = amount.ToString(); 
    }

    private IEnumerator MoneyChangeDone()
    {
        yield return new WaitForSeconds(2f);

        _canvasGroup.DOFade(0f, _fadeDuration)
            .SetId(this)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });

        _doneCoroutine = null;
    }
}