using UnityEngine;
using MoreMountains.Feedbacks;

/// <summary>Revival-only protection; independent of dodge's shared Invincible tag.</summary>
public sealed class RevivalProtection : MonoBehaviour
{
    private PlayerEvents _events;
    private GameObject _shield;
    private Renderer[] _shieldRenderers;
    private Vector3 _shieldScale;
    private float _endsAt;
    private float _duration;
    private bool _active;
    public bool IsProtected => _active && Time.time < _endsAt;

    public void Activate(PlayerController player, float duration, MMF_Player feedbackPrefab, GameObject shieldPrefab = null)
    {
        EndProtection();
        _duration = Mathf.Max(0f, duration);
        _endsAt = Time.time + _duration;
        _events = player.Events;
        _events.BeforeDamaged += PreventDamage;
        _active = true;
        if (shieldPrefab)
        {
            _shield = Instantiate(shieldPrefab, player.transform, false);
            _shield.name = "RevivalProtectionShield";
            FitShieldToPlayer(player);
            _shieldScale = _shield.transform.localScale;
            _shield.transform.localScale = _shieldScale * .65f;
            _shieldRenderers = _shield.GetComponentsInChildren<Renderer>(true);
        }
        if (feedbackPrefab)
        {
            var feedback = Instantiate(feedbackPrefab, player.transform.position, Quaternion.identity);
            feedback.Initialization();
            feedback.PlayFeedbacks();
            Destroy(feedback.gameObject, Mathf.Max(6f, feedback.TotalDuration + 1f));
        }
    }

    private void FitShieldToPlayer(PlayerController player)
    {
        var controller = player.GetComponent<CharacterController>();
        var particles = _shield.GetComponent<ParticleSystem>();
        var particleRenderer = particles ? particles.GetComponent<ParticleSystemRenderer>() : null;
        if (!controller || !particles || !particleRenderer || !particleRenderer.mesh) return;

        _shield.transform.position = controller.transform.TransformPoint(controller.center);
        var main = particles.main;
        // The source sphere mesh is two units wide. Its particle size is not its world diameter.
        // Use the character capsule, excluding the weapon and any already active particle bounds.
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        Vector3 characterScale = controller.transform.lossyScale;
        float bodyHeight = Mathf.Abs(characterScale.y) * controller.height;
        float bodyWidth = Mathf.Max(Mathf.Abs(characterScale.x), Mathf.Abs(characterScale.z)) * controller.radius * 2f;
        float targetDiameter = Mathf.Max(bodyHeight, bodyWidth) * 1.15f;
        Vector3 meshSize = particleRenderer.mesh.bounds.size;
        Vector3 effectScale = _shield.transform.lossyScale;
        float meshDiameter = Mathf.Max(meshSize.x, Mathf.Max(meshSize.y, meshSize.z));
        float worldScale = Mathf.Max(Mathf.Abs(effectScale.x), Mathf.Max(Mathf.Abs(effectScale.y), Mathf.Abs(effectScale.z)));
        float currentDiameter = meshDiameter * main.startSize.constantMax * worldScale;
        if (currentDiameter > .001f && targetDiameter > .001f)
            _shield.transform.localScale *= targetDiameter / currentDiameter;
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
        // The sphere grows in, follows the player and warns before protection ends.
        if (_shield)
        {
            float appear = Mathf.SmoothStep(.65f, 1f, Mathf.Clamp01((_duration - remaining) / .18f));
            _shield.transform.localScale = _shieldScale * appear;
            float warning = Mathf.Min(.6f, _duration);
            bool visible = remaining > warning || Mathf.FloorToInt((warning - remaining) / .1f) % 2 == 0;
            foreach (var renderer in _shieldRenderers) if (renderer) renderer.enabled = visible;
        }
    }

    private void EndProtection()
    {
        if (_events != null) _events.BeforeDamaged -= PreventDamage;
        _events = null;
        _active = false;
        if (_shield) Destroy(_shield);
        _shield = null;
        _shieldRenderers = null;
    }
    private void OnDisable() => EndProtection();
    private void OnDestroy() => EndProtection();
}
