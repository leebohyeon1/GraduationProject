using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class KeyGuideLayout : MonoBehaviour
{
    private sealed class Item
    {
        public RectTransform rect;
        public TMP_Text text;
        public string value;
        public TMP_FontAsset font;
        public float fontSize, icon, width;
    }
    private readonly List<Item> _items = new();
    private bool _rebuild = true;
    private float _parentWidth = -1;
    private void OnEnable() => _rebuild = true;
    private void OnTransformChildrenChanged() => _rebuild = true;

    private void LateUpdate()
    {
        bool dirty = _rebuild;
        if (_rebuild)
        {
            _rebuild = false;
            var layout = GetComponent<LayoutGroup>();
            if (layout) layout.enabled = false;
            var fitter = GetComponent<ContentSizeFitter>();
            if (fitter) fitter.enabled = false;
            _items.Clear();
            foreach (Transform child in transform)
            {
                var image = child.GetComponent<Image>();
                if (!image) continue;
                foreach (var text in child.GetComponentsInChildren<TMP_Text>(true))
                    if (text.name != "DeviceGuideLabel")
                    { _items.Add(new Item { rect=image.rectTransform, text=text }); break; }
            }
        }
        var parent = (RectTransform)transform;
        if (_parentWidth != parent.rect.width) { _parentWidth = parent.rect.width; dirty = true; }
        foreach (var item in _items)
        {
            if (!item.text || !item.rect) { _rebuild = true; return; }
            var text = item.text;
            if (dirty || item.value != text.text || item.font != text.font ||
                item.fontSize != text.fontSize || item.icon != item.rect.rect.width || text.havePropertiesChanged)
            {
                item.value = text.text;
                item.font = text.font;
                item.fontSize = text.fontSize;
                item.icon = item.rect.rect.width;
                text.textWrappingMode = TextWrappingModes.NoWrap;
                item.width = Mathf.Max(75, text.GetPreferredValues(text.text).x);
                dirty = true;
            }
        }
        if (!dirty) return;
        float total = 0;
        foreach (var item in _items) total += item.icon + 12 + item.width + 40;
        float x = parent.rect.xMax - Mathf.Max(0,total-40) - 24;
        foreach (var item in _items)
        {
            item.rect.anchorMin=item.rect.anchorMax=new Vector2(.5f,.5f);
            item.rect.pivot=new Vector2(.5f,.5f);
            item.rect.anchoredPosition=new Vector2(x+item.icon*.5f,0);
            var tr=item.text.rectTransform;
            tr.anchorMin=tr.anchorMax=new Vector2(.5f,.5f);
            tr.pivot=new Vector2(0,.5f);
            tr.anchoredPosition=new Vector2(item.icon*.5f+12,0);
            tr.sizeDelta=new Vector2(item.width,60);
            item.text.alignment=TextAlignmentOptions.Left;
            item.text.enableAutoSizing=true;
            item.text.textWrappingMode=TextWrappingModes.NoWrap;
            x+=item.icon+12+item.width+40;
        }
    }
}
