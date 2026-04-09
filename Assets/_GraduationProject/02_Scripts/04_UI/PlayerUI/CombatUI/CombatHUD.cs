using UnityEngine;
using static Pathfinding.SimpleSmoothModifier;

/// <summary>
/// 플레이어의 전투 HUD를 관리하는 클래스
/// </summary>
public class CombatHUD : PlayerUIBase
{
    [Header("Positioning Settings")]
    [SerializeField] private Vector3 _worldOffset = new Vector3(0, 2.0f, 0); // 플레이어 머리 위 높이 조절
    [SerializeField] private float _smoothTime = 0.1f; // 따라다니는 지연 시간 (낮을수록 빠름)

    private RectTransform _rectTransform;
    private Camera _mainCamera;
    private Vector3 _currentVelocity; // SmoothDamp용 참조 변수

    /// <summary>
    /// 객체 초기화
    /// </summary>
    /// <param name="player"></param>
    public override void Initialize(PlayerController player)
    {
        base.Initialize(player);

        _rectTransform = GetComponent<RectTransform>();
        _mainCamera = Camera.main; // 메인 카메라 캐싱 (최적화)

        p_player.Combat.BattleStateChaged += OnBattleStateChanged;

        // 시작 시 비활성화 및 초기 위치 설정
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 객체 해제
    /// </summary>
    public override void Dispose()
    {
        if (p_player != null)
        {
            p_player.Combat.BattleStateChaged -= OnBattleStateChanged;
        }
        base.Dispose();
    }

    /// <summary>
    /// 매 프레임 UI 위치 갱신 (카메라/플레이어 이동 후 처리를 위해 LateUpdate 사용)
    /// </summary>
    private void LateUpdate()
    {
        if (p_player == null || _mainCamera == null) return;

        UpdatePosition(false);
    }

    /// <summary>
    /// 실제 위치 계산 로직
    /// </summary>
    /// <param name="isInstant">true면 부드러운 이동 없이 즉시 이동</param>
    private void UpdatePosition(bool isInstant)
    {
        // 1. 플레이어의 월드 좌표에 오프셋(머리 위 등)을 더함
        Vector3 targetWorldPos = p_player.transform.position + _worldOffset;

        // 2. 월드 좌표를 스크린(UI) 좌표로 변환
        Vector3 targetScreenPos = _mainCamera.WorldToScreenPoint(targetWorldPos);

        // 3. 화면 밖으로 나갔을 때의 처리 (선택 사항: 필요 시 주석 해제)
        // if (targetScreenPos.z < 0) { ... } 

        if (isInstant)
        {
            _rectTransform.position = targetScreenPos;
            _currentVelocity = Vector3.zero; // 물리 속도 초기화
        }
        else
        {
            // 4. 현재 위치에서 목표 위치로 부드럽게 이동 (SmoothDamp)
            _rectTransform.position = Vector3.SmoothDamp(
                _rectTransform.position,
                targetScreenPos,
                ref _currentVelocity,
                _smoothTime
            );
        }
    }

    /// <summary>
    /// 전투 상태가 바뀌었을 때 발생하는 이벤트
    /// </summary>
    /// <param name="isBattle">전투 상태인가</param>
    private void OnBattleStateChanged(bool isBattle)
    {
        if (isBattle)
        {
            // 활성화 순서 중요:
            // 1. 오브젝트를 먼저 켬 (Update가 돌아가도록)
            gameObject.SetActive(true);

            // 2. 켜자마자 현재 플레이어 위치로 '즉시' 이동시킴 (날아오는 현상 방지)
            UpdatePosition(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
