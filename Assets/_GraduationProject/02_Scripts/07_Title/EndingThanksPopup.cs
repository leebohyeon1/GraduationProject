using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

/// <summary>One-time thanks/QR modal after the authored ending returns to Title.</summary>
public sealed class EndingThanksPopup : MonoBehaviour
{
    public const string SteamUrl = "https://store.steampowered.com/app/5484070/Soom/";
    public const string SocialUrl = "https://x.com/Team_SmallBoots";
    private static EndingThanksPopup _instance;
    private static int _closedFrame = -1;
    public static bool IsOpen => _instance && _instance._visible;
    public static bool BlocksTitleInput => IsOpen || Time.frameCount == _closedFrame;

    private bool _pending, _visible;
    private int _queuedFrame;
    private float _acceptInputAt;
    private CanvasGroup _group;
    private UnityEngine.UI.Button _close;
    private TMP_Text _description, _closeLabel;
    private readonly System.Collections.Generic.List<TMP_Text> _labels = new System.Collections.Generic.List<TMP_Text>();
    private InputReaderSO _reader;
    private LocalizedTmpFont _font;
    private GameObject _previousSelection;

    public static void NotifyTitleTransition(SceneTransitionInteractor transition)
    {
        var loader = SceneLoadingManager.Instance;
        if (!transition || !transition.TargetSceneData || transition.TargetSceneData.name != "Title" ||
            !loader || loader.IsTeleporting || !GamePlayTagManager.Instance || !GamePlayTagManager.Instance.HasTag("Boss_Clear")) return;
        var director = transition.GetComponent<PlayableDirector>();
        // This is the existing ending signal's owner, not the pause/game-over Title path.
        if (!director || !director.playableAsset || director.playableAsset.name != "Whale_TimeLine") return;
        if (_instance) return;
        var go = new GameObject("EndingThanksPending", typeof(RectTransform));
        _instance = go.AddComponent<EndingThanksPopup>();
        _instance._pending = true;
        _instance._queuedFrame = Time.frameCount;
        DontDestroyOnLoad(go);
    }

    public static void ShowIfPending(TitleManager owner, InputReaderSO reader)
    {
        if (!_instance || !_instance._pending || !owner) return;
        _instance._pending = false;
        _instance._visible = true;
        _instance._reader = reader;
        _instance.gameObject.name = "EndingThanksPopup";
        // Keep an independent root Canvas while giving it the Title scene's lifetime.
        SceneManager.MoveGameObjectToScene(_instance.gameObject, owner.gameObject.scene);
        _instance.Build();
    }

