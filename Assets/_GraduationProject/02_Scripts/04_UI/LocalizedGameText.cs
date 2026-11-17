using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

/// <summary>Routes authored and code-updated labels through Unity Localization tables.</summary>
[RequireComponent(typeof(TMP_Text))]
public class LocalizedGameText : MonoBehaviour
{
    private TMP_Text _text;
    private LocalizedString _localized;
    private LocalizedTmpFont _font;
    private StringTable _sourceTable;
    private string _source;
    private string _rendered;
    private bool _ready;
    private bool _subscribed;
    private bool _fontReady;
    public bool IsReady => _ready && _fontReady;
    private float _authoredFontSize;
    private void Awake() { _text = GetComponent<TMP_Text>(); _authoredFontSize = _text.fontSize; _text.overflowMode=TextOverflowModes.Overflow; }

    private void OnEnable() => StartCoroutine(Initialize());
    private IEnumerator Initialize()
    {
        _text = GetComponent<TMP_Text>();
        yield return LocalizationSettings.InitializationOperation;
        if (!isActiveAndEnabled) yield break;
        var request = LocalizationSettings.StringDatabase.GetTableAsync("GameText", LocalizationSettings.AvailableLocales.GetLocale("ko"));
        yield return request;
        if (!isActiveAndEnabled || request.Result == null) yield break;
        _sourceTable = request.Result;
        _localized = new LocalizedString { TableReference = "GameText" };
        _font = new LocalizedTmpFont { TableReference = "GameFonts", TableEntryReference = "UIFont" };
        _font.AssetChanged += ApplyFont;
        _ready = true;
        Bind(_text.text);
    }

    private void LateUpdate()
    {
        if (_ready && _text.text != _rendered) Bind(_text.text);
    }

    private void Bind(string source)
    {
        string key = (source ?? "").Replace("\r\n", "\n").Trim();
        if (_subscribed) { _localized.StringChanged -= ApplyText; _subscribed = false; }
        _source = source;
        _rendered = source;
        var entry = _sourceTable.SharedData.GetEntry(key)
            ?? _sourceTable.SharedData.Entries.FirstOrDefault(e =>
                string.Equals(e.Key, key, System.StringComparison.OrdinalIgnoreCase));
        if (entry == null) return;
        key = entry.Key;
        _localized.TableEntryReference = key;
        _subscribed = true;
        _localized.StringChanged += ApplyText;
    }

    private void ApplyText(string value)
    {
        if (!_text || !isActiveAndEnabled) return;
        // A gameplay script may have replaced this label while an async request was pending.
        if (_text.text != _rendered && _text.text != _source) return;
        if (GetComponentInParent<KeyGuideLayout>() || GetComponentInParent<InputGuideUI>()) value=value.Replace("\r", "").Replace("\n", " ");
        _rendered = value;
        _text.enableAutoSizing = true;
        _text.fontSizeMax = _authoredFontSize;
        _text.fontSizeMin = Mathf.Min(_authoredFontSize, Mathf.Max(12, _authoredFontSize * 0.6f));
        // Skill descriptions are paragraphs, even when the source has no newline.
        bool skillBody = name == "Skill_Description_Text" && GetComponentInParent<SkillDescription>(true);
        if (skillBody)
        {
            _text.fontSizeMin = 20;
            _text.fontSizeMax = 24;
            _text.wordWrappingRatios = 0;
            value = value.Replace("홀드", "<nobr>홀드</nobr>");
            _rendered = value;
        }
        // Keep explicit paragraph breaks; compact labels should remain one line.
        _text.textWrappingMode = !skillBody && !value.Contains("\n") && value.Length <= 65
            ? TextWrappingModes.NoWrap : TextWrappingModes.Normal;
        _text.overflowMode = TextOverflowModes.Overflow;
        _text.text = value;
        _text.SetAllDirty();
    }
    private void ApplyFont(TMP_FontAsset font)
    {
        if (_text && font) { _text.font = font; _text.fontSharedMaterial=font.material; _fontReady=true; _text.SetAllDirty(); _text.ForceMeshUpdate(true,true); }
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        _ready = false;
        if (_subscribed && _localized != null) _localized.StringChanged -= ApplyText;
        if (_font != null) _font.AssetChanged -= ApplyFont;
        _subscribed = false;
        // Keep the source key across a panel closing/reopening.
        if (_text && _text.text == _rendered && _source != null) _text.text = _source;
    }
}



