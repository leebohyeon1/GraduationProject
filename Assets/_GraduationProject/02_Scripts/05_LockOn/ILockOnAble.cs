using System;
using UnityEngine;

public interface ILockOnAble
{
    Transform LockOnIndicatorParent { get; }

    event Action OnLockReleased;
}
