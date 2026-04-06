using System;
using System.Security.Cryptography;
using UnityEngine;

public class LockOnTarget : MonoBehaviour, ILockOnAble
{
    [SerializeField] private Transform _lockOnIndicatorParent;
    public Transform LockOnIndicatorParent => _lockOnIndicatorParent;

    private bool _canLockOn;
    public bool CanLockOn => _canLockOn;

    public event Action OnLockReleased;


    private void Start()
    {
        if(_lockOnIndicatorParent == null)
        {
            _lockOnIndicatorParent = this.transform;
        }

        _canLockOn = true;
    }

    public void SetCanLockOn(bool canLockOn)
    {
        _canLockOn = canLockOn;
    }

    public void TriggerLockReleased()
    {
        SetCanLockOn(false);
        OnLockReleased?.Invoke();
    }

    private void OnBecameInvisible()
    {
        TriggerLockReleased();
    }

    private void OnDisable()
    {
        TriggerLockReleased();
    }
}
