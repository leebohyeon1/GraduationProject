
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class ParrySystem : MonoBehaviour, IParryable, ICounterable, IEventListener<bool>
{
    // Parry system implementation
    public bool IsParryable { get; private set; } = false;
    public bool IsParry { get; private set; } = false;
    public bool IsCounterable { get; private set; } = false;
    float _stunExitTime = -Mathf.Infinity;
    public bool _isStunned { get; private set; } = false;
    public float StunExitTime => _stunExitTime;
    public OnParry OnParry => _onParry;

    [SerializeField] private float _stunTime = 3f;
    [SerializeField] private OnParry _onParry;
    private Enemy _owner;
    public void Initialize(Enemy enemy)
    {
        _owner = enemy;
        IsParryable = false;
        IsCounterable = false;
        ClearStun(); IsParry = false;

        OnParry.Subscribe(this);
    }

    private void OnDisable()
    {
        OnParry.Unsubscribe(this);
    }

    public void SetParryable(string value)
    {
        IsParryable = value == "true" ? true : false;
    }
    public void SetCounterAttack(bool value)
    {
        IsCounterable = value;
    }
    public void ExecuteCounterEffect()
    {
        SetCounterAttack(false);
        // _owner.AnimationEvent("CounterAttack"); 
        // _owner.TakeDamage(30); // 카운터 공격 시 데미지 적용 
    }

    public bool Parry(GameObject parryInstigator)
    {
        SetParryable("false");
        Debug.Log(_owner.name + " was parried by " + parryInstigator.name);
        _owner.StiffnessSystem.AddStiffness(_owner.CurrentStiffness);
        IsParry = true;
        return true;
    }

    public void ApplyStun()
    {
        if (_isStunned || _owner.EnemyHealth.IsDead) return; // 이미 스턴 상태라면 무시
        _isStunned = true;
        _stunExitTime = Time.time + _stunTime;
        _owner.Movement.StopMovement(); // 스턴 상태에서는 이동을 멈춥니다.
        Debug.Log("ApplyStun ");
        _owner.animator.SetBool("Stun", true); // 스턴 애니메이션 트리거
    }
    public void ApplyStun(float stunDuration)
    {
        if (_isStunned || _owner.EnemyHealth.IsDead) return; // 이미 스턴 상태라면 무시
        _isStunned = true;
        _stunExitTime = Time.time + stunDuration;
        _owner.Movement.StopMovement(); // 스턴 상태에서는 이동을 멈춥니다.
        Debug.Log("dsd");
        _owner.animator.SetBool("Stun", true); // 스턴 애니메이션 트리거
    }

    public void ClearStun()
    {
        _owner.animator.SetBool("Stun", false);
        _isStunned = false;
    }

    public void OnEventTrigger(bool eventName)
    {
        IsParry = false;
    }
}