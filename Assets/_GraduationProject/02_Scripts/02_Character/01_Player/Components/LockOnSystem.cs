using BH_Lib.AssetManager;
using BH_Lib.DI;
using BH_Lib.Log;
using UnityEngine;
using UnityEngine.Rendering;

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
    private Camera _mainCamera;

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

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    public bool LockOn(InputDeviceType deviceType, Vector2 moveInput, Vector2 mousePosition)
    { 
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position + _offset, _scanRadius, _scanResults, _lockOnLayer);

        if (hitCount == 0)
        {
            return false;
        }

        Collider closest = _scanResults[0];
        float closestDistance = Vector3.Distance(transform.position + _offset, closest.transform.position);

        Vector3 point = transform.position;

        switch (deviceType)
        {
            case InputDeviceType.KeyboardMouse:
                float distanceToCamera = Vector3.Distance(transform.position, _mainCamera.transform.position);
                point = _mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distanceToCamera));
                break;
            case InputDeviceType.Gamepad:
                point = transform.position + _offset;
                break;
        }

        for (int i = 1; i < hitCount; i++)
        {
            float dist = Vector3.Distance(point, _scanResults[i].transform.position);

            if (dist < closestDistance)
            {
                closest = _scanResults[i];
                closestDistance = dist;
            }
        }

        if (_currentTarget != null && _currentTarget != this.transform)
        {
            LockOff(); // 이 함수는 내부적으로 SetTarget(this.transform)을 호출하므로, 아래 SetTarget이 필요 없음
        }

        if (closest.TryGetComponent<IDamageable>(out var component))
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

    private void OnDisable()
    {
        if (_currentTarget != null && _currentTarget != this.transform)
        {
            if (_currentTarget.TryGetComponent<IDamageable>(out var component))
            {
                component.OnDied -= LockOff;
            }
        }
    }
}
