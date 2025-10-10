using BH_Lib.DI;
using UnityEngine;

public class SkillEnchantUI : PopUpUI
{
    [SerializeField] private SkillEnchantNPC _skillEnchantNPC;
    [SerializeField] private GameObject _skillEnchantPanel;

    private PlayerSkillData _skillData => _skillEnchantNPC.GetPlayerSkillData();

    protected override void Start()
    {
        base.Start();

        if (_skillEnchantNPC != null)
        {
            _skillEnchantNPC.OnInteract += HandleInteract;
        }
    }

    private void OnDisable()
    {
        _skillEnchantNPC.OnInteract -= HandleInteract;
    }

    public override void OpenPopUp()
    {
        base.OpenPopUp();
        _skillEnchantPanel.SetActive(true);
    }

    public override void ClosePopUp()
    {
        base.ClosePopUp();
        _skillEnchantPanel.SetActive(false);
    }


    private void HandleInteract()
    {
        OpenPopUp();
    }
}

