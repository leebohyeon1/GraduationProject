using BH_Lib.AssetManager;
using BH_Lib.DI;
using BH_Lib.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LockOnSystem : MonoBehaviour
{
    [SerializeField] private GameObject _lockOnIndicator;
    [SerializeField] private OnLockOnSO _onLockOnEvent;

    private Transform _currentTarget;
    private Camera _mainCamera;

    [Header("Scan Settings")]
    [SerializeField] private LayerMask _lockOnLayer;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _scanRadius = 10f;
    [SerializeField] private float _priorityScanAngle = 90f;
    private Collider[] _scanResults = new Collider[10];

    [Header("Gizmos")]
    [SerializeField] private bool _showGizmos = true;

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

        // 1. 검색 기준 위치 설정 (마우스 vs 플레이어 위치)
        Vector3 searchOrigin = transform.position + _offset;
        if (deviceType == InputDeviceType.KeyboardMouse)
        {
            float distanceToCamera = Vector3.Distance(transform.position, _mainCamera.transform.position);
            searchOrigin = _mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distanceToCamera));
        }

        Collider closestTarget = null;
        Collider priorityTarget = null; // 각도 내에 있는 우선순위 타겟

        float closestDist = float.MaxValue;
        float priorityDist = float.MaxValue;

        Vector3 originalPoint = transform.position;

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = _scanResults[i];
            float dist = Vector3.Distance(searchOrigin, col.transform.position);

            // A. 조건 무관 가장 가까운 타겟 갱신
            if (dist < closestDist)
            {
                closestDist = dist;
                closestTarget = col;
            }

            // B. 게임패드일 경우 우선순위(시야각) 타겟 별도 추적
            if (deviceType == InputDeviceType.Gamepad)
            {
                Vector3 dirToTarget = (col.transform.position - (transform.position + _offset)).normalized;
                if (Vector3.Angle(transform.forward, dirToTarget) <= _priorityScanAngle * 0.5f)
                {
                    if (dist < priorityDist)
                    {
                        priorityDist = dist;
                        priorityTarget = col;
                    }
                }
            }
        }

        // 3. 최종 타겟 결정 (우선순위 타겟 존재 시 그걸 사용, 아니면 일반 가장 가까운 타겟)
        Collider finalTarget = (priorityTarget != null) ? priorityTarget : closestTarget;

        // 유효성 검사 (IDamageable 등)
        if (finalTarget == null || !finalTarget.TryGetComponent<IDamageable>(out var component))
        {
            return false;
        }

        // 4. 상태 갱신
        if (_currentTarget != null && _currentTarget != transform)
        {
            LockOff();
        }

        component.OnDied -= LockOff; // 중복 구독 방지 (선택사항, 안전장치)
        component.OnDied += LockOff;

        SetTarget(finalTarget.transform);
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

        Gizmos.color = Color.yellow;
        Vector3 forward = transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, _priorityScanAngle * 0.5f, 0) * forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -_priorityScanAngle * 0.5f, 0) * forward;
        Gizmos.DrawLine(transform.position + _offset, transform.position + _offset + rightBoundary * _scanRadius);
        Gizmos.DrawLine(transform.position + _offset, transform.position + _offset + leftBoundary * _scanRadius);
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
