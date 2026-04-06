using UnityEngine;

public abstract class SettingPageUI : MonoBehaviour
{
    [Header("Page Info")]
    [SerializeField] protected string _pageTitle;
    [SerializeField] protected SettingPageType _pageType;

    public string PageTitle => _pageTitle;
    public string SettingName => _pageTitle; // SettingComponent 통합을 위해 추가
    public SettingPageType PageType => _pageType;

    public virtual void OnPageOpen() 
    {
        gameObject.SetActive(true);
    }

    public virtual void OnPageClose() 
    {
        gameObject.SetActive(false);
    }
}
