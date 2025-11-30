using UnityEngine;
using DG.Tweening;

public class CutOutObject : MonoBehaviour
{
    [SerializeField] private Material[] _transparentMaterials;

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
    private Material[] _originalMaterials;
    private Material[] _activeInstanceMaterials;

    private int _cutHeightID;
    private int _ditherPropertyID;

    // Dither 값 (0: 보임, 1: 사라짐)
    private const float VALUE_VISIBLE = 0.0f;
    private const float VALUE_INVISIBLE = 1.0f;

    private bool _isOccluded = false;

    // 현재 Dither 값을 추적하기 위한 변수 (Tween용)
    private float _currentDitherValue = 0.0f;
    private Tween _fadeTween;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();

        // 각 인스턴스가 고유한 머티리얼을 갖도록 보장
        _originalMaterials = _renderer.sharedMaterials;

        _cutHeightID = Shader.PropertyToID(_cutHeightProperty);
        _ditherPropertyID = Shader.PropertyToID(_ditherProperty);
    }

    public void SetOcclusionStatus(bool isOccluded)
    {
        if (_targetObject == null)
        {
            return;
        }

        if (_isOccluded == isOccluded)
        {
            return;
        }

        // 상태 업데이트
        _isOccluded = isOccluded;

        // 트윈 중복 실행 방지
        _fadeTween?.Kill();

        // 가려진 상태(True)라면, 먼저 투명 머티리얼로 교체해야 합니다.
        if (isOccluded)
        {
            if (NeedToUpdateMaterials(_transparentMaterials))
            {
                Debug.Log("메테리얼 교체");
                _renderer.materials = _transparentMaterials;

            }
        }

        _activeInstanceMaterials = _renderer.materials;

        // 교체 직후 초기 Dither 값 세팅 (갑자기 튀는 현상 방지)
        UpdateMaterialsDither(_currentDitherValue);

        if (_activeInstanceMaterials != null)
        {
            float height = _targetObject.position.y + _heightOffset;
            foreach (var mat in _activeInstanceMaterials)
            {
                mat.SetFloat(_cutHeightID, height);
            }
        }

        float targetDither = isOccluded ? CutOutController.VALUE_INVISIBLE : CutOutController.VALUE_VISIBLE;

        _fadeTween = DOTween.To(
                 () => _currentDitherValue,           // getter
                 x =>                                 // setter
                 {
                     _currentDitherValue = x;
                     UpdateMaterialsDither(x);        // 값이 변할 때마다 모든 머티리얼 업데이트
                 },
                 targetDither,                        // 목표값
                 _transitionDuration                  // 시간
             )
             .SetEase(_transitionCurve)
             .OnComplete(() =>
             {
                 // 다시 보이는 상태(False)이고, Dither가 0(완전 보임)이 되었다면 원상복구
                 if (!isOccluded)
                 {
                     // 원래 머티리얼로 복구
                     if (NeedToUpdateMaterials(_originalMaterials))
                     {
                         _renderer.materials = _originalMaterials;
                     }
                     // 인스턴스 참조 해제 (메모리 관리)
                     _activeInstanceMaterials = null;
                 }
             });
    }

    private void UpdateMaterialsDither(float value)
    {
        if (_activeInstanceMaterials == null)
        {
            return;
        }

        for (int i = 0; i < _activeInstanceMaterials.Length; i++)
        {
            _activeInstanceMaterials[i].SetFloat(_ditherPropertyID, value);
        }
    }

    private bool NeedToUpdateMaterials(Material[] targetMaterials)
    {
        if (_renderer == null || targetMaterials == null)
        {
            return false;
        }

        // 1. 개수가 다르면 무조건 교체
        if (_renderer.sharedMaterials.Length != targetMaterials.Length)
        {
            return true;
        }

        // 2. 내용물이 하나라도 다르면 교체
        // sharedMaterials를 사용하여 인스턴스화 없이 원본 에셋을 비교
        for (int i = 0; i < targetMaterials.Length; i++)
        {
            if (_renderer.sharedMaterials[i] != targetMaterials[i])
            {
                return true;
            }
        }

        return false;
    }

    public void SetTarget(Transform target)
    {
        _targetObject = target; 
    }
}
