using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;

public abstract class TotemBase : MonoBehaviour, IDamageable
{
    [Header("Base Settings")]
    [SerializeField] protected float _moveSpeed = 10f;
    [SerializeField] protected TotemType _type;
    [SerializeField] protected bool _isMovable = true; // 이동 가능 여부 (기본값 true)
    [SerializeField] protected TotemReceiveType _receiveType;
    
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
    
    [SerializeField]protected FeedbackPlayManager feedback;
    public string respawn {get; private set;} = "_respawnFeedbackName";
    public string blocked {get; private set;} = "_blockedFeedbackName";
    public string hit {get; private set;} = "_hitFeedbackName";
    public string arrival {get; private set;} = "_arrivalFeedbackName";
    public string broken {get; private set;} = "_brokenFeedbackName";

    protected int _currentDurability;


    private void Reset()
    {
        string[] names = new string[] 
        { 
            respawn,
            blocked,
            hit,
            arrival,
            broken
        };

        // 여기서 초기화하면 gameObject 접근이 가능합니다.
        feedback = new FeedbackPlayManager(names);
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
        gameObject.layer = LayerMask.NameToLayer("HitObject");
        feedback.start(this.gameObject);
    }
    public void DestroyTotem()
    {
        if (IsDead) return;

        _state = TotemState.Destroyed;
        
        //feedback.PlayFeedback(broken);
    }
    public void TakeDamage(DamageData damageData)
    {
        // 이동 불가능한 토템이면 반응 안 함 (흔들림도 X 혹은 흔들림 O?)
        // 보통 고정 벽은 때려도 꿈쩍 않는 게 자연스러움.
        if (!_isMovable) 
        {
            OnHitBlocked();
            return;
        }

        if (_state != TotemState.Idle && _state != TotemState.Destroyed) return;
        if (IsDead) return;

        if (!IsChargedAttack(damageData.AttackType))
        {
            OnHitBlocked();
            Debug.Log($"[TotemBase] Attack {damageData.AttackType} not effective. Blocked!");
            OnHitBlocked();
            return;
        }
        Debug.Log($"[TotemBase] Received {damageData.AttackType} attack. Processing damage and potential movement.");
        Debug.Log($"totem feedback : {feedback}");
        feedback.PlayFeedback(hit);
        Vector3 incomingDir = (transform.position - damageData.AttackerTransform.position).normalized;
        Vector3 localDir = PuzzleGridManager.Instance.transform.InverseTransformDirection(incomingDir);
        Vector2Int moveDir = GetCardinalDirection(localDir);

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
        if(_receiveType == TotemReceiveType.All) return true;
        if(_receiveType == TotemReceiveType.Strong)
        {
            if(type.ToString().StartsWith("Strong"))
                return true;
        }
        if(_receiveType == TotemReceiveType.Normal)
        {
            if (type.ToString().StartsWith("Normal"))
                return true;
            
        }
        return false;
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
        feedback.PlayFeedback(respawn);

        StopAllCoroutines();
        transform.DOKill();

        _currentGridPos = _startGridPos;
        transform.position = PuzzleGridManager.Instance.GridToWorld(_startGridPos);
        _state = TotemState.Idle;
        
    }

    protected virtual void OnMoveComplete()
    {
        feedback.PlayFeedback(arrival);
        
    }

    protected virtual void OnHitBlocked()
    {
        feedback.PlayFeedback(blocked);
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

