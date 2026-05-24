using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// 스킬 업그레이드 버튼 UI
/// </summary>
public class SkillUpgradeButtonUI : MonoBehaviour, IEventListener<PlayerAbilitySO>, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    public string SkillName => _learnAbility != null ? _learnAbility.AbilityName : "Unknown Skill";
    public string SkillDescription => _learnAbility != null ? _learnAbility.AbilityDescription : "No description available.";

    [Header("References")]
    [SerializeField] private Toggle _skillToggleButton;
    [SerializeField] private Image _skillIcon;
    [SerializeField] private Image _skillLockImage;
    [SerializeField] private Color _lockColor;
    [SerializeField] private Color _unlockColor;

    private PlayerController _playerController;
    private SkillUI _parentSkillUI;

    [Header("Conditions")]
    [SerializeField] private int _price;    // 가격
    public int Price => _price;
    [SerializeField] private int _specialPrice; // 특수 가격
    public int SpecialPrice => _specialPrice;                                            
    [SerializeField] private List<PlayerAbilitySO> _needAbilities;
    [SerializeField] private List<GamePlayTagSO> _needTags;
    private bool _isLearned = false;

    [Header("Ability")]
    [SerializeField] private PlayerAbilitySO _learnAbility;

    [Header("Events")]
    [SerializeField] private OnAbilitySelectedSO _abilitySelected;

    [Header("Animations")]
    [SerializeField] private float _scaleDuration = 0.2f;
    [SerializeField] private float _selectedScale = 1.1f;
    private Vector3 _originalScale;

    public void Initialize(PlayerController player)
    {
        _playerController = player;
        _parentSkillUI = GetComponentInParent<SkillUI>();
        _abilitySelected.Subscribe(this);
        _originalScale = transform.localScale;

        // 초기화 시 UI 상태 갱신
        UpdateUIState();
    }

    private void OnDisable()
    {
        // 꺼질 때 트윈 제거 및 스케일 초기화
        transform.DOKill();
        transform.localScale = _originalScale;

        // 켜질 때 UI 상태 갱신
        UpdateUIState();
    }

    private void OnDestroy()
    {
        _abilitySelected.Unsubscribe(this);
        transform.DOKill();
    }

    //==========================================================================================================================
    // Animation & Selection Handler ===========================================================================================
    //==========================================================================================================================

    public void OnSelect(BaseEventData eventData)
    {
        // 선택 시 스케일 업 애니메이션
        transform.DOScale(_originalScale * _selectedScale, _scaleDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);

        // 스킬 설명 UI 갱신
        if (_parentSkillUI != null)
        {
            _parentSkillUI.UpdateDescription(this);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        // 선택 해제 시 원래 크기로
        transform.DOScale(_originalScale, _scaleDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스 오버 시 포커스 강제 (패드와 통일감)
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void Select()
    {
        // 패드 조작 등에서 수동으로 선택할 때 사용
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
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

        // [개선] 패드 조작을 위해 모든 상태에서 선택(Select)이 가능해야 하므로 interactable은 항상 true 유지
        _skillToggleButton.interactable = true;

        // UI 상태 업데이트
        if (alreadyHasSkill)
        {
            _isLearned = true;
            _skillToggleButton.SetIsOnWithoutNotify(true);
            _skillToggleButton.image.color = _unlockColor; // 배운 상태 색상
            _skillIcon.color = _unlockColor; // 아이콘도 밝게

            if (_skillLockImage != null) _skillLockImage.gameObject.SetActive(false);
        }
        else if (canPurchase)
        {
            _isLearned = false;
            _skillToggleButton.SetIsOnWithoutNotify(false);
            _skillToggleButton.image.color = Color.white; // 배울 수 있는 상태 색상
            _skillIcon.color = Color.white; // 아이콘도 밝게

            if (_skillLockImage != null) _skillLockImage.gameObject.SetActive(false);
        }
        else
        {
            _isLearned = false;
            _skillToggleButton.SetIsOnWithoutNotify(false);
            _skillToggleButton.image.color = Color.white; // 배울 수 없는 상태 기본 컬러
            _skillIcon.color = _lockColor; // 아이콘은 어둡게

            if (_skillLockImage != null) _skillLockImage.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 스킬 보유 여부 확인
    /// </summary>
    private bool HasSkill()
    {
        if (_playerController.Ability.HasAbility(_learnAbility.Id))
        {
            return true;
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
        _abilitySelected.Publish(_learnAbility);

        // 상태 갱신
        UpdateUIState();
    }
}