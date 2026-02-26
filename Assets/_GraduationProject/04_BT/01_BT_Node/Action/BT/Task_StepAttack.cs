using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using Pathfinding;

/// <summary>
/// ActionSO 트리거 시마다 순차적으로 이동하고, 조건부로 마법 이펙트를 생성하는 다단계 공격 노드입니다.
/// 단계별로 서로 다른 공격 데이터를 적용하여 가변 사거리에 대응합니다.
/// </summary>
[CreateAssetMenu(fileName = "Task_StepAttack", menuName = "BehaviorTree/Action/Task_StepAttack")]
public class Task_StepAttack : BaseAttackNode
{
    [Header("Step Movement Settings")]
    [Tooltip("ActionSO가 트리거될 때마다 이동할 거리 리스트")]
    public List<float> stepDistances = new List<float>();

    [Header("Phase Attack Data Override")]
    [Tooltip("각 단계(ActionSO 트리거 순서)마다 교체할 공격 데이터의 블랙보드 키 리스트")]
    public List<string> phaseAttackDataKeys = new List<string>();

    [Header("Magic Effect Settings")]
    [Tooltip("마법 이펙트 발동 여부를 확인할 블랙보드 불리언 키 (string)")]
    public string magicCheckKey = "IsMagicReady";
    [Tooltip("발동 시 생성할 이펙트 프리팹")]
    public GameObject magicEffectPrefab;
    [Tooltip("이펙트 생성 위치 오프셋 (로컬)")]
    public Vector3 magicEffectOffset;

    private int _currentStep = -1;
    private Vector3 _targetPosition;
    private bool _isMoving;

    protected override void InitialMovementSetup()
    {
        _currentStep = -1;
        _isMoving = false;
        Log("<color=cyan>[StepAttack]</color> 초기화 완료. 준비 단계 시작.");
    }

    protected override void OnActionSOTriggered()
    {
        _currentStep++;
        Log($"<color=cyan>[StepAttack]</color> ActionSO 트리거 감지 - <color=yellow>현재 단계: {_currentStep}</color>");

        // 1. 다단계 공격 데이터 교체 (가변 사거리 대응)
        if (phaseAttackDataKeys != null && _currentStep < phaseAttackDataKeys.Count)
        {
            string newKey = phaseAttackDataKeys[_currentStep];
            if (!string.IsNullOrEmpty(newKey))
            {
if (brain.blackboard.GetValue<EnemyAttackData>(newKey, out var newData))
{
                    _data = newData; // 부모 클래스의 히트 판정 데이터 교체
                    
                    // [핵심 수정] 교체된 데이터의 공격자 정보를 반드시 갱신해야 합니다.
                    // 누락 시 PlayerCombat에서 NullReferenceException이 발생하여 FAILURE 원인이 됩니다.
                    _data.damageData.AttackerTransform = runner.transform;
runner.SetCurrentAttackData(_data); // Gizmo 및 시스템 동기화
Log($"<color=cyan>[StepAttack]</color> Step {_currentStep}: 공격 데이터 교체 -> <color=orange>{_data.AttackName}</color> (사거리: {_data.damageRadius})");
}
                else
                {
                    Log($"<color=red>[StepAttack Error]</color> 블랙보드에서 키 '{newKey}'에 해당하는 데이터를 찾을 수 없습니다.", true);
                }
            }
        }

        // 2. 다단계 이동 처리
        if (stepDistances != null && _currentStep < stepDistances.Count)
        {
            float dist = stepDistances[_currentStep];
            if (dist > 0)
            {
                _targetPosition = runner.transform.position + runner.transform.forward * dist;
                _isMoving = true;
                
                if (runner.aIPath != null)
                {
                    runner.aIPath.canMove = true;
                    runner.aIPath.isStopped = false;
                    runner.aIPath.destination = _targetPosition;
                }
                
                Log($"<color=cyan>[StepAttack]</color> Step {_currentStep}: 이동 시작 -> 목표 거리 {dist}m");
            }
            else
            {
                Log($"<color=cyan>[StepAttack]</color> Step {_currentStep}: 이동 거리 0 (정지)");
            }
        }

        // 3. 조건부 마법 이펙트 생성
        if (!string.IsNullOrEmpty(magicCheckKey))
        {
            if (brain.blackboard.GetValueOrDefault<bool>(magicCheckKey, false))
            {
                if (magicEffectPrefab != null)
                {
                    Vector3 spawnPos = runner.transform.position + runner.transform.TransformDirection(magicEffectOffset);
                    Instantiate(magicEffectPrefab, spawnPos, runner.transform.rotation);
                    Log($"<color=cyan>[StepAttack]</color> Step {_currentStep}: 마법 이펙트 발동 성공!");
                }
                else
                {
                    Log($"<color=orange>[StepAttack Warning]</color> 마법 조건은 충족되었으나 프리팹이 할당되지 않았습니다.");
                }
            }
        }
    }

    protected override void UpdateMovement()
    {
        if (!_isMoving) return;

        float distToTarget = Vector3.Distance(runner.transform.position, _targetPosition);
        if (distToTarget <= 0.3f)
        {
            _isMoving = false;
            if (runner.aIPath != null)
            {
                runner.aIPath.isStopped = true;
            }
            Log($"<color=cyan>[StepAttack]</color> Step {_currentStep}: 목표 지점 도달 (이동 종료)");
        }
    }

    protected override bool IsMovementFinished => !_isMoving;

    protected override void SpecificCleanup()
    {
        Log($"<color=cyan>[StepAttack]</color> 리소스 정리 (현재 단계: {_currentStep}, 이동 중 여부: {_isMoving})");
        _isMoving = false;
        _currentStep = -1;
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.stepDistances = new List<float>(stepDistances);
        node.phaseAttackDataKeys = new List<string>(phaseAttackDataKeys);
        return node;
    }
}
