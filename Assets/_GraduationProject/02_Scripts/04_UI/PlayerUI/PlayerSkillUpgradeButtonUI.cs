using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 업그레이드 버튼 UI
/// </summary>
public class PlayerSkillUpgradeButtonUI : MonoBehaviour, IEventListener<PlayerAbilitySO>
{
    [Header("References")]
    [SerializeField] private Toggle _skillToggleButton;
    private PlayerController _playerController;

    [Header("Conditions")]
    [SerializeField] private int _price;    // 가격
    [SerializeField] private int _specialPrice; // 특수 가격 (필요하다면 사용) 
    [SerializeField] private List<PlayerAbilitySO> _needAbilities;
    private bool _isLearned = false;

    [Header("Ability")]
    [SerializeField] private List<PlayerAbilitySO> _learnAbilities;

    [Header("Events")]
    [SerializeField] private OnAbilitySelectedSO _abilitySelected;

    public void Initialize(PlayerController player)
    {
        _playerController = player;
        _abilitySelected.Subscribe(this);

        // 초기화 시 UI 상태 갱신
        UpdateUIState();
    }

    private void OnEnable()
    {
        // 켜질 때 UI 상태 갱신
        UpdateUIState();
    }

    private void OnDestroy()
    {
        _abilitySelected.Unsubscribe(this);
    }

    /// <summary>
    /// 스킬 배웠을 때 이벤트 처리
    /// </summary>
    /// <param name="ability">플레이어가 배운 스킬</param>
    public void OnEventTrigger(PlayerAbilitySO ability)
    {
        // 이벤트 발생 시 UI 상태 갱신
        UpdateUIState();
    }

    /// <summary>
    /// UI 상태 (토글 체크 여부, 상호작용 여부) 통합 관리
    /// </summary>
    private void UpdateUIState()
    {
        if (_playerController == null)
        {
            return;
        }

        // 이미 배웠는지 확인
        bool alreadyHasSkill = HasSkill();

        // 배웠다면 토글을 켜진 상태로 변경 (이벤트 트리거 방지를 위해 SetIsOnWithoutNotify 사용 권장)
        _skillToggleButton.SetIsOnWithoutNotify(alreadyHasSkill);

        if (alreadyHasSkill)
        {
            _isLearned = true;
            _skillToggleButton.interactable = false; // 이미 배웠으므로 클릭 불가
        }
        else
        {
            _isLearned = false;
            // 배우지 않았다면 구매 조건(돈, 선행 스킬) 체크하여 활성화 여부 결정
            _skillToggleButton.interactable = CheckPurchaseCondition();
        }
    }

    /// <summary>
    /// 스킬 보유 여부 확인
    /// </summary>
    private bool HasSkill()
    {
        for (int i = 0; i < _learnAbilities.Count; i++)
        {
            if (_playerController.Ability.HasAbility(_learnAbilities[i]))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 구매 가능 조건 체크 (보유 여부 제외, 돈과 선행 스킬만 체크)
    /// </summary>
    private bool CheckPurchaseCondition()
    {
        // 플레이어 현재 돈 체크
        if (_playerController.Money.CurrentMoney >= _price 
            && _playerController.Money.CurrentSpecialMoney >= _specialPrice)
        {
            // 선행 기술 체크
            for (int i = 0; i < _needAbilities.Count; i++)
            {
                if (!_playerController.Ability.HasAbility(_needAbilities[i]))
                {
                    return false;
                }
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// 스킬 학습 (Toggle의 OnValueChanged나 Button의 OnClick에 연결)
    /// </summary>
    public void AcquireSkill()
    {
        Debug.Log("스킬 학습 시도");

        // 방어 코드: 이미 배웠거나 조건 불만족 시 리턴
        if (_isLearned || !CheckPurchaseCondition())
        {
            // 강제로 토글이 켜졌다면 다시 끔 (UI 동기화)
            if (!_isLearned)
            {
                _skillToggleButton.SetIsOnWithoutNotify(false);
            }

            return;
        }

        // --- 실제 스킬 구매 로직 ---
        // 돈 차감 (필요하다면 로직 추가)
        _playerController.Money.UseMoney(_price); 
        _playerController.Money.UseSpecialMoney(_specialPrice); 

        // 배우는 스킬 이벤트 발생 및 등록
        for (int i = 0; i < _learnAbilities.Count; i++)
        {
            _abilitySelected.Publish(_learnAbilities[i]);
        }

        // 상태 갱신
        UpdateUIState();
    }
}