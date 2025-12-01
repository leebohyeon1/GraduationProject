using UnityEngine;
using DG.Tweening;

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

    private Renderer _renderer;
    private Material[] _materials; // 런타임에 수정할 메테리얼 인스턴스

    private int _cutHeightID;
    private int _ditherPropertyID;
    private int _baseGlancingID;

    private bool _isOccluded = false;

    // 현재 Dither 값을 추적하기 위한 변수
    private float _currentDitherValue = 0.0f;
    private Tween _fadeTween;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();

        if (_renderer != null)
        {
            // materials 프로퍼티에 접근하여 인스턴스를 생성하고 캐싱 (GC 방지)
            _materials = _renderer.materials;
        }

        _cutHeightID = Shader.PropertyToID(_cutHeightProperty);
        _baseGlancingID = Shader.PropertyToID(_baseGlancingAngleCut);
        _ditherPropertyID = Shader.PropertyToID(_ditherProperty);

        // 초기화
        UpdateMaterialsDither(0f);
    }

    public void SetOcclusionStatus(bool isOccluded)
    {
        if (_targetObject == null) return;
        if (_isOccluded == isOccluded) return;

        _isOccluded = isOccluded;

        _fadeTween?.Kill();

        // 1. 높이값 설정 (프로퍼티 체크 추가)
        if (_materials != null)
        {
            float height = _targetObject.position.y + _heightOffset;
            foreach (var mat in _materials)
            {
                // _CutHeight 프로퍼티가 있는 경우에만 값 설정
                if (mat.HasProperty(_cutHeightID))
                {
                    mat.SetFloat(_cutHeightID, height);
                }
            }
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
        if (_materials == null) return;

        for (int i = 0; i < _materials.Length; i++)
        {
            Material mat = _materials[i];

            // _Dither 프로퍼티가 있는지 체크
            if (mat.HasProperty(_ditherPropertyID))
            {
                mat.SetFloat(_ditherPropertyID, value);
            }

            // _BaseGlancingAngleCut 프로퍼티가 있는지 체크
            if (mat.HasProperty(_baseGlancingID))
            {
                mat.SetFloat(_baseGlancingID, 1 - value);
            }
        }
    }

    public void SetTarget(Transform target)
    {
        _targetObject = target;
    }
}