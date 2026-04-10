using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotemDissolveTarget : MonoBehaviour
{
    private static readonly Dictionary<string, List<TotemDissolveTarget>> TargetsById = new Dictionary<string, List<TotemDissolveTarget>>();

    [Header("Link")]
    [SerializeField] private string _targetId;

    [Header("Dissolve")]
    [SerializeField] private float _duration = 1.2f;
    [SerializeField] private float _collisionDisableThreshold = 0.5f;
    [SerializeField] private string _dissolvePropertyName = "_DissolveValue";
    [SerializeField] private bool _disableObjectOnComplete = true;

    [Header("References")]
    [SerializeField] private Renderer[] _targetRenderers;
    [SerializeField] private Collider[] _targetColliders;
    [SerializeField] private TotemGimmickFeedbackPlayer _feedbackPlayer;

    private readonly MaterialPropertyBlock _propertyBlock = new MaterialPropertyBlock();
    private TotemGimmickState _state = TotemGimmickState.Alive;
    private bool _isThresholdReached;
    private int _dissolvePropertyId;

    public string TargetId => _targetId;
    public bool IsDissolving => _state == TotemGimmickState.Processing;
    public bool IsCompleted => _state == TotemGimmickState.Destroyed;

    private void Awake()
    {
        _dissolvePropertyId = Shader.PropertyToID(_dissolvePropertyName);

        if (_targetRenderers == null || _targetRenderers.Length == 0)
        {
            _targetRenderers = GetComponentsInChildren<Renderer>(true);
        }

        if (_targetColliders == null || _targetColliders.Length == 0)
        {
            _targetColliders = GetComponentsInChildren<Collider>(true);
        }
    }

    private void OnEnable()
    {
        Register(this);
    }

    private void OnDisable()
    {
        Unregister(this);
    }

    public void BeginDissolve()
    {
        if (_state != TotemGimmickState.Alive)
        {
            return;
        }

        Debug.Log($"[TotemDissolveTarget] Begin dissolve. targetId={_targetId}, object={name}");
        StartCoroutine(DissolveRoutine());
    }

    public static int CollectTargets(string targetId, List<TotemDissolveTarget> resultBuffer)
    {
        resultBuffer.Clear();
        if (string.IsNullOrWhiteSpace(targetId))
        {
            return 0;
        }

        if (!TargetsById.TryGetValue(targetId, out List<TotemDissolveTarget> list))
        {
            return 0;
        }

        for (int i = 0; i < list.Count; i++)
        {
            TotemDissolveTarget target = list[i];
            if (target == null || !target.isActiveAndEnabled)
            {
                continue;
            }

            resultBuffer.Add(target);
        }

        return resultBuffer.Count;
    }

    private IEnumerator DissolveRoutine()
    {
        _state = TotemGimmickState.Processing;
        _feedbackPlayer?.PlayFeedback(TotemGimmickFeedbackType.DissolveStart, transform.position);
        _feedbackPlayer?.PlayFeedback(TotemGimmickFeedbackType.DissolveLoop, transform.position);

        float duration = Mathf.Max(0.01f, _duration);
        float elapsed = 0f;
        _isThresholdReached = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float dissolveValue = Mathf.Clamp01(elapsed / duration);

            SetDissolveValue(dissolveValue);

            if (!_isThresholdReached && dissolveValue >= _collisionDisableThreshold)
            {
                _isThresholdReached = true;
                DisableColliders();
                Debug.Log($"[TotemDissolveTarget] Threshold reached. value={dissolveValue:F2}, targetId={_targetId}, object={name}");
                _feedbackPlayer?.PlayFeedback(TotemGimmickFeedbackType.DissolveThreshold, transform.position);
            }

            yield return null;
        }

        SetDissolveValue(1f);
        DisableColliders();
        Debug.Log($"[TotemDissolveTarget] Dissolve complete. targetId={_targetId}, object={name}");
        _feedbackPlayer?.PlayFeedback(TotemGimmickFeedbackType.DissolveComplete, transform.position);

        _state = TotemGimmickState.Destroyed;

        if (_disableObjectOnComplete)
        {
            gameObject.SetActive(false);
        }
    }

    private void SetDissolveValue(float value)
    {
        for (int i = 0; i < _targetRenderers.Length; i++)
        {
            Renderer targetRenderer = _targetRenderers[i];
            if (targetRenderer == null)
            {
                continue;
            }

            targetRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(_dissolvePropertyId, value);
            targetRenderer.SetPropertyBlock(_propertyBlock);
        }
    }

    private void DisableColliders()
    {
        for (int i = 0; i < _targetColliders.Length; i++)
        {
            if (_targetColliders[i] == null)
            {
                continue;
            }

            _targetColliders[i].enabled = false;
        }
    }

    private static void Register(TotemDissolveTarget target)
    {
        if (target == null || string.IsNullOrWhiteSpace(target.TargetId))
        {
            return;
        }

        if (!TargetsById.TryGetValue(target.TargetId, out List<TotemDissolveTarget> list))
        {
            list = new List<TotemDissolveTarget>();
            TargetsById[target.TargetId] = list;
        }

        if (!list.Contains(target))
        {
            list.Add(target);
        }
    }

    private static void Unregister(TotemDissolveTarget target)
    {
        if (target == null || string.IsNullOrWhiteSpace(target.TargetId))
        {
            return;
        }

        if (!TargetsById.TryGetValue(target.TargetId, out List<TotemDissolveTarget> list))
        {
            return;
        }

        list.Remove(target);
        if (list.Count == 0)
        {
            TargetsById.Remove(target.TargetId);
        }
    }
}
