using UnityEngine;

/// <summary>
/// 포물선 비행 후 지뢰로 전환되는 적 투사체입니다.
/// </summary>
[DisallowMultipleComponent]
public class ParabolicMineProjectile : MonoBehaviour
{
    private static readonly Collider[] ExplosionHitBuffer = new Collider[8];

    private enum MineState
    {
        None,
        Flying,
        DroppingAfterWallHit,
        ArmedMine,
        Exploding,
        Finished
    }

    private const float DefaultGroundCastOffset = 2.0f;
    private const float DefaultGroundCastDistance = 256.0f;
    private const float DefaultMineHoverOffset = 0.05f;
    private const float DefaultDropDuration = 0.2f;
    private const float DefaultHitProbeRadius = 0.2f;

    private Enemy _enemy;
    private Transform _cachedTransform;
    private Transform _playerTransform;
    private Collider _playerCollider;
    private PlayerHealth _playerHealth;
    private BlackBoard _blackboard;
    private GameObject _owner;
    private DamageData _damageData;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private Vector3 _currentPosition;
    private Vector3 _dropTargetPosition;

    private float _projectileDuration;
    private float _jumpHeight;
    private float _mineDuration;
    private float _detectRadius;
    private float _explodeDelay;
    private float _explosionRadius;
    private float _flightElapsed;
    private float _armedStartTime;
    private float _dropSpeed;
    private float _explodeTriggerTime;
    private float _hitProbeRadius = DefaultHitProbeRadius;

    private int _explosionDamage;

    private LayerMask _groundLayerMask;
    private LayerMask _wallLayerMask;
    private LayerMask _playerLayerMask;

    private string _feedbackName;
    private Collider[] _selfColliders;
    private int _enemyLayer;

    private bool _hasArmedTrigger;
    private bool _hasAppliedDamage;
    private bool _hasRegisteredHit;
    private MineState _state;

    /// <summary>
    /// 투사체 비행/지뢰 상태를 초기화합니다.
    /// </summary>
    public void Setup(
        Enemy enemy,
        Vector3 startPos,
        Vector3 targetPos,
        GameObject owner,
        float projectileDuration,
        float jumpHeight,
        float mineDuration,
        float detectRadius,
        float explodeDelay,
        DamageData damageData,
        int explosionDamage,
        float explosionRadius,
        LayerMask groundLayer,
        LayerMask wallLayer,
        LayerMask playerLayer,
        string feedbackName)
    {
        _enemy = enemy;
        _owner = owner;
        _blackboard = _enemy != null ? _enemy._aiController._aiBrain.blackboard : null;
        _damageData = damageData;
        _projectileDuration = Mathf.Max(0.05f, projectileDuration);
        _jumpHeight = Mathf.Max(0f, jumpHeight);
        _mineDuration = Mathf.Max(0f, mineDuration);
        _detectRadius = Mathf.Max(0f, detectRadius);
        _explodeDelay = Mathf.Max(0f, explodeDelay);
        _explosionDamage = explosionDamage;
        _explosionRadius = Mathf.Max(0f, explosionRadius);
        _groundLayerMask = groundLayer.value != 0 ? groundLayer : LayerMask.GetMask("Ground");
        _wallLayerMask = wallLayer.value != 0 ? wallLayer : LayerMask.GetMask("Wall");
        _playerLayerMask = playerLayer.value != 0 ? playerLayer : LayerMask.GetMask("Player");
        _feedbackName = feedbackName;
        _startPosition = startPos;
        _targetPosition = targetPos;
        _currentPosition = startPos;
        _state = MineState.Flying;
        _hasArmedTrigger = false;
        _hasAppliedDamage = false;
        _hasRegisteredHit = false;
        _flightElapsed = 0f;
        _explodeTriggerTime = -1f;

        CachePlayerReferences();
        IgnoreOwnerAndEnemyCollisions();

        _cachedTransform.position = _startPosition;
        Vector3 initialDirection = _targetPosition - _startPosition;
        if (initialDirection.sqrMagnitude > 0.0001f)
        {
            _cachedTransform.rotation = Quaternion.LookRotation(initialDirection.normalized);
        }
    }

