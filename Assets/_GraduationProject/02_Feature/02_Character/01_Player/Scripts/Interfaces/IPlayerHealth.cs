using System;

public interface IPlayerHealth : IDamageable, IHealable
{
    void ResetHitState();
    bool IsAlive { get; }
    bool IsHit { get; }
}
