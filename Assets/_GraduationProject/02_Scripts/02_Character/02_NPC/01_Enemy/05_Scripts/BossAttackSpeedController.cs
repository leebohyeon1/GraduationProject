using UnityEngine;

/// <summary>
/// 보스 몬스터의 체력에 따라 공격 애니메이션 속도를 조절하는 컴포넌트입니다.
/// </summary>
public class BossAttackSpeedController : MonoBehaviour
{
    private Enemy _enemy;
    private EnemyHealth _health;
    private Animator _animator;
    private Enemy_AnimationEventHandler _animHandler;

    [Header("Speed Scaling Settings")]
    [Tooltip("애니메이션 속도가 증가하기 시작하는 체력 비율 (0.5 = 50%)")]
    [SerializeField] private float _startHealthThreshold = 0.5f;
    
    [Tooltip("최대 애니메이션 속도에 도달하는 체력 비율 (0.1 = 10%)")]
    [SerializeField] private float _maxSpeedThreshold = 0.1f;
    
    [Tooltip("도달할 수 있는 최대 애니메이션 속도")]
    [SerializeField] private float _maxAnimationSpeed = 2.0f;

    [Header("Exception Tags")]
    [Tooltip("속도 조절에서 제외할 애니메이션 태그 (예: 가짜 공격)")]
    [SerializeField] private string _fakeAttackTag = "Boss_Fake_Attack";

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _health = GetComponent<EnemyHealth>();
        _animator = GetComponent<Animator>();
        _animHandler = GetComponent<Enemy_AnimationEventHandler>();
    }

    private void Update()
    {
        if (_enemy == null || _health == null || _animator == null || _health.IsDead)
            return;

        float targetSpeed = 1.0f;

        // 현재 체력 비율 계산
        float healthRatio = (float)_health.CurrentHealth / _health.MaxHealth;

        // 공격 관련 상태인지 확인
        if (IsAttackState())
        {
            // 현재 재생 중인 애니메이션 정보 가져오기
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo nextStateInfo = _animator.GetNextAnimatorStateInfo(0);

            // 가짜 공격(Fake Attack)인지 확인 - 태그 기준
            bool isFakeAttack = stateInfo.IsTag(_fakeAttackTag) || nextStateInfo.IsTag(_fakeAttackTag);

            if (!isFakeAttack)
            {
                // 체력이 임계치 이하일 때 속도 계산
                if (healthRatio <= _startHealthThreshold)
                {
                    float t = Mathf.InverseLerp(_startHealthThreshold, _maxSpeedThreshold, healthRatio);
                    targetSpeed = Mathf.Lerp(1.0f, _maxAnimationSpeed, t);
                }
            }
        }

        // 애니메이터 속도 적용
        _animator.speed = targetSpeed;

        // 이펙트(피드백) 핸들러 속도 적용
        if (_animHandler != null)
        {
            _animHandler.SpeedMultiplier = targetSpeed;
        }
    }

    /// <summary>
    /// 현재 보스가 공격 관련 상태인지 확인합니다.
    /// </summary>
    private bool IsAttackState()
    {
        EnemyStateController.EnemyState currentState = _enemy.CurrentState;
        
        // Attack, Beam, Rush 상태를 공격 관련 상태로 간주합니다.
        return currentState == EnemyStateController.EnemyState.Attack ||
               currentState == EnemyStateController.EnemyState.Beam ||
               currentState == EnemyStateController.EnemyState.Rush;
    }

    /// <summary>
    /// 기획자가 인스펙터에서 설정한 값이 올바른지 확인하기 위한 OnValidate
    /// </summary>
    private void OnValidate()
    {
        if (_maxSpeedThreshold >= _startHealthThreshold)
        {
            _maxSpeedThreshold = _startHealthThreshold - 0.01f;
        }
        
        _startHealthThreshold = Mathf.Clamp01(_startHealthThreshold);
        _maxSpeedThreshold = Mathf.Clamp01(_maxSpeedThreshold);
        _maxAnimationSpeed = Mathf.Max(1.0f, _maxAnimationSpeed);
    }
}
