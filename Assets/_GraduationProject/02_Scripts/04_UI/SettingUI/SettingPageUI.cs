using UnityEngine;

public abstract class SettingPageUI : MonoBehaviour
{
    [Header("Page Info")]
    [SerializeField] protected string _pageTitle;

    public string PageTitle => _pageTitle;
    public string SettingName => _pageTitle; // SettingComponent 통합을 위해 추가

    public virtual void OnPageOpen() 
    {
        gameObject.SetActive(true);
    }

    public virtual void OnPageClose() 
    {
        gameObject.SetActive(false);
    }
}
