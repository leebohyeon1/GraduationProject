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

        // 패드 조작을 위해 첫 번째 버튼에 포커스
        if (_upgradeButtonList != null && _upgradeButtonList.Count > 0)
        {
            _upgradeButtonList[0].Select();
        }
    }

    public void UpdateDescription(SkillUpgradeButtonUI skillUpgradeButtonUI)
    {
        _skillDescription.SetDescription(
            skillUpgradeButtonUI.SkillName,
            skillUpgradeButtonUI.SkillDescription,
            skillUpgradeButtonUI.Price.ToString(),
            skillUpgradeButtonUI.SpecialPrice.ToString()
        );
    }
}
