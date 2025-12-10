using System;
using UnityEngine;

public class SurvivorLikeEnemyConfig : MonoBehaviour
{
    private IDamageable _damageable;

    public event Action<GameObject> Died;

    private void Start()
    {
        _damageable = GetComponent<IDamageable>();

        _damageable.OnDied += OnDied;
    }

    private void OnDisable()
    {
        _damageable.OnDied -= OnDied;
    }

    private void OnDied()
    {
        gameObject.SetActive(false);
        Died?.Invoke(gameObject);
    }
}
