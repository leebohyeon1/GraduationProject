using System;
using UnityEngine;

public class TotemDestructibleObject : MonoBehaviour, IDamageable
{
    [Header("Destructible")]
    [SerializeField] private TotemDestructibleType _destructibleType = TotemDestructibleType.Fragile;
    [SerializeField] private int _maxHealth = 1;
    [SerializeField] private bool _disableRenderersOnDestroyed = true;

    [Header("References")]
    [SerializeField] private Collider[] _colliders;
    [SerializeField] private Renderer[] _renderers;
    [SerializeField] private TotemGimmickFeedbackPlayer _feedbackPlayer;

    private int _currentHealth;
    private TotemGimmickState _state = TotemGimmickState.Alive;
    private BoxCollider[] _passageBlockers;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsDead => _state == TotemGimmickState.Destroyed;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    private void Awake()
    {
        _maxHealth = Mathf.Max(1, _maxHealth);
        _currentHealth = _maxHealth;

        // Invisible passage blockers are separate, direct children of the totem.
        // The authored collider list can contain only a null slot; cache these
        // boxes explicitly without collecting Rayfire fragment physics.
        var blockers = new System.Collections.Generic.List<BoxCollider>();
        foreach (var box in GetComponentsInChildren<BoxCollider>(true))
        {
            if (box.transform.parent == transform && !box.isTrigger &&
                !box.GetComponent<Renderer>() && !box.GetComponent<Rigidbody>() &&
                !box.GetComponent<RayFire.RayfireRigid>() &&
                !box.GetComponent<RayFire.RayfireRigidRoot>())
                blockers.Add(box);
        }
        _passageBlockers = blockers.ToArray();

        if (_colliders == null || _colliders.Length == 0)
        {
            _colliders = GetComponentsInChildren<Collider>(true);
        }

        if (_renderers == null || _renderers.Length == 0)
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
        }
    }

    public void TakeDamage(DamageData damageData)
    {
        Debug.Log($"[TotemDestructibleObject] Received damage. amount={damageData.DamageAmount}, type={damageData.AttackType}, object={name}");
        if (IsDead)
        {
            return;
        }

        UIAudioFeedback.Play(5);
        if (!CanApplyDamage(damageData.AttackType))
        {
            Debug.Log($"[TotemDestructibleObject] Blocked attack. type={damageData.AttackType}, object={name}");
            _feedbackPlayer?.PlayFeedback(TotemGimmickFeedbackType.HitBlocked, transform.position);
            return;
        }

        int damageAmount = Mathf.Max(1, damageData.DamageAmount);
        _currentHealth = Mathf.Max(0, _currentHealth - damageAmount);

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        Debug.Log($"[TotemDestructibleObject] Hit success. hp={_currentHealth}/{_maxHealth}, object={name}");
        _feedbackPlayer?.PlayFeedback(TotemGimmickFeedbackType.HitSuccess, transform.position);

        if (_currentHealth > 0)
        {
            return;
        }

        HandleDestroyed();
    }

    private bool CanApplyDamage(AttackType attackType)
    {
        Debug.Log($"[TotemDestructibleObject] Checking if can apply damage. attackType={attackType}, destructibleType={_destructibleType}, object={name}");
        if (_destructibleType == TotemDestructibleType.Fragile)
        {
            return true;
        }

        return IsStrongAttack(attackType);
    }

    private bool IsStrongAttack(AttackType attackType)
    {
        return attackType == AttackType.Strong_1
               || attackType == AttackType.Strong_2
               || attackType == AttackType.Strong_3
               || attackType == AttackType.Strong_Counter;
    }

    private void HandleDestroyed()
    {
        _state = TotemGimmickState.Destroyed;
        Debug.Log($"[TotemDestructibleObject] Destroyed. object={name}");

        DisableColliders();
        _feedbackPlayer?.PlayFeedback(TotemGimmickFeedbackType.DestroyedStart, transform.position);

        if (_disableRenderersOnDestroyed)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null)
                {
                    continue;
                }

                _renderers[i].enabled = false;
            }
        }

        _feedbackPlayer?.PlayFeedback(TotemGimmickFeedbackType.DestroyedComplete, transform.position);

        OnDied?.Invoke();
    }

    private void DisableColliders()
    {
        for (int i = 0; i < _colliders.Length; i++)
        {
            if (_colliders[i] == null)
            {
                continue;
            }

            _colliders[i].enabled = false;
        }

        // Root blockers can be added after the serialized collider list was assigned.
        // Leave unlisted child debris colliders to their destruction feedback/physics.
        foreach (Collider blocker in GetComponents<Collider>())
        {
            if (blocker != null && !blocker.isTrigger)
            {
                blocker.enabled = false;
            }
        }

        // Run before DestroyedStart activates and launches the pre-cut pieces.
        if (_passageBlockers != null)
            foreach (var blocker in _passageBlockers)
                if (blocker) blocker.enabled = false;
    }
}
