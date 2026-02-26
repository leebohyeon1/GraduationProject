using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using Pathfinding;

/// <summary>
/// 각 단계(Step)별 이동 정보를 담는 클래스입니다.
/// </summary>
[System.Serializable]
public class StepMovementData
{
    public float distance;
    public float duration = 0.5f;
    public float speed = 0f; // 0이면 자동 계산
    public AnimationCurve curve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
}

/// <summary>
/// ActionSO 트리거 시마다 순차적으로 '이동 방식'을 통째로 변경하며 작동하는 다단계 공격 노드입니다.
/// </summary>
[CreateAssetMenu(fileName = "Task_StepAttack", menuName = "BehaviorTree/Action/Task_StepAttack")]
public class Task_StepAttack : BaseAttackNode
{
    [Header("Step Management")]
    [Tooltip("각 ActionSO 트리거 시 사용할 이동 데이터 리스트")]
    public List<StepMovementData> stepMovements = new List<StepMovementData>();

    [Header("Phase Attack Data Override")]
    public List<string> phaseAttackDataKeys = new List<string>();

    [Header("Global Constraints")]
    public LayerMask obstacleMask;
    public float hitRadius = 1.0f;

    private int _currentStep = -1;
    private Vector3 _targetPosition;
    private bool _isMoving;
    private float _stepStartTime;
    private float _calculatedBaseSpeed;
    private StepMovementData _currentStepData;

    protected override void InitialMovementSetup()
    {
        _currentStep = -1;
        _isMoving = false;
        if (runner.aIPath != null) runner.aIPath.enableRotation = false;
        Log("<color=cyan>[StepAttack]</color> 초기화 완료.");
    }

    protected override void OnActionSOTriggered()
    {
        _currentStep++;
        Log($"<color=cyan>[StepAttack]</color> ActionSO 트리거 - Step {_currentStep}");

        // 1. 공격 데이터 교체 로직
        if (phaseAttackDataKeys != null && _currentStep < phaseAttackDataKeys.Count)
        {
            string newKey = phaseAttackDataKeys[_currentStep];
            if (!string.IsNullOrEmpty(newKey) && brain.blackboard.GetValue<EnemyAttackData>(newKey, out var newData))
            {
                _data = newData;
                var d = _data.damageData;
                d.AttackerTransform = runner.transform;
                _data.damageData = d;
                runner.SetCurrentAttackData(_data);
            }
        }

        // 2. 이동 방식(Mechanics) 통째로 교체
        if (stepMovements != null && _currentStep < stepMovements.Count)
        {
            _currentStepData = stepMovements[_currentStep];
            if (_currentStepData.distance > 0)
            {
                _targetPosition = runner.transform.position + runner.transform.forward * _currentStepData.distance;
                _isMoving = true;
                _stepStartTime = Time.time;

                // 속도 산출 (수동 속도가 0이면 거리/시간으로 계산)
                _calculatedBaseSpeed = _currentStepData.speed > 0 ? _currentStepData.speed : (_currentStepData.distance / _currentStepData.duration);

                if (runner.aIPath != null)
                {
                    runner.aIPath.isStopped = true;
                    runner.aIPath.canMove = false;
                }
                Log($"Step {_currentStep}: {_currentStepData.distance}m 이동 시작 (속도: {_calculatedBaseSpeed:F2}m/s)");
            }
        }
    }

    protected override void UpdateMovement()
    {
        if (!_isMoving || _currentStepData == null) return;

        float elapsedTime = Time.time - _stepStartTime;
        float normalizedTime = Mathf.Clamp01(elapsedTime / _currentStepData.duration);

        if (normalizedTime >= 1.0f)
        {
            _isMoving = false;
            return;
        }

        // 현재 단계의 커브와 속도 적용
        float currentSpeed = _calculatedBaseSpeed * _currentStepData.curve.Evaluate(normalizedTime);
        float stepSize = currentSpeed * Time.deltaTime;

        Vector3 currentPos = runner.transform.position;
        Vector3 nextPos = Vector3.MoveTowards(currentPos, _targetPosition, stepSize);
        Vector3 moveDir = (nextPos - currentPos).normalized;

        if (moveDir != Vector3.zero)
        {
            if (!Physics.Raycast(currentPos + Vector3.up * 0.5f, moveDir, 0.5f, obstacleMask))
            {
                CharacterController cc = runner.GetComponent<CharacterController>();
                if (cc != null) cc.Move(nextPos - currentPos);
                else runner.transform.position = nextPos;

                if (runner.aIPath != null) runner.aIPath.Teleport(runner.transform.position);
            }
            else
            {
                _isMoving = false;
                Log($"Step {_currentStep}: 장애물 충돌 정지");
                return;
            }
        }

        if (Vector3.Distance(runner.transform.position, runner.player.transform.position) <= hitRadius)
        {
            _isMoving = false;
        }

        if (Vector3.Distance(runner.transform.position, _targetPosition) < 0.1f)
        {
            _isMoving = false;
        }
    }

    protected override bool IsMovementFinished => !_isMoving;

    protected override void SpecificCleanup()
    {
        _isMoving = false;
        _currentStep = -1;
        if (runner.aIPath != null) runner.aIPath.enableRotation = true;
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        // 리스트 데이터 깊은 복사
        node.stepMovements = new List<StepMovementData>();
        foreach(var m in stepMovements) 
        {
            node.stepMovements.Add(new StepMovementData {
                distance = m.distance,
                duration = m.duration,
                speed = m.speed,
                curve = new AnimationCurve(m.curve.keys)
            });
        }
        node.phaseAttackDataKeys = new List<string>(phaseAttackDataKeys);
        return node;
    }
}
