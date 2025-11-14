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
        _owner.tag = "Enemy";
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
        _owner.tag = "DeadEnemy";
    }
    private IEnumerator DieSequence(Vector3 direction)
    {
    SetRagdollState(true);
    
    _characterController.enabled = false;
    _owner.animator.enabled = false;
    yield return new WaitForSeconds(0.1f);
    Vector3 combinedForce = (direction * KnockbackForce) + (Vector3.up * upwardForce);
    CombineAddForce(combinedForce, direction);
    yield return new WaitForSeconds(1f);
    centralRigidbody.linearVelocity = Vector3.zero;
    SetRagdollState(true);
    SetZeroJoint(false);
}
    public void TakeDamage(DamageData damageData)
    {
        if (Health <= 0) return;
        _owner._aiController._aiBrain.blackboard.SetValue("OnTakeHit", true);
        _owner.groupAi.CombatAll();
        if (!_owner._aiController.IsActionable())
        {
            _owner.SetState(Enemy.EnemyState.Hit);
            _owner.AnimationEvent("Hit");
        }
        _owner.animHandler.PlayFeedback("Damage_FB");
        _maxHealth -= damageData.DamageAmount;
        _owner._aiController._aiBrain.blackboard.SetValue("SelfHealth", _maxHealth);
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
            Die();
            _KnockbackCoroutine = StartCoroutine(DieSequence(knockbackDir));

        }
    }
    private void SetRagdollState(bool isActive)
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = !isActive;
        }
    }
    private void CombineAddForce(Vector3 force, Vector3 direction)
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.AddForce(force, ForceMode.Impulse);
        }
    }
    private Rigidbody[] ragdollRigidbodies;
    private CharacterJoint[] ragdollCharacterJoints;
    void Start()
    {
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollCharacterJoints = GetComponentsInChildren<CharacterJoint>();
        SetRagdollState(false);
    }

    public float KnockbackForce = 30f;
    public float upwardForce = 5f;
    public Rigidbody centralRigidbody;
     private void SetZeroJoint(bool isActive)
    {
        foreach (CharacterJoint cj in ragdollCharacterJoints)
        {
            cj.swingLimitSpring = new SoftJointLimitSpring { damper = isActive ? 50 : 0 };
        }
    }
}