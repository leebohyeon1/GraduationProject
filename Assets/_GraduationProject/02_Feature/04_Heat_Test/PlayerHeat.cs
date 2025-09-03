using UnityEngine;

public class PlayerHeat : MonoBehaviour, IHeatable
{
    [SerializeField] private int _maxHeat = 100;
    [SerializeField] private int _currentHeat = 0;

    public int maxHeat => _maxHeat;
    public int currentHeat => _currentHeat;

    public void ChangeHeat(int amount)
    {
        _currentHeat = Mathf.Clamp(_currentHeat + amount, 0, _maxHeat);
    }
    
}
