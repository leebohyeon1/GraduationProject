using System;
using System.Collections;
using BH_Lib.Log;
using UnityEngine;

public class TestEnemy : CharacterBase, IDamageable
{
    private MeshRenderer _meshRenderer;

    private int _currentHealth = 100;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    public int Health => _currentHealth;

    public int MaxHealth => 200;

    public bool IsDead => _currentHealth <= 0;

    public bool IsInvincible => false;

    public bool IsHit => throw new NotImplementedException();

    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        OnDied += Die;

        _currentHealth = MaxHealth;
    }

    public void TakeDamage(int damageAmount, IAttacker attacker)
    {
        if (IsDead) return;

        _currentHealth -= damageAmount;

        PlayFeedback("Damaged", transform.position);

        if (IsDead)
        {
            _currentHealth = 0;
            OnDied?.Invoke();
        }
    }

    private void Die()
    {
        _meshRenderer.enabled = false;
        GetComponent<Collider>().enabled = false;   

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);
        _meshRenderer.enabled = true;
        GetComponent<Collider>().enabled = true;   

        _currentHealth = MaxHealth;
    }

    public void ResetHitState()
    {
        throw new NotImplementedException();
    }
}