    private void Update()
    {
        var loader = SceneLoadingManager.Instance;
        if (_pending)
        {
            if (Time.frameCount <= _queuedFrame) return;
            // A rejected/failed transition must not leak the popup into a later ordinary Title visit.
            if (!loader || loader.LastLoadError != null || (!loader.IsTeleporting &&
                (!loader.CurrentActiveChunk || loader.CurrentActiveChunk.name != "Title"))) Destroy(gameObject);
            return;
        }
        if (!_visible) return;
        if (loader && loader.LastLoadError != null) { Destroy(gameObject); return; }
        if (loader && loader.IsTeleporting)
        {
            _group.alpha = 0;
            _acceptInputAt = Time.unscaledTime + .35f;
            return;
        }
        _group.alpha = Mathf.MoveTowards(_group.alpha, 1, Time.unscaledDeltaTime * 4);
        if (EventSystem.current && (!EventSystem.current.currentSelectedGameObject ||
            !EventSystem.current.currentSelectedGameObject.transform.IsChildOf(transform)))
            EventSystem.current.SetSelectedGameObject(_close.gameObject);
        if (Time.unscaledTime < _acceptInputAt) return;
        var keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.fKey.wasPressedThisFrame ||
            keyboard.escapeKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)) Close();
    }

    private void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 250;
        var scaler = gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = .5f;
        gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        _group = gameObject.AddComponent<CanvasGroup>();
        _group.alpha = 0;
        _acceptInputAt = Time.unscaledTime + .35f;
        _previousSelection = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
        var root = (RectTransform)transform;
        var shade = Rect("Backdrop", root, Vector2.zero, Vector2.zero);
        shade.anchorMin = Vector2.zero; shade.anchorMax = Vector2.one; shade.sizeDelta = Vector2.zero;
        AddImage(shade, new Color(0, 0, 0, .72f), true);
        var panel = Rect("ThanksPanel", root, new Vector2(1120, 800), Vector2.zero);
        AddImage(panel, new Color(.035f, .065f, .06f, .98f), false);
        var line = Rect("Accent", panel, new Vector2(960, 2), new Vector2(0, 230));
        AddImage(line, new Color(.65f, .78f, .68f, .9f), false);
        Label("Heading", panel, "Thank you for Playing", new Vector2(0, 318), new Vector2(1000, 76), 52);
        _description = Label("Description", panel, "", new Vector2(0, 256), new Vector2(1000, 48), 26);
        QR(panel, "Steam", "SOOM", "EndingThanks/steam", -250);
        QR(panel, "X", "SmallBoots", "EndingThanks/x", 250);
        var closeRect = Rect("Close", panel, new Vector2(260, 64), new Vector2(0, -326));
        var closeImage = AddImage(closeRect, new Color(.18f, .3f, .25f, 1), true);
        _close = closeRect.gameObject.AddComponent<UnityEngine.UI.Button>();
        _close.targetGraphic = closeImage;
        var colors = _close.colors; colors.highlightedColor = new Color(.85f, 1, .88f); colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color(.6f, .8f, .65f); _close.colors = colors;
        var nav = _close.navigation; nav.mode = UnityEngine.UI.Navigation.Mode.None; _close.navigation = nav;
        _close.onClick.AddListener(Close);
        _closeLabel = Label("CloseLabel", closeRect, "", Vector2.zero, new Vector2(240, 56), 28);
        if (_reader) { _reader.CancelEvent += Close; _reader.SubmitEvent += Close; }
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        _font = new LocalizedTmpFont { TableReference = "GameFonts", TableEntryReference = "UIFont" };
        _font.AssetChanged += ApplyFont;
        OnLocaleChanged(LocalizationSettings.SelectedLocale);
    }

    private void QR(RectTransform parent, string platform, string project, string resource, float x)
    {
        Label(platform + "Label", parent, platform, new Vector2(x, 178), new Vector2(430, 50), 30);
        var background = Rect(platform + "QR", parent, new Vector2(352, 352), new Vector2(x, -35));
        AddImage(background, Color.white, false);
        var qr = Rect("Code", background, new Vector2(328, 328), Vector2.zero);
        var image = AddImage(qr, Color.white, false);
        image.sprite = Resources.Load<Sprite>(resource);
        image.preserveAspect = true;
        if (!image.sprite) Debug.LogError("Ending QR sprite is missing: " + resource);
        Label(platform + "Project", parent, project, new Vector2(x, -242), new Vector2(430, 48), 27);
    }

    private TMP_Text Label(string name, RectTransform parent, string value, Vector2 position, Vector2 size, float fontSize)
    {
        var rect = Rect(name, parent, size, position);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        var config = Resources.Load<GameUIIntegrationSettings>("GameUIIntegrationSettings");
        if (config) text.font = config.guideFont;
        text.text = value;
        text.fontSize = fontSize; text.enableAutoSizing = true; text.fontSizeMax = fontSize; text.fontSizeMin = fontSize * .75f;
        text.alignment = TextAlignmentOptions.Center; text.textWrappingMode = TextWrappingModes.NoWrap;
        text.color = new Color(.95f, .96f, .91f, 1); text.raycastTarget = false;
        _labels.Add(text);
        return text;
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = size; rect.anchoredPosition = position; return rect;
    }

    private static UnityEngine.UI.Image AddImage(RectTransform rect, Color color, bool raycast)
    {
        var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = color; image.raycastTarget = raycast; return image;
    }

    private void OnLocaleChanged(Locale locale)
    {
        string code = locale ? locale.Identifier.Code : "en";
        if (code.StartsWith("ko")) { _description.text = "SOOM을 플레이해 주셔서 감사합니다. QR 코드로 소식을 만나보세요."; _closeLabel.text = "닫기"; }
        else if (code.StartsWith("ja")) { _description.text = "プレイしていただきありがとうございます。QRコードから最新情報をご覧ください。"; _closeLabel.text = "閉じる"; }
        else if (code.StartsWith("zh")) { _description.text = "感谢游玩 SOOM！扫描二维码，关注游戏最新消息。"; _closeLabel.text = "关闭"; }
        else { _description.text = "Thank you for playing SOOM. Scan the QR codes to follow our updates."; _closeLabel.text = "Close"; }
    }

    private void ApplyFont(TMP_FontAsset font)
    {
        if (!font) return;
        foreach (var text in _labels) if (text) { text.font = font; text.fontSharedMaterial = font.material; }
    }

    public void Close()
    {
        if (!_visible || Time.unscaledTime < _acceptInputAt) return;
        _visible = false;
        _closedFrame = Time.frameCount;
        UIAudioFeedback.Play(2);
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(_previousSelection && _previousSelection.activeInHierarchy ? _previousSelection : null);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_reader) { _reader.CancelEvent -= Close; _reader.SubmitEvent -= Close; }
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        if (_font != null) _font.AssetChanged -= ApplyFont;
        if (_instance == this) _instance = null;
    }
}
