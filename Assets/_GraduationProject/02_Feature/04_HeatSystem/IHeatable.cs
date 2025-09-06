using UnityEngine;

public interface IHeatable
{
    public int MaxHeat { get; }
    public int CurrentHeat { get; }
    public void ChangeHeat(int amount);
    
}
