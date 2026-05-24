using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbility : MonoBehaviour, IDisposable, IEventListener<PlayerAbilitySO>
{
    [Header("References")]
    private PlayerEvents _events;
    private PlayerData _runtimeData;
    public PlayerData RuntimeData => _runtimeData;

    [SerializeField] private List<PlayerAbilitySO> _abilitySet = new List<PlayerAbilitySO>();
    [SerializeField] private List<PlayerAbilityTagSO> _abilityTags = new List<PlayerAbilityTagSO>(); // For Inspector visibility if needed

    [Header("Event")]
    [SerializeField] private OnAbilitySelectedSO _abilitySelected;

    public IEnumerable<PlayerAbilitySO> ActiveAbilities => _abilitySet;

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    /// <param name="player">플레이어</param>
    public void Initialize(PlayerController player)
    {
        _events = player.Events;

        _events.BeforeDamaged += OnBeforeDamaged;
        _abilitySelected.Subscribe(this);

        StartCoroutine(InitializeDataCoroutine(player.RuntimeData));

        // 이벤트 해제 구독
        player.RegisterDisposable(this);
    }

    /// <summary>
    /// 컴포넌트 해제
    /// </summary>
    public void Dispose()
    {
        StopAllCoroutines(); // 진행 중인 초기화 중단
        _events.BeforeDamaged -= OnBeforeDamaged;
        _abilitySelected.Unsubscribe(this);

        // 리스트를 순회하며 요소를 삭제할 때는 역순으로 진행하여
        // "Collection was modified" 에러를 방지합니다.
        for (int i = _abilitySet.Count - 1; i >= 0; i--)
        {
            var ability = _abilitySet[i];
            if (ability != null)
            {
                // [수정] RemoveAbility를 호출하면 _runtimeData.AcquiredAbilityIds에서도
                // 데이터가 삭제되어 버리므로, 씬 종료 시 세이브 데이터가 날아가는 원인이 됩니다.
                // 여기서는 순수하게 인게임 능력만 해제합니다.
                ability.UnregisterAbility(this);
            }
        }
        _abilitySet.Clear();
    }

    private System.Collections.IEnumerator InitializeDataCoroutine(PlayerData data)
    {
        _runtimeData = data;
        
        // 1초 대기
        yield return new WaitForSeconds(1.0f);

        // 저장된 능력이 있다면 불러오기
        if (data != null && data.AcquiredAbilityIds != null)
        {
            foreach (string abilityId in data.AcquiredAbilityIds)
            {
                // DataManager를 통해 ID에 해당하는 스킬 SO를 찾아옴
                PlayerAbilitySO abilitySO = DataManager.Instance.GetAbility(abilityId);
                if (abilitySO != null)
                {
                    LoadAbility(abilitySO);
                }
            }
        }
    }

    //==========================================================================================================================
    // Ability Management ======================================================================================================
    //==========================================================================================================================

    #region Ability Management
    /// <summary>
    /// 스킬 로드 (세이브 데이터로부터 로드할 때 사용)
    /// </summary>
    /// <param name="ability">로드할 스킬</param>
    private void LoadAbility(PlayerAbilitySO ability)
    {
        if (ability == null) return;
        
        if (!_abilitySet.Contains(ability))
        {
            Debug.Log("기술 로드: " + ability.Id);
            _abilitySet.Add(ability);
            ability.RegisterAbility(this);
        }
    }

    /// <summary>
    /// 기술 추가 함수 (인게임에서 새로운 기술을 획득할 때 사용)
    /// </summary>
    /// <param name="ability">추가할 기술</param>
    public void AddAbility(PlayerAbilitySO ability)
    {
        if (ability == null) return;

        if(!_abilitySet.Contains(ability))
        {
            _abilitySet.Add(ability);
            ability.RegisterAbility(this);    // 기술 등록
            
            // 저장용 데이터에도 ID 추가
            if (_runtimeData != null && !_runtimeData.AcquiredAbilityIds.Contains(ability.Id))
            {
                _runtimeData.AcquiredAbilityIds.Add(ability.Id);
            }
            
            Debug.Log("기술 획득 및 저장 등록: " + ability.Id);
        }
        else
        {
            Debug.LogWarning($"이미 보유 중인 기술입니다: {ability.Id}");
        }
    }

    /// <summary>
    /// 기술 제거 함수
    /// </summary>
    /// <param name="id">삭제할 기술 ID</param>
    public void RemoveAbility(string id)
    {
        for (int i = 0; i < _abilitySet.Count; i++)
        {
            var ability = _abilitySet[i];
            if (ability != null && ability.Id == id)
            {
                ability.UnregisterAbility(this);  // 기술 해제
                _abilitySet.RemoveAt(i);

                // 저장용 데이터에서도 ID 제거
                if (_runtimeData != null)
                {
                    _runtimeData.AcquiredAbilityIds.Remove(id);
                }

                Debug.Log($"기술 해제 및 저장 제거: {id}");
                return;
            }
        }
    }

    /// <summary>
    /// 기술을 가지고 있는지 확인하는 함수
    /// </summary>
    /// <param name="id">아이디</param>
    /// <returns>가지고 있는지 여부</returns>
    public bool HasAbility(string id)
    {
        foreach (var ability in _abilitySet)
        {
            if (ability == null)
            {
                continue;
            }

            if (ability.Id == id)
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// 아이디에 맞는 기술 반환
    /// </summary>
    /// <param name="id">아이디</param>
    /// <returns>기술 스크립터블 오브젝트</returns>
    public PlayerAbilitySO GetAbility(string id)
    {
        foreach (var tag in _abilitySet)
        {
            // 태그가 비어있으면 계속
            if (tag == null)
            {
                continue;
            }

            if (tag.Id == id)
            {
                return tag;
            }
        }

        return null;
    }
    #endregion

    //==========================================================================================================================
    // Tag Management ==========================================================================================================
    //==========================================================================================================================

    #region Tag Management
    /// <summary>
    /// 태그 추가 함수 (중첩 관리)
    /// </summary>
    /// <param name="tag">등록할 태그</param>
    public void AddTag(PlayerAbilityTagSO tag)
    {
        if (tag == null)
        {
            return;
        }

        _abilityTags.Add(tag); 
    }

    /// <summary>
    /// 태그 제거 함수 (중첩 관리)
    /// </summary>
    /// <param name="tag">제거할 태그</param>
    public void RemoveTag(string id)
    {
        if (id == null)
        {
            return;
        }

        foreach (var tag in _abilityTags)
        {
            if(tag.Id == id)
            {
                _abilityTags.Remove(tag);
                return;
            }
        }

    }

    /// <summary>
    /// 태그를 가지고 있는지 확인하는 함수
    /// </summary>
    /// <param name="id">확인할 태그 아이디</param>
    /// <returns>가지고 있는지 여부</returns>
    public bool HasTag(string id)
    {
        foreach (var tag in _abilityTags)
        {
            if (tag != null && tag.Id == id)
            {
                return true;
            }
        }
        return false;
    }
    #endregion


    //==========================================================================================================================
    // Event Handler ===========================================================================================================
    //==========================================================================================================================
    
    /// <summary>
    /// 기술 선택 이벤트 호출
    /// </summary>
    /// <param name="ability">기술</param>
    public void OnEventTrigger(PlayerAbilitySO ability)
    {

        AddAbility(ability);
    }

    /// <summary>
    /// 데미지 받기 전 이벤트 발행
    /// </summary>
    /// <param name="damageContext">받은 데미지 데이터</param>
    private void OnBeforeDamaged(ref PlayerDamageContext damageContext)
    {
        DamageData damageData = damageContext.Data;

        // 무적 태그가 있으면 무적
        if (HasTag("Invincible"))
        {
            damageData.DamageAmount = 0;
            damageData.StiffnessAmount = 0;
            damageData.KnockbackDuration = 0;
            damageData.KnockbackForce = 0;

            damageContext.HasSuperArmor = true;
        }
        else if(HasTag("SuperArmor")) // 슈퍼아머 태그가 있으면 슈퍼아머
        {
            damageContext.HasSuperArmor = true;
        }

        damageContext.Data = damageData;
    }
}
