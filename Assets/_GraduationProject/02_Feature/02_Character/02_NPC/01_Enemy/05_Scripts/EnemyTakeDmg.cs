using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class EnemyTakeDmg : MonoBehaviour, IDamageable
{
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public bool IsDead => Health <= 0;
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;
    int _maxHealth = 100;
    public int Maxhealth => _maxHealth;
    private CharacterController _characterController;
    private Coroutine _KnockbackCoroutine;
    [Header("Knockback Settings")]
    [SerializeField] AnimationCurve _KnockbackCurve;
    [SerializeField] float _KnockbackDuration = 0.5f;
    public bool IsInvincible => throw new NotImplementedException();
    [SerializeField] float _KnockbackForce = 5f;
    Enemy _owner;


    public void Attack(IDamageable target)
    {
        if (target == null || target.IsDead)
        {
            return;
        }

    }
    public bool Knockbackable { get; private set; } = true;
    public void SetKnockbackable(bool value)
    {
        Knockbackable = value;
    }
    public void TakeDamage(int amount, int heatTier, DamageData damageData)
    {
        if (Health <= 0) return;
        _owner.groupAi.CombatAll();
        if (!_owner._aiController.IsActionable())
        {
            _owner.SetState(Enemy.EnemyState.Hit);
            _owner.AnimationEvent("Hit");
        }
        _owner.animHandler.PlayFeedback("Damage_FB");
        Health -= amount;
        Debug.Log($"Enemy took {amount} damage. Current Health: {Health}");
        if (Knockbackable)
        {
            Vector3 knockbackDir = (transform.position - damageData.AttackerTransform.position).normalized;
            knockbackDir.y = 0;
            if(_KnockbackCoroutine != null)
            {
                StopCoroutine(_KnockbackCoroutine);
            }
            _KnockbackCoroutine = StartCoroutine(KnockbackCoroutine(knockbackDir, damageData));
        }
        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }
    private IEnumerator KnockbackCoroutine(Vector3 direction, DamageData damageData)
    {
        float elapsedTime = 0;
        while (elapsedTime < damageData.KnockbackDuration)
        {
            float curveValue = damageData.KnockbackCurve.Evaluate(elapsedTime / damageData.KnockbackDuration);
            Vector3 move = direction * damageData.KnockbackForce * curveValue * Time.deltaTime;
            _characterController.Move(move);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _KnockbackCoroutine = null;
    }
    public void InitializeHealth(int maxHealth, Enemy owner)
    {
        _owner = owner;
        MaxHealth = maxHealth;
        Health = maxHealth;
        if (_owner.animator.GetBool("Die"))
            _owner.animator.SetBool("Die", false);
        _characterController = _owner.GetComponent<CharacterController>();
        SetKnockbackable(true);
    }

    public void Die()
    {
        _owner.animator.SetBool("Die", true);
        _owner.animator.speed = 1;
        _owner.Movement.StopMovement();
        _owner.SetState(Enemy.EnemyState.Die);
        _owner.groupAi.GroupRemove(_owner);
    }
    
}