    private void Awake()
    {
        _cachedTransform = transform;
        _selfColliders = GetComponentsInChildren<Collider>(true);
        _enemyLayer = LayerMask.NameToLayer("Enemy");

        Collider ownCollider = GetComponent<Collider>();
        if (ownCollider != null)
        {
            _hitProbeRadius = Mathf.Max(DefaultHitProbeRadius, ownCollider.bounds.extents.magnitude * 0.5f);
        }
    }

    private void Update()
    {
        switch (_state)
        {
            case MineState.Flying:
                UpdateFlying();
                break;
            case MineState.DroppingAfterWallHit:
                UpdateDropping();
                break;
            case MineState.ArmedMine:
                UpdateArmedMine();
                break;
        }
    }

    private void CachePlayerReferences()
    {
        if (_enemy == null || _enemy.player == null)
        {
            return;
        }

        _playerTransform = _enemy.player.transform;
        _playerHealth = _enemy.player.GetComponent<PlayerHealth>();
        if (_playerHealth == null)
        {
            _playerHealth = _enemy.player.GetComponentInChildren<PlayerHealth>();
        }

        _playerCollider = _enemy.player.GetComponent<Collider>();
        if (_playerCollider == null)
        {
            _playerCollider = _enemy.player.GetComponentInChildren<Collider>();
        }
    }

    private void UpdateFlying()
    {
        float previousNormalizedTime = Mathf.Clamp01(_flightElapsed / _projectileDuration);
        Vector3 previousPosition = EvaluateParabola(previousNormalizedTime);

        _flightElapsed += Time.deltaTime;

        float normalizedTime = Mathf.Clamp01(_flightElapsed / _projectileDuration);
        Vector3 nextPosition = EvaluateParabola(normalizedTime);

        if (TryGetFirstImpact(previousPosition, nextPosition, out RaycastHit hit))
        {
            HandleImpact(hit);
            return;
        }

        _currentPosition = nextPosition;
        _cachedTransform.position = _currentPosition;
        UpdateRotation(previousPosition, nextPosition);

        if (IsPlayerWithinRadius(_currentPosition, _hitProbeRadius))
        {
            ExplodeNow(_playerHealth);
            return;
        }

        if (normalizedTime >= 1.0f)
        {
            Vector3 landingPosition = ResolveGroundPoint(_targetPosition, _currentPosition);
            ArmMine(landingPosition);
        }
    }

    private void UpdateDropping()
    {
        Vector3 nextPosition = Vector3.MoveTowards(_currentPosition, _dropTargetPosition, _dropSpeed * Time.deltaTime);
        _currentPosition = nextPosition;
        _cachedTransform.position = _currentPosition;

        if (IsPlayerWithinRadius(_currentPosition, _hitProbeRadius))
        {
            ExplodeNow(_playerHealth);
            return;
        }

        if ((_currentPosition - _dropTargetPosition).sqrMagnitude <= 0.0001f)
        {
            ArmMine(_dropTargetPosition);
        }
    }

    private void UpdateArmedMine()
    {
        if (_mineDuration > 0f && Time.time - _armedStartTime >= _mineDuration)
        {
            Debug.Log("[ParabolicMineProjectile] Mine duration expired -> force explode");
            ExplodeNow(null);
            return;
        }

        if (!_hasArmedTrigger && IsPlayerWithinRadius(_cachedTransform.position, _detectRadius))
        {
            _hasArmedTrigger = true;
            _explodeTriggerTime = Time.time + _explodeDelay;
        }

        if (_hasArmedTrigger && Time.time >= _explodeTriggerTime)
        {
            ExplodeNow(null);
        }
    }

    private Vector3 EvaluateParabola(float normalizedTime)
    {
        Vector3 position = Vector3.Lerp(_startPosition, _targetPosition, normalizedTime);
        float baseY = Mathf.Lerp(_startPosition.y, _targetPosition.y, normalizedTime);
        float arcOffset = 4f * _jumpHeight * normalizedTime * (1f - normalizedTime);
        position.y = baseY + arcOffset;
        return position;
    }

    private bool TryGetFirstImpact(Vector3 from, Vector3 to, out RaycastHit hit)
    {
        Vector3 delta = to - from;
        float distance = delta.magnitude;
        if (distance <= 0.0001f)
        {
            hit = default;
            return false;
        }

        LayerMask collisionMask = _groundLayerMask | _wallLayerMask | _playerLayerMask;
        return Physics.Raycast(from, delta / distance, out hit, distance, collisionMask, QueryTriggerInteraction.Collide);
    }

