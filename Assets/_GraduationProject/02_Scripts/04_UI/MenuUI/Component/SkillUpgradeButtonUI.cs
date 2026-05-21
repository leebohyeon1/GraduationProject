using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 업그레이드 버튼 UI
/// </summary>
public class SkillUpgradeButtonUI : MonoBehaviour, IEventListener<PlayerAbilitySO>
{
    [SerializeField] private string _skillName; // 스킬 이름 
    public string SkillName => _skillName;
    [SerializeField] private string _skillDescription;  // 스킬 설명
    public string SkillDescription => _skillDescription;

    [Header("References")]
    [SerializeField] private Toggle _skillToggleButton;
    [SerializeField] private Image _skillIcon;
    [SerializeField] private Image _skillLockImage;
    [SerializeField] private Color _lockColor;

    private PlayerController _playerController;

    [Header("Conditions")]
    [SerializeField] private int _price;    // 가격
    public int Price => _price;
    [SerializeField] private int _specialPrice; // 특수 가격
    public int SpecialPrice => _specialPrice;                                            
    [SerializeField] private List<PlayerAbilitySO> _needAbilities;
    [SerializeField] private List<GamePlayTagSO> _needTags;
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

        // 1. 이미 배웠는지 확인
        bool alreadyHasSkill = HasSkill();
        
        // 2. 배울 수 있는 조건인지 확인
        bool canPurchase = CheckPurchaseCondition();

        // UI 상태 업데이트
        if (alreadyHasSkill)
        {
            _isLearned = true;
            _skillToggleButton.interactable = false;
            _skillToggleButton.SetIsOnWithoutNotify(true);
            _skillToggleButton.image.color = Color.yellow; // 배운 상태 색상

            if (_skillLockImage != null) _skillLockImage.gameObject.SetActive(false);
        }
        else if (canPurchase)
        {
            _isLearned = false;
            _skillToggleButton.interactable = true;
            _skillToggleButton.SetIsOnWithoutNotify(false);
            _skillToggleButton.image.color = Color.white; // 배울 수 있는 상태 색상
            _skillIcon.color = Color.white; // 아이콘도 밝게

            if (_skillLockImage != null) _skillLockImage.gameObject.SetActive(false);
        }
        else
        {
            _isLearned = false;
            _skillToggleButton.interactable = false;
            _skillToggleButton.SetIsOnWithoutNotify(false);
            _skillToggleButton.image.color = Color.white; // 배울 수 있는 상태 색상
            _skillIcon.color = _lockColor; // 아이콘도 어둡게

            if (_skillLockImage != null) _skillLockImage.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 스킬 보유 여부 확인
    /// </summary>
    private bool HasSkill()
    {
        for (int i = 0; i < _learnAbilities.Count; i++)
        {
            if (_playerController.Ability.HasAbility(_learnAbilities[i].Id))
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
                if (!_playerController.Ability.HasAbility(_needAbilities[i].Id))
                {
                    return false;
                }
            }

            // 선행 태그 체크
            for (int i = 0; i < _needTags.Count; i++)
            {
                if (!GamePlayTagManager.Instance.HasTag(_needTags[i].ID))
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