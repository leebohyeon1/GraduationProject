using UnityEngine;
using System.Collections.Generic;

public class CutoutController : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("기준이 될 오브젝트 (예: 플레이어)")]
    public Transform targetObject;

    [Tooltip("오브젝트 머리 위로 얼마나 여유를 두고 자를지 설정 (잘리는 높이 결정)")]
    public float heightOffset = 2.0f;

    [Header("Transition Settings")]
    [Tooltip("변환에 걸리는 시간 (초 단위)")]
    [Range(0.1f, 5.0f)]
    public float transitionDuration = 0.5f;

    [Tooltip("변화의 움직임 곡선 (0: 불투명/보임, 1: 투명/사라짐)")]
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Layer Settings")]
    [Tooltip("벽으로 인식할 레이어")]
    public LayerMask wallLayer;

    [Header("Material Property")]
    [Tooltip("셰이더에서 잘리는 높이를 제어하는 프로퍼티 이름")]
    [SerializeField]
    private string cutHeightProperty = "_CutHeight";

    [Tooltip("셰이더에서 디더링/투명도를 제어하는 프로퍼티 이름 (예: _Cutoff, _AlphaClipThreshold)")]
    [SerializeField]
    private string ditherProperty = "_Cutoff";

    [Tooltip("체크하면 씬의 모든 벽에 적용 (전역 변수), 해제하면 가리는 벽만 개별 적용")]
    public bool applyGlobally = false;

    private Camera mainCamera;
    private int cutHeightID;
    private int ditherPropertyID;

    // Dither 값 범위 (0: 보임, 1: 사라짐)
    // 셰이더 설정에 따라 반대일 수 있으니 확인 필요
    private const float VALUE_VISIBLE = 0.0f;
    private const float VALUE_INVISIBLE = 1.0f;

    // 개별 적용 시 각 Renderer의 "진행도(Progress)"를 저장하는 딕셔너리
    private Dictionary<Renderer, float> _renderersProgress = new Dictionary<Renderer, float>();

    void Start()
    {
        mainCamera = Camera.main;
        cutHeightID = Shader.PropertyToID(cutHeightProperty);
        ditherPropertyID = Shader.PropertyToID(ditherProperty);

        // 초기화: 모든 벽을 보이게 설정 (Dither 0)
        Shader.SetGlobalFloat(ditherPropertyID, VALUE_VISIBLE);
        // 높이는 기본값으로
        Shader.SetGlobalFloat(cutHeightID, 1000.0f);
    }

    void Update()
    {
        if (targetObject == null) return;

        // 목표 높이 계산 (플레이어 위치 + 오프셋)
        float currentCutHeight = targetObject.position.y + heightOffset;

        HandleOcclusionCurve(currentCutHeight);
    }

    // 방식 2: 개별 벽 커브 적용
    void HandleOcclusionCurve(float height)
    {
        Vector3 dir = targetObject.position - mainCamera.transform.position;
        float dist = dir.magnitude;

        // 1. 이번 프레임에 레이캐스트에 걸린 벽들을 식별
        HashSet<Renderer> currentHits = new HashSet<Renderer>();
        RaycastHit[] hits = Physics.RaycastAll(mainCamera.transform.position, dir, dist, wallLayer);

        foreach (RaycastHit hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
            {
                currentHits.Add(rend);

                if (!_renderersProgress.ContainsKey(rend))
                {
                    _renderersProgress.Add(rend, 0.0f);
                }
            }
        }

        // 2. 관리 중인 모든 벽 업데이트
        List<Renderer> renderersToRemove = new List<Renderer>();
        List<Renderer> keys = new List<Renderer>(_renderersProgress.Keys);

        foreach (Renderer rend in keys)
        {
            if (rend == null)
            {
                renderersToRemove.Add(rend);
                continue;
            }

            // A. CutHeight 위치 갱신 (구멍이 뚫릴 위치는 항상 플레이어 위를 따라다님)
            rend.material.SetFloat(cutHeightID, height);

            // B. Dither 애니메이션 계산
            float currentProgress = _renderersProgress[rend];
            bool isHit = currentHits.Contains(rend);

            // 히트되면 진행도 증가 (투명해짐), 아니면 감소 (다시 보임)
            if (isHit)
            {
                currentProgress += Time.deltaTime / transitionDuration;
            }
            else
            {
                currentProgress -= Time.deltaTime / transitionDuration;
            }

            currentProgress = Mathf.Clamp01(currentProgress);
            _renderersProgress[rend] = currentProgress;

            // 커브 평가 및 Dither 값 적용
            float curveValue = transitionCurve.Evaluate(currentProgress);
            float finalDither = Mathf.Lerp(VALUE_VISIBLE, VALUE_INVISIBLE, curveValue);

            rend.material.SetFloat(ditherPropertyID, finalDither);

            // 3. 최적화: 완전히 보이고(0.0), 현재 히트되지 않고 있다면 목록에서 제거
            if (!isHit && currentProgress <= 0.0f)
            {
                // 확실하게 초기화
                rend.material.SetFloat(ditherPropertyID, VALUE_VISIBLE);
                renderersToRemove.Add(rend);
            }
        }

        // 목록 정리
        foreach (Renderer r in renderersToRemove)
        {
            if (_renderersProgress.ContainsKey(r))
            {
                _renderersProgress.Remove(r);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (targetObject != null && mainCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(mainCamera.transform.position, targetObject.position);

            Gizmos.color = Color.red;
            Vector3 center = targetObject.position;
            center.y += heightOffset;
            Gizmos.DrawWireCube(center, new Vector3(1, 0.05f, 1));
        }
    }
}