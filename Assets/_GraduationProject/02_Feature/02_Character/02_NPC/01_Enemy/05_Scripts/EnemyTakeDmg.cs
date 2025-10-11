using System;
using UnityEngine;

public class EnemyTakeDmg : MonoBehaviour, IDamageable
{
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public bool IsDead => Health <= 0;
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;
    int _maxHealth = 100;
    public int Maxhealth => _maxHealth;
    
    public bool IsInvincible => throw new NotImplementedException();

    Enemy _owner;

    public void TakeDamage(int amount)
    {
        if (Health <= 0) return;
        _owner.groupAi.CombatAll();
        if (!_owner._aiController.IsActionable())
        {
            _owner.SetState(Enemy.EnemyState.Hit);
        }
        Health -= amount;
        Debug.Log($"Enemy took {amount} damage. Current Health: {Health}");

        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }
    public void Attack(IDamageable target)
    {
        if (target == null || target.IsDead)
        {
            return;
        }

    }

    public void TakeDamage(int amount, int StiffenessAmount)
    {
        if (Health <= 0) return;
        _owner.groupAi.CombatAll();
        if (!_owner._aiController.IsActionable())
        {
            _owner.SetState(Enemy.EnemyState.Hit);
        }
        Health -= amount;
        Debug.Log($"Enemy took {amount} damage. Current Health: {Health}");

        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }
    public void InitializeHealth(int maxHealth, Enemy owner)
    {
        _owner = owner;
        MaxHealth = maxHealth;
        Health = maxHealth;
        if(_owner.animator.GetBool("Die"))
            _owner.animator.SetBool("Die", false);
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