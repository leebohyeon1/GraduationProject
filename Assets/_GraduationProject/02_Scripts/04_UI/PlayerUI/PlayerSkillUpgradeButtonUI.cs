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
    }

    private void OnEnable()
    {
        if(CheckCondition())
        {
            _skillToggleButton.interactable = true;
        }
        else
        {
            _skillToggleButton.interactable = false;
        }
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
        if (CheckCondition())
        {
            _skillToggleButton.interactable = true;
        }
        else
        {
            _skillToggleButton.interactable = false;
        }
    }

    /// <summary>
    /// 스킬 학습
    /// </summary>
    public void AcquireSkill()
    {
        if(_isLearned)
        {
            return;
        }

        foreach (var item in _needAbilities)
        {
            _playerController.Ability.AddAbility(item);
        }

        _abilitySelected.Publish(_learnAbilities[0]);
        _isLearned = true;
        _skillToggleButton.interactable = false;
    }

    /// <summary>
    /// 조건 체크
    /// </summary>
    /// <returns>조건에 만족했는가</returns>
    public bool CheckCondition()
    {
        if (_isLearned)
        {
            return false;
        }

        // 플레이어 현재 돈 체크
        if (_playerController.Money.CurrentMoney >= _price)
        {
            // 가지고 있는 기술 체크
            for (int i = 0; i < _needAbilities.Count; i++)
            {
                if (!_playerController.Ability.HasAbility(_needAbilities[i]))
                {
                    return false;
                }
            }

            return true;
        }
        else
        {
            return false;
        }
    }
}
