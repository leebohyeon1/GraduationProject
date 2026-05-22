using UnityEngine;

public class Mon_Stiffness : StiffnessSystem
{
    private Enemy _owner;
    public override void AddStiffness(int amount, AttackType attackType)
    {
        if (_owner.Interact != null && !_owner.Interact._isInteracted)
        {
            return;
        }
        if (_owner._stateController.CurrentState == EnemyStateController.EnemyState.Die)
            return;

        // 1. ParrySystem의 구체적인 스턴 타입을 먼저 체크
        // 강스턴(Full)일 때만 차단, 약스턴(Weak)일 때는 상태가 Stunned라도 경직도 증가 허용
        if (_owner.ParrySystem.CurrentStun == StunType.Full)
            return;

        // 2. 만약 ParrySystem이 None인데 StateController가 Stunned라면 (다른 요인에 의한 스턴) 차단
        if (_owner.ParrySystem.CurrentStun == StunType.None && 
            _owner._stateController.CurrentState == EnemyStateController.EnemyState.Stunned)
            return;

        base.AddStiffness(amount, attackType);
    }
    public void Initialize(Enemy owner)
    {
        _owner = owner;
        _currentStiffness = 0;
    }
    protected override void OnLightStagger()
    {
        // _owner.ParrySystem.SetCounterAttack(true);
        _owner.ParrySystem.ApplyWeakStun(_weakStiffnessDuration);
    }
    protected override void OnHeavyStagger(bool isCounterAttack = false)
    {
        // _owner.ParrySystem.SetCounterAttack(true);
        _owner.ParrySystem.ApplyStun(_stiffnessDuration, isCounterAttack);
    }
}