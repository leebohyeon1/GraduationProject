using DG.Tweening;
using System;
using UnityEngine;

/// <summary>
/// 플레이어의 이동, 회전, 중력 등 물리적인 움직임을 담당하는 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour, IDisposable
{
    #region Private Fields
    private CharacterController _characterController; // 캐릭터 컨트롤러
    private Camera _camera; // 메인 카메라
    private PlayerEvents _events;
    private PlayerDataSO _data;

    private Vector3 _velocity;          // 현재 속도 (중력 포함)
    private float _currentMoveSpeed;    // 현재 이동 속도
    private float _currentRotationSpeed;// 현재 회전 속도

    private float _moveAccelTimer;              // 이동 가속 타이머
    private float _rotateAccelTimer;            // 회전 가속 타이머
    #endregion

    [Header("Properties")]
    public Vector3 Velocity => _velocity;
    public float CurrentMoveSpeed => _currentMoveSpeed;
    public float CurrentRotationSpeed => _currentRotationSpeed; 

    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize(PlayerController player)
    {
        _characterController = GetComponent<CharacterController>();
        _camera = Camera.main;

        _events = player.Events;
        _data = player.Data;
    }

    public void Dispose()
    {
    }

    #region Move
    /// <summary>
    /// 캐릭터 컨트롤러를 속도에 따른 이동
    /// </summary>
    /// <param name="velocity">속도</param>
    public void CharacterControllerMove(Vector3 velocity, float deltaTime)
    {
        // 중력 적용
        ApplyGravity();

        _characterController.Move(velocity * deltaTime);
    }

    /// <summary>
    /// 캐릭터를 이동 방향을 향해서 
    /// 현재 이동속도로 움직입니다.
    /// </summary>
    /// <param name="moveDirection">이동 방향</param>
    /// <param name="deltaTime">델타 타임</param>
    public void Move(Vector3 moveDirection, float deltaTime)
    {
        // 중력을 제외하고 적용
        Vector3 velocity = moveDirection * _currentMoveSpeed;
        _velocity = new Vector3(velocity.x, _velocity.y, velocity.z);

        CharacterControllerMove(_velocity, deltaTime);
    }

    /// <summary>
    /// 캐릭터를 이동 방향으로 
    /// 일정 속도로 움직입니다.
    /// </summary>
    /// <param name="moveDirection">이동 방향</param>
    /// <param name="speed">속도</param>
    /// <param name="deltaTime">델타 타임</param>
    public void Move(Vector3 moveDirection, float speed, float deltaTime)
    {
        // 중력을 제외하고 적용
        Vector3 velocity = moveDirection.normalized * speed;
        _velocity = new Vector3(velocity.x, _velocity.y, velocity.z);

        CharacterControllerMove(_velocity, deltaTime);
    }

    /// <summary>
    /// 입력이 있으면 카메라 기준으로 플레이어 이동
    /// 입력이 없거나 반대 방향으로 이동 시 감속
    /// </summary>
    /// <param name="moveInput">이동 입력</param>
    /// <param name="deltaTime">델타 타임</param>
    public void MoveByInput(Vector3 moveInput, float deltaTime)
    {
        // 입력 유무에 따른 타이머 증감 처리
        bool isInput = moveInput.sqrMagnitude > 0.01f;

        // 방향 전환 체크 (입력이 있고, 현재 움직이는 방향과 반대일 때)
        bool isReversed = isInput && _currentMoveSpeed > (_data.MoveSpeed / 2f) &&
                         Vector3.Dot(moveInput, transform.forward) < -0.3f;

        if (isInput && !isReversed)
        {
            // 가속: 타이머 증가
            _moveAccelTimer += deltaTime / _data.MoveAccelerationTime;
        }
        else
        {
            // 감속 혹은 방향 전환: 타이머 감소
            _moveAccelTimer -= deltaTime / _data.MoveDecelerationnTime;
        }

        _moveAccelTimer = Mathf.Clamp01(_moveAccelTimer);

        // 커브와 이동 입력에 따른 스피드 계산
        float speedRatio = _data.MoveCurve.Evaluate(_moveAccelTimer);
        _currentMoveSpeed = _data.MoveSpeed * speedRatio * moveInput.magnitude;

        // 카메라 방향 기준으로 이동 방향 계산
        Vector3 relativeDirection = GetRelativeVectorToCamera(moveInput);

        Move(relativeDirection, deltaTime);
    }

    /// <summary>
    /// 중력을 적용합니다.
    /// </summary>
    protected void ApplyGravity()
    {
        if (_characterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // 지면에 붙어있도록 약간의 하향력 유지
        }
        else
        {
            _velocity.y += Physics.gravity.y;
        }

        // 최대 낙하 속도 제한
        if (_velocity.y < -30f)
        {
            _velocity.y = -30f;
        }
    }
    #endregion

    #region Rotate
    /// <summary>
    /// 캐릭터를 회전 방향을 향해
    /// 현재 회전속도로 회전
    /// </summary>
    /// <param name="rotateDirection">회전 방향</param>
    /// <param name="deltaTime">델타 타임</param>
    public void Rotate(Vector3 rotateDirection, float deltaTime)
    {     
        // 각 벡터 방향 구하기
        Vector3 currentDirecrion = transform.forward;
        Vector3 targetDirection = rotateDirection.normalized;

        // 회전 방향이 비슷하면 감속
        if (Vector3.Dot(targetDirection, currentDirecrion) >= 0.9f)
        {
            // 회전 감속
            _rotateAccelTimer -= deltaTime / _data.RotateDecelerationTime;
        }
        else
        {
            // 회전 가속
            _rotateAccelTimer += deltaTime / _data.RotateDecelerationTime;
        }

        _rotateAccelTimer = Mathf.Clamp01(_rotateAccelTimer);

        // 커브를 이용한 회전 속도 계산
        float rotationRatio = _data.RotationCurve.Evaluate(_rotateAccelTimer);
        _currentRotationSpeed = _data.RotateSpeed * rotationRatio;

        // 회전 적용
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            _currentRotationSpeed * deltaTime
        );
    }

    /// <summary>
    /// 캐릭터를 회전 방향을 향해
    /// 일정 속도로 회전합니다.
    /// </summary>
    /// <param name="rotateDirection">회전 방향</param>
    /// <param name="rotationSpeed">회전 속도</param>
    /// <param name="deltaTime">델타 타임</param>
    public void Rotate(Vector3 rotateDirection, float rotationSpeed, float deltaTime)
    {
        Vector3 targetDirection = rotateDirection.normalized;

        // 회전 적용
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * deltaTime
        );
    }

    /// <summary>
    /// 이동 방향으로 회전
    /// </summary>
    /// <param name="deltaTime">델타 타임</param>
    public void RotateToVelocity(float deltaTime)
    {
        // 중력을 제외한 값 사용
        Vector3 velocity = new Vector3(_velocity.x, 0, _velocity.z);

        // 속도가 거의 없으면 회전 타이머 감소 후 리턴
        if (_currentMoveSpeed <= 0.1f)
        {
            _rotateAccelTimer -= deltaTime / _data.RotateDecelerationTime;
            _rotateAccelTimer = Mathf.Clamp01(_rotateAccelTimer);

            // 커브를 이용한 회전 속도 계산
            _currentRotationSpeed = _data.RotateSpeed * _data.RotationCurve.Evaluate(_rotateAccelTimer);
            return;
        }

        Rotate(velocity, deltaTime);
    }
    #endregion

    #region Dodge

    /// <summary>
    /// 원하는 방향으로 스텝 데이터에 따라 스텝
    /// </summary>
    /// <param name="direction">스텝 방향</param>
    /// <param name="stepData">스텝 데이터</param>
    /// <param name="id">트윈 아이디</param>
    /// <param name="useGravity">중력 사용 여부</param>
    /// <param name="compeleteCallback">종료 시 콜백</param>
    public void Step(Vector3 direction, StepData stepData, object id, bool useGravity = false,
        TweenCallback compeleteCallback = null)
    {
        float currentDistance = 0f;

        DOTween.To(
            () => currentDistance,
            x =>
            {
                Vector3 moveDirection = direction;
                float deltaDistance = x - currentDistance;
                Vector3 displacement = moveDirection * deltaDistance;

                Rotate(moveDirection, stepData.StepRotateSpeed, Time.fixedDeltaTime);

                if (useGravity)
                {
                    ApplyGravity();
                }
                // 캐릭터 컨트롤러 이동
                _characterController.Move(displacement);

                currentDistance = x;
            },
            stepData.StepDistance,
            stepData.StepDuration)
            .SetEase(stepData.StepCurve)
            .SetId(id)
            .SetUpdate(UpdateType.Fixed)
            .OnComplete(compeleteCallback);
    }
    #endregion

    #region Util Methods
    /// <summary>
    /// 기존 벡터를 
    /// 카메라 기준 로컬 벡터 변환
    /// </summary>
    /// <param name="vector">벡터</param>
    /// <returns>카메라 방향 기준 벡터</returns>
    public Vector3 GetRelativeVectorToCamera(Vector3 vector)
    {
        // 카메라 방향 기준으로 이동 방향 계산
        Vector3 relativeDirection = vector.x * _camera.transform.right + vector.z * Vector3.Scale(new Vector3(1,0,1), _camera.transform.forward);
        relativeDirection.y = 0; // 수평면에서만 이동하도록 y축 성분 제거
        relativeDirection.Normalize();  // 정규화

        return relativeDirection;
    }

    /// <summary>
    /// 플레이어에서 마우스방향으로 벡터 반환
    /// </summary>
    /// <param name="mousePosition">마우스 위치</param>
    /// <returns>마우스 방향 벡터</returns>
    public Vector3 GetDirectionToMouse(Vector3 mousePosition)
    {
        Vector3 currentPosition = transform.position;

        // 마우스 포지션을 월드 포지션으로 변환
        float distance = Vector3.Distance(currentPosition, _camera.transform.position);
        Vector3 point = _camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.z, distance));

        // 방향 계산
        Vector3 vectorToMouse = point - currentPosition;
        vectorToMouse.y = 0;
        vectorToMouse.Normalize();

        return vectorToMouse;
    }

    #endregion

    #region Config Methods
    /// <summary>
    /// 카메라 설정
    /// </summary>
    /// <param name="camera">설정할 카메라</param>
    public void SetCamera(Camera camera)
    {
        _camera = camera;
    }

    #endregion
}