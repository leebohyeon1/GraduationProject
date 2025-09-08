using UnityEngine;

public interface IPlayerRangedAttack
{
    public float RangedAttackChargeTime { get; }
    public int RangedAttackDamage { get; }
    
    public void RotateTowardsAimDirection(InputDeviceType deviceType, Vector2 lookInput, Vector2 mousePosition);
    public void FireProjectile();
}
