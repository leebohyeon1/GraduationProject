using UnityEngine;

public interface IHeatable
{
    public int maxHeat { get; }
    public int currentHeat { get; }
    public void ChangeHeat(int amount);
}
