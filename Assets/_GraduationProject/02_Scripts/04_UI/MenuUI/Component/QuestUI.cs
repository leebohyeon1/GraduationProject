using TMPro;
using UnityEngine;

public class QuestUI : MenuUIComponent
{
    [Header("Quest Core Info")]
    [SerializeField] private TMP_Text[] _questTitleTexts;

    [Header("Quest Descriptions")]
    [SerializeField] private TMP_Text _summaryText;
    [SerializeField] private TMP_Text _detailText;
    [SerializeField] private TMP_Text _goalText;

    private void Start()
    {
        // 이벤트 구독
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.QuestAccepted += UpdateUI;

            // 현재 진행 중인 퀘스트가 있다면 초기 표시
            UpdateUI(QuestManager.Instance.CurrentQuestData);
        }
    }

    private void OnDestroy()
    {
        // 이벤트 해제 (메모리 누수 방지)
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.QuestAccepted -= UpdateUI;
        }
    }

    /// <summary>
    /// 전달받은 QuestData의 주요 정보를 UI에 표시합니다.
    /// </summary>
    /// <param name="data">표시할 퀘스트 데이터</param>
    public void UpdateUI(QuestData data)
    {
        if (data == null)
        {
            ClearUI();
            return;
        }

        // ID와 제목 설정
        foreach(var titleText in _questTitleTexts)
        {
            titleText.text = data.Title;
        }

        // 요약, 상세 설명, 목표 텍스트 설정
        if (_summaryText != null)
        {
            _summaryText.text = data.DescriptionSummary;
        }

        if (_detailText != null)
        {
            _detailText.text = data.DescriptionDetail;
        }

        if (_goalText != null)
        {
            _goalText.text = data.GoalText;

        }
    }

    /// <summary>
    /// UI 내용을 초기화합니다.
    /// </summary>
    public void ClearUI()
    {
        foreach (var titleText in _questTitleTexts)
        {
            titleText.text = "";
        }

        if (_summaryText != null)
        {
            _summaryText.text = string.Empty;
        }

        if (_detailText != null)
        {
            _detailText.text = string.Empty;
        }

        if (_goalText != null)
        {
            _goalText.text = string.Empty;
        }

    }
}