    private void HandleImpact(RaycastHit hit)
    {
        int hitLayerMask = 1 << hit.collider.gameObject.layer;

        if ((_playerLayerMask.value & hitLayerMask) != 0 && IsPlayerCollider(hit.collider))
        {
            _currentPosition = hit.point;
            _cachedTransform.position = _currentPosition;
            ExplodeNow(_playerHealth);
            return;
        }

        if ((_wallLayerMask.value & hitLayerMask) != 0)
        {
            BeginWallDrop(hit.point);
            return;
        }

        if ((_groundLayerMask.value & hitLayerMask) != 0)
        {
            ArmMine(hit.point);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleContact(other, other != null ? other.ClosestPoint(_cachedTransform.position) : _cachedTransform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null)
        {
            return;
        }

        Vector3 contactPoint = collision.contactCount > 0
            ? collision.GetContact(0).point
            : _cachedTransform.position;

        HandleContact(collision.collider, contactPoint);
    }

    private bool IsPlayerCollider(Collider hitCollider)
    {
        if (_playerTransform == null || hitCollider == null)
        {
            return false;
        }

        Transform hitTransform = hitCollider.transform;
        return hitTransform == _playerTransform || hitTransform.IsChildOf(_playerTransform);
    }

    private void BeginWallDrop(Vector3 hitPoint)
    {
        Debug.Log($"[ParabolicMineProjectile] Wall hit -> vertical drop at {hitPoint}");
        Vector3 dropStartPosition = hitPoint;
        Vector3 groundPoint = ResolveGroundPoint(hitPoint, hitPoint);
        _dropTargetPosition = new Vector3(hitPoint.x, groundPoint.y, hitPoint.z);

        if (_dropTargetPosition.y >= dropStartPosition.y)
        {
            ArmMine(_dropTargetPosition);
            return;
        }

        _state = MineState.DroppingAfterWallHit;
        _currentPosition = dropStartPosition;
        _cachedTransform.position = _currentPosition;
        _dropSpeed = Mathf.Max(4f, Mathf.Abs(_currentPosition.y - _dropTargetPosition.y) / DefaultDropDuration);
    }

    private void HandleContact(Collider other, Vector3 contactPoint)
    {
        if (_state == MineState.Exploding || _state == MineState.Finished)
        {
            return;
        }

        if (other == null || (_owner != null && other.gameObject == _owner))
        {
            return;
        }

        if (_enemyLayer >= 0 && other.gameObject.layer == _enemyLayer)
        {
            IgnoreCollider(other);
            return;
        }

        int otherLayerMask = 1 << other.gameObject.layer;

        if (_state != MineState.Flying)
        {
            return;
        }

        if ((_wallLayerMask.value & otherLayerMask) != 0)
        {
            BeginWallDrop(contactPoint);
            return;
        }

        if ((_groundLayerMask.value & otherLayerMask) != 0)
        {
            ArmMine(contactPoint);
        }
    }

    private void IgnoreOwnerAndEnemyCollisions()
    {
        if (_owner != null)
        {
            IgnoreCollisionsWithGameObject(_owner);
        }

        if (_enemyLayer < 0)
        {
            return;
        }

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy foundEnemy = enemies[i];
            if (foundEnemy == null)
            {
                continue;
            }

            IgnoreCollisionsWithGameObject(foundEnemy.gameObject);
        }
    }

