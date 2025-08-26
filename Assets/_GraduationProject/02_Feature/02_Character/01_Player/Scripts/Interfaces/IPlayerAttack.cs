using UnityEngine;

public interface IPlayerAttack: IAttacker
{
    void TryAttack(InputDeviceType deviceType, Vector2 lookInput, Vector2 mousePosition);
    void PerformAttack();
    void ResetComboCount();
    float AttackSpeed { get; }
    float AttackRadius { get; }
    Transform AttackPoint { get; }
    int ComboCount { get; }
}
