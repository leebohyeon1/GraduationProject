using TMPro;
using UnityEngine;

public class SkillDescription : MonoBehaviour
{
    [SerializeField] private TMP_Text _skillName;
    [SerializeField] private TMP_Text _skillDescription;
    [SerializeField] private TMP_Text _moneyAmount;
    [SerializeField] private UnityEngine.Video.VideoPlayer _videoPlayer;
    // [SerializeField] private TMP_Text _specialmoneyAmount;

    private void OnEnable() => ConfigureLayout();
    private void ConfigureLayout()
    {
        var panel = (RectTransform)transform;
        panel.sizeDelta = new Vector2(820, 720);
        panel.anchoredPosition = new Vector2(-24, 0);
        var background = transform.Find("Image (1)") as RectTransform;
        if (background) { background.anchorMin=Vector2.zero; background.anchorMax=Vector2.one; background.offsetMin=background.offsetMax=Vector2.zero; }
        Layout(_skillName.rectTransform, new Vector2(0, 305), new Vector2(740, 55));
        var video = transform.Find("Skill_Video") as RectTransform;
        if (video) Layout(video, new Vector2(0, 110), new Vector2(533.33f, 300));
        Layout(_skillDescription.rectTransform, new Vector2(0, -160), new Vector2(740, 180));
        _skillDescription.alignment = TextAlignmentOptions.TopLeft;
        _skillDescription.textWrappingMode = TextWrappingModes.Normal;
        _skillDescription.enableAutoSizing = true;
        _skillDescription.fontSizeMin = 20;
        _skillDescription.fontSizeMax = 24;
        var cost = transform.Find("Skill_Cost") as RectTransform;
        if (cost)
        {
            var layout = cost.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            if (layout) layout.enabled = false;
            Layout(cost, new Vector2(-70, -305), new Vector2(180, 50));
            var label = cost.GetComponent<TMP_Text>();
            if (label) label.alignment = TextAlignmentOptions.Right;
            var icon = cost.Find("Money_Icon") as RectTransform;
            if (icon)
            {
                icon.anchorMin=icon.anchorMax=new Vector2(1,.5f);
                icon.pivot=new Vector2(.5f,.5f);icon.anchoredPosition=new Vector2(35,0);icon.sizeDelta=new Vector2(36,36);
                _moneyAmount.rectTransform.anchorMin=_moneyAmount.rectTransform.anchorMax=new Vector2(1,.5f);
                _moneyAmount.rectTransform.pivot=new Vector2(0,.5f);
                _moneyAmount.rectTransform.anchoredPosition=new Vector2(12,0);
                _moneyAmount.rectTransform.sizeDelta=new Vector2(90,50);
                _moneyAmount.alignment=TextAlignmentOptions.Left;
            }
        }
    }
    private static void Layout(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
        rect.anchoredPosition=position;rect.sizeDelta=size;
    }

    public void SetDescription(string skillName, string skillDescription, string moneyAmount, string specialMoneyAmount, UnityEngine.Video.VideoClip videoClip = null)
    {
        ConfigureLayout();
        _skillName.text = skillName;
        _skillDescription.text = skillDescription;
        _moneyAmount.text = moneyAmount;
        // _specialmoneyAmount.text = specialMoneyAmount;

        if (_videoPlayer != null)
        {
            if (videoClip != null)
            {
                _videoPlayer.clip = videoClip;
                _videoPlayer.Play();
            }
            else
            {
                _videoPlayer.Stop();
                _videoPlayer.clip = null;
            }
        }
    }
}
