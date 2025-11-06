using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class EnemyTakeDmg : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyStat enemyStat;
    public int Health => _maxHealth;
    public int MaxHealth => enemyStat.Maxhealth;
    public bool IsDead => Health <= 0;
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;
    int _maxHealth = 100;
    public int Maxhealth => _maxHealth;
    private CharacterController _characterController;
    private Coroutine _KnockbackCoroutine;
    public bool IsInvincible => throw new NotImplementedException();
    Enemy _owner;
    public void InitializeHealth( Enemy owner)
    {
        _owner = owner;
        _maxHealth = enemyStat.Maxhealth;
        if (_owner.animator.GetBool("Die"))
            _owner.animator.SetBool("Die", false);
        _characterController = _owner.GetComponent<CharacterController>();
        SetKnockbackable(true);
    }

    public void Attack(IDamageable target)
    {
        if (target == null || target.IsDead)
        {
            return;
        }

    }
    
    public bool Knockbackable { get; private set; } = true;
    public void SetKnockbackable(bool value)
    {
        Knockbackable = value;
    }
   
    private IEnumerator KnockbackCoroutine(Vector3 direction, DamageData damageData)
{
    float elapsedTime = 0;
    Debug.Log("Knockback Start" + damageData.KnockbackDuration);

    Vector3 horizontalDirection = direction;
    horizontalDirection.y = 0;
    horizontalDirection.Normalize();

    if (horizontalDirection.sqrMagnitude < 0.01f)
    {
        _KnockbackCoroutine = null;
        yield break;
    }


    while (elapsedTime < damageData.KnockbackDuration)
    {
        float curveValue = damageData.KnockbackCurve.Evaluate(elapsedTime / damageData.KnockbackDuration);
        
        Vector3 move = horizontalDirection * damageData.KnockbackForce * curveValue * Time.deltaTime;

        if (!_characterController.isGrounded)
        {
            move.y += Physics.gravity.y * Time.deltaTime; 
        }

        _characterController.Move(move);
        
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    _KnockbackCoroutine = null;
}

    public void Die()
    {
        _owner.animator.SetBool("Die", true);
        _owner.animator.speed = 1;
        _owner.Movement.StopMovement();
        _owner.SetState(Enemy.EnemyState.Die);
        _owner.groupAi.GroupRemove(_owner);
        GetComponent<Animator>().enabled = false;

    }
    private IEnumerator DeathKnockbackCoroutine(Vector3 direction)
    {
        float elapsedTime = 0;
        Debug.Log("DEATH Knockback Start");

        // Y값을 포함한 넉백 방향을 정규화합니다.
        direction.Normalize();

        while (elapsedTime < KnockbackDuration) // 이 스크립트의 KnockbackDuration 사용
        {
            // 이 스크립트의 knockbackCurve 사용
            float curveValue = knockbackCurve.Evaluate(elapsedTime / KnockbackDuration);

            // 이 스크립트의 KnockbackForce 사용
            Vector3 move = direction * KnockbackForce * curveValue * Time.deltaTime;

            // 중력은 항상 적용
            if (!_characterController.isGrounded)
            {
                move.y += Physics.gravity.y * Time.deltaTime;
            }

            _characterController.Move(move);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _KnockbackCoroutine = null;
        Debug.Log("DEATH Knockback Finished");
    }
    public void TakeDamage(DamageData damageData)
    {
        if (Health <= 0) return;
        Debug.Log($"Enemy Take Damage: {damageData.DamageAmount}");
        Debug.Log($"Enemy Take Damage: {Health}");
        _owner.groupAi.CombatAll();
        if (!_owner._aiController.IsActionable())
        {
            _owner.SetState(Enemy.EnemyState.Hit);
            _owner.AnimationEvent("Hit");
        }
        _owner.animHandler.PlayFeedback("Damage_FB");
        _maxHealth -= damageData.DamageAmount;
        if (_owner.HealthBar)
        {
            _owner.BillboardUI?.SetHealthBar(Maxhealth, Health);
        }
        if (Knockbackable)
        {
            Vector3 knockbackDir = (transform.position - damageData.AttackerTransform.position).normalized;
            knockbackDir.y = 0;
            if (_KnockbackCoroutine != null)
            {
                StopCoroutine(_KnockbackCoroutine);
            }
            _KnockbackCoroutine = StartCoroutine(KnockbackCoroutine(knockbackDir, damageData));
        }
        if (Health <= 0)
        {
            if (_KnockbackCoroutine != null)
            {
                StopCoroutine(_KnockbackCoroutine);
            }
            Vector3 knockbackDir = (transform.position - damageData.AttackerTransform.position).normalized;
            knockbackDir.y = knockbackDirY;
            Die();
            damageData.KnockbackDuration = KnockbackDuration;
            damageData.KnockbackForce = KnockbackForce;
            _KnockbackCoroutine = StartCoroutine(DeathKnockbackCoroutine(knockbackDir));

        }
    }
    public AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float knockbackDirY = 0.6f;
    public float KnockbackDuration = 0.1f;
    public int KnockbackForce = 30;
    
}