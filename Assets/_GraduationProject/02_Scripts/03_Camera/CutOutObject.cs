using UnityEngine;
using DG.Tweening;

public class CutOutObject : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform _targetObject;
    [SerializeField] private float _heightOffset = 2.0f;

    [Header("Transition Settings")]
    [Range(0.1f, 5.0f)]
    [SerializeField] private float _transitionDuration = 0.5f;
    [SerializeField] private AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Material Property")]
    [SerializeField] private string _cutHeightProperty = "_CutHeight";

    [SerializeField] private string _ditherProperty = "_Dither";

    private Renderer _renderer;
    private int _cutHeightID;
    private int _ditherPropertyID;

    // Dither 값 (0: 보임, 1: 사라짐)
    private const float VALUE_VISIBLE = 0.0f;
    private const float VALUE_INVISIBLE = 1.0f;
    
    private Tween _fadeTween;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        // 각 인스턴스가 고유한 머티리얼을 갖도록 보장
        _renderer.material = new Material(_renderer.material);

        _cutHeightID = Shader.PropertyToID(_cutHeightProperty);
        _ditherPropertyID = Shader.PropertyToID(_ditherProperty);
    }

    public void SetOcclusionStatus(bool isOccluded)
    {
        if (_targetObject == null) return;
        
        _fadeTween?.Kill();

        float height = _targetObject.position.y + _heightOffset;
        _renderer.material.SetFloat(_cutHeightID, height);

        float targetDither = isOccluded ? VALUE_INVISIBLE : VALUE_VISIBLE;

        _fadeTween = _renderer.material.DOFloat(targetDither, _ditherPropertyID, _transitionDuration)
            .SetEase(_transitionCurve);
    }

    public void SetTarget(Transform target)
    {
        _targetObject = target; 
    }
}
