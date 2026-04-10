using UnityEngine;
using DG.Tweening;
using System;

public abstract class TotemBase : MonoBehaviour, IDamageable
{
    [Header("Base Settings")]
    [SerializeField] protected float _moveSpeed = 10f;
    [SerializeField] protected TotemType _type;
    [SerializeField] protected bool _isMovable = true; // 이동 가능 여부 (기본값 true)
    
    protected Vector2Int _startGridPos;
    protected Vector2Int _currentGridPos;
    protected TotemState _state = TotemState.Idle;
    
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    public bool IsDead => _state == TotemState.Destroyed;
    
    public int Health => 1;
    public int MaxHealth => 1;
    public bool IsInvincible => false;
    
    // 외부에서 설정 가능하도록 프로퍼티 제공
    public bool IsMovable { get => _isMovable; set => _isMovable = value; }

    public int CurrentHealth => Health;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        if (PuzzleGridManager.Instance != null)
        {
            _currentGridPos = PuzzleGridManager.Instance.WorldToGrid(transform.position);
            _startGridPos = _currentGridPos;
            
            transform.position = PuzzleGridManager.Instance.GridToWorld(_currentGridPos);
            
            PuzzleGridManager.Instance.RegisterTotem(this, _currentGridPos);
        }
    }

    public void TakeDamage(DamageData damageData)
    {
        // 이동 불가능한 토템이면 반응 안 함 (흔들림도 X 혹은 흔들림 O?)
        // 보통 고정 벽은 때려도 꿈쩍 않는 게 자연스러움.
        if (!_isMovable) 
        {
            // OnHitBlocked(); // 필요하면 흔들림 추가
            return;
        }

        if (_state != TotemState.Idle && _state != TotemState.Destroyed) return;
        if (IsDead) return;

        if (!IsChargedAttack(damageData.AttackType))
        {
            OnHitBlocked();
            return;
        }

        Vector3 incomingDir = (transform.position - damageData.AttackerTransform.position).normalized;
        Vector2Int moveDir = GetCardinalDirection(incomingDir);

        if (moveDir == Vector2Int.zero) return;

        Vector2Int targetGridPos = PuzzleGridManager.Instance.GetSlideTargetPosition(_currentGridPos, moveDir);

        if (targetGridPos == _currentGridPos)
        {
            OnHitBlocked();
            return;
        }

        StartCoroutine(SlideToPosition(targetGridPos));
    }

    private bool IsChargedAttack(AttackType type)
    {
        return type == AttackType.Strong_1 || 
               type == AttackType.Strong_2 || 
               type == AttackType.Strong_Counter||
               type == AttackType.Strong_3;
    }

    private System.Collections.IEnumerator SlideToPosition(Vector2Int targetGridPos)
    {
        _state = TotemState.Sliding;
        
        PuzzleGridManager.Instance.UpdateTotemPosition(this, _currentGridPos, targetGridPos);
        
        Vector3 targetWorldPos = PuzzleGridManager.Instance.GridToWorld(targetGridPos);
        float distance = Vector3.Distance(transform.position, targetWorldPos);
        float duration = distance / _moveSpeed;

        yield return transform.DOMove(targetWorldPos, duration).SetEase(Ease.OutQuad).WaitForCompletion();

        _currentGridPos = targetGridPos;
        _state = TotemState.Idle;
        
        OnMoveComplete();
    }

    public virtual void ResetToStart()
    {
        StopAllCoroutines();
        transform.DOKill();

        _currentGridPos = _startGridPos;
        transform.position = PuzzleGridManager.Instance.GridToWorld(_startGridPos);
        _state = TotemState.Idle;
        
        transform.localScale = Vector3.one; 
    }

    protected virtual void OnMoveComplete() { }

    protected virtual void OnHitBlocked()
    {
        transform.DOShakePosition(0.3f, 0.2f);
    }

    private Vector2Int GetCardinalDirection(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
        {
            return dir.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        else
        {
            return dir.z > 0 ? Vector2Int.up : Vector2Int.down;
        }
    }
}
