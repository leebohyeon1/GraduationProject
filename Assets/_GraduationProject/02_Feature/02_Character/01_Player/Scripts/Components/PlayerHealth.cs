using BH_Lib.Log;
using UnityEngine;


public class PlayerHealth : HealthSystem, IStiffness
{
    private PlayerRuntimeData _runtimeData;
    private bool _isDefending;

    /// <summary>
    /// 경직도 관련
    /// </summary>
    private int _currentStiffness;
    private int _stiffnessThreshold;
    private float _stiffnessDuration;

    #region Properties
    public bool IsDefending => _isDefending;

    public int CurrentStiffness => _currentStiffness;
    public int StiffnessThreshold => _stiffnessThreshold;
    public float StiffnessDuration => _stiffnessDuration;
    #endregion

    public void Initialize(PlayerRuntimeData data)
    {
        _runtimeData = data;
        p_maxHealth = _runtimeData.MaxHealth;
        p_health = MaxHealth;
    }

    /// <summary>
    /// 방어 상태를 설정합니다
    /// PlayerDefendState에서 호출
    /// </summary>
    /// <param name="isDefending">방어 상태 여부</param>
    public void SetDefending(bool isDefending)
    {
        _isDefending = isDefending;
    }

    public override void TakeDamage(int damageAmount, IAttacker attacker = null)
    {
        if (IsDead || IsInvincible)
        {
            return;
        }

        if (_isDefending)
        {
            damageAmount = Mathf.RoundToInt(damageAmount *
                _runtimeData.CombatData.DefendDamageReductionRate);

        }

        ChangeHealth(-damageAmount);

        if (IsDead)
        {
            Die();
        }
        else
        {
            p_isHit = true;
        }
    }

    public override void TakeDamage(int damageAmount, int StiffenessAmount, IAttacker attacker = null)
    {

    }

    public void AddStiffness(int amount)
    {

    }

    private void LightStagger()
    {

    }

    private void DefendStagger()
    {

    }

    private void HeavyStagger()
    {

    }
}
