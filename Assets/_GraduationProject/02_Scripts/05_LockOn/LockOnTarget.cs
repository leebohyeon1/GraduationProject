using System;
using System.Security.Cryptography;
using UnityEngine;

public class LockOnTarget : MonoBehaviour, ILockOnAble
{
    [SerializeField] private Transform _lockOnIndicatorParent;
    public Transform LockOnIndicatorParent => _lockOnIndicatorParent;

    private bool _canLockOn = true;
    public bool CanLockOn => _canLockOn;

    public event Action OnLockReleased;

    private void OnEnable()
    {
        SetCanLockOn(true);

    }

    private void Start()
    {
        if(_lockOnIndicatorParent == null)
        {
            _lockOnIndicatorParent = this.transform;
        }
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
