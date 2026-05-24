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
/// 플레이어와의 거리를 체크하여 지정된 거리(n) 이하일 때 멈추는 로직이 포함되어 있습니다.
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
    [Tooltip("플레이어와 이 거리 이하로 가까워지면 이동 중단 (n)")]
    public float stopDistance = 1.5f;

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

    }

    protected override void OnActionSOTriggered()
    {
        _currentStep++;

        if (phaseAttackDataKeys != null && _currentStep < phaseAttackDataKeys.Count)
        {
            string newKey = phaseAttackDataKeys[_currentStep];
            if (!string.IsNullOrEmpty(newKey) && brain.blackboard.GetValue<EnemyAttackData>(newKey, out var newData))
            {
                // [사용자 요청] 현재 Phase보다 작거나 같은 데이터인 경우에만 교체 작업을 수행합니다.
                int currentPhase = brain.blackboard.GetValueOrDefault<int>(EnemyBlackboardKeys.Phase, 0);
                
                if (newData.Phase <= currentPhase)
                {
                    _data = newData;
                    var d = _data.damageData;
                    d.AttackerTransform = runner.transform;
                    _data.damageData = d;
                    runner.SetCurrentAttackData(_data);
                    // Log($"Step {_currentStep}: 데이터 교체 완료 -> <color=orange>{_data.AttackName}</color> (Phase {newData.Phase} <= {currentPhase})");
                }
                else
                {
                    // Log($"Step {_currentStep}: 데이터 교체 건너뜀. 요구 Phase: {newData.Phase}, 현재: {currentPhase}");
                }
            }
        }

        // 2. 이동 방식(Mechanics) 통째로 교체
        if (stepMovements != null && _currentStep < stepMovements.Count)
        {
            _currentStepData = stepMovements[_currentStep];
            if (_currentStepData.distance > 0)
            {
                //커브 1초 이상일 때 첫 키와 마지막 키의 시간값을 강제로 0과 1로 매핑하여 보정
                if (_currentStepData.curve != null && _currentStepData.curve.keys.Length >= 2)
                {
                    Keyframe[] keys = _currentStepData.curve.keys;
                    
                    // 첫 키의 시간은 0, 마지막 키의 시간은 1로 강제 매핑 (값은 유지)
                    float firstVal = keys[0].value;
                    float lastVal = keys[keys.Length - 1].value;
                    
                    _currentStepData.curve.MoveKey(0, new Keyframe(0f, firstVal));
                    _currentStepData.curve.MoveKey(keys.Length - 1, new Keyframe(1f, lastVal));
                }
              
                _targetPosition = runner.transform.position + runner.transform.forward * _currentStepData.distance;
                _isMoving = true;
                _stepStartTime = Time.time;

                if (runner.aIPath != null)
                {
                    runner.aIPath.isStopped = true;
                    runner.aIPath.canMove = false;
                }
                // Log($"Step {_currentStep}: {_currentStepData.distance}m 이동 시작");
            }
        }
    }

    protected override void UpdateMovement()
    {
        if (!_isMoving || _currentStepData == null) return;

        Vector3 myPos = runner.transform.position;
        Vector3 playerPos = runner.player.transform.position;
        
        // [사용자 요청] 플레이어와 거리 n(stopDistance) 비교 후 정지
        float currentDist = Vector3.Distance(myPos, playerPos);
        if (ignoreYDistance)
        {
            currentDist = Vector2.Distance(new Vector2(myPos.x, myPos.z), new Vector2(playerPos.x, playerPos.z));
        }

        if (currentDist <= stopDistance)
        {
            _isMoving = false;
            // Log($"<color=orange>[StepAttack]</color> 거리 유지 브레이크 작동 (거리: {currentDist:F2}m <= {stopDistance}m)");
            return;
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
            // 벽 및 플레이어 충돌 체크
            int playerLayer = 1 << LayerMask.NameToLayer("Player");
            int combinedMask = obstacleMask | playerLayer;
            
            float castDistance = stepSize + 0.3f;
            Vector3 castOrigin = myPos + Vector3.up * 0.5f;
            float castRadius = (runner.Movement != null ? runner.Movement.CharacterRadius : 0.5f);

            if (Physics.SphereCast(castOrigin, castRadius, finalMoveDir, out RaycastHit hit, castDistance, combinedMask))
            {
                _isMoving = false;
                // Log($"<color=orange>[StepAttack]</color> 물리 충돌 예측 정지: {hit.collider.name}");
                return;
            }

            CharacterController cc = runner.GetComponent<CharacterController>();
            if (cc != null)
            {
                Vector3 moveVector = nextPos - myPos;
                moveVector.y = -1.5f; // 등반 방지 하향 압력
                cc.Move(moveVector);
            }
            else
            {
                runner.transform.position = nextPos;
            }

            if (runner.aIPath != null) runner.aIPath.Teleport(runner.transform.position);
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
        
        if (runner._animationBridge != null)
        {
            runner._animationBridge.ClearIsAttacking();
        }
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.stopDistance = stopDistance;
        node.obstacleMask = obstacleMask;
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
