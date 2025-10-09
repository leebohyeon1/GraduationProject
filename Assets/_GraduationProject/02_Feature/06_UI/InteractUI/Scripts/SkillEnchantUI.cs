using BH_Lib.DI;
using UnityEngine;

public class SkillEnchantUI : PopUpUI
{
    [SerializeField] private InteractableObject _interactableObject;
    [SerializeField] private GameObject _skillEnchantPanel;
    private Player _player;

    private void OnEnable()
    {
        if (_interactableObject != null)
        {
            _interactableObject.OnInteract += HandleInteract;
        }

        if (_player == null)
        {
            _player = DIContainer.Instance.Resolve<Player>();
        }
    }

    private void OnDisable()
    {
        _interactableObject.OnInteract -= HandleInteract;
    }

    private void OpenUI()
    {
        p_uiManager.RegisterUI(this);
        _skillEnchantPanel.SetActive(true);
    }

    protected override void CloseUI()
    {
        _skillEnchantPanel.SetActive(false);
    }


    private void HandleInteract()
    {
        OpenUI();
    }
}

