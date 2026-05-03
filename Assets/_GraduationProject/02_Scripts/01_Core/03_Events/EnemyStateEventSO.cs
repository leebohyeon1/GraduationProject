using System;
using UnityEngine;

/// <summary>
/// 몬스터의 인식 상태를 정의합니다.
/// </summary>
public enum EnemyStateType
{
    Detected, // 발견됨
    Lost,     // 놓침
    Dead      // 사망함
}

/// <summary>
/// 몬스터 상태 이벤트와 함께 전달될 데이터 구조체입니다.
/// </summary>
[Serializable]
public struct EnemyStateData
{
    public Enemy enemy;
    public EnemyStateType stateType;
}

/// <summary>
/// EnemyStateData를 전달하는 스크립터블 오브젝트 이벤트 채널입니다.
/// </summary>
[CreateAssetMenu(fileName = "EnemyStateEventChannel", menuName = "Events/Enemy State Event Channel")]
public class EnemyStateEventSO : EventSO<EnemyStateData>
{
}
