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
    [SerializeField] private float _reminderDelay = 75f;
    [SerializeField] private float _reminderCooldown = 120f;
    private float _walkingTime, _travel, _lastReminder = -120f;
    private Vector3 _lastPosition, _origin;
    private GameObject _menuHint;
    private int _trackedQuest = -1;

    private void Update()
    {
        if(!p_player || !QuestManager.Instance) return;
        var quest=QuestManager.Instance.CurrentQuestData;
        Vector3 position=p_player.transform.position;
        if(quest==null || quest.ID!=_trackedQuest) { _trackedQuest=quest==null?-1:quest.ID; ResetWandering(position); return; }
        if(!p_player.InputReader || p_player.InputReader.CurrentInputMode!=InputReaderSO.InputMode.Gameplay ||
           p_player.Health.IsDead || p_player.Combat.IsBattleState || Time.timeScale==0) { _lastPosition=position; return; }
        float step=Vector3.Distance(position,_lastPosition); _lastPosition=position;
        if(step>10) { ResetWandering(position); return; } // teleports aren't wandering
        if(step>0.01f) { _walkingTime+=Time.deltaTime; _travel+=step; }
        bool circling=_travel>30f && Vector3.Distance(_origin,position)<_travel*0.35f;
        if(_walkingTime>=_reminderDelay && (circling || _walkingTime>=_reminderDelay*2) && Time.time-_lastReminder>=_reminderCooldown)
        { ShowReminder(); ResetWandering(position); }
    }
    private void ResetWandering(Vector3 position) { _walkingTime=0; _travel=0; _origin=_lastPosition=position; }
    public void ShowReminder()
    {
        var data=QuestManager.Instance?QuestManager.Instance.CurrentQuestData:null;
        if(data==null) return;
        OnQuestAccepted(data);
        EnsureMenuHint();
        if(_menuHint) _menuHint.SetActive(true);
        _lastReminder=Time.time;
        _questSequence?.Kill();
        _questSequence=DOTween.Sequence().Append(_questCanvas.DOFade(1,0.5f)).AppendInterval(6).Append(_questCanvas.DOFade(0,1));
    }
    private void EnsureMenuHint()
    {
        if(_menuHint) return;
        var config=Resources.Load<GameUIIntegrationSettings>("GameUIIntegrationSettings"); if(!config) return;
        _menuHint=new GameObject("QuestMenuHint",typeof(RectTransform));
        var rect=_menuHint.GetComponent<RectTransform>(); rect.SetParent(_questDiscriptionSummary.transform.parent,false);
        var summary=_questDiscriptionSummary.rectTransform;
        rect.anchorMin=rect.anchorMax=new Vector2(0,0.5f); rect.pivot=Vector2.zero;
        rect.sizeDelta=new Vector2(((RectTransform)rect.parent).rect.width,38); rect.anchoredPosition=new Vector2(0,summary.anchoredPosition.y-summary.rect.height-18);
        var icon=new GameObject("MenuKey",typeof(RectTransform),typeof(UnityEngine.UI.Image)); icon.transform.SetParent(rect,false);
        var ir=icon.GetComponent<RectTransform>(); ir.anchorMin=ir.anchorMax=new Vector2(0,0.5f);ir.sizeDelta=new Vector2(36,36);ir.anchoredPosition=new Vector2(18,0);
        var labelGo=new GameObject("DeviceGuideLabel",typeof(RectTransform),typeof(TextMeshProUGUI));labelGo.transform.SetParent(ir,false);
        var label=labelGo.GetComponent<TextMeshProUGUI>();label.font=config.guideFont;label.fontSize=22;label.alignment=TextAlignmentOptions.Center;label.rectTransform.sizeDelta=new Vector2(48,36);
        var sprite=System.Array.Find(config.playStationSprites,s=>s && s.name.Contains("options"));
        icon.AddComponent<InputGuideUI>().Configure(config.actions,"Share/Escape",null,icon.GetComponent<UnityEngine.UI.Image>(),label,sprite);
        var textGo=new GameObject("Hint",typeof(RectTransform),typeof(TextMeshProUGUI));textGo.transform.SetParent(rect,false);
        var text=textGo.GetComponent<TextMeshProUGUI>();text.font=config.guideFont;text.fontSize=22;text.text="Menu";text.raycastTarget=false;
        text.rectTransform.anchorMin=Vector2.zero;text.rectTransform.anchorMax=Vector2.one;text.rectTransform.offsetMin=new Vector2(50,0);text.rectTransform.offsetMax=Vector2.zero;
        textGo.AddComponent<LocalizedGameText>();
    }

    private void Start() { EnsureMenuHint(); if(_menuHint)_menuHint.SetActive(false); }

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

            // 게임 로드 후 초기화 시 현재 진행 중인 퀘스트가 있다면 즉시 표시
            if (QuestManager.Instance.CurrentQuestData != null)
            {
                OnQuestAccepted(QuestManager.Instance.CurrentQuestData);
            }
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
        if (this == null || _questCanvas == null) return;

        if(_menuHint) _menuHint.SetActive(false);
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