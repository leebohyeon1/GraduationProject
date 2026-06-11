using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 락온 시스템을 구현한 클래스
/// </summary>
public class PlayerLockOn : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _lockOnIndicator;

    private Transform _currentTarget;
    private Camera _mainCamera;

    [Header("Scan Settings")]
    [SerializeField] private LayerMask _lockOnLayer;
    [SerializeField] private float _scanRadius = 10f;
    private Collider[] _scanResults = new Collider[10];

    public bool IsLockOn {get; private set;}    

    [Header("Gizmos")]
    [SerializeField] private bool _showGizmos = true;

    [Header("LockOn Cooldown")]
    [SerializeField] private float _lockOnCooldown = 0.5f;
    private float _lastLockOnTime = -1f;

    public event Action<bool> LockOnEvent;

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

    private void OnDestroy()
    {
        LockOnEvent = null;
    }

    /// <summary>
    /// 락온 가능 여부 체크 (쿨타임 포함)
    /// </summary>
    private bool CanLockOnOperation()
    {
        return Time.time >= _lastLockOnTime + _lockOnCooldown;
    }

    /// <summary>
    /// 락온
    /// </summary>
    /// <param name="deviceType">입력 기기</param>
    /// <param name="mousePosition">마우스 위치</param>
    /// <returns></returns>
    public bool LockOn(InputDeviceType deviceType, Vector3 mousePosition)
    { 
        if (!CanLockOnOperation()) return false;

        if (deviceType == InputDeviceType.KeyboardMouse)
        {
            float distanceToCamera = Vector3.Distance(transform.position, _mainCamera.transform.position);
            Vector3 searchOrigin = _mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.z, distanceToCamera));
            Collider target = FindBestTarget(searchOrigin, Vector3.zero);

            if (target != null)
            {
                ApplyLockOn(target.transform);
                return true;
            }
        }
        else if (deviceType == InputDeviceType.Gamepad)
        {
            // 패드 락온: 플레이어 위치에서 카메라 전방 방향(또는 입력 방향) 벡터를 기준으로 탐색
            Vector3 inputDir = _mainCamera.transform.forward;
            inputDir.y = 0;
            inputDir.Normalize();

            Collider target = FindBestTarget(transform.position, inputDir);

            if (target != null)
            {
                ApplyLockOn(target.transform);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 락온 해제
    /// </summary>
    public void LockOff()
    {
        if (CurrentTarget == null || CurrentTarget == this.transform)
        {
            IsLockOn = false;
            return;
        }

        if (!CanLockOnOperation()) return;

        if (CurrentTarget.TryGetComponent<ILockOnAble>(out var lockOnAble))
        {
            lockOnAble.SetCanLockOn(true);
            lockOnAble.OnLockReleased -= ChangeLockOnTarget;
        }

        // 타겟 해제 후 비활성화 
        LockOnIndicator.SetActive(false);
        SetTarget(this.transform, null);
        IsLockOn = false;
        LockOnEvent?.Invoke(false);

        _lastLockOnTime = Time.time;
    }

    /// <summary>
    /// 락온 대상 변경
    /// 현재 타겟과 낵가 락온한 방향으로 벡터를 그어주고
    /// 그 벡터와 가깝고 나와 가까운 적을 락온
    /// </summary>
    /// <param name="changeDirection">변경 방향 (패드 스틱 입력)</param>
    public void ChangeLockOnTargetByGamePad(Vector2 changeDirection)
    {
        if (!CanLockOnOperation()) return;

        if (!IsLockOn || CurrentTarget == null)
        {
            // 락온 중이 아닐 때 패드 입력이 들어오면 (드물지만) 일반 락온 시도
            LockOn(InputDeviceType.Gamepad, Vector3.zero);
            return;
        }

        // 카메라 기준 입력 방향 벡터 계산
        Vector3 cameraForward = _mainCamera.transform.forward;
        Vector3 cameraRight = _mainCamera.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 targetDir = (cameraForward * changeDirection.y + cameraRight * changeDirection.x).normalized;

        // 현재 락온 대상 위치에서 입력 방향으로 벡터를 그어 탐색
        Collider target = FindBestTarget(CurrentTarget.position, targetDir);

        if (target != null)
        {
            ApplyLockOn(target.transform);
        }
    }

    /// <summary>
    /// 락온 대상 변경
    /// 마우스 위치 주변에서
    /// 가장 가까운 적 탐색
    /// </summary>
    /// <param name="mousePosition">마우스 위치</param>
    public void ChangeLockOnTargetByMouse(Vector3 mousePosition)
    {
        if (!CanLockOnOperation()) return;

        float distanceToCamera = Vector3.Distance(transform.position, _mainCamera.transform.position);
        Vector3 searchOrigin = _mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.z, distanceToCamera));

        Collider target = FindBestTarget(searchOrigin, Vector3.zero);

        if (target != null)
        {
            ApplyLockOn(target.transform);
        }
        else
        {
            LockOff();
        }
    }

    /// <summary>
    /// 현재 위치 기준으로 락온 타겟 변경
    /// </summary>
    public void ChangeLockOnTarget()
    {
        // 쿨타임 체크 생략 (타겟이 파괴되거나 등의 강제 해제 상황 대응을 위해)
        Collider target = FindBestTarget(transform.position, _mainCamera.transform.forward);

        if (target != null)
        {
            ApplyLockOn(target.transform);
        }
        else
        {
            LockOff();
        }
    }

    /// <summary>
    /// 최적의 락온 타겟 탐색    
    /// </summary>
    /// <param name="searchOrigin">탐색 시작점 (플레이어 또는 현재 타겟 위치)</param>
    /// <param name="directionVector">기준 방향 벡터 (Vector3.zero일 경우 거리만 체크)</param>
    /// <returns></returns>
    private Collider FindBestTarget(Vector3 searchOrigin, Vector3 directionVector)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _scanRadius, _scanResults, _lockOnLayer);

        if (hitCount == 0) return null;

        Collider bestTarget = null;
        float highestScore = float.MinValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = _scanResults[i];
            if (hitCollider == null || !IsTargetValid(hitCollider)) continue;

            float distanceToPlayer = Vector3.Distance(transform.position, hitCollider.transform.position);
            
            // 점수 계산 로직: 
            // 1. 거리가 가까울수록 가점
            // 2. 입력 방향 벡터와 타겟 방향 벡터가 일치할수록 가점
            
            float score = (1f - (distanceToPlayer / _scanRadius)) * 10f; // 거리 점수 (최대 10점)

            if (directionVector != Vector3.zero)
            {
                Vector3 dirToTarget = (hitCollider.transform.position - searchOrigin).normalized;
                float dot = Vector3.Dot(directionVector.normalized, dirToTarget);
                
                // 도트 프로덕트가 0보다 큰 경우(전방)에만 가중치 부여
                if (dot > 0)
                {
                    score += dot * 20f; // 방향 점수 (최대 20점)
                }
                else
                {
                    score -= 10f; // 반대 방향일 경우 감점
                }
            }

            if (score > highestScore)
            {
                highestScore = score;
                bestTarget = hitCollider;
            }
        }

        return bestTarget;
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

        // ILockOnAble 컴포넌트 여부
        if (!collider.TryGetComponent<ILockOnAble>(out var lockOnAble) || !lockOnAble.CanLockOn)
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

        // 이벤트 구독
        if (newTarget.TryGetComponent<ILockOnAble>(out var newlockOnAble))
        {
            newlockOnAble.SetCanLockOn(false);
            newlockOnAble.OnLockReleased += ChangeLockOnTarget;

            // 인디케이터 이동
            SetTarget(newTarget, newlockOnAble.LockOnIndicatorParent);
            _lockOnIndicator.SetActive(true);
            IsLockOn = true;
            LockOnEvent?.Invoke(true);

            _lastLockOnTime = Time.time;
        }

    }

    /// <summary>
    /// 타겟 설정
    /// </summary>
    /// <param name="target">타겟</param>
    public void SetTarget(Transform target, Transform indicatorTranform)
    {
        _currentTarget = target;
        if (_lockOnIndicator != null)
        {
            LockOnIndicator.transform.parent = indicatorTranform;
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
        Gizmos.DrawWireSphere(transform.position, _scanRadius);
    }

}
