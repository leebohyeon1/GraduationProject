using UnityEngine;
using Pathfinding;
using System;
using Unity.Mathematics;
using UnityEditor.Rendering;
using System.Collections;




#if UNITY_EDITOR
using UnityEditor; // Handles 클래스를 사용하기 위해 반드시 필요합니다.
#endif
[RequireComponent(typeof(AIPath),typeof(AiController)),RequireComponent(typeof(Enemy_AnimationEventHandler),typeof(ParrySystem)),RequireComponent(typeof(Monster_HeatSystem))]
public class Enemy : CharacterBase, IAttacker, IDamageable
{
    private AiController _aiController;
    Animator animator;
    public AudioClip deathSoundClip;
    

    AIPath aIPath;
    public Player player;

    public int Maxhealth { get; set; }
    public int CurrentHealth { get; set; }
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;
    
    Rigidbody rb;

    public Enemy_AnimationEventHandler animHandler;
    bool _isStunned = false;
    float _stunExitTime = -Mathf.Infinity;
    public float StunExitTime => _stunExitTime;
    [SerializeField] private float _stunTime = 3f;
    public Vector3[] wayPoints;
    public int wayPointIndex = 0;

    [SerializeField]private TierStatDatabaseSO tierStatDatabase;
    public EnemyMovement Movement { get; private set; }
    protected override void Awake()
    {
        // health = new Health(100);
        // health.OnDeath += OnEnemyDeath;
        base.Awake();

        // TODO: 적 데이터에서 최대 체력 가져오기
        InitializeHealth(MaxHealth);
        player = GameObject.FindFirstObjectByType<Player>();
        rb = GetComponent<Rigidbody>();

        _aiController = GetComponent<AiController>();
        _aiController.Initialize(this);

        StatCalculator.Initialize(tierStatDatabase);
        animHandler = GetComponent<Enemy_AnimationEventHandler>();
        GetComponent<HeatSystem>().Init(ActorType.Monster);

    }

    void Start()
    {
        animator = GetComponent<Animator>();
        aIPath = GetComponent<AIPath>();
        if (aIPath == null)
        {
            Debug.LogError("AIPath component not found in the scene.");
        }
        Movement = new EnemyMovement(this);

    }
    #region Behavior Tree Conditions
    public bool IsStunned()
    {
        return _isStunned;
    }
    #endregion

    #region parry
    public void ApplyStun()
    {
        if (_isStunned || IsDead) return; // 이미 스턴 상태라면 무시
        _isStunned = true;
        _stunExitTime = Time.time + _stunTime;
        Movement.StopMovement(); // 스턴 상태에서는 이동을 멈춥니다.
        animator.SetTrigger("Stun"); // 스턴 애니메이션 트리거
    }
    public void ApplyStun(float stunDuration)
    {
        if (_isStunned || IsDead) return; // 이미 스턴 상태라면 무시
        _isStunned = true;
        _stunExitTime = Time.time + stunDuration;
        Movement.StopMovement(); // 스턴 상태에서는 이동을 멈춥니다.
        animator.SetTrigger("Stun"); // 스턴 애니메이션 트리거
    }

    public void ClearStun()
    {
        _isStunned = false;
    }
 
    #endregion
    #region Enemy State Management
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Beam,
        Noise,
        Die,
        Stunned, // 스턴 상태 추가
        Rush,
        Hit
    }
    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;


    public int Health => throw new NotImplementedException();

    public int MaxHealth => 100;

    public bool IsDead => CurrentHealth <= 0;

    public bool IsInvincible => throw new NotImplementedException();

    public bool IsHit => throw new NotImplementedException();

    public void SetState(EnemyState state)
    {
        CurrentState = state;
    }
    #endregion

    public Animator GetAnimator()
    {
        return animator;
    }
    public void AnimationEvent(string eventName)
    {
        if (animator != null)
        {
            animator.SetTrigger(eventName);
        }
    }
    public void AnimationBool(string boolName, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(boolName, value);
        }
    }
    private void OnEnemyDeath()
    {
        // soundManager.PlaySFXAtPosition(deathSoundClip, transform.position);
        //eventManager.Trigger("EnemyKilled", this);
        Destroy(gameObject);
    }




    // IHealth 인터페이스 구현
    public void InitializeHealth(int maxHealth)
    {
        Maxhealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    public void Die()
    {
        animator.SetBool("Die", true);
        SetState(EnemyState.Die);
    }

    public void TakeDamage(int amount, IAttacker attacker = null)
    {
        if (CurrentHealth <= 0) return;
        if (!_aiController.IsActionable())
        {
            _aiController._aiBrain.SetState(Enemy.EnemyState.Hit);
        }
        CurrentHealth -= amount;
        Debug.Log($"Enemy took {amount} damage. Current Health: {CurrentHealth}");
        _aiController.CombatEnter();
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
        OnHealthChanged.Invoke(CurrentHealth + amount, CurrentHealth);
    }
    public void TakeDamage(int percentDamage, float Time)
    {
        int perDmg = Maxhealth / percentDamage;
        StartCoroutine(PerDmgTimer(perDmg, Time));
    }

    private IEnumerator PerDmgTimer(int perDmg, float time)
    {
        float timer = 0f;
        while (timer < time)
        {
            TakeDamage(perDmg);
            timer += 1f;
            Debug.Log("데미지 받는중");
            yield return new WaitForSeconds(1f);
        }
    }
    

    [SerializeField] GameObject LastRushHitObject;
    public GameObject GetLastRushHitObject()
    {
        return LastRushHitObject;
    }
    public void SetLastRushHitObject(GameObject obj)
    {
        LastRushHitObject = obj;
    }
    private void OnCollisionEnter(Collision collision)
    {
        // 현재 상태가 '돌진'일 때만 충돌을 감지합니다.
        if (CurrentState != EnemyState.Rush) return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // 바닥과의 충돌은 무시하고 함수를 종료합니다.
            return;
        }

        Debug.Log($"<color=red>--ENEMY--: Collision Detected with {collision.gameObject.name} while rushing!</color>");
        SetLastRushHitObject(collision.gameObject);
    }


    #region gizmo
    [Header("Attack Range")]
    public float _currentAttackRadius;
    public Vector3 _currentAttackOffset;
    public void SetCurrentAttackData(float radius, Vector3 offset)
    {
        _currentAttackRadius = radius;
        _currentAttackOffset = offset;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 attackOrigin = transform.position + transform.TransformDirection(_currentAttackOffset);

        Gizmos.DrawWireSphere(attackOrigin, _currentAttackRadius);
#if UNITY_EDITOR
        // 기즈모 라벨의 스타일을 설정합니다.
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;

        // 1m부터 30m까지 원과 텍스트를 그립니다.
         for (int i = 1; i <= 30; i++)
        {
            Handles.color = Color.Lerp(Color.green, Color.blue, i / 30f);

            Handles.DrawWireDisc(transform.position, Vector3.up, i);
            
            Vector3 textPosition = transform.position + transform.forward * i;
            Handles.Label(textPosition, $"{i}m", style);
        }
#endif
    }
    
    public void Attack(IDamageable target)
    {
        if (target == null || target.IsDead)
        {
            return;
        }

    }

    public void ResetHitState()
    {
        throw new NotImplementedException();
    }


    #endregion
}
