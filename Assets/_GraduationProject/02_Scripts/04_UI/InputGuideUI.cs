using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Displays the effective action binding for the last-used input device.</summary>
public class InputGuideUI : MonoBehaviour
{
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private string _actionPath;
    [SerializeField] private string _keyboardActionPath;
    [SerializeField] private UnityEngine.UI.Image _image;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private Sprite _playStationSprite;
    private InputDeviceDetector _detector;
    private InputDeviceDetector.GuideDevice _shownDevice;
    private bool _shown;
    public bool holdKeyboard;

    public void Configure(InputActionAsset actions, string actionPath, string keyboardActionPath,
        UnityEngine.UI.Image image, TMP_Text label, Sprite playStationSprite)
    {
        _actions = actions; _actionPath = actionPath; _keyboardActionPath = keyboardActionPath;
        _image = image; _label = label; _playStationSprite = playStationSprite;
        Refresh();
    }

    private void OnEnable() { _shown = false; Refresh(); }
    private void Update()
    {
        var detector = InputDeviceDetector.Instance;
        if (detector != _detector) { _detector = detector; _shown = false; }
        var device = _detector ? _detector.CurrentGuideDevice : InputDeviceDetector.GuideDevice.KeyboardMouse;
        if (!_shown || device != _shownDevice) Refresh();
    }

    public void Refresh()
    {
        _detector = InputDeviceDetector.Instance;
        _shownDevice = _detector ? _detector.CurrentGuideDevice : InputDeviceDetector.GuideDevice.KeyboardMouse;
        bool keyboard = _shownDevice == InputDeviceDetector.GuideDevice.KeyboardMouse;
        string path = keyboard && !string.IsNullOrEmpty(_keyboardActionPath) ? _keyboardActionPath : _actionPath;
        var action = _actions ? _actions.FindAction(path, false) : null;
        string label = "";
        if (action != null)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (binding.isComposite || binding.isPartOfComposite) continue;
                string effective = binding.effectivePath ?? "";
                bool match = keyboard ? effective.Contains("<Keyboard>") || effective.Contains("<Mouse>") : effective.Contains("<Gamepad>");
                if (!match) continue;
                label = keyboard ? action.GetBindingDisplayString(i, InputBinding.DisplayStringOptions.DontIncludeInteractions) : PadLabel(effective, _shownDevice);
                break;
            }
            if (action.name == "Navigate") label = keyboard ? "A/D" : "D-pad";
            if (string.IsNullOrEmpty(label) && action.name == "Submit") label = keyboard ? "Enter" : "A";
            if (string.IsNullOrEmpty(label) && action.name == "Cancel") label = keyboard ? "Esc" : "B";
        }
        Sprite selectedSprite = _shownDevice == InputDeviceDetector.GuideDevice.PlayStation ? _playStationSprite : null;
        if (_shownDevice == InputDeviceDetector.GuideDevice.Xbox)
        {
            var config = Resources.Load<GameUIIntegrationSettings>("GameUIIntegrationSettings");
            string spriteName = label == "LS" ? "Left Stick" : label == "RS" ? "Right Stick" : label == "D-pad" ? "Dpad" : label;
            if (config && config.xboxSprites != null)
                selectedSprite = System.Array.Find(config.xboxSprites, s => s && s.name == "Xbox " + spriteName);
        }
        if (keyboard)
        {
            var config = Resources.Load<GameUIIntegrationSettings>("GameUIIntegrationSettings");
            if (holdKeyboard) label = "Hold LMB";
            string key = label.Replace(" ", "").Replace("/", "_").ToLowerInvariant();
            if (config && config.keyboardSprites != null)
                selectedSprite = System.Array.Find(config.keyboardSprites, s => s && s.name == key);
        }
        bool useSprite = selectedSprite != null;
        if (_image) { _image.preserveAspect = true; _image.enabled = useSprite; if (useSprite) _image.sprite = selectedSprite; }
        if (_label) { _label.text = label; _label.enabled = !useSprite; }
        _shown = true;
    }

    private static string PadLabel(string path, InputDeviceDetector.GuideDevice device)
    {
        string control = path.Substring(path.IndexOf('/') + 1);
        bool ps = device == InputDeviceDetector.GuideDevice.PlayStation;
        switch (control)
        {
            case "buttonSouth": return ps ? "Cross" : "A";
            case "buttonEast": return ps ? "Circle" : "B";
            case "buttonWest": return ps ? "Square" : "X";
            case "buttonNorth": return ps ? "Triangle" : "Y";
            case "leftShoulder": return ps ? "L1" : "LB";
            case "rightShoulder": return ps ? "R1" : "RB";
            case "leftTrigger": return ps ? "L2" : "LT";
            case "rightTrigger": return ps ? "R2" : "RT";
            case "leftStickPress": return ps ? "L3" : "LS";
            case "rightStickPress": return ps ? "R3" : "RS";
            case "leftStick": return "LS";
            case "rightStick": return "RS";
            case "start": return ps ? "Options" : "Menu";
            case "select": return ps ? "Share" : "View";
            default: return control;
        }
    }
}


