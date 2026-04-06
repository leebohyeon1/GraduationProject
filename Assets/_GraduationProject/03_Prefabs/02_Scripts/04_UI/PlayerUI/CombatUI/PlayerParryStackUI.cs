using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어의 패리 스택 및 타이머를 시각화하는 UI 클래스
/// </summary>
public class PlayerParryStackUI : PlayerUIBase
{
    [Header("UI Elements")]
    [SerializeField] private List<Image> _stackIcons;       // 스택 아이콘 (최대 3개)
    [SerializeField] private TextMeshProUGUI _multiplierText; // 데미지 배율 텍스트

    [Header("Settings")]
    [SerializeField] private Color _activeColor = Color.white;
    [SerializeField] private Color _inactiveColor = new Color(1f, 1f, 1f, 0.2f);

    public override void Initialize(PlayerController player)
    {
        base.Initialize(player);

        // 이벤트 구독
        p_player.Combat.ParryStackChanged += UpdateStackDisplay;

        // 초기 상태 설정
        UpdateStackDisplay(p_player.Combat.ParryStacks);
    }

    public override void Dispose()
    {
        if (p_player != null && p_player.Combat != null)
        {
            p_player.Combat.ParryStackChanged -= UpdateStackDisplay;
        }
        base.Dispose();
    }

    private void Update()
    {
        if (p_player == null)
        {
            return;
        }

        // 데미지 배율 텍스트 갱신 (실시간 데이터 반영)
        if (_multiplierText != null)
        {
            float multiplier = p_player.Combat.ParryStackMultiplier;
            _multiplierText.text = $"x{multiplier:F1}";
            _multiplierText.gameObject.SetActive(p_player.Combat.ParryStacks > 0);
        }
    }

    /// <summary>
    /// 스택 개수에 따라 아이콘 활성화/비활성화
    /// </summary>
    /// <param name="currentStacks">현재 스택 수</param>
    private void UpdateStackDisplay(int currentStacks)
    {
        Debug.Log(currentStacks);   
        for (int i = 0; i < _stackIcons.Count; i++)
        {
            // i + 1번째 스택이 활성화되었는지 확인
            bool isActive = i < currentStacks;
            _stackIcons[i].color = isActive ? _activeColor : _inactiveColor;
            
            // 시각적 피드백 (간단한 스케일 애니메이션 등 추가 가능)
            if (isActive)
            {
                _stackIcons[i].transform.localScale = Vector3.one * 1.1f;
            }
            else
            {
                _stackIcons[i].transform.localScale = Vector3.one;
            }
        }
    }
}
