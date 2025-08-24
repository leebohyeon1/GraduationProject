using UnityEngine;
using UnityEngine.Events;
using BH_Lib.DI;

/// <summary>
/// 입력 기기에 따라 공격 방향을 제공하는 컨트롤러
/// </summary>
[Register(typeof(IAttackDirectionProvider), LifetimeScope.Singleton)]
public class AttackDirectionController : DIMonoBehaviour, IAttackDirectionProvider
{
    private float _gamepadDeadZone = 0.2f;
    private float _mouseDeadZone = 50f; // 픽셀 단위
    
    private Vector3 _currentAttackDirection = Vector3.forward;
    
    /// <summary>
    /// 현재 공격 방향 (월드 좌표 기준)
    /// </summary>
    public Vector3 CurrentAttackDirection => _currentAttackDirection;
    
    /// <summary>
    /// 공격 방향이 변경되었을 때 발생하는 이벤트
    /// </summary>
    public UnityEvent<Vector3> OnAttackDirectionChanged { get; private set; } = new UnityEvent<Vector3>();
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    /// <summary>
    /// 마우스 위치를 기준으로 공격 방향을 계산하는 함수
    /// </summary>
    /// <param name="mousePosition">마우스 스크린 좌표</param>
    /// <param name="playerTransform">플레이어 Transform</param>
    /// <param name="camera">메인 카메라</param>
    /// <returns>공격 방향 벡터</returns>
    public Vector3 CalculateMouseDirection(Vector2 mousePosition, Transform playerTransform, Camera camera)
    {
        if (camera == null || playerTransform == null)
        {
            return Vector3.forward;
        }
        
        // 마우스 위치를 월드 좌표로 변환
        Ray ray = camera.ScreenPointToRay(mousePosition);
        Plane groundPlane = new Plane(Vector3.up, playerTransform.position.y);
        
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldMousePosition = ray.GetPoint(distance);
            Vector3 direction = (worldMousePosition - playerTransform.position).normalized;
            
            // Y축 제거 (수평면에서만 방향 계산)
            direction.y = 0;
            
            if (direction.magnitude > 0.1f) // 최소 거리 체크
            {
                return direction.normalized;
            }
        }
        
        return _currentAttackDirection;
    }
    
    /// <summary>
    /// 게임패드 스틱 입력을 기준으로 공격 방향을 계산하는 함수
    /// </summary>
    /// <param name="stickInput">게임패드 스틱 입력 값</param>
    /// <param name="cameraTransform">카메라 Transform (방향 기준)</param>
    /// <returns>공격 방향 벡터</returns>
    public Vector3 CalculateGamepadDirection(Vector2 stickInput, Transform cameraTransform)
    {
        // 데드존 체크
        if (stickInput.magnitude < _gamepadDeadZone)
        {
            return _currentAttackDirection;
        }
        
        Vector3 direction;
        
        if (cameraTransform != null)
        {
            // 카메라 기준으로 방향 계산
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            
            // Y축 제거 (수평면에서만 계산)
            cameraForward.y = 0;
            cameraRight.y = 0;
            
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            // 스틱 입력을 카메라 기준 방향으로 변환
            direction = (cameraRight * stickInput.x + cameraForward * stickInput.y).normalized;
        }
        else
        {
            // 카메라가 없으면 월드 좌표 기준으로 계산
            direction = new Vector3(stickInput.x, 0, stickInput.y).normalized;
        }
        
        return direction;
    }
    
    /// <summary>
    /// 입력 기기에 따라 자동으로 적절한 공격 방향을 업데이트하는 함수
    /// </summary>
    /// <param name="inputValue">입력 값 (마우스 위치 또는 스틱 입력)</param>
    /// <param name="inputDeviceType">현재 입력 기기 타입</param>
    /// <param name="playerTransform">플레이어 Transform</param>
    /// <param name="camera">메인 카메라</param>
    public void UpdateAttackDirection(Vector2 inputValue, InputDeviceType inputDeviceType, Transform playerTransform, Camera camera)
    {
        Vector3 newDirection;
        
        switch (inputDeviceType)
        {
            case InputDeviceType.KeyboardMouse:
                newDirection = CalculateMouseDirection(inputValue, playerTransform, camera);
                break;
                
            case InputDeviceType.Gamepad:
                newDirection = CalculateGamepadDirection(inputValue, camera?.transform);
                break;
                
            default:
                newDirection = _currentAttackDirection;
                break;
        }
        
        // 방향이 실제로 변경되었을 때만 업데이트
        if (Vector3.Angle(_currentAttackDirection, newDirection) > 1f) // 1도 이상 차이날 때
        {
            SetAttackDirection(newDirection);
        }
    }
    
    /// <summary>
    /// 공격 방향을 직접 설정하는 함수
    /// </summary>
    /// <param name="direction">새로운 공격 방향</param>
    private void SetAttackDirection(Vector3 direction)
    {
        _currentAttackDirection = direction.normalized;
        OnAttackDirectionChanged?.Invoke(_currentAttackDirection);
    }
}