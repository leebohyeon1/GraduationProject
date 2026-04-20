using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class PlayerQuestUI : PlayerUIBase
{
    [SerializeField] private CanvasGroup _questCanvas;
    [SerializeField] private TMP_Text _questName;
    [SerializeField] private TMP_Text _questDiscriptionSummary;

    // 진행 중인 애니메이션을 관리하기 위한 시퀀스 변수
    private Sequence _questSequence;

    private void Awake()
    {
        _questCanvas.alpha = 0.0f;
    }

    public override void Initialize(PlayerController player)
    {
        base.Initialize(player);

        if(QuestManager.Instance != null)
        {
            QuestManager.Instance.QuestAccepted += OnQuestAccepted;
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        // 메모리 누수를 방지하기 위해 이벤트 구독 해제
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.QuestAccepted -= OnQuestAccepted;
        }

        // UI가 파괴될 때 실행 중인 트윈이 있다면 강제 종료
        _questSequence?.Kill();
    }

    private void OnQuestAccepted(QuestData data)
    {
        _questCanvas.alpha = 0.0f;
        _questName.text = data.Title;
        _questDiscriptionSummary.text = data.GoalText;

        // 이전에 실행 중이던 시퀀스가 있다면 종료 (애니메이션 중첩 방지)
        _questSequence?.Kill();

        // 새로운 시퀀스 생성
        _questSequence = DOTween.Sequence();

        _questSequence
            // 1. 0.5초 동안 불투명하게 (Alpha 1)
            .Append(_questCanvas.DOFade(1f, 1f))
            // 2. 2.0초 동안 불투명 상태 유지
            .AppendInterval(2f)
            // 3. 0.5초 동안 다시 투명하게 (Alpha 0)
            .Append(_questCanvas.DOFade(0f, 1f));
    }
}