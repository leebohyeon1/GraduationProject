using System;
using UnityEngine;
using DG.Tweening; // DOTween을 사용하기 위해 반드시 추가해야 합니다!

public class PotionTutorial : MonoBehaviour
{
    private bool _isTutorial = false;
    public GameObject TutorialUI;
    private PlayerController PlayerController;

    private void Start()
    {
        PlayerController = FindFirstObjectByType<PlayerController>();
        PlayerController.Health.OnHealthChanged += ShowTutorial;

        // 시작할 때 UI의 크기를 0으로 초기화하고 꺼둡니다.
        if (TutorialUI != null)
        {
            TutorialUI.transform.localScale = Vector3.zero;
            TutorialUI.SetActive(false);
        }
    }

    private void ShowTutorial(int arg1, int arg2)
    {
        if (_isTutorial)
        {
            return;
        }

        _isTutorial = true;
        PlayerController.InputReader.InteractHoldEvent += CloseTutorial;

        // 2. UI를 활성화합니다. (현재 크기는 0인 상태)
        TutorialUI.SetActive(true);

        // 3. DOTween 애니메이션 재생 (크기가 0에서 1로 커짐)
        // SetUpdate(true) : TimeScale=0 이어도 애니메이션이 정상 작동하게 합니다.
        // SetEase(Ease.OutBack) : 끝날 때 살짝 튕기는 찰진 연출을 줍니다.
        TutorialUI.transform.DOScale(Vector3.one, 0.5f).SetUpdate(true).SetEase(Ease.OutBack)
                        .OnComplete(() => {
                            Time.timeScale = 0f;
                        });
    }

    private void CloseTutorial()
    {
        PlayerController.Health.OnHealthChanged -= ShowTutorial;
        PlayerController.InputReader.InteractHoldEvent -= CloseTutorial;

        // 1. 닫힐 때도 DOTween으로 크기가 작아지는 연출을 줍니다.
        TutorialUI.transform.DOScale(Vector3.zero, 0.3f).SetUpdate(true).SetEase(Ease.InBack).OnComplete(() =>
        {
            // 2. OnComplete는 애니메이션이 완전히 끝난 직후에 실행됩니다.
            // UI가 다 작아지면 끄고, 시간을 다시 흐르게 만듭니다.
            TutorialUI.SetActive(false);
            Time.timeScale = 1f;
        });
    }
}