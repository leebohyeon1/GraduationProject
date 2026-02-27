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
        DataManager.Instance.SaveGame(); // 게임 저장
        SceneManager.LoadScene("Title");
    }

    public void OnQuitToDesktop()
    {
        DataManager.Instance.SaveGame(); // 게임 저장
        Application.Quit();
    }

}
