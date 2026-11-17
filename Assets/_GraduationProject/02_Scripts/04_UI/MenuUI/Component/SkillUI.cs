using System.Collections.Generic;
using UnityEngine;

public class SkillUI : MenuUIComponent
{
    private MenuUI _menuUI;
    public static SkillUpgradeButtonUI TutorialTarget { get; private set; }
    public static bool TutorialLocked => TutorialTarget && !TutorialTarget.IsLearned;
    public static void CompleteTutorial() { TutorialTarget = null; GameNotice.Dismiss(); }
    private void OnDestroy() { if (TutorialTarget && TutorialTarget.transform.IsChildOf(transform)) CompleteTutorial(); }
    public void BeginSelectedTutorial()
    {
        int index = _upgradeButtonList.FindIndex(b => b.IsSelected);
        BeginTutorial(index >= 0 ? index : 0);
    }
    public void BeginTutorial(int index)
    {
        if (index < 0 || index >= _upgradeButtonList.Count) return;
        var button = _upgradeButtonList[index];
        if (button.IsLearned || !button.CheckUnlockCondition()) return;
        // Do not trap a saved character that cannot afford the tutorial purchase.
        if (!button.HasEnoughMoney()) { GameNotice.Show("돈이 부족합니다."); UIAudioFeedback.Play(7); return; }
        TutorialTarget = button;
        GameNotice.Show("스킬을 해금할 수 있습니다. 스킬을 선택하고 상호작용 키를 꾹 눌러 해금하세요.", float.PositiveInfinity, .82f);
        button.Select();
    }

    [SerializeField] private List<SkillUpgradeButtonUI> _upgradeButtonList;
    [SerializeField] private SkillDescription _skillDescription;

    public override void Initialize(MenuUI menu)
    {
        base.Initialize(menu);
        _menuUI = menu;

        foreach (var button in _upgradeButtonList)
        {
            button.Initialize(_menuUI.Player);
        }
    }

    public override void Dispose()
    {
       
    }

    public override void OnOpen()
    {
        base.OnOpen();
        if (DragonTotemReward.TutorialPending)
        {
            // OpenSkillPanel consumes the pending flag after selecting the reward skill.
            GameNotice.Show("스킬을 해금할 수 있습니다. 스킬을 선택하고 상호작용 키를 꾹 눌러 해금하세요.", 8f);
        }

        if (_upgradeButtonList != null && _upgradeButtonList.Count > 0)
        {
            SelectSkillByIndex(0);
        }
    }

    /// <summary>
    /// 특정 인덱스의 스킬 버튼을 강제로 선택합니다.
    /// </summary>
    /// <param name="index">선택할 스킬 버튼의 인덱스</param>
    public void SelectSkillByIndex(int index)
    {
        if (_upgradeButtonList != null && index >= 0 && index < _upgradeButtonList.Count)
        {
            _upgradeButtonList[index].Select();
        }
    }

    public void UpdateDescription(SkillUpgradeButtonUI skillUpgradeButtonUI)
    {
        _skillDescription.SetDescription(
            skillUpgradeButtonUI.SkillName,
            skillUpgradeButtonUI.SkillDescription,
            skillUpgradeButtonUI.GetPriceString(),
            skillUpgradeButtonUI.GetSpecialPriceString(),
            skillUpgradeButtonUI.SkillVideo
        );
    }
}
