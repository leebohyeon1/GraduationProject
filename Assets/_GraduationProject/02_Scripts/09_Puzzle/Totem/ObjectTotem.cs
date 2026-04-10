using UnityEngine;
using DG.Tweening;

public class ObjectTotem : TotemBase
{
    [Header("Object Settings")]
    [SerializeField] private int _maxDurability = 3;
    [SerializeField] private Vector2Int _targetGridPos;
    [SerializeField] private MeshRenderer _renderer;
    
    private int _currentDurability;
    public bool IsAtTarget { get; private set; }
    private Color _originalColor; // 원래 색상 저장용

    protected override void Start()
    {
        base.Start();
        _type = TotemType.Object;
        _currentDurability = _maxDurability;
        
        if (_renderer == null) _renderer = GetComponentInChildren<MeshRenderer>();
        if (_renderer != null) _originalColor = _renderer.material.color;
    }

    protected override void OnMoveComplete()
    {
        base.OnMoveComplete();
        
        _currentDurability--;
        // UI 연동 필요 (이벤트 발생 등)
        Debug.Log($"[ObjectTotem] Moved! Durability: {_currentDurability}/{_maxDurability}");

        if (_currentDurability <= 0)
        {
            DeactivateTotem();
            return;
        }

        CheckTargetReached();
    }

    public override void ResetToStart()
    {
        base.ResetToStart();
        
        // 상태 복구
        _currentDurability = _maxDurability;
        IsAtTarget = false;
        
        // 시각적 복구
        if (_renderer != null)
        {
            _renderer.material.color = _originalColor;
            _renderer.material.DOKill(); // 색상 트윈 중단
        }
        
        Debug.Log("[ObjectTotem] Reset Complete.");
    }

    private void CheckTargetReached()
    {
        bool wasAtTarget = IsAtTarget;
        IsAtTarget = (_currentGridPos == _targetGridPos);

        if (IsAtTarget && !wasAtTarget)
        {
            PuzzleGridManager.Instance.CheckWinCondition();
        }
    }

    private void DeactivateTotem()
    {
        _state = TotemState.Destroyed;
        Debug.Log("[ObjectTotem] Broken! (Remains on field)");
        
        if (_renderer != null)
        {
            _renderer.material.DOColor(Color.gray, 0.5f);
        }
        transform.DOShakePosition(0.5f, 0.5f);
    }
}
