using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 락온 시스템을 구현한 클래스
/// </summary>
public class LockOnSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _lockOnIndicator;
    [SerializeField] private OnLockOnSO _onLockOnEvent;

    private Transform _currentTarget;
    private Camera _mainCamera;

    [Header("Scan Settings")]
    [SerializeField] private LayerMask _lockOnLayer;
    [SerializeField] private float _scanRadius = 10f;
    [SerializeField] private float _priorityScanAngle = 90f;
    private Collider[] _scanResults = new Collider[10];

    public bool IsLockOn {get; private set;}    

    [Header("Gizmos")]
    [SerializeField] private bool _showGizmos = true;

    [Header("Properties")]
    public GameObject LockOnIndicator => _lockOnIndicator;
    public Transform CurrentTarget => _currentTarget;

    private async void Start()
    {
        if(_lockOnIndicator == null)
        {
            _lockOnIndicator = await Addressables.InstantiateAsync("LockOnIndicator").Task;
        }

        _lockOnIndicator.SetActive(false);

        _mainCamera = Camera.main;
    }

    private void OnDisable()
    {
        LockOff();
    }

    /// <summary>
    /// 락온
    /// </summary>
    /// <param name="deviceType">입력 기기</param>
    /// <param name="lockOnInput">락온 방향</param>
    /// <param name="mousePosition">마우스 위치</param>
    /// <returns></returns>
    public bool LockOn(InputDeviceType deviceType, Vector2 lockOnInput, Vector2 mousePosition)
    { 
        // 1. 검색 기준 위치 설정 (마우스 vs 플레이어 위치)
        Vector3 searchOrigin = transform.position;
        if (deviceType == InputDeviceType.KeyboardMouse)
        {
            float distanceToCamera = Vector3.Distance(transform.position, _mainCamera.transform.position);
            searchOrigin = _mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distanceToCamera));
        }

        Collider target = FindBestTarget(searchOrigin, deviceType == InputDeviceType.Gamepad);

        // 유효성 검사
        if (target == null || !target.TryGetComponent<ILockOnAble>(out var component))
        {
            return false;
        }

        // 4. 상태 갱신
        ApplyLockOn(target.transform);

        return true;
    }

    /// <summary>
    /// 락온 타겟 변경
    /// </summary>
    /// <param name="deviceType">입력 기기</param>
    /// <param name="lockOnInput">락온 방향</param>
    /// <param name="mousePosition">마우스 위치</param>
    public void ChangeLockOnTarget(InputDeviceType deviceType, Vector2 lockOnInput, Vector2 mousePosition)
    {
        if (deviceType == InputDeviceType.Gamepad)
        {
            Vector3 originalOffset = Vector3.zero;

            originalOffset.x += lockOnInput.x * 10;
            originalOffset.z += lockOnInput.y * 10;

            Vector3 searchOrigin = CurrentTarget.position + originalOffset;

            Collider target = FindBestTarget(searchOrigin, true);

            // 유효성 검사
            if (target == null || !target.TryGetComponent<ILockOnAble>(out var component))
            {
                return;
            }

            // 상태 갱신
            ApplyLockOn(target.transform);
        }
        else if(deviceType == InputDeviceType.KeyboardMouse)
        {
            // 1. 검색 기준 위치 설정 (마우스 vs 플레이어 위치)
            float distanceToCamera = Vector3.Distance(transform.position, _mainCamera.transform.position);
            Vector3 searchOrigin = _mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distanceToCamera));

            Collider target = FindBestTarget(searchOrigin, false);

            // 유효성 검사
            if (target == null || !target.TryGetComponent<ILockOnAble>(out var component))
            {
                return;
            }

            // 상태 갱신
            ApplyLockOn(target.transform);
        }

    }

    /// <summary>
    /// 현재 위치 기준으로 락온 타겟 변경
    /// </summary>
    public void ChangeLockOnTarget()
    {
        Collider target = FindBestTarget(transform.position, true);

        // 유효성 검사
        if (target == null || !target.TryGetComponent<ILockOnAble>(out var component))
        {
            return;
        }

        // 상태 갱신
        ApplyLockOn(target.transform);
    }

    /// <summary>
    /// 락온 해제
    /// </summary>
    public void LockOff()
    {
        if(CurrentTarget == null || CurrentTarget == this.transform)
        {
            return;
        }

        if (CurrentTarget.TryGetComponent<ILockOnAble>(out var lockOnAble))
        {
            lockOnAble.OnLockReleased -= LockOff;
        }

        if(CurrentTarget.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.OnDied -= ChangeLockOnTarget;
        }

        // 타겟 해제 후 비활성화 
        LockOnIndicator.SetActive(false);
        SetTarget(this.transform);

        _onLockOnEvent.Publish(false);
    }

    /// <summary>
    /// 타겟 설정
    /// </summary>
    /// <param name="target">타겟</param>
    public void SetTarget(Transform target)
    {
        _currentTarget = target;
        if (_lockOnIndicator != null)
        {
            LockOnIndicator.transform.parent = target;
            LockOnIndicator.transform.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// 최적의 락온 타겟 탐색    
    /// </summary>
    /// <param name="searchOrigin">타겟 중심점</param>
    /// <param name="usePriorityAngle">우선순위 각도 사용 여부</param>
    /// <returns></returns>
    private Collider FindBestTarget(Vector3 searchOrigin, bool usePriorityAngle)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(searchOrigin, _scanRadius, _scanResults, _lockOnLayer);

        if (hitCount == 0)
        {
            return null;
        }

        Collider closestTarget = null;
        Collider priorityTarget = null; // 각도 내에 있는 우선순위 타겟

        float closestDist = float.MaxValue;
        float priorityDist = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = _scanResults[i];

            if(!IsTargetValid(hitCollider))
            {
                continue;
            }

            float dist = Vector3.Distance(searchOrigin, hitCollider.transform.position);

            // A. 조건 무관 가장 가까운 타겟 갱신
            if (dist < closestDist)
            {
                closestDist = dist;
                closestTarget = hitCollider;
            }

            if (usePriorityAngle)
            {
                // 우선순위(시야각) 타겟 별도 추적
                Vector3 dirToTarget = (hitCollider.transform.position - (transform.position)).normalized;
                if (Vector3.Angle(transform.forward, dirToTarget) <= _priorityScanAngle * 0.5f)
                {
                    if (dist < priorityDist)
                    {
                        priorityDist = dist;
                        priorityTarget = hitCollider;
                    }
                }
            }
        }

        return priorityTarget != null ? priorityTarget : closestTarget;
    }

    /// <summary>
    /// 타겟이 락온 가능한 유효한 상태인지 검사
    /// </summary>
    private bool IsTargetValid(Collider collider)
    {
        // 자기 자신이거나 현재 타겟이면 제외
        if (collider.transform == transform)
        {
            return false;
        }
        if (_currentTarget != null && collider.transform == _currentTarget)
        {
            return false;
        }

        // 화면 밖인지 검사
        Vector3 viewPos = _mainCamera.WorldToViewportPoint(collider.transform.position);
        if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1)
        {
            return false;
        }

        // ILockOnAble 컴포넌트 여부
        if (!collider.TryGetComponent<ILockOnAble>(out _))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 락온 적용
    /// </summary>
    /// <param name="newTarget">락온 대상</param>
    private void ApplyLockOn(Transform newTarget)
    {
        // 기존 타겟 정리
        LockOff();

        // 새 타겟 설정
        _currentTarget = newTarget;

        // 이벤트 구독
        if (_currentTarget.TryGetComponent<ILockOnAble>(out var newlockOnAble))
        {
            newlockOnAble.OnLockReleased += LockOff;
        }

        if (_currentTarget.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.OnDied += ChangeLockOnTarget;
        }

        // 인디케이터 이동
        SetTarget(_currentTarget.transform);
        _lockOnIndicator.SetActive(true);
        _onLockOnEvent.Publish(true);
    }

    private void OnDrawGizmos()
    {
        if (!_showGizmos)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _scanRadius);

        Gizmos.color = Color.yellow;
        Vector3 forward = transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, _priorityScanAngle * 0.5f, 0) * forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -_priorityScanAngle * 0.5f, 0) * forward;
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * _scanRadius);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * _scanRadius);
    }

}
