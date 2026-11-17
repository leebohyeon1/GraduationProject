using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Installs localized labels and input guides on scene and dynamically spawned UI.</summary>
public class GameUIIntegration : MonoBehaviour
{
    private GameUIIntegrationSettings _settings;
    private bool _refresh;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        var settings = Resources.Load<GameUIIntegrationSettings>("GameUIIntegrationSettings");
        if (!settings) return;
        var go = new GameObject("GameUIIntegration");
        DontDestroyOnLoad(go);
        go.AddComponent<GameUIIntegration>()._settings = settings;
        if (!InputDeviceDetector.Instance) go.AddComponent<InputDeviceDetector>();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        UnityEngine.InputSystem.InputSystem.onActionChange += UIAudioFeedback.OnActionChange;
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        _refresh = true;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.InputSystem.InputSystem.onActionChange -= UIAudioFeedback.OnActionChange;
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => _refresh = true;
    private void OnTextChanged(Object obj)
    {
        if (obj is TMP_Text text && text.gameObject.scene.IsValid() &&
            !text.GetComponent<LocalizedGameText>() && text.name != "DeviceGuideLabel") _refresh = true;
    }
    private void LateUpdate()
    {
        if (!_refresh || !_settings) return;
        _refresh = false;
        foreach (var text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (text.name != "DeviceGuideLabel" && !text.GetComponent<LocalizedGameText>()) text.gameObject.AddComponent<LocalizedGameText>();
            if (text is TextMeshProUGUI && text.name != "DeviceGuideLabel" && !text.GetComponent<LocalizedScreenBounds>()) text.gameObject.AddComponent<LocalizedScreenBounds>();
        }
        foreach (var selectable in Object.FindObjectsByType<UnityEngine.UI.Selectable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (!selectable.GetComponent<UIAudioFeedback>()) selectable.gameObject.AddComponent<UIAudioFeedback>();
        foreach (var image in Object.FindObjectsByType<UnityEngine.UI.Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!image.sprite || image.GetComponent<InputGuideUI>()) continue;
            var spriteName = image.sprite.name;
            if (!spriteName.StartsWith("playstation") && spriteName != "X_button" && spriteName != "O_button" && spriteName != "control_button") continue;
            ConfigureGuide(image);
        }
    }
    private void ConfigureGuide(UnityEngine.UI.Image image)
    {
        string action = null;
        string keyboard = null;
        string sprite = image.sprite.name;
        string parent = image.transform.parent ? image.transform.parent.name : "";
        if (sprite == "X_button") { action="UI/Submit"; sprite="playstation_button_cross"; }
        else if (sprite == "O_button") { action="UI/Cancel"; sprite="playstation_button_circle"; }
        else if (sprite == "control_button") { action="UI/Navigate"; sprite="playstation_dpad_horizontal_outline"; }
        switch (parent)
        {
            case "Attack": action="Player/NormalAttack"; sprite="playstation_button_square"; break;
            case "Counter_Attack": action="Player/CounterAndCharge"; sprite="playstation_button_circle"; break;
            case "Dodge": action="Player/Dodge"; sprite="playstation_button_cross"; break;
            case "Potion": action="Player/Potion"; sprite="playstation_trigger_l1"; break;
            case "Lock_On":
            case "Lock_Off": action="Player/ToggleLockOnForGamepad"; keyboard="Player/LockOnForKeyboard"; sprite="playstation_stick_r_press"; break;
            case "Menu": action="Share/Escape"; break;
            case "InteractiveUI": action="Player/Interact"; break;
            case "SkipKey": action="CutScene/Skip"; break;
            case "TopBar": action=image.name.Contains("Right") ? "UI/Next" : "UI/Previous"; break;
            case "Settings": action=image.name.Contains("Right") ? "UI/SubNext" : "UI/SubPrevious"; break;
        }
        if (image.GetComponentInParent<SkillUI>(true) && action == "UI/Submit")
        { action = "Player/Interact"; sprite = "playstation_button_triangle"; }
        if (action == null)
        {
            if (sprite.Contains("square")) action="Player/NormalAttack";
            else if (sprite.Contains("circle")) action="Player/CounterAndCharge";
            else if (sprite.Contains("cross")) action="Player/Dodge";
            else if (sprite.Contains("triangle")) action="Player/Interact";
            else if (sprite == "playstation_trigger_l1") action="Player/Potion";
            else return;
        }
        bool chargedObject = false;
        for (Transform ancestor=image.transform; ancestor; ancestor=ancestor.parent)
            if (ancestor.name == "Hard_Destroyed_Totem") chargedObject=true;
        if (chargedObject) keyboard="Player/NormalAttack";
        var authoredLabel=image.GetComponentInChildren<TMP_Text>(true);
        if (action == "UI/Submit" && authoredLabel && authoredLabel.text.Contains("Hold"))
        { action="Player/Interact"; sprite="playstation_button_triangle"; }
        if (image.transform.parent && image.transform.parent.parent && image.transform.parent.parent.name=="Console_KeyGuide" && !image.transform.parent.GetComponent<KeyGuideLayout>())
            image.transform.parent.gameObject.AddComponent<KeyGuideLayout>();
        var go = new GameObject("DeviceGuideLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(image.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(-8, -2); rect.offsetMax = new Vector2(8, 2);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.font = _settings.guideFont;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true; text.fontSizeMin=10; text.fontSizeMax=28;
        text.color = Color.white; text.raycastTarget=false;
        var psSprite=_settings.playStationSprites.FirstOrDefault(s=>s && s.name==sprite);
        var guide=image.gameObject.AddComponent<InputGuideUI>(); guide.holdKeyboard=chargedObject; guide.Configure(_settings.actions, action, keyboard, image, text, psSprite);
    }
}


