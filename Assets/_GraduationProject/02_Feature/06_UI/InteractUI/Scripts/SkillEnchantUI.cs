using BH_Lib.DI;
using UnityEngine;

public class SkillEnchantUI : PopUpUI
{
    [SerializeField] private InteractableObject _interactableObject;
    [SerializeField] private GameObject _skillEnchantPanel;
    private Player _player;

    protected override void Start()
    {
        base.Start();

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

