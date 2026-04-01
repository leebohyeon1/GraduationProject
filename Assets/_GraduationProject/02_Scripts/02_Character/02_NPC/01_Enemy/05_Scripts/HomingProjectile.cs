using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class HomingProjectile : MonoBehaviour
{
    private Transform _target;
    private Collider _targetCollider;
    private DamageData _damage;
    
    // 설정값들
    private float _duration;
    private float _startSpeed;
    private float _acceleration;
    private float _maxSpeed;
    private float _turnForce;
    private float _straightSpeed;
    private LayerMask _obstacleMask;

    private float _startTime;
    private bool _isInitialized = false;
    private bool _isStraightMode = false;
    private Enemy _enemy;
    // 초기화 함수 (전략 스크립트에서 호출)

    [SerializeField] private float FeedbackDelay = 0.1f; // 피드백 딜레이 (예: 폭발 효과)
    [SerializeField] private string feedbackname = "null"; // 피격 효과 프리팹
    public void Initialize(Transform target, DamageData damage, LayerMask obstacleMask, Enemy enemy,
                           float duration, float startSpd, float accel, float maxSpd, float turnForce, float straightSpd)
    {
        _target = target;
        _damage = damage;
        _obstacleMask = obstacleMask;
        _duration = duration;
        _startSpeed = startSpd;
        _acceleration = accel;
        _maxSpeed = maxSpd;
        _turnForce = turnForce;
        _straightSpeed = straightSpd;

        _startTime = Time.time;
        _isInitialized = true;
        _damage.AttackerTransform = this.transform;
        _enemy = enemy;

        if (_target != null)
        {
            _targetCollider = _target.GetComponent<Collider>();
            
            // 만약 타겟 본체에 콜라이더가 없고 자식에 있다면 (예: 리깅된 캐릭터)
            if (_targetCollider == null)
                _targetCollider = _target.GetComponentInChildren<Collider>();
        }
        // 초기 방향은 발사체 자체의 forward (생성 시 설정됨)
    }

    void Update()
    {
        if (!_isInitialized) return;

        float elapsedTime = Time.time - _startTime;

        // 1. 모드 전환 체크 (시간 초과 시 직선 비행)
        if (elapsedTime >= _duration && !_isStraightMode)
        {
            _isStraightMode = true;
        }

        float currentSpeed = 0f;
        Vector3 moveDir = transform.forward;

        if (!_isStraightMode)
        {
            // [유도 모드]
            // 속도 계산 (v = v0 + at)
            currentSpeed = _startSpeed + (_acceleration * elapsedTime);
            currentSpeed = Mathf.Min(currentSpeed, _maxSpeed);
            
            if (_target != null)
            {
                Vector3 targetCenterPos;

                if (_targetCollider != null)
                {
                    // 콜라이더가 있다면 그 중심점(bounds.center)을 타겟으로 잡음
                    targetCenterPos = _targetCollider.bounds.center;
                }
                else
                {
                    // 예외 처리: 콜라이더를 못 찾았을 경우 임시로 1m 위를 조준
                    targetCenterPos = _target.position + Vector3.up * 1.0f;
                }
                // 유도 회전 로직
                Vector3 dirToTarget = (targetCenterPos - transform.position).normalized;
                // 높이 보정 (타겟의 가슴팍 등) 필요시 target.position + Vector3.up * 1.0f
                
                Vector3 newDir = Vector3.RotateTowards(transform.forward, dirToTarget, _turnForce * Mathf.Deg2Rad * Time.deltaTime, 0.0f);
                transform.rotation = Quaternion.LookRotation(newDir);
            }
        }
        else
        {
            // [직선 모드]
            currentSpeed = _straightSpeed;
            // 회전 없이 직진
        }

        // 2. 이동 적용
        transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isInitialized) return;

        // 벽 충돌 (LayerMask 연산)
        if (((1 << other.gameObject.layer) & _obstacleMask) != 0)
        {
            // 벽에 부딪힘 -> 소멸
            _enemy.animHandler.PlayFeedbackAtPosition(feedbackname, transform.position);
            Destroy(gameObject);
            return;
        }

        // 플레이어 충돌
        if (other.CompareTag("Player")) // 혹은 Component 체크
        {
            // 데미지 주기 (구현된 PlayerHealth 스크립트 사용)
            var health = other.GetComponent<PlayerHealth>(); 
            if (health != null)
            {
                // 간단한 데미지 처리 (구조체 필요시 수정)
                health.TakeDamage(_damage); 
                _enemy._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.DidLastAttackHit, true);
                Debug.Log($"[Projectile] Player Hit! Damage: {_damage}");
            }
            _enemy.animHandler.PlayFeedbackAtPosition(feedbackname, transform.position);

            Destroy(gameObject);
        }
    }
}