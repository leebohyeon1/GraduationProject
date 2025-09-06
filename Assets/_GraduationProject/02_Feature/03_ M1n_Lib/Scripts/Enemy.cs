using UnityEngine;
using Pathfinding;
using System;
using BH_Lib.AssetManager;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor; // Handles 클래스를 사용하기 위해 반드시 필요합니다.
#endif
[RequireComponent(typeof(AIPath),typeof(AiController))]
public class Enemy : CharacterBase, IAttacker, IDamageable
{
    private AiController _aiController;
    Animator animator;
    public AudioClip deathSoundClip;
    public AudioClip EnemyCallingSoundClip;
    [Header("Enemy Settings")]

    [SerializeField] private float chaseRange = 10f;
    public float ChaseRange => chaseRange;

    [SerializeField] private float detectionRange = 10f;
    public float DetectionRange => detectionRange;

    [SerializeField] private float enemyDetect = 10f;
    public float EnemyDetect => enemyDetect;

    AIPath aIPath;
    public Player player;

    public int Maxhealth { get; set; }
    public int CurrentHealth { get; set; }
    Rigidbody rb;

    public bool CanParry { get; private set; } // 적이 플레이어의 공격을 막을 수 있는지 여부
    bool _isStunned = false;
    float _stunExitTime = -Mathf.Infinity;
    public float StunExitTime => _stunExitTime;
    public Vector3[] wayPoints;
    public int wayPointIndex = 0;
    //특수 공격이 공유하는 쿨타임 연속 특수 기술 방지용
    // [Header("Beam Attack Assets")]
    // [SerializeField] GameObject _beamWarningEffect;
    // [SerializeField] GameObject _beamAttackEffect;

    // GameObject _currentBeamWarning;
    // GameObject _currentBeamAttack;
    
    [SerializeField] private HeatDataBase heatDataBase;
    [SerializeField]private TierStatDatabase tierStatDatabase;
    public EnemyMovement Movement { get; private set; }

    event Action<int, int> IDamageable.OnHealthChanged
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    protected override void Awake()
    {
        // health = new Health(100);
        // health.OnDeath += OnEnemyDeath;
        base.Awake();

        // TODO: 적 데이터에서 최대 체력 가져오기
        InitializeHealth(MaxHealth, OnEnemyDeath);
        player = GameObject.FindFirstObjectByType<Player>();
        rb = GetComponent<Rigidbody>();

        _aiController = GetComponent<AiController>();
        _aiController.Initialize(this);

        StatCalculator.Initialize(tierStatDatabase);

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
    void Update()
    {
    }


    public virtual void parryied()
    {
        // player.IncreaseGauge(3);
    }

    #region Behavior Tree Conditions



    public bool IsStunned()
    {
        return _isStunned;
    }


    
    #endregion

    #region Animation Event
    public bool IsActive { get; private set; }
    public bool IsHitWindowOpen { get; private set; }
    public bool IsActionFinished { get; private set; }
    public bool IsSound { get; private set; }

    public void AnimationEvent_StartAction()
    {
        IsActive = true;
    }

    public void AnimationEvent_StartSound()
    {
        IsSound = true;
    }
    public void AnimationEvent_EndSound()
    {
        IsSound = false;
    }

    //공격 판정 킴
    public void AnimationEvent_OpenHitWindow()
    {
        IsHitWindowOpen = true;
    }

    // 공격 판정을 끔
    public void AnimationEvent_CloseHitWindow()
    {
        IsHitWindowOpen = false;
    }

    // 행동이 끝
    public void AnimationEvent_FinishAction()
    {
        IsActionFinished = true;
    }
    public void ResetActionFlags()
    {
        IsActionFinished = false;
        IsHitWindowOpen = false;
        IsActive = false;
    }
    #endregion
    #region parry
    public void ApplyStun(float duration)
    {
        if (_isStunned || IsDead) return; // 이미 스턴 상태라면 무시
        _isStunned = true;
        _stunExitTime = Time.time + duration;
        Movement.StopMovement(); // 스턴 상태에서는 이동을 멈춥니다.
        animator.SetTrigger("Stun"); // 스턴 애니메이션 트리거
    }

    public void ClearStun()
    {
        _isStunned = false;
    }
    //  스킬 쿨타임을 시작시키는 함수

    public void Parryenable()
    {
        CanParry = true;
    }
    public void Parrydisable()
    {
        CanParry = false;
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
        Rush
    }
    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    public int AttackDamage => throw new NotImplementedException();

    public float AttackSpeed => throw new NotImplementedException();

    public int Health => throw new NotImplementedException();

    public int MaxHealth => 100;

    public bool IsDead => CurrentHealth <= 0;

    public int maxHeat => throw new NotImplementedException();

    public int currentHeat => throw new NotImplementedException();

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
    private void OnEnemyDeath()
    {
        // soundManager.PlaySFXAtPosition(deathSoundClip, transform.position);
        //eventManager.Trigger("EnemyKilled", this);
        Destroy(gameObject);
    }




    // IHealth 인터페이스 구현
    public void InitializeHealth(int maxHealth, Action OnDeathCallback)
    {
        Maxhealth = maxHealth;
        CurrentHealth = maxHealth;
        OnDeath += OnDeathCallback;
    }

    public void Die()
    {
        SetState(EnemyState.Die);
    }

    public void TakeDamage(int amount, IAttacker attacker = null)
    {
        if (CurrentHealth <= 0) return;
        if (_aiController.IsActionable())
        {
            AnimationEvent("Hit");
        }
        CurrentHealth -= amount;
        Debug.Log($"Enemy took {amount} damage. Current Health: {CurrentHealth}");
        _aiController.CombatEnter();
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
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

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

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

        target.TakeDamage(AttackDamage, this);
    }


    #endregion
}

