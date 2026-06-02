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

        // [수정] 여기서 항상 0번을 선택하면 MenuUI에서 호출한 특정 인덱스 선택이 덮어씌워질 수 있음
        // 기본 선택이 필요한 상황(예: 메뉴를 그냥 열었을 때)은 MenuUI에서 처리하도록 위임하거나
        // 아무것도 선택되지 않았을 때만 기본 선택을 수행하도록 변경할 수 있습니다.
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
