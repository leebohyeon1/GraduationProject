using UnityEngine;
using DG.Tweening; // DOTween 사용을 위해 필수!

public class AttackTutorial : MonoBehaviour
{
    private bool _isTutorial = false;
    public GameObject tutorialUI;

    public InputReaderSO inputReader;

    private void Start()
    {
        // 시작할 때 튜토리얼 UI의 크기를 0으로 초기화하고 꺼둡니다.
        if (tutorialUI != null)
        {
            tutorialUI.transform.localScale = Vector3.zero;
            tutorialUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isTutorial)
        {
            return;
        }

        if (other.TryGetComponent(out PlayerController player))
        {
            _isTutorial = true;

            // 1. 이벤트 구독
            inputReader.InteractHoldEvent += CloseTutorial;


            // 3. UI 활성화 및 커지는 DOTween 연출 (SetUpdate(true)로 Unscaled Time 적용)
            tutorialUI.SetActive(true);
            tutorialUI.transform.DOScale(Vector3.one, 0.5f).SetUpdate(true).SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    // 애니메이션이 끝나면 게임 시간을 멈춥니다.
                    Time.timeScale = 0f;
                });
        }
    }

    public void CloseTutorial()
    {
        // 이벤트 구독은 닫기 버튼(혹은 키)을 누르자마자 해제해 줍니다. (중복 실행 방지)
        inputReader.InteractHoldEvent -= CloseTutorial;

        // 작아지는 DOTween 연출
        tutorialUI.transform.DOScale(Vector3.zero, 0.3f).SetUpdate(true).SetEase(Ease.InBack).OnComplete(() =>
        {
            // 애니메이션이 끝나면 UI를 끄고 게임 시간을 다시 흐르게 합니다.
            tutorialUI.SetActive(false);
            Time.timeScale = 1f;
        });
    }
}