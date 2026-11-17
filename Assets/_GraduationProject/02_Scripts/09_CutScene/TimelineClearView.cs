using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DanielIlett.SnapshotShaders2.URP;
[RequireComponent(typeof(PlayableDirector))]
public class TimelineClearView : MonoBehaviour
{
    public Volume volume;
    private PlayableDirector _director;
    private DepthOfField _depth;
    private BlurSettings _blur;
    private bool _applied,_depthWas,_blurWas;
    private void Awake() { _director=GetComponent<PlayableDirector>(); }
    private void LateUpdate() {
        bool playing=_director && _director.state==PlayState.Playing;
        if(playing && !_applied && volume) {
            volume.profile.TryGet(out _depth); volume.profile.TryGet(out _blur);
            _depthWas=_depth && _depth.active; _blurWas=_blur && _blur.active; _applied=true;
        }
        if(playing && _applied) { if(_depth)_depth.active=false;if(_blur)_blur.active=false; }
        else Restore();
    }
    private void Restore() { if(!_applied)return; if(_depth)_depth.active=_depthWas;if(_blur)_blur.active=_blurWas;_applied=false; }
    private void OnDisable() { Restore(); }
}
