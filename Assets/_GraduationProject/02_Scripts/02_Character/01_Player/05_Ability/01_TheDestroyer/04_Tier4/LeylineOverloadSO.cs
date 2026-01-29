using DG.Tweening;
using System.Collections;
using UnityEngine;

/// <summary>
/// 중량형 4티어
/// 좌클릭 + 우클릭 시 액티브 스킬
/// </summary>
[CreateAssetMenu(fileName = "LeylineOverloadSO", menuName = "Project/Player/Ability/TheDestroyer/Tier4/LeylineOverloadSO")]
public class LeylineOverloadSO : PlayerAbilitySO
{
    public float _cooldown;     

    public CanSpecialAttackSO SpecialAttackSO;
    [Range(0f, 10f)]
    public float ShieldMultipliers; // 보호막 배수

    private bool _normalAttackInput = false;
    private bool _counterAttackInput = false;

    private CanSpecialAttackSO _runtimeSpecialAttackSO;
    private Coroutine _cooldownCoroutine = null;

    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();

        p_owner.InputReader.NormalAttackEvent += OnNormalAttack;
        p_owner.InputReader.NormalAttackCancelEvent += OnNormalAttackCancel;

        p_owner.InputReader.NormalCounterEvent += OnCounterAttack;
        p_owner.InputReader.NormalCounterCancelEvent += OnCounterCancel;

        _cooldownCoroutine = null;

        // 런타임 인스턴스 생성
        if (_runtimeSpecialAttackSO == null)
        {
            _runtimeSpecialAttackSO = Instantiate(SpecialAttackSO);
        }

        // 태그 등록
        p_owner.Ability.AddTag(_runtimeSpecialAttackSO);
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        if (p_owner != null)
        {
            p_owner.InputReader.NormalAttackEvent -= OnNormalAttack;
            p_owner.InputReader.NormalAttackCancelEvent -= OnNormalAttackCancel;
            p_owner.InputReader.NormalCounterEvent -= OnCounterAttack;
            p_owner.InputReader.NormalCounterCancelEvent -= OnCounterCancel;
            p_owner.Events.AttackFinished -= OnAttackFinished;

            // 쿨다운 코루틴이 돌고 있다면 강제로 멈춰야 안전함
            if (_cooldownCoroutine != null)
            {
                p_owner.StopCoroutine(_cooldownCoroutine);
                _cooldownCoroutine = null;
            }

            // 등록했던 태그를 확실히 제거 (Destroy 전 안전하게)
            if (_runtimeSpecialAttackSO != null)
            {
                p_owner.Ability.RemoveTag(_runtimeSpecialAttackSO);
            }
        }

        p_ability = null;
        p_owner = null;

        // 런타임 오브젝트 파괴
        if (_runtimeSpecialAttackSO != null)
        {
            Destroy(_runtimeSpecialAttackSO);
            _runtimeSpecialAttackSO = null;
        }
    }

    #region Input
    private void OnNormalAttack()
    {
        _normalAttackInput = true;

        if(_normalAttackInput && _counterAttackInput)
        {
            ActiveSkill();
        }
    }

    private void OnNormalAttackCancel()
    {
        _normalAttackInput = false;
    }

    private void OnCounterAttack()
    {
        _counterAttackInput = true;

        if (_normalAttackInput && _counterAttackInput)
        {
            ActiveSkill();
        }
    }

    private void OnCounterCancel() 
    {
        _counterAttackInput = false;
    }
    #endregion

    /// <summary>
    /// 공격 이 끝났을 때 이벤트
    /// </summary>
    private void OnAttackFinished()
    {
        if (p_owner != null && _runtimeSpecialAttackSO != null)
        {
            _runtimeSpecialAttackSO.Revert(p_owner);
            p_owner.Events.AttackFinished -= OnAttackFinished;
        }
    }

    private void ActiveSkill()
    {
        if (_cooldownCoroutine != null)
        {
            Debug.Log("지맥 폭발 쿨다운");
            return;
        }

        Debug.Log("지맥 폭발 사용");
        
        // 보호막 모두 사용
        float bonusDamage = p_owner.Health.CurrentShieldAmount * ShieldMultipliers;
        p_owner.Health.DecreaseShield(p_owner.Health.CurrentShieldAmount);

        PlayerAttackConfig attackConfig = SpecialAttackSO.AttackConfig;
        attackConfig.AttackDamage += Mathf.RoundToInt(bonusDamage);
        _runtimeSpecialAttackSO.AttackConfig = attackConfig;

        p_owner.Events.AttackFinished += OnAttackFinished;

        // 특수 공격 실행
        _runtimeSpecialAttackSO.Apply(p_owner);
        p_owner.Ability.RemoveTag(_runtimeSpecialAttackSO);

        _cooldownCoroutine = p_owner.StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(_cooldown);

        _cooldownCoroutine = null;
        p_owner.Ability.AddTag(_runtimeSpecialAttackSO);
    }
}
