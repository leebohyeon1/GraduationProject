using UnityEngine;
using TMPro;
// Vertex dissolve uses the already loaded TMP font material; no runtime shader/font construction.
public class WorldTextDissolve : MonoBehaviour
{
    [Range(0,1)] public float Progress;
    private TMP_Text[] _labels;
    private float _previous=-1f;
    private void Start() { _labels=GetComponentsInChildren<TMP_Text>(true); foreach(var t in _labels)t.ForceMeshUpdate(true); }
    private void LateUpdate() {
        if (_labels==null) return;
        bool dirty=Progress!=_previous;
        foreach(var t in _labels) dirty |= t.havePropertiesChanged;
        if(!dirty)return;
        _previous=Progress;
        foreach(var t in _labels) {
            t.ForceMeshUpdate(true);
            var info=t.textInfo;
            for(int i=0;i<info.characterCount;i++) {
                var c=info.characterInfo[i];if(!c.isVisible)continue;
                float noise=Mathf.Repeat(i*.618034f,1f);
                byte alpha=(byte)(255*Mathf.Clamp01((Progress*1.2f-noise)/.2f));
                var colors=info.meshInfo[c.materialReferenceIndex].colors32;
                for(int v=0;v<4;v++)colors[c.vertexIndex+v].a=alpha;
            }
            t.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}
