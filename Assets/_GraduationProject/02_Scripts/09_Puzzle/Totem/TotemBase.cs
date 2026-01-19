using UnityEngine;
using DG.Tweening;
using System;

public abstract class TotemBase : MonoBehaviour, IDamageable
{
    [Header("Base Settings")]
    [SerializeField] protected float _moveSpeed = 10f;
    [SerializeField] protected TotemType _type;
    
    protected Vector2Int _startGridPos;
    protected Vector2Int _currentGridPos;
    protected TotemState _state = TotemState.Idle;
    
    // IDamageable Events
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    public bool IsDead => _state == TotemState.Destroyed;
    
    // IDamageable Properties
    public int Health => 1;
    public int MaxHealth => 1;
    public bool IsInvincible => false;

    protected virtual void Awake()
    {
        // 초기화 시점 로직 필요 시 작성
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
        if (_state != TotemState.Idle && _state != TotemState.Destroyed) return;
        if (IsDead) return;

        Vector3 incomingDir = (transform.position - damageData.AttackerTransform.position).normalized;
        
        // 4방향(Cardinal) 판정으로 복구
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

    // 4방향 벡터 변환 (X, Z 중 더 큰 축을 선택)
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