    private void IgnoreCollisionsWithGameObject(GameObject targetObject)
    {
        if (targetObject == null || _selfColliders == null)
        {
            return;
        }

        Collider[] targetColliders = targetObject.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < targetColliders.Length; i++)
        {
            IgnoreCollider(targetColliders[i]);
        }
    }

    private void IgnoreCollider(Collider targetCollider)
    {
        if (targetCollider == null || _selfColliders == null)
        {
            return;
        }

        for (int i = 0; i < _selfColliders.Length; i++)
        {
            Collider selfCollider = _selfColliders[i];
            if (selfCollider == null || selfCollider == targetCollider)
            {
                continue;
            }

            Physics.IgnoreCollision(selfCollider, targetCollider, true);
        }
    }

    private Vector3 ResolveGroundPoint(Vector3 preferredPosition, Vector3 fallbackPosition)
    {
        Vector3 rayOrigin = preferredPosition + Vector3.up * DefaultGroundCastOffset;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, DefaultGroundCastDistance, _groundLayerMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        rayOrigin = fallbackPosition + Vector3.up * DefaultGroundCastOffset;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, DefaultGroundCastDistance, _groundLayerMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return fallbackPosition;
    }

    private void ArmMine(Vector3 groundPosition)
    {
        Debug.Log($"[ParabolicMineProjectile] Mine armed at {groundPosition}");
        _state = MineState.ArmedMine;
        _currentPosition = groundPosition + Vector3.up * DefaultMineHoverOffset;
        _cachedTransform.position = _currentPosition;
        _armedStartTime = Time.time;
        _hasArmedTrigger = false;
        _explodeTriggerTime = -1f;

        if (IsPlayerWithinRadius(_cachedTransform.position, _detectRadius))
        {
            _hasArmedTrigger = true;
            _explodeTriggerTime = Time.time + _explodeDelay;
        }
    }

    private bool IsPlayerWithinRadius(Vector3 origin, float radius)
    {
        if (_playerTransform == null || _playerHealth == null || _playerHealth.IsDead)
        {
            return false;
        }

        Vector3 closestPoint = _playerCollider != null
            ? _playerCollider.ClosestPoint(origin)
            : _playerTransform.position;

        return (closestPoint - origin).sqrMagnitude <= radius * radius;
    }

    private void ExplodeNow(PlayerHealth directHitTarget)
    {
        if (_state == MineState.Exploding || _state == MineState.Finished)
        {
            return;
        }

        Debug.Log($"[ParabolicMineProjectile] Explode at {_cachedTransform.position}");
        _state = MineState.Exploding;
        PlayFeedback(_cachedTransform.position);

        bool didHit = false;
        if (directHitTarget != null)
        {
            didHit = ApplyDamage(directHitTarget);
        }

        if (!didHit)
        {
            ApplyExplosionDamage();
        }

        _state = MineState.Finished;
        ProjectilePoolManager.ReleaseProjectile(gameObject);
    }

    private void ApplyExplosionDamage()
    {
        if (_playerHealth == null || _playerHealth.IsDead)
        {
            return;
        }

        if (IsPlayerInsideExplosionRange())
        {
            ApplyDamage(_playerHealth);
        }
    }

    private bool IsPlayerInsideExplosionRange()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            _cachedTransform.position,
            _explosionRadius,
            ExplosionHitBuffer,
            _playerLayerMask,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = ExplosionHitBuffer[i];
            if (IsPlayerCollider(hitCollider))
            {
                return true;
            }
        }

        return false;
    }

    private bool ApplyDamage(PlayerHealth target)
    {
        if (_hasAppliedDamage || target == null || target.IsDead)
        {
            return false;
        }

        DamageData appliedDamage = _damageData;
        appliedDamage.AttackerTransform = _cachedTransform;
        if (_explosionDamage > 0)
        {
            appliedDamage.DamageAmount = _explosionDamage;
        }

        target.TakeDamage(appliedDamage);
        _hasAppliedDamage = true;
        RegisterSuccessfulHit();
        return true;
    }

    private void RegisterSuccessfulHit()
    {
        if (_hasRegisteredHit || _blackboard == null)
        {
            return;
        }

        AttackOutcomeRecorder.RecordSuccessfulHit(_blackboard);
        _hasRegisteredHit = true;
    }

    private void PlayFeedback(Vector3 position)
    {
        if (_enemy == null || _enemy.animHandler == null || string.IsNullOrEmpty(_feedbackName))
        {
            return;
        }

        _enemy.animHandler.PlayFeedbackAtPosition(_feedbackName, position);
    }

    private void OnDisable()
    {
        _state = MineState.None;
        _hasArmedTrigger = false;
        _hasAppliedDamage = false;
        _hasRegisteredHit = false;
        _enemy = null;
        _playerTransform = null;
        _playerCollider = null;
        _playerHealth = null;
        _blackboard = null;
        _owner = null;
    }

    private void UpdateRotation(Vector3 previousPosition, Vector3 nextPosition)
    {
        Vector3 direction = nextPosition - previousPosition;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        _cachedTransform.rotation = Quaternion.LookRotation(direction.normalized);
    }

}
