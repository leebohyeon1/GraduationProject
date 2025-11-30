using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 장애물 투명화 컨트롤러
/// </summary>
public class SeeThroughController : MonoBehaviour, IEventListener<SeeThroughTargetTransform>
{
    [SerializeField] private OnRegisterSeeThroughTargetSO _onRegisterCutOutTargetSO;

    [Header("Target Settings")]
    private HashSet<Transform> _targetObjects = new HashSet<Transform>();

    [Header("Layer Settings")]
    public LayerMask _wallLayer;

    private Camera _mainCamera;

    private HashSet<SeeThroughObject> _previouslyHitObjects = new HashSet<SeeThroughObject>();

    public const float VALUE_VISIBLE = 0.0f;
    public const float VALUE_INVISIBLE = 1.0f;

    private void Awake()
    {
        _onRegisterCutOutTargetSO.Subscribe(this);
    }

    private void OnDestroy()
    {
        _onRegisterCutOutTargetSO.Unsubscribe(this);
    }

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_targetObjects.Count == 0)
        {
            return;
        }

        HandleOcclusionCurve();
    }

    void HandleOcclusionCurve()
    {
        HashSet<SeeThroughObject> newHits = new HashSet<SeeThroughObject>();

        foreach (var target in _targetObjects)
        {
            Vector3 dir = target.position - _mainCamera.transform.position;
            float dist = dir.magnitude;
            
            RaycastHit[] hits = Physics.RaycastAll(_mainCamera.transform.position, dir, dist, _wallLayer);

            foreach (RaycastHit hit in hits)
            {
                SeeThroughObject cutOutObject = hit.collider.GetComponent<SeeThroughObject>();
                if (cutOutObject != null)
                {
                    newHits.Add(cutOutObject);
                    cutOutObject.SetTarget(target); // 타겟 정보 전달
                    cutOutObject.SetOcclusionStatus(true);
                }
            }
        }

        // 이전 프레임에서는 감지되었지만, 현재 프레임에서는 감지되지 않은 오브젝트 처리
        foreach (SeeThroughObject prevHit in _previouslyHitObjects)
        {
            if (!newHits.Contains(prevHit))
            {
                prevHit.SetOcclusionStatus(false);
            }
        }
        
        _previouslyHitObjects = newHits;
    }
    
    public void OnEventTrigger(SeeThroughTargetTransform value)
    {
        if(value.IsRegister && !_targetObjects.Contains(value.Target))
        {
            _targetObjects.Add(value.Target);    
        }
        else
        {
            _targetObjects.Remove(value.Target);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (_targetObjects != null && _mainCamera != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform t in _targetObjects) {
                Gizmos.DrawLine(_mainCamera.transform.position, t.position);
            } 
        }
    }
}