    #region Beam Warning
    // public void ToggleBeamWarning(bool isActive, float beamLength)
    // {
    //     if (_beamWarningEffect == null) return;

    //     if (isActive && _currentBeamWarning == null)
    //     {
    //         _currentBeamWarning = Instantiate(_beamWarningEffect, transform);
    //         _currentBeamWarning.transform.localRotation = Quaternion.Euler(90, 0, 0);
    //         _currentBeamWarning.transform.localScale = new Vector3(
    //             _currentBeamWarning.transform.localScale.x, // 기존 X 스케일 유지 (두께)
    //             beamLength,                                 // 길이 설정
    //             _currentBeamWarning.transform.localScale.z); // 기존 Z 스케일 유지 (두께)
    //         _currentBeamWarning.transform.localPosition = new Vector3(0, 0.5f, beamLength);
    //     }
    //     else if (!isActive && _currentBeamWarning != null)
    //     {
    //         Destroy(_currentBeamWarning);
    //     }
    // }
    // 매 프레임 플레이어를 향해 부드럽게 회전합니다. (애니메이터의 AimYaw 파라미터 업데이트용)
    // public void UpdateAimingAtPlayer()
    // {
    //     if (player == null) return;

    //     Vector3 direction = player.transform.position - transform.position;
    //     direction.y = 0; // Y축은 고정
    //     Quaternion lookRotation = Quaternion.LookRotation(direction);

    //     // Slerp를 사용하여 부드러운 회전 적용
    //     transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);

    //     // (필요 시) 애니메이터 파라미터 'AimYaw'를 업데이트하는 로직 추가
    //     // float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
    //     // animator.SetFloat("AimYaw", angle);
    // }

    // 빔 발사를 시작하고, 틱 데미지를 주는 코루틴을 실행합니다.
    // public void StartBeamAttack(float duration, float beamLength, float beamWidth)
    // {
    //     if (_beamAttackEffect == null) return;

    //     ToggleBeamWarning(false, 0);

    //     if (_currentBeamAttack == null)
    //     {

    //         _currentBeamAttack = Instantiate(_beamAttackEffect, transform);
    //         _currentBeamAttack.transform.localRotation = Quaternion.Euler(90, 0, 0);
    //         _currentBeamAttack.transform.localScale = new Vector3(beamWidth * 2, beamLength, beamWidth * 2);
    //         _currentBeamAttack.transform.localPosition = new Vector3(0, 0.5f, beamLength);
    //         if (_currentBeamAttack.TryGetComponent<BeamDamager>(out BeamDamager damager))
    //         {
    //             damager.Initialize(this);
    //         }
    //     }
    // }


    // // 빔 발사를 중지합니다.
    // public void StopBeamAttack()
    // {
    //     if (_currentBeamAttack != null)
    //     {
    //         Destroy(_currentBeamAttack);
    //         StopCoroutine("BeamTickDamageCoroutine"); // 코루틴도 확실히 중지
    //     }
    // }

    // // 0.2초마다 틱 데미지를 주는 코루틴
    // private IEnumerator BeamTickDamageCoroutine(float duration, float beamLength, float beamWidth)
    // {
    //     float timer = 0f;
    //     float tickInterval = 0.2f;
    //     float nextTickTime = Time.time;

    //     while (timer < duration)
    //     {
    //         timer += Time.deltaTime;

    //         if (Time.time >= nextTickTime)
    //         {
    //             nextTickTime = Time.time + tickInterval;

    //             if (Physics.SphereCast(transform.position, beamWidth, transform.forward, out RaycastHit hit, beamLength))
    //             {
    //                 if (hit.collider.TryGetComponent<Player>(out Player player))
    //                 {
    //                     player.TakeDamage(1, this); // 기획서에 명시된 틱당 데미지(8)로 수정 필요
    //                     Debug.Log("Beam Tick Damage!");
    //                 }
    //                 Debug.Log($"Beam Hit: {hit.collider.name}");
    //             }
    //             Debug.Log($"Beam Missed: {hit.collider.name}");

    //         }
    //         yield return null;
    //     }
    // }
    #endregion