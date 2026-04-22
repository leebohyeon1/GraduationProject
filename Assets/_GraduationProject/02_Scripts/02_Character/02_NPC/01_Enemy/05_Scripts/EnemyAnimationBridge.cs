using System.Collections;
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
    public void TriggerEvent(string eventNamm,float delay)
    {
        delayAnimationTrigger(eventNamm, delay);
    }
    IEnumerator delayAnimationTrigger(string triggerName, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_animator != null)
        {
            _animator.SetTrigger(triggerName);
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

    public void ResetAllAnimationStates()
    {
        if (_animator == null) return;
        
        foreach (var parameter in _animator.parameters)
        {
            if(parameter.name == "IsCombat" || parameter.name == "Stun" || parameter.name == "WeakStun" || parameter.name == "Walk") continue; // IsCombat 파라미터는 초기화에서 제외
            // Debug.Log($"[EnemyAnimationBridge] Resetting parameter '{parameter.name}' of type '{parameter.type}'");
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Trigger:
                    _animator.ResetTrigger(parameter.name);
                    // Debug.Log($"[EnemyAnimationBridge] Trigger '{parameter.name}' 초기화");
                    break;
                case AnimatorControllerParameterType.Bool:
                    _animator.SetBool(parameter.name, false);
                    break;
            }
        }
        
        IsAttacking = false;
    }
}