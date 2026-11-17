using System;
using System.Collections;
using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;

public class MascotHitReaction : MonoBehaviour, IDamageable
{
    public MMF_Player squashFeedback;
    public float textHeight = 1.5f;
    public int CurrentHealth => 1;
    public int MaxHealth => 1;
    public bool IsDead => false;
    public event Action<int,int> OnHealthChanged { add {} remove {} }
    public event Action OnDied { add {} remove {} }
    private TextMeshPro _text;
    private Coroutine _reaction;
    private float _lastHit=-1;
    private Transform _visual;
    private Transform _squashDriver;
    private Vector3 _visualScale;
    private void Awake()
    {
        var animator=GetComponentInParent<Animator>();
        if (animator && squashFeedback)
        {
            _visual=animator.transform;
            _visualScale=_visual.localScale;
            // Keep the animated root's parent and local position unchanged.
            _squashDriver=new GameObject("MascotSquashDriver").transform;
            _squashDriver.SetParent(transform,false);
            foreach(var feedback in squashFeedback.FeedbacksList)
                if (feedback is MMF_SquashAndStretch squash) { squash.SquashAndStretchTarget=_squashDriver; squash.DetermineScaleOnPlay=true; }
        }
    }
    private void LateUpdate()
    {
        if (_visual && _squashDriver) _visual.localScale=Vector3.Scale(_visualScale,_squashDriver.localScale);
    }
    public void TakeDamage(DamageData damage)
    {
        if (damage.SourceEnemy || !damage.AttackerTransform || !damage.AttackerTransform.GetComponentInParent<PlayerController>()) return;
        if(Time.time-_lastHit<0.18f) return;
        _lastHit=Time.time;
        if(squashFeedback) squashFeedback.PlayFeedbacks();
        var settings = Resources.Load<GameUIIntegrationSettings>("GameUIIntegrationSettings");
        if (settings && settings.mascotHit && MoreMountains.Tools.MMSoundManager.HasInstance)
            MoreMountains.Tools.MMSoundManager.Instance.PlaySound(settings.mascotHit, MoreMountains.Tools.MMSoundManager.MMSoundManagerTracks.Sfx, transform.position, volume:.65f, pitch:UnityEngine.Random.Range(.95f,1.05f));
        if(!_text) {
            var go=new GameObject("MascotReactionText"); go.transform.position=transform.position;
            _text=go.AddComponent<TextMeshPro>(); _text.fontSize=4; _text.alignment=TextAlignmentOptions.Center;
            _text.rectTransform.sizeDelta=new Vector2(5,1.5f); _text.color=Color.white;
            _text.outlineWidth=0.2f;
            var config=Resources.Load<GameUIIntegrationSettings>("GameUIIntegrationSettings"); if(config) _text.font=config.guideFont;
            go.AddComponent<LocalizedGameText>();
        }
        _text.text=UnityEngine.Random.value<0.5f?"아야!":"아오!";
        if(_reaction!=null) StopCoroutine(_reaction);
        _reaction=StartCoroutine(ShowReaction());
    }
    private IEnumerator ShowReaction()
    {
        _text.gameObject.SetActive(true);
        for(float t=0;t<1.1f;t+=Time.deltaTime) {
            _text.transform.position=transform.position+Vector3.up*(Mathf.Max(textHeight+0.6f, ReactionHeight())+t*0.35f);
            if(Camera.main) _text.transform.rotation=Camera.main.transform.rotation;
            _text.alpha=1-Mathf.InverseLerp(0.65f,1.1f,t);
            yield return null;
        }
        _text.gameObject.SetActive(false); _reaction=null;
    }
    private float ReactionHeight()
    {
        float top = transform.position.y + textHeight + .6f;
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            if (!(renderer is TMPro.TMP_SubMesh)) top = Mathf.Max(top, renderer.bounds.max.y + .8f);
        return top - transform.position.y;
    }
    private void OnDestroy(){if(_text) Destroy(_text.gameObject);}
    private void OnDisable(){ if(_reaction!=null) StopCoroutine(_reaction); if(_text) _text.gameObject.SetActive(false); _reaction=null; if(_visual) _visual.localScale=_visualScale; }
}
