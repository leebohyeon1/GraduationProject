using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 특정 GamePlayTag의 상태에 따라 이벤트를 발생시키는 범용 핸들러
/// </summary>
public class TagHandler : MonoBehaviour
{
    public enum QueryType { All, Any, None }

    [Header("Tag Settings")]
    [Tooltip("체크할 조건 타입 (All: 모두 소지, Any: 하나라도 소지, None: 하나도 없음)")]
    [SerializeField] private QueryType _queryType = QueryType.All;
    
    [Tooltip("감시할 태그 리스트")]
    [SerializeField] private List<GamePlayTagSO> _targetTags = new List<GamePlayTagSO>();

    [Header("Response Settings")]
    [Tooltip("조건 충족 시 이 게임 오브젝트를 자동으로 활성화/비활성화할지 여부")]
    [SerializeField] private bool _autoToggleGameObject = false;

    [Header("Events")]
    public UnityEvent OnRequirementMet;    // 조건 충족 시 (처음 한 번)
    public UnityEvent OnRequirementFailed; // 조건 미충족 시 (처음 한 번)
    public UnityEvent<bool> OnStatusChanged; // 상태가 바뀔 때마다 (bool 전달)

    private bool _isMet = false;
    private bool _initialized = false;

    private void Start()
    {
        if (GamePlayTagManager.Instance != null)
        {
            GamePlayTagManager.Instance.UpdateTag += HandleTagUpdate;
        }
        
        // 씬 로드 시점이나 활성화 시점에 즉시 체크
        RefreshStatus(true);
    }

    private void OnDestroy()
    {
        if (GamePlayTagManager.Instance != null)
        {
            GamePlayTagManager.Instance.UpdateTag -= HandleTagUpdate;
        }
    }

    /// <summary>
    /// 태그가 업데이트될 때 실행되는 콜백
    /// </summary>
    private void HandleTagUpdate(GamePlayTagSO changedTag)
    {
        // 최적화: 변경된 태그가 내가 감시하는 리스트에 없으면 계산하지 않음
        // (None 타입은 어떤 태그든 영향을 줄 수 있으므로 항상 체크)
        if (_queryType != QueryType.None && !_targetTags.Contains(changedTag))
            return;

        RefreshStatus();
    }

    /// <summary>
    /// 현재 태그 상태를 기반으로 조건을 재계산
    /// </summary>
    public void RefreshStatus(bool forceInvoke = false)
    {
        if (GamePlayTagManager.Instance == null) return;

        bool currentStatus = false;

        switch (_queryType)
        {
            case QueryType.All:
                currentStatus = _targetTags.Count > 0 && _targetTags.All(t => GamePlayTagManager.Instance.HasTag(t));
                break;
            case QueryType.Any:
                currentStatus = _targetTags.Any(t => GamePlayTagManager.Instance.HasTag(t));
                break;
            case QueryType.None:
                currentStatus = ! _targetTags.Any(t => GamePlayTagManager.Instance.HasTag(t));
                break;
        }

        // 상태가 변했거나 강제 실행이 필요한 경우에만 이벤트 발생
        if (currentStatus != _isMet || !_initialized || forceInvoke)
        {
            _isMet = currentStatus;
            _initialized = true;

            ExecuteResponse(_isMet);
        }
    }

    private void ExecuteResponse(bool met)
    {
        if (_autoToggleGameObject)
        {
            gameObject.SetActive(met);
        }

        if (met) OnRequirementMet?.Invoke();
        else OnRequirementFailed?.Invoke();

        OnStatusChanged?.Invoke(met);
    }

    // 외부 스크립트에서 현재 상태를 확인하고 싶을 때 사용
    public bool IsConditionMet() => _isMet;
}
