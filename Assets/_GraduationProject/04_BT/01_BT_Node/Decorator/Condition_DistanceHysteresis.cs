using UnityEngine;
using BehaviorTree;

public class Condition_DistanceHysteresis : ConditionNode
{
    public enum CheckType
    {
        Inside,  // 범위 안에 있어야 성공 (거리가 가까워지면 True)
        Outside  // 범위 밖에 있어야 성공 (거리가 멀어지면 True)
    }

    [Header("Target Settings")]
    [Tooltip("거리를 잴 대상입니다. 비어있으면 자동으로 플레이어(runner.player)를 타겟으로 합니다.")]
    public GameObject target;

    [Header("Distance Settings")]
    [Tooltip("진입 판정 거리 (이 거리보다 가까워지면 진입으로 간주)")]
    public float minRange = 5.0f;

    [Tooltip("이탈 판정 거리 (이 거리보다 멀어지면 이탈로 간주)")]
    public float maxRange = 6.0f;

    [Tooltip("검사 타입 (Inside: 안쪽이면 성공, Outside: 바깥쪽이면 성공)")]
    public CheckType checkType = CheckType.Inside;

    // --- 내부 상태 변수 ---
    private bool _currentState = false;
    private bool _hasInitialized = false;

    // Clone 시 런타임 변수 초기화
    public override Node Clone()
    {
        Condition_DistanceHysteresis node = Instantiate(this);
        node.target = this.target; // 외부에서 주입된 타겟이 있다면 복사
        node.minRange = this.minRange;
        node.maxRange = this.maxRange;
        node.checkType = this.checkType;
        
        node._hasInitialized = false;
        node._currentState = false;
        return node;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        // 타겟이 할당되지 않았다면 플레이어로 초기화 시도
        if (target == null && runner != null)
        {
            target = runner.player.gameObject;
            // 주의: 여기서 할당하면 Clone된 인스턴스에만 적용되므로 안전함
            // 하지만 GameObject는 씬 객체이므로 SO에 저장되지 않게 주의 (Clone이므로 괜찮음)
            // 명시적으로 변수에 넣지 않고 CheckCondition에서 처리하는 것이 더 깔끔할 수 있음
        }
    }

    protected override bool CheckCondition()
    {
        // 1. 실제 타겟 결정 (우선순위: 직접 할당된 target > runner.player)
        Transform currentTargetTrans = null;

        if (target != null)
        {
            currentTargetTrans = target.transform;
        }
        else if (runner != null && runner.player != null)
        {
            currentTargetTrans = runner.player.transform;
        }

        // 타겟이 유효하지 않으면 (죽거나 사라짐) -> False 반환
        if (currentTargetTrans == null)
        {
            _currentState = false;
            _hasInitialized = false; // 타겟을 잃었으므로 초기화 상태로 리셋
            return false;
        }

        // 2. 거리 계산 (Y축 무시 - 평면 거리)
        float dist = Vector3.Distance(
            new Vector3(runner.transform.position.x, 0, runner.transform.position.z),
            new Vector3(currentTargetTrans.position.x, 0, currentTargetTrans.position.z)
        );

        // 3. 히스테리시스 로직
        if (!_hasInitialized)
        {
            // 상황 B: 첫 실행 시 엄격한 검사
            if (checkType == CheckType.Inside)
            {
                _currentState = dist <= minRange;
            }
            else // Outside
            {
                _currentState = dist >= maxRange;
            }
            _hasInitialized = true;
        }
        else
        {
            // 초기화 이후: 버퍼 구간(Hysteresis) 적용
            if (checkType == CheckType.Inside)
            {
                if (_currentState)
                {
                    // True 상태 유지: MaxRange 밖으로 나가기 전까지
                    if (dist > maxRange) _currentState = false;
                }
                else
                {
                    // False 상태 유지: MinRange 안으로 들어오기 전까지
                    if (dist <= minRange) _currentState = true;
                }
            }
            else // Outside
            {
                if (_currentState)
                {
                    // True 상태 유지: MinRange 안으로 들어오기 전까지
                    if (dist <= minRange) _currentState = false;
                }
                else
                {
                    // False 상태 유지: MaxRange 밖으로 나가기 전까지
                    if (dist > maxRange) _currentState = true;
                }
            }
        }

        return _currentState;
    }

    public override void initNode()
    {
        base.initNode();
        _hasInitialized = false;
        _currentState = false;
    }
}