using UnityEngine;
using UnityEngine.UI;

public class ExitSettingUI : SettingPageUI
{
    [SerializeField] private Button _quitToTitleButton;
    [SerializeField] private Button _quitToDesktopButton;

    private MenuSettingUI _parentUI;

    private void Awake()
    {
        _parentUI = GetComponentInParent<MenuSettingUI>();
    }

    private void Start()
    {
        if (_parentUI == null) return;

        if (_quitToTitleButton != null)
            _quitToTitleButton.onClick.AddListener(OnQuitToTitle);

        if (_quitToDesktopButton != null)
            _quitToDesktopButton.onClick.AddListener(OnQuitToDesktop);
    }

    private void OnQuitToTitle()
    {
        _parentUI.OnQuitToTitle();
    }

    private void OnQuitToDesktop()
    {
        _parentUI.OnQuitToDesktop();
    }
}
