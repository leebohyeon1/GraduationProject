using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System;

public class ObjectTotem : TotemBase
{
    [Header("Object Settings")]
    [SerializeField] private int _maxDurability = 3;
    [SerializeField] private Vector2Int _targetGridPos;
    [SerializeField] private MeshRenderer _renderer;

    [Header("Gizmo")]
    [SerializeField] private bool _showTargetGizmo = true;
    [SerializeField] private Color _targetGizmoColor = Color.green;
    [SerializeField] private PuzzleGridManager _gizmoGridManager;

    private Color _originalColor;

    public bool IsAtTarget { get; private set; }
    public Vector2Int TargetGridPos => _targetGridPos;

    protected override void Start()
    {
        base.Start();
        _type = TotemType.Object;
        _currentDurability = _maxDurability;

        if (_renderer == null)
        {
            _renderer = GetComponentInChildren<MeshRenderer>();
        }

        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
        }
    }

    protected override void OnMoveComplete()
    {
        base.OnMoveComplete();

        _currentDurability--;
        Debug.Log($"[ObjectTotem] Moved! Durability: {_currentDurability}/{_maxDurability}");


        CheckTargetReached();
        if (_currentDurability <= 0 && !IsAtTarget )
        {
            DeactivateTotem();
        }
    }

    public override void ResetToStart()
    {
        base.ResetToStart();
        _currentDurability = _maxDurability;
        IsAtTarget = false;

        if (_renderer != null)
        {
            _renderer.material.color = _originalColor;
            _renderer.material.DOKill();
        }

        Debug.Log("[ObjectTotem] Reset Complete.");
    }

    private void CheckTargetReached()
    {
        bool wasAtTarget = IsAtTarget;
        IsAtTarget = _currentGridPos == _targetGridPos;

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
        _feedbackQueue.Enqueue(() => 
        {
            PuzzleGridManager.Instance.BreakAllTotems();
        });
        _feedbackQueue.Enqueue(() => 
        {
            PuzzleGridManager.Instance.ResetPuzzle();
        });
        transform.DOShakePosition(0.5f, 0.5f);
    }
    private void Update() {
        if (_feedbackQueue.Count > 0 && !feedback.IsPlaying)
        {
            Action feedbackAction = _feedbackQueue.Dequeue();
            feedbackAction.Invoke();
        }
    }
    Queue<Action> _feedbackQueue = new Queue<Action>();
    private void OnDrawGizmosSelected()
    {
        if (!_showTargetGizmo)
        {
            return;
        }

        PuzzleGridManager gridManager = _gizmoGridManager != null ? _gizmoGridManager : PuzzleGridManager.Instance;
        if (gridManager == null)
        {
            return;
        }

        Vector3 targetWorldPos = gridManager.GridToWorld(_targetGridPos);

        Gizmos.color = _targetGizmoColor;
        Gizmos.DrawWireCube(targetWorldPos + Vector3.up * 0.05f, new Vector3(1.7f, 0.1f, 1.7f));
        Gizmos.DrawSphere(targetWorldPos + Vector3.up * 0.2f, 0.15f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.2f, targetWorldPos + Vector3.up * 0.2f);
    }
}
