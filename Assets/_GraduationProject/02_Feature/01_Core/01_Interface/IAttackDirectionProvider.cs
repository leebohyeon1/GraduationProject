using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 공격 방향 제공 인터페이스
/// </summary>
public interface IAttackDirectionProvider
{
    /// <summary>
    /// 현재 공격 방향 (월드 좌표 기준)
    /// </summary>
    public Vector3 CurrentAttackDirection { get; }
    
    /// <summary>
    /// 공격 방향이 변경되었을 때 발생하는 이벤트
    /// </summary>
    public UnityEvent<Vector3> OnAttackDirectionChanged { get; }
    
    /// <summary>
    /// 마우스 위치를 기준으로 공격 방향을 계산하는 함수
    /// </summary>
    /// <param name="mousePosition">마우스 스크린 좌표</param>
    /// <param name="playerTransform">플레이어 Transform</param>
    /// <param name="camera">메인 카메라</param>
    /// <returns>공격 방향 벡터</returns>
    public Vector3 CalculateMouseDirection(Vector2 mousePosition, Transform playerTransform, Camera camera);
    
    /// <summary>
    /// 게임패드 스틱 입력을 기준으로 공격 방향을 계산하는 함수
    /// </summary>
    /// <param name="stickInput">게임패드 스틱 입력 값</param>
    /// <param name="cameraTransform">카메라 Transform (방향 기준)</param>
    /// <returns>공격 방향 벡터</returns>
    public Vector3 CalculateGamepadDirection(Vector2 stickInput, Transform cameraTransform);
    
    /// <summary>
    /// 입력 기기에 따라 자동으로 적절한 공격 방향을 업데이트하는 함수
    /// </summary>
    /// <param name="inputValue">입력 값 (마우스 위치 또는 스틱 입력)</param>
    /// <param name="inputDeviceType">현재 입력 기기 타입</param>
    /// <param name="playerTransform">플레이어 Transform</param>
    /// <param name="camera">메인 카메라</param>
    public void UpdateAttackDirection(Vector2 inputValue, InputDeviceType inputDeviceType, Transform playerTransform, Camera camera);
}