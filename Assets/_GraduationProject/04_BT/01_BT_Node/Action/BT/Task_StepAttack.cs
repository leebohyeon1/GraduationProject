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
/// 플레이어 머리 위로 올라타는 현상을 방지하기 위한 강력한 물리 브레이크 및 수직 벡터 억제 로직이 포함되어 있습니다.
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
    [Tooltip("플레이어와 이 거리 이하로 가까워지면 이동 중단 (Climbing 방지)")]
    public float hitRadius = 1.2f;

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
        
        if (runner.aIPath != null)
        {
            runner.aIPath.enableRotation = false;
        }

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

                if (runner.aIPath != null)
                {
                    runner.aIPath.isStopped = true;
                    runner.aIPath.canMove = false;
                }
                Log($"Step {_currentStep}: {_currentStepData.distance}m 이동 시작");
            }
        }
    }

    protected override void UpdateMovement()
    {
        if (!_isMoving || _currentStepData == null) return;

        Vector3 myPos = runner.transform.position;
        Vector3 playerPos = runner.player.transform.position;
        
        // [강화] 플레이어 등반 방지 브레이크 (수평 거리 기반)
        float horizontalDist = Vector2.Distance(new Vector2(myPos.x, myPos.z), new Vector2(playerPos.x, playerPos.z));
        
        // 몬스터 반지름 + 플레이어 반지름 + 안전 여유분
        float combinedRadius = (runner.Movement != null ? runner.Movement.CharacterRadius : 0.5f) + 0.5f;
        float stopDistance = Mathf.Max(hitRadius, combinedRadius + 0.15f);

        if (horizontalDist <= stopDistance)
        {
            // 전진 방향이 플레이어를 향하고 있는지 확인
            Vector3 toPlayer = (playerPos - myPos).normalized;
            Vector3 moveDir = (_targetPosition - myPos).normalized;
            
            if (Vector3.Dot(moveDir, toPlayer) > 0.5f) 
            {
                _isMoving = false;
                Log($"<color=orange>[StepAttack]</color> 등반 방지 브레이크: 플레이어 근접 정지 (거리: {horizontalDist:F2}m)");
                return;
            }
        }

        float elapsedTime = Time.time - _stepStartTime;
        float normalizedTime = Mathf.Clamp01(elapsedTime / _currentStepData.duration);

        if (normalizedTime >= 1.0f)
        {
            _isMoving = false;
            return;
        }

        // 속도 계산
        _calculatedBaseSpeed = _currentStepData.speed > 0 ? _currentStepData.speed : (_currentStepData.distance / _currentStepData.duration);
        float currentSpeed = _calculatedBaseSpeed * _currentStepData.curve.Evaluate(normalizedTime);
        float stepSize = currentSpeed * Time.deltaTime;

        Vector3 nextPos = Vector3.MoveTowards(myPos, _targetPosition, stepSize);
        Vector3 finalMoveDir = (nextPos - myPos).normalized;

        if (finalMoveDir != Vector3.zero)
        {
            // 벽 충돌 체크
            if (!Physics.Raycast(myPos + Vector3.up * 0.5f, finalMoveDir, 0.5f, obstacleMask))
            {
                CharacterController cc = runner.GetComponent<CharacterController>();
                if (cc != null)
                {
                    Vector3 moveVector = nextPos - myPos;
                    // [핵심 해결] 수평 이동만 추출하고 강제적인 하향 벡터(중력)를 적용하여 플레이어를 밟고 올라가는 것을 물리적으로 차단합니다.
                    moveVector.y = -0.5f; 
                    cc.Move(moveVector);
                }
                else
                {
                    runner.transform.position = nextPos;
                }

                if (runner.aIPath != null) runner.aIPath.Teleport(runner.transform.position);
            }
            else
            {
                _isMoving = false;
                Log($"Step {_currentStep}: 장애물 충돌 정지");
            }
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
        if (runner.aIPath != null)
        {
            runner.aIPath.enableRotation = true;
        }
        
        // 스턴 애니메이션이 씹히지 않도록 공격 상태를 확실히 종료
        if (runner._animationBridge != null)
        {
            runner._animationBridge.ClearIsAttacking();
        }
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
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
