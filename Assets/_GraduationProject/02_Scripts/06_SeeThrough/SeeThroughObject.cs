using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// 투명화되는 대상
/// </summary>
public class SeeThroughObject : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform _targetObject;
    [SerializeField] private float _heightOffset = 2.0f;

    [Header("Transition Settings")]
    [Range(0.1f, 5.0f)]
    [SerializeField] private float _transitionDuration = 0.8f;
    [SerializeField] private AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Material Property")]
    [SerializeField] private string _cutHeightProperty = "_CutHeight";
    [SerializeField] private string _baseGlancingAngleCut = "_BaseGlancingAngleCut";
    [SerializeField] private string _ditherProperty = "_Dither";

    private readonly List<MaterialBinding> _materialBindings = new List<MaterialBinding>();
    private MaterialPropertyBlock _propertyBlock;

    private int _cutHeightID;
    private int _ditherPropertyID;
    private int _baseGlancingID;

    private bool _isOccluded = false;

    // 현재 Dither 값을 추적하기 위한 변수
    private float _currentDitherValue = 0.0f;
    private Tween _fadeTween;

    private sealed class MaterialBinding
    {
        public Renderer Renderer;
        public int MaterialIndex;
        public bool HasCutHeight;
        public bool HasDither;
        public bool HasBaseGlancing;
    }

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
        _cutHeightID = Shader.PropertyToID(_cutHeightProperty);
        _baseGlancingID = Shader.PropertyToID(_baseGlancingAngleCut);
        _ditherPropertyID = Shader.PropertyToID(_ditherProperty);

        CacheMaterialBindings();

        // 초기화
        UpdateMaterialsDither(0f);
    }

    private void OnDestroy()
    {
        _fadeTween?.Kill();
    }

    public void SetOcclusionStatus(bool isOccluded)
    {
        if (_targetObject == null) return;
        if (_isOccluded == isOccluded) return;

        _isOccluded = isOccluded;

        _fadeTween?.Kill();

        // 1. 높이값 설정 (프로퍼티 체크 추가)
        float height = _targetObject.position.y + _heightOffset;
        for (int i = 0; i < _materialBindings.Count; i++)
        {
            MaterialBinding binding = _materialBindings[i];
            if (!binding.HasCutHeight || binding.Renderer == null) continue;

            binding.Renderer.GetPropertyBlock(_propertyBlock, binding.MaterialIndex);
            _propertyBlock.SetFloat(_cutHeightID, height);
            binding.Renderer.SetPropertyBlock(_propertyBlock, binding.MaterialIndex);
        }

        float targetDither = isOccluded ? SeeThroughController.VALUE_INVISIBLE : SeeThroughController.VALUE_VISIBLE;

        // 2. 트윈 실행
        _fadeTween = DOTween.To(
                 () => _currentDitherValue,
                 x =>
                 {
                     _currentDitherValue = x;
                     UpdateMaterialsDither(x);
                 },
                 targetDither,
                 _transitionDuration
             )
             .SetEase(_transitionCurve);
    }

    private void UpdateMaterialsDither(float value)
    {
        for (int i = 0; i < _materialBindings.Count; i++)
        {
            MaterialBinding binding = _materialBindings[i];
            if (binding.Renderer == null || (!binding.HasDither && !binding.HasBaseGlancing)) continue;

            binding.Renderer.GetPropertyBlock(_propertyBlock, binding.MaterialIndex);

            if (binding.HasDither)
            {
                _propertyBlock.SetFloat(_ditherPropertyID, value);
            }

            if (binding.HasBaseGlancing)
            {
                _propertyBlock.SetFloat(_baseGlancingID, 1 - value);
            }

            binding.Renderer.SetPropertyBlock(_propertyBlock, binding.MaterialIndex);
        }
    }

    private void CacheMaterialBindings()
    {
        _materialBindings.Clear();

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer == null || IsVfxRenderer(renderer)) continue;

            Material[] sharedMaterials = renderer.sharedMaterials;
            for (int materialIndex = 0; materialIndex < sharedMaterials.Length; materialIndex++)
            {
                Material material = sharedMaterials[materialIndex];
                if (material == null) continue;

                bool hasCutHeight = material.HasProperty(_cutHeightID);
                bool hasDither = material.HasProperty(_ditherPropertyID);
                bool hasBaseGlancing = material.HasProperty(_baseGlancingID);
                if (!hasCutHeight && !hasDither && !hasBaseGlancing) continue;

                _materialBindings.Add(new MaterialBinding
                {
                    Renderer = renderer,
                    MaterialIndex = materialIndex,
                    HasCutHeight = hasCutHeight,
                    HasDither = hasDither,
                    HasBaseGlancing = hasBaseGlancing
                });
            }
        }
    }

    private static bool IsVfxRenderer(Renderer renderer)
    {
        return renderer.GetType().FullName == "UnityEngine.VFX.VFXRenderer";
    }

    public void SetTarget(Transform target)
    {
        _targetObject = target;
    }
}
