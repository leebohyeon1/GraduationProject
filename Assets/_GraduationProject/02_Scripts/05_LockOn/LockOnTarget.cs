using System;
using UnityEngine;

public class LockOnTarget : MonoBehaviour, ILockOnAble
{
    [SerializeField] private Transform _lockOnIndicatorParent;

    public Transform LockOnIndicatorParent => _lockOnIndicatorParent;

    public event Action OnLockReleased;


    private void Start()
    {
        if(_lockOnIndicatorParent == null)
        {
            _lockOnIndicatorParent = this.transform;
        }
    }

    public void TriggerLockReleased()
    {
        OnLockReleased?.Invoke();
    }

    private void OnBecameInvisible()
    {
        TriggerLockReleased();
    }
}
