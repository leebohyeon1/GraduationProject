
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class ParrySystem : MonoBehaviour, IParryable, ICounterable
{
    // Parry system implementation
    public bool IsCounterable { get; private set; } = false;
    float _stunExitTime = -Mathf.Infinity;
    public bool _isStunned { get; private set; } = false;
    public float StunExitTime => _stunExitTime;

    [SerializeField] private float _stunTime = 3f;
    private Enemy _owner;
    public void Initialize(Enemy enemy)
    {
        _owner = enemy;
        IsCounterable = false;
        ClearStun(); 
    }

    private void OnDisable()
    {
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
        Debug.Log(_owner.name + " was parried by " + parryInstigator.name);
        _owner.StiffnessSystem.AddStiffness(100);
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
}