using BH_Lib.Log;
using UnityEngine;


public class PlayerHealth : HealthSystem, IStiffness
{
    private PlayerData _runtimeData;

    /// <summary>
    /// 경직도 관련
    /// </summary>
    private int _currentStiffness;
    private int _stiffnessThreshold = 100;
    private float _stiffnessDuration;

    #region Properties
    public int CurrentStiffness => _currentStiffness;
    public int StiffnessThreshold => _stiffnessThreshold;
    public float StiffnessDuration => _stiffnessDuration;
    #endregion

    public void Initialize(PlayerData data)
    {
        _runtimeData = data;
        p_maxHealth = _runtimeData.MaxHealth;
        p_health = MaxHealth;
    }

    public override void TakeDamage(int damageAmount, IAttacker attacker = null)
    {
        if (IsDead || IsInvincible)
        {
            return;
        }

        if (_runtimeData.IsDefending)
        {
            damageAmount = Mathf.RoundToInt(damageAmount *
                _runtimeData.CombatData.DefendDamageReductionRate);

        }

        ChangeHealth(-damageAmount);

        if (IsDead)
        {
            Die();
        }
    }

    public override void TakeDamage(int damageAmount, int stiffenessAmount, IAttacker attacker = null)
    {
        // 죽었거나 무적이면 리턴
        if (IsDead || IsInvincible)
        {
            return;
        }

        // 방어중일 때 수치 경감
        if (_runtimeData.IsDefending)
        {
            damageAmount = Mathf.RoundToInt(damageAmount *
                _runtimeData.CombatData.DefendDamageReductionRate);

            stiffenessAmount = Mathf.RoundToInt(stiffenessAmount * 0.5f);
        }

        ChangeHealth(-damageAmount);
        AddStiffness(stiffenessAmount);

        if (IsDead)
        {
            Die();
        }
    }

    public void AddStiffness(int amount)
    {
        ChangeStiffness(amount);

        // 현재 경직도가 최대 경직도를 넘을 때
        if(_currentStiffness >= _stiffnessThreshold)
        {
            // 경직도 초기화
            ChangeStiffness(-_currentStiffness);
            // 강한 경직
            HeavyStagger();
        }
        else
        {
            // 약한 경직
            LightStagger();
        }
    }

    /// <summary>
    /// 경직도 변경 함수
    /// </summary>
    /// <param name="amount">경직도 변경량</param>
    private void ChangeStiffness(int amount)
    {
        _currentStiffness += amount;
    }

    /// <summary>
    /// 약한 경직
    /// </summary>
    private void LightStagger()
    {
        _stiffnessDuration = _runtimeData.CombatData.LightStaggerDuration;
        _runtimeData.SetDamaged(PlayerDamagedType.Normal);
        Log.PrintColor(Color.red, "약한 경직");
    }

    /// <summary>
    /// 강한 경직
    /// </summary>
    private void HeavyStagger()
    {
        _stiffnessDuration = _runtimeData.CombatData.HeavyStaggerDuration;
        _runtimeData.SetDamaged(PlayerDamagedType.Strong);
        Log.PrintColor(Color.red, "강한 경직");
    }
}
