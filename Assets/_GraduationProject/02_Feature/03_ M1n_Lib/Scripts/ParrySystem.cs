
using UnityEngine;

public class ParrySystem : MonoBehaviour, IParryable
{
    // Parry system implementation
    public bool IsParryable { get;private set; } = false;

    public bool CanCounterAttack { get;private set; } = false;
    private Enemy _enemy;
    private SourceMapDatabaseSO _database;
    public void Initialize(SourceMapDatabaseSO Database, Enemy enemy)
    {
        _enemy = enemy;
        _database = Database;
    }
    public void SetParryable(string value)
    {
        IsParryable = value == "true" ? true : false;
    }
    public void SetCounterAttack(string value)
    {
        CanCounterAttack = value == "true" ? true : false;
    }
    public void CounterAttack()
    {
        SetCounterAttack("false");
        // _enemy.AnimationEvent("CounterAttack");
    }

    public bool Parry(GameObject parryInstigator)
    {
        SetParryable("false");
        return true;
    }
}