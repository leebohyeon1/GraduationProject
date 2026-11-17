using UnityEngine;
using TMPro;
public class GameNotice : MonoBehaviour
{
    private static GameNotice _instance;
    private TMP_Text _text;
    private CanvasGroup _group;
    private float _until;
    private LocalizedGameText _localizer;
    public static void Show(string message, float duration = 2.5f, float verticalPosition = .22f)
    {
        if (!_instance) {
            var go = new GameObject("GameNotice", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(CanvasGroup));
            DontDestroyOnLoad(go);
            var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 32760;
            var scaler = go.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080);
            _instance=go.AddComponent<GameNotice>(); _instance._group=go.GetComponent<CanvasGroup>(); _instance._group.blocksRaycasts=false;
            var label=new GameObject("Message", typeof(RectTransform)); label.transform.SetParent(go.transform,false);
            _instance._text=label.AddComponent<TextMeshProUGUI>(); var t=_instance._text;
            t.font=Resources.Load<GameUIIntegrationSettings>("GameUIIntegrationSettings").guideFont;
            t.fontSize=30; t.alignment=TextAlignmentOptions.Center; t.raycastTarget=false;
            t.rectTransform.anchorMin=t.rectTransform.anchorMax=new Vector2(.5f,.22f); t.rectTransform.sizeDelta=new Vector2(1150,130);
            var shadow=label.AddComponent<UnityEngine.UI.Shadow>(); shadow.effectColor=new Color(0,0,0,.9f); shadow.effectDistance=new Vector2(2,-2);
            _instance._localizer=label.AddComponent<LocalizedGameText>();
        }
        _instance._text.rectTransform.anchorMin=_instance._text.rectTransform.anchorMax=new Vector2(.5f,verticalPosition);
        _instance._text.text=message; _instance._group.alpha=_instance._localizer.IsReady?1:0; _instance._until=Time.unscaledTime+duration;
    }
    public static void Dismiss() { if (_instance) _instance._until=0f; }
    private void Update() { _group.alpha=_localizer && _localizer.IsReady ? Mathf.Clamp01((_until-Time.unscaledTime)*3f) : 0; }
}
