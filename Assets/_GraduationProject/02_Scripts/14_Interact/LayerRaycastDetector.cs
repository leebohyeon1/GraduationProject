using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 특정 방향으로 레이캐스트를 쏘아 처음 부딪힌 오브젝트의 레이어를 감지하고 이벤트를 발생시키는 컴포넌트
/// </summary>
public class LayerRaycastDetector : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("레이 발사 시작 지점 오프셋")]
    [SerializeField] private Vector3 _rayOffset = Vector3.zero;
    
    [Tooltip("레이 발사 방향")]
    [SerializeField] private Vector3 _rayDirection = Vector3.down;
    
    [Tooltip("레이 발사 거리")]
    [SerializeField] private float _rayDistance = 1.0f;
    
    [Tooltip("레이가 충돌할 수 있는 레이어들 (타겟 레이어와 방해물 레이어를 모두 포함해야 함)")]
    [SerializeField] private LayerMask _hitMask = -1; 
    
    [Tooltip("트리거 콜라이더 감지 여부 설정")]
    [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.UseGlobal;

    [Header("Detection Target")]
    [Tooltip("감지하고자 하는 레이어 이름")]
    [SerializeField] private string _targetLayerName = "Water";
    
    [Header("Response Settings")]
    [Tooltip("상태에 따라 활성화/비활성화할 오브젝트 (필요 없으면 비워둠)")]
    [SerializeField] private GameObject _targetObject; 
    
    [Tooltip("논리 반전 (체크 시 레이어 감지 시 오브젝트 비활성화, 미감지 시 활성화)")]
    [SerializeField] private bool _invertLogic = false; 

    [Header("Events")]
    public UnityEvent OnTargetLayerDetected;
    public UnityEvent OnTargetLayerLost;

    private int _targetLayerIndex;
    private bool _isDetected = false;

    private void Awake()
    {
        _targetLayerIndex = LayerMask.NameToLayer(_targetLayerName);
        if (_targetLayerIndex == -1)
        {
            Debug.LogWarning($"<color=yellow>[LayerRaycastDetector]</color> {gameObject.name}: '{_targetLayerName}' 레이어를 찾을 수 없습니다. 레이어 이름을 확인해주세요.");
        }
    }

    private void Update()
    {
        CheckLayer();
    }

    private void CheckLayer()
    {
        if (_targetLayerIndex == -1) return;

        Vector3 origin = transform.position + _rayOffset;
        bool hitSomething = Physics.Raycast(origin, _rayDirection, out RaycastHit hit, _rayDistance, _hitMask, _triggerInteraction);
        
        bool currentlyDetected = false;
        
        if (hitSomething)
        {
            // 처음 부딪힌 오브젝트의 레이어가 타겟 레이어인지 확인
            if (hit.collider.gameObject.layer == _targetLayerIndex)
            {
                currentlyDetected = true;
            }
        }

        // 상태 변화가 있을 때만 이벤트 및 상태 업데이트 수행
        if (currentlyDetected != _isDetected)
        {
            _isDetected = currentlyDetected;
            ApplyResponse(_isDetected);
        }
    }

    private void ApplyResponse(bool detected)
    {
        if (detected)
        {
            OnTargetLayerDetected?.Invoke();
            if (_targetObject != null) _targetObject.SetActive(!_invertLogic);
        }
        else
        {
            OnTargetLayerLost?.Invoke();
            if (_targetObject != null) _targetObject.SetActive(_invertLogic);
        }
    }

    /// <summary>
    /// 에디터 뷰에서 레이캐스트 범위를 시각적으로 확인
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _isDetected ? Color.green : Color.cyan;
        Vector3 origin = transform.position + _rayOffset;
        Gizmos.DrawRay(origin, _rayDirection * _rayDistance);
        
        // 레이 끝 지점에 작은 구체 표시
        Gizmos.DrawWireSphere(origin + _rayDirection * _rayDistance, 0.1f);
    }

    // 현재 감지 상태를 외부에서 확인할 수 있는 프로퍼티
    public bool IsDetected => _isDetected;
}
