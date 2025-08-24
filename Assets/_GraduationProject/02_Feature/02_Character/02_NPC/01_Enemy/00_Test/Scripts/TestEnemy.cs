using System;
using System.Collections;
using BH_Lib.Log;
using UnityEngine;

public class TestEnemy : MonoBehaviour, IDamageable
{
    private MeshRenderer _meshRenderer;

    private int _currentHealth = 100;
    public int Health => _currentHealth;

    public int MaxHealth => 200;

    public bool IsDead => _currentHealth <= 0;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        OnHealthChanged += (current, max) =>
        {
            float healthRatio = (float)current / max;
            _meshRenderer.material.color = Color.Lerp(Color.black, Color.green, healthRatio);
            if (IsDead)
            {
                Die();
            }
        };
        OnDeath += Die;

        _currentHealth = MaxHealth;
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
    }

    public void TakeDamage(int damageAmount, IAttacker attacker)
    {
        if (IsDead) return;

        _currentHealth -= damageAmount;
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);

        if (_currentHealth < 0)
        {
            _currentHealth = 0;
            OnDeath?.Invoke();
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
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
    }

    

}
