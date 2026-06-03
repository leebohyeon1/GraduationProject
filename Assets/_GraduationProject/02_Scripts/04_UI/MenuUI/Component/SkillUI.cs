using System.Collections.Generic;
using UnityEngine;

public class SkillUI : MenuUIComponent
{
    private MenuUI _menuUI;

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
            skillUpgradeButtonUI.Price.ToString(),
            skillUpgradeButtonUI.SpecialPrice.ToString(),
            skillUpgradeButtonUI.SkillVideo
        );
    }
}
