using UnityEngine;
using DG.Tweening; // DOTween 필수!

public class MenuiTutorial : MonoBehaviour
{
    // 메뉴 튜토리얼이 이미 켜져 있는지 확인하기 위한 변수를 하나 추가했습니다.
    private bool _isTutorial = false;

    public GameObject tutorialUI;
    public InputReaderSO inputReader;

    private void Start()
    {
        // 시작 시 크기를 0으로 맞추고 비활성화합니다.
        if (tutorialUI != null)
        {
            tutorialUI.transform.localScale = Vector3.zero;
            tutorialUI.SetActive(false);
        }
    }

    public void Show()
    {
        // 이미 튜토리얼이 켜져 있다면 중복 실행되지 않도록 막습니다.
        if (_isTutorial) return;
        _isTutorial = true;

        // 1. 이벤트 구독 (상호작용 키를 누르면 Close 함수 실행)
        inputReader.InteractHoldEvent += Close;
        // 3. UI 켜기 및 DOTween 팝업 애니메이션
        tutorialUI.SetActive(true);
        tutorialUI.transform.DOScale(Vector3.one, 0.5f).SetUpdate(true).SetEase(Ease.OutBack)
            .OnComplete(()=> {
                Time.timeScale = 0f;
            });
    }

    public void Close()
    {
        // 닫기가 두 번 실행되지 않도록 이벤트 구독을 먼저 해제합니다.
        inputReader.InteractHoldEvent -= Close;

        // DOTween 닫기 애니메이션
        tutorialUI.transform.DOScale(Vector3.zero, 0.3f).SetUpdate(true).SetEase(Ease.InBack).OnComplete(() =>
        {
            // 애니메이션이 끝나면 UI를 끄고, 시간을 흐르게 하고, 튜토리얼 상태를 해제합니다.
            tutorialUI.SetActive(false);
            Time.timeScale = 1f;
        });
    }
}