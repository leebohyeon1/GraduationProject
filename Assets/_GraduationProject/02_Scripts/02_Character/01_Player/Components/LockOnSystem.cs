using BH_Lib.AssetManager;
using BH_Lib.DI;
using BH_Lib.Log;
using UnityEngine;

public class LockOnSystem : MonoBehaviour
{
    [SerializeField] private GameObject _lockOnIndicator;
    [SerializeField] private OnLockOnSO _onLockOnEvent;

    [Header("Scan Settings")]
    [SerializeField] private LayerMask _lockOnLayer;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _scanRadius = 10f;
    private Collider[] _scanResults = new Collider[10];

    [Header("Gizmos")]
    [SerializeField] private bool _showGizmos = true;


    private Transform _currentTarget;

    #region properties
    public GameObject LockOnIndicator => _lockOnIndicator;
    public Transform CurrentTarget => _currentTarget;
    #endregion
    private async void OnEnable()
    {
        if(_lockOnIndicator == null)
        {
            AssetManager assetManager = DIContainer.Instance.Resolve<AssetManager>();
            _lockOnIndicator = await assetManager.InstantiateAsync("LockOnIndicator", this.transform);
        }
        _lockOnIndicator.SetActive(false);
    }

    public bool LockOn()
    { 
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position + _offset, _scanRadius, _scanResults, _lockOnLayer);

        if (hitCount == 0)
        {
            return false;
        }

        Collider closest = _scanResults[0];
        float closestDistance = Vector3.Distance(transform.position + _offset, closest.transform.position);

        for (int i = 1; i < hitCount; i++)
        {
            float dist = Vector3.Distance(transform.position + _offset, _scanResults[i].transform.position);
            if (dist < closestDistance)
            {
                closest = _scanResults[i];
                closestDistance = dist;
            }
        }

        if(closest.TryGetComponent<IDamageable>(out var component))
        {
            component.OnDied += LockOff;
        }
        else
        {
            return false;
        }

        // 타겟 지정 후 활성화
        SetTarget(closest.transform);
        LockOnIndicator.SetActive(true);

        _onLockOnEvent.Publish(true);

        return true;
    }

    public void LockOff()
    {
        if (CurrentTarget.TryGetComponent<IDamageable>(out var component))
        {
            component.OnDied -= LockOff;
        }

        // 타겟 해제 후 비활성화 
        LockOnIndicator.SetActive(false);
        SetTarget(this.transform);

        _onLockOnEvent.Publish(false);
    }

    public void SetTarget(Transform target)
    {
        _currentTarget = target;
        if (_lockOnIndicator != null)
        {
            LockOnIndicator.transform.parent = target;
            LockOnIndicator.transform.localPosition = Vector3.zero;
        }
    }

    private void OnDrawGizmos()
    {
        if (!_showGizmos)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + _offset, _scanRadius);
    }
}
