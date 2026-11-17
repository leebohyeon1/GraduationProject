using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 플레이어의 패리 스택 및 타이머를 시각화하는 UI 클래스
/// </summary>
public class PlayerParryStackUI : PlayerUIBase
{
    [Header("UI Elements")]
    [SerializeField] private List<Image> _stackIcons;       // 스택 아이콘 (최대 3개)
    
    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.2f; // 애니메이션 지속 시간
    [SerializeField] private Vector3 _punchVector = new Vector3(0.2f, 0.2f, 0.2f); // 펀치 강도
    [SerializeField] private int _vibrato = 5;               // 진동 횟수
    
    private int _lastStackCount = 0;

    public override void Initialize(PlayerController player)
    {
        if (p_player != null && p_player.Combat != null)
            p_player.Combat.CounterStackChanged -= UpdateStackDisplay;
        StopIconTweens();
        base.Initialize(player);

        // 이벤트 구독
        p_player.Combat.CounterStackChanged += UpdateStackDisplay;

        // 초기 상태 설정
        _lastStackCount = p_player.Combat.CounterStacks;
        UpdateStackDisplay(_lastStackCount);
    }

    public override void Dispose()
    {
        if (p_player != null && p_player.Combat != null)
        {
            p_player.Combat.CounterStackChanged -= UpdateStackDisplay;
        }

        StopIconTweens();
        p_player = null;
        base.Dispose();
    }

    private void OnDestroy() => Dispose();

    private void StopIconTweens()
    {
        if (_stackIcons == null) return;
        foreach (var icon in _stackIcons)
        {
            if (icon != null) icon.transform.DOKill();
        }
    }

    /// <summary>
    /// 스택 개수에 따라 아이콘 활성화/비활성화
    /// </summary>
    /// <param name="currentStacks">현재 스택 수</param>
    private void UpdateStackDisplay(int currentStacks)
    {
        if (_stackIcons == null) return;
        currentStacks = Mathf.Clamp(currentStacks, 0, _stackIcons.Count);
        for (int i = 0; i < _stackIcons.Count; i++)
        {
            // Completion happens after the loop: capture the target, never the loop index.
            var icon = _stackIcons[i];
            if (icon == null) continue;
            var iconTransform = icon.transform;
            bool isActive = i < currentStacks;
            
            // 상태 변화 체크
            bool isNewlyActivated = isActive && (i >= _lastStackCount);
            bool isNewlyDeactivated = !isActive && (i < _lastStackCount);
            
            if (isNewlyActivated)
            {
                icon.gameObject.SetActive(true);
                iconTransform.DOKill();
                
                // 팝업 애니메이션 후 펀치
                iconTransform.localScale = Vector3.zero;
                iconTransform.DOScale(Vector3.one, _animationDuration).OnComplete(() =>
                {
                    if (icon != null && icon.gameObject.activeInHierarchy)
                        iconTransform.DOPunchScale(_punchVector, _animationDuration, _vibrato);
                });
            }
            else if (isNewlyDeactivated)
            {
                // 줄어들 때 (비활성화 될 때) 쪼그라드는 애니메이션 재생 후 SetActive(false)
                iconTransform.DOKill();
                iconTransform.DOScale(Vector3.zero, _animationDuration).OnComplete(() =>
                {
                    if (icon != null) icon.gameObject.SetActive(false);
                });
            }
            else
            {
                // 상태 변화가 없는 경우
                iconTransform.DOKill();
                icon.gameObject.SetActive(isActive);
                if (isActive)
                {
                    iconTransform.localScale = Vector3.one;
                }
            }
        }

        _lastStackCount = currentStacks;
    }
}
