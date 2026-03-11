using UnityEngine;
using UnityEngine.UI;

public class ExitSettingUI : MonoBehaviour
{
    [SerializeField] private Button _quitToTitleButton;
    [SerializeField] private Button _quitToDesktopButton;

    private void Start()
    {
        // 버튼 클릭 시 MenuSettingUI의 메서드 호출
        if (_quitToTitleButton != null)
        {
            _quitToTitleButton.onClick.AddListener(OnQuitToTitle);
        }

        if (_quitToDesktopButton != null)
        {
            _quitToDesktopButton.onClick.AddListener(OnQuitToDesktop);
        }
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
