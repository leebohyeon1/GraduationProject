using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingUI : MenuUIComponent
{
    public override void Initialize(MenuUI menu)
    {
        base.Initialize(menu);
    }

    public override void Dispose()
    {

    }   

    public void OnQuitToTitle()
    {
        SceneManager.LoadScene("Title");
    }

    public void OnQuitToDesktop()
    {
        Application.Quit();
    }

}
