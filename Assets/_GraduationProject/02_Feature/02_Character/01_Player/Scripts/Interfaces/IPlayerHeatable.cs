using UnityEngine;

public interface IPlayerHeatable : IHeatable
{
    int GetCostMana(string id, int tier = -1);
}
