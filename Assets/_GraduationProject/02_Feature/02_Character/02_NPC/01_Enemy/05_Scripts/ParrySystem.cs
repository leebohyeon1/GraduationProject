
using Unity.VisualScripting;
using UnityEngine;

public class ParrySystem : MonoBehaviour, IParryable, ICounterable
{
    // Parry system implementation
    public bool IsParryable { get; private set; } = false;
    public bool IsCounterable { get; private set; } = false;

    private Enemy _enemy;
    public void Initialize(Enemy enemy)
    {
        _enemy = enemy;
    }
    public void SetParryable(string value)
    {
        IsParryable = value == "true" ? true : false;
    }
    public void SetCounterAttack(bool value)
    {
        IsCounterable = value;
    }
    public void ExecuteCounterEffect()
    {
        SetCounterAttack(false);
        // _enemy.AnimationEvent("CounterAttack"); 
        // _enemy.TakeDamage(30); // 카운터 공격 시 데미지 적용 
    }

    public bool Parry(GameObject parryInstigator)
    {
        SetParryable("false");
        _enemy.StiffnessSystem.AddStiffness(_enemy.CurrentStiffness);
        return true;
    }


}