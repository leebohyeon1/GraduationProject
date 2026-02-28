using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSettingUI : MenuUIComponent
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
        SceneLoadingManager.Instance.TeleportToSceneByName("Title");
    }

    public void OnQuitToDesktop()
    {
        DataManager.Instance.SaveGame(); // 게임 저장
        Application.Quit();
    }

}
