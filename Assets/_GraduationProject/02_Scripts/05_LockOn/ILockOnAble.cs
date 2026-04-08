using System;
using UnityEngine;

public interface ILockOnAble
{
    Transform LockOnIndicatorParent { get; }
    bool CanLockOn { get; }

    event Action OnLockReleased;
    public void SetCanLockOn(bool canLockOn);
}
