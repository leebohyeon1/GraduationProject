using UnityEngine;
using MoreMountains.Tools;
using Unity.Cinemachine;

public class StoneDoorFeedback : MonoBehaviour
{
    public Transform[] movingParts;
    public AudioClip rumble;
    [Range(0,1)] public float volume = 0.55f;
    [Range(0,0.3f)] public float shakeStrength = 0.06f;
    private Vector3[] _positions;
    private Quaternion[] _rotations;
    private AudioSource _sound;
    private CinemachineImpulseSource _impulse;
    private float _lastMotion, _nextShake;
    private void OnEnable()
    {
        if(movingParts==null) movingParts=new Transform[0];
        _positions=new Vector3[movingParts.Length]; _rotations=new Quaternion[movingParts.Length];
        for(int i=0;i<movingParts.Length;i++) if(movingParts[i]) { _positions[i]=movingParts[i].position; _rotations[i]=movingParts[i].rotation; }
        _lastMotion=-10;
        _impulse=GetComponent<CinemachineImpulseSource>();
        if(!_impulse) _impulse=gameObject.AddComponent<CinemachineImpulseSource>();
        _impulse.ImpulseDefinition.ImpulseShape=CinemachineImpulseDefinition.ImpulseShapes.Rumble;
        _impulse.ImpulseDefinition.ImpulseDuration=0.65f;
        _impulse.ImpulseDefinition.ImpulseType=CinemachineImpulseDefinition.ImpulseTypes.Dissipating;
        _impulse.ImpulseDefinition.DissipationDistance=60;
    }
    private void LateUpdate()
    {
        bool moving=false; Vector3 soundPosition=Vector3.zero; int count=0;
        for(int i=0;i<movingParts.Length;i++) if(movingParts[i]) {
            soundPosition+=movingParts[i].position; count++;
            moving |= (movingParts[i].position-_positions[i]).sqrMagnitude>0.000001f || Quaternion.Angle(movingParts[i].rotation,_rotations[i])>0.01f;
            _positions[i]=movingParts[i].position; _rotations[i]=movingParts[i].rotation;
        }
        soundPosition=count>0?soundPosition/count:transform.position;
        if(moving) _lastMotion=Time.time;
        if(Time.time-_lastMotion<0.2f) {
            if(!_sound && rumble && MMSoundManager.HasInstance)
                _sound=MMSoundManager.Instance.PlaySound(rumble,MMSoundManager.MMSoundManagerTracks.Sfx,soundPosition,loop:true,volume:volume,pitch:0.75f,spatialBlend:0.85f,minDistance:12,maxDistance:80);
            if(_sound) _sound.transform.position=soundPosition;
            if(Time.time>=_nextShake && PlayerPrefs.GetInt("ScreenShake",1)==1) { _impulse.GenerateImpulseAtPositionWithVelocity(soundPosition,Vector3.down*shakeStrength); _nextShake=Time.time+0.55f; }
        } else StopSound();
    }
    private void StopSound(){ if(_sound) {if(MMSoundManager.HasInstance) MMSoundManager.Instance.StopSound(_sound); else _sound.Stop(); _sound=null;} }
    private void OnDisable()=>StopSound();
}
