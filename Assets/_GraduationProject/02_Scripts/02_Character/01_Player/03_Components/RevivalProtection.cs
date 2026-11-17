using UnityEngine;
using HighlightPlus;
using MoreMountains.Feedbacks;

/// <summary>Revival-only protection; independent of dodge's shared Invincible tag.</summary>
public sealed class RevivalProtection : MonoBehaviour
{
    private PlayerEvents _events;
    private HighlightEffect _outline;
    private GameObject _outlineOwner;
    private float _endsAt;
    private float _duration;
    private bool _active;
    private float _originalOutline, _originalWidth;
    private Color _originalColor;
    private bool _originalHighlighted;
    public bool IsProtected => _active && Time.time < _endsAt;

    public void Activate(PlayerController player, float duration, MMF_Player feedbackPrefab)
    {
        EndProtection();
        _duration = Mathf.Max(0f, duration);
        _endsAt = Time.time + _duration;
        _events = player.Events;
        _events.BeforeDamaged += PreventDamage;
        _active = true;
        _outline = player.GetComponentInChildren<HighlightEffect>(true);
        if (!_outline)
        {
            _outlineOwner = new GameObject("RevivalOutline");
            _outlineOwner.transform.SetParent(player.transform, false);
            _outline = _outlineOwner.AddComponent<HighlightEffect>();
            _outline.SetTargets(player.transform, player.GetComponentsInChildren<SkinnedMeshRenderer>(true));
        }
        _originalOutline = _outline.outline;
        _originalWidth = _outline.outlineWidth;
        _originalColor = _outline.outlineColor;
        _originalHighlighted = _outline.highlighted;
        _outline.outlineColor = new Color(.08f, .55f, 1f, 1f);
        _outline.outlineWidth = 1.2f;
        _outline.outline = 1f;
        _outline.highlighted = true;
        _outline.UpdateMaterialProperties();
        if (feedbackPrefab)
        {
            var feedback = Instantiate(feedbackPrefab, player.transform.position, Quaternion.identity);
            feedback.Initialization();
            feedback.PlayFeedbacks();
            Destroy(feedback.gameObject, Mathf.Max(6f, feedback.TotalDuration + 1f));
        }
    }

    private void PreventDamage(ref PlayerDamageContext context)
    {
        if (!IsProtected) return;
        context.Data.DamageAmount = 0;
        context.Data.StiffnessAmount = 0;
        context.Data.KnockbackDuration = 0;
        context.Data.KnockbackForce = 0;
        context.HasSuperArmor = true;
    }

    private void Update()
    {
        if (!_active) return;
        float remaining = _endsAt - Time.time;
        if (remaining <= 0f) { EndProtection(); return; }
        // Last 0.6 seconds: three clear on/off pulses, then restore the original effect.
        if (_outline)
        {
            float warning = Mathf.Min(.6f, _duration);
            float intensity = remaining > warning || Mathf.FloorToInt((warning - remaining) / .1f) % 2 == 0 ? 1f : 0f;
            if (_outline.outline != intensity)
            {
                _outline.outline = intensity;
                _outline.UpdateMaterialProperties();
            }
        }
    }

    private void EndProtection()
    {
        if (_events != null) _events.BeforeDamaged -= PreventDamage;
        _events = null;
        if (_active && _outline)
        {
            _outline.outline = _originalOutline;
            _outline.outlineWidth = _originalWidth;
            _outline.outlineColor = _originalColor;
            _outline.highlighted = _originalHighlighted;
            _outline.UpdateMaterialProperties();
        }
        _active = false;
        if (_outlineOwner) Destroy(_outlineOwner);
        _outlineOwner = null;
        _outline = null;
    }
    private void OnDisable() => EndProtection();
    private void OnDestroy() => EndProtection();
}
