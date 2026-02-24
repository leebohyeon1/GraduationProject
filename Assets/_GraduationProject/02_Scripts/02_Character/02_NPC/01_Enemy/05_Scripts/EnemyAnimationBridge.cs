using UnityEngine;

public class EnemyAnimationBridge : MonoBehaviour
{
    private Enemy _owner;
    private Animator _animator;
    public Animator Animator => _animator;
    public bool IsAttacking { get; set; }
    public void Initialize(Enemy owner, Animator animator)
    {
        _owner = owner;
        _animator = animator;
    }

    // 애니메이션 이벤트에서 호출되는 메서드 예시
    public void TriggerEvent(string eventName)
    {
        if(_animator != null)
        {
            _animator.SetTrigger(eventName);
        }
    }
    public void SetBool(string boolName, bool value)
    {
        if(_animator != null)
        {
            _animator.SetBool(boolName, value);
        }
    }

    public void ClearIsAttacking()
    {
        IsAttacking = false;
    }

    public void ResetAllTriggers()
    {
        if (_animator == null) return;
        foreach (var parameter in _animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger)
            {
                _animator.ResetTrigger(parameter.name);
            }
        }
    }
}