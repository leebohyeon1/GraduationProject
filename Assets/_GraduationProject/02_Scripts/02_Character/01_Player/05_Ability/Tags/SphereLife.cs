using UnityEngine;
using MoreMountains.Feedbacks;

[CreateAssetMenu(fileName = "SphereLife", menuName = "Project/Player/Ability/Tag/SphereLife")]
public class SphereLife : PlayerAbilityTagSO
{
    private PlayerController _player;
    private bool _used;
    public float _invincibleDuration = 5f;
    public PlayerAbilityTagSO _invincibleTag;
    public MMF_Player reviveFeedbackPrefab;
    public GameObject protectionEffectPrefab;

    public override void Apply(PlayerController player)
    {
        _player = player;
        _used = false;
        player.Health.OnHealthChanged += OnHealthChanged;
    }

    public override void Revert(PlayerController player)
    {
        player.Health.OnHealthChanged -= OnHealthChanged;
        _player = null;
    }

    private void OnHealthChanged(int previousHealth, int currentHealth)
    {
        if (_used || !_player || previousHealth <= 0 || currentHealth > 0) return;
        // Consume this life before raising the nested health-change event.
        _used = true;
        var protection = _player.GetComponent<RevivalProtection>();
        if (!protection) protection = _player.gameObject.AddComponent<RevivalProtection>();
        protection.Activate(_player, _invincibleDuration, reviveFeedbackPrefab, protectionEffectPrefab);
        _player.Health.ChangeHealth(1 - currentHealth);
    }
}
