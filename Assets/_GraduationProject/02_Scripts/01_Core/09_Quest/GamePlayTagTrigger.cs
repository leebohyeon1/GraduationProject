using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GamePlayTagTrigger : MonoBehaviour
{
    [Header("트리거 발동 조건 (선택 사항)")]
    [Tooltip("이 태그들을 모두 가지고 있어야 트리거가 발동합니다.")]
    [SerializeField] private List<GamePlayTagSO> _requiredTags;
    
    [Tooltip("이 태그들 중 하나라도 가지고 있으면 트리거가 발동하지 않습니다.")]
    [SerializeField] private List<GamePlayTagSO> _forbiddenTags;

    [Header("결과: 추가할 태그 목록")]
    [SerializeField] private List<GamePlayTagSO> _tagsToAdd;

    [Header("옵션 설정")]
    [SerializeField] private bool _oneTimeOnly = true;
    private bool _hasTriggered = false;

    private void Awake()
    {
        // IsTrigger 설정 자동화
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. 기본 상태 체크 (이미 발동했거나 플레이어가 아니면 리턴)
        if (_oneTimeOnly && _hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        // 2. 태그 조건 검사 (필수/금지 태그 확인)
        if (!CheckConditions()) return;

        bool isNewTagAdded = false;

        // 3. 태그 추가 시도
        foreach (var tag in _tagsToAdd)
        {
            if (tag == null) continue;

            // 현재 플레이어가 해당 태그를 가지고 있지 않을 때만 추가
            if (!GamePlayTagManager.Instance.HasTag(tag))
            {
                GamePlayTagManager.Instance.AddTag(tag);
                isNewTagAdded = true;
            }
        }

        // 4. 새로운 태그가 하나라도 추가되었다면 트리거 완료 처리
        if (isNewTagAdded)
        {
            _hasTriggered = true;
            Debug.Log($"<color=green>[Trigger]</color> 조건을 만족하여 태그가 추가되었습니다: {gameObject.name}");
        }
    }

    /// <summary>
    /// 설정된 조건(필수 태그, 금지 태그)을 모두 만족하는지 확인합니다.
    /// </summary>
    private bool CheckConditions()
    {
        // 필수 태그 검사: 설정된 태그를 모두 가지고 있어야 함
        if (_requiredTags != null && _requiredTags.Count > 0)
        {
            foreach (var tag in _requiredTags)
            {
                if (tag != null && !GamePlayTagManager.Instance.HasTag(tag))
                {
                    return false;
                }
            }
        }

        // 금지 태그 검사: 설정된 태그 중 하나라도 가지고 있으면 안 됨
        if (_forbiddenTags != null && _forbiddenTags.Count > 0)
        {
            foreach (var tag in _forbiddenTags)
            {
                if (tag != null && GamePlayTagManager.Instance.HasTag(tag))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
