using UnityEngine;
using TMPro;

// Keep translated screen-space labels inside their canvas; scroll views retain their clipping.
[DefaultExecutionOrder(2000)]
[RequireComponent(typeof(TMP_Text))]
public class LocalizedScreenBounds : MonoBehaviour
{
    private readonly Vector3[] _corners=new Vector3[4];
    private RectTransform _root, _rect;
    private bool _excluded;
    private void OnEnable() => CacheHierarchy();
    private void OnTransformParentChanged() => CacheHierarchy();
    private void OnCanvasHierarchyChanged() => CacheHierarchy();
    private void CacheHierarchy()
    {
        _rect = (RectTransform)transform;
        _root = null;
        _excluded = false;
        for (var parent=transform; parent; parent=parent.parent)
            if (parent.name=="Whale_TimeLine") { _excluded=true; return; }
        var canvas=GetComponentInParent<Canvas>();
        _excluded=!canvas || GetComponentInParent<SkillDescription>(true) || GetComponentInParent<PlayerInteractUI>(true) ||
            canvas.rootCanvas.renderMode==RenderMode.WorldSpace ||
            GetComponentInParent<UnityEngine.UI.ScrollRect>() ||
            GetComponentInParent<UnityEngine.UI.RectMask2D>();
        if (!_excluded) _root=(RectTransform)canvas.rootCanvas.transform;
    }
    private void LateUpdate()
    {
        if (_excluded || !_root) return;
        var root=_root;
        var rect=_rect;
        rect.GetWorldCorners(_corners);
        Vector3 min=root.InverseTransformPoint(_corners[0]);
        Vector3 max=root.InverseTransformPoint(_corners[2]);
        var bounds=root.rect;
        float width=max.x-min.x,available=bounds.width-32;
        if(width>available && width>0)rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,rect.rect.width*available/width);
        rect.GetWorldCorners(_corners);min=root.InverseTransformPoint(_corners[0]);max=root.InverseTransformPoint(_corners[2]);
        var delta=Vector3.zero;
        if(min.x<bounds.xMin+16)delta.x=bounds.xMin+16-min.x;
        else if(max.x>bounds.xMax-16)delta.x=bounds.xMax-16-max.x;
        if(min.y<bounds.yMin+12)delta.y=bounds.yMin+12-min.y;
        else if(max.y>bounds.yMax-12)delta.y=bounds.yMax-12-max.y;
        if(delta.sqrMagnitude>.01f)rect.position+=root.TransformVector(delta);
    }
}
