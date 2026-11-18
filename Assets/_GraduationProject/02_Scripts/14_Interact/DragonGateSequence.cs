using System.Collections;
using UnityEngine;
using MoreMountains.Tools;

public class DragonGateSequence : MonoBehaviour
{
    public GamePlayTagSO[] requiredTotems;
    public Light[] leftLights, rightLights;
    public MaterialOpacityChanger[] leftIndicators, rightIndicators;
    public CanvasGroup missingTotemsUI;
    private bool _busy, _encounterStarted;
    private WorldTextDissolve _messageDissolve;
    private PlayerQuestUI _quest;
    private void Awake() {
        SetIndicators(0);
        if(missingTotemsUI) {
            missingTotemsUI.alpha=0;
            _messageDissolve=missingTotemsUI.GetComponent<WorldTextDissolve>();
            if (!_messageDissolve) _messageDissolve=missingTotemsUI.gameObject.AddComponent<WorldTextDissolve>();
        }
    }
    private void Start() { _quest=FindFirstObjectByType<PlayerQuestUI>(); }
    private void SetIndicators(float value) {
        foreach(var indicator in leftIndicators)if(indicator)indicator.SetOpacity(value);
        foreach(var indicator in rightIndicators)if(indicator)indicator.SetOpacity(value);
    }
    private IEnumerator RevealMessage(bool visible) {
        if (!missingTotemsUI) yield break;
        missingTotemsUI.alpha=1;
        for(float t=0;t<.5f;t+=Time.unscaledDeltaTime) {
            _messageDissolve.Progress=visible?t/.5f:1f-t/.5f;
            yield return null;
        }
        _messageDissolve.Progress=visible?1f:0f;
        missingTotemsUI.alpha=visible?1f:0f;
    }
    public void Interact()
    {
        if(!_busy && !_encounterStarted)StartCoroutine(CheckTotems());
    }
    private IEnumerator CheckTotems()
    {
        _busy=true;
        if(missingTotemsUI)missingTotemsUI.alpha=0;
        foreach(var light in leftLights)if(light)light.intensity=0;
        foreach(var light in rightLights)if(light)light.intensity=0;
        foreach(var indicator in leftIndicators)if(indicator)indicator.SetOpacity(0);
        foreach(var indicator in rightIndicators)if(indicator)indicator.SetOpacity(0);
        int found=0;
        foreach(var tag in requiredTotems)if(tag && GamePlayTagManager.Instance.HasTag(tag.ID))found++;
        var config=Resources.Load<GameUIIntegrationSettings>("GameUIIntegrationSettings");
        for(int i=0;i<requiredTotems.Length;i++) {
            var left=i<leftIndicators.Length?leftIndicators[i]:null;
            var right=i<rightIndicators.Length?rightIndicators[i]:null;
            bool owned=requiredTotems[i] && GamePlayTagManager.Instance.HasTag(requiredTotems[i].ID);
            if (!owned) { yield return FlashMissing(left,right); continue; }
            // Confirm each collected totem with a discrete light and bell beat.
            if(left)left.SetOpacity(1);
            if(right)right.SetOpacity(1);
            if(config && config.gateCharge && MMSoundManager.HasInstance)
                MMSoundManager.Instance.PlaySound(config.gateCharge,MMSoundManager.MMSoundManagerTracks.Sfx,transform.position,volume:.5f,pitch:Mathf.Pow(2f,i*2f/12f));
            yield return new WaitForSeconds(.65f);
        }
        yield return new WaitForSeconds(.35f);
        if(requiredTotems.Length>0 && found==requiredTotems.Length) {
            _encounterStarted=true;
            GetComponent<InteractableObject>().OnInteract.Invoke();
        } else {
            UIAudioFeedback.Play(3);
            SetIndicators(0);
            if(_quest)_quest.ShowReminder();
            DungeonDirectionGuide.ShowRemainingDungeons();
            yield return RevealMessage(true);
            yield return new WaitForSeconds(6);
            yield return RevealMessage(false);
        }
        _busy=false;
    }
    private IEnumerator FlashMissing(params MaterialOpacityChanger[] indicators)
    {
        var originals=new System.Collections.Generic.List<(Renderer renderer,int index,MaterialPropertyBlock block)>();
        int[] colorProperties={Shader.PropertyToID("_Color"),Shader.PropertyToID("_BaseColor"),Shader.PropertyToID("_EmissionColor")};
        foreach(var indicator in indicators) if(indicator) {
            var renderer=indicator.GetComponent<Renderer>();
            if(!renderer)continue;
            var materials=renderer.sharedMaterials;
            for(int i=0;i<materials.Length;i++) {
                var material=materials[i];
                if(!material)continue;
                var original=new MaterialPropertyBlock();
                renderer.GetPropertyBlock(original,i);
                var flashing=new MaterialPropertyBlock();
                renderer.GetPropertyBlock(flashing,i);
                if(flashing.isEmpty)renderer.GetPropertyBlock(flashing);
                bool changed=false;
                foreach(int property in colorProperties)if(material.HasProperty(property)) {
                    flashing.SetColor(property,Color.red);changed=true;
                }
                if(!changed)continue;
                originals.Add((renderer,i,original));
                renderer.SetPropertyBlock(flashing,i);
            }
        }
        try {
            for(int blink=0;blink<2;blink++) {
                foreach(var indicator in indicators)if(indicator)indicator.SetOpacity(1);
                yield return new WaitForSeconds(.18f);
                foreach(var indicator in indicators)if(indicator)indicator.SetOpacity(0);
                yield return new WaitForSeconds(.18f);
            }
        } finally {
            foreach(var entry in originals)if(entry.renderer)
                entry.renderer.SetPropertyBlock(entry.block.isEmpty?null:entry.block,entry.index);
            foreach(var indicator in indicators)if(indicator)indicator.SetOpacity(0);
        }
    }
}
