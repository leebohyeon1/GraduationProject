using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 플레이어의 이동, 회전, 중력 등 물리적인 움직임을 담당하는 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour, IDisposable, IDragable
{
    [Header("References")]
    private CharacterController _characterController; // 캐릭터 컨트롤러
    private Camera _camera; // 메인 카메라
    private PlayerEvents _events;
    private PlayerData _runtimeData;

    [Header("Movement Setting")]
    // 최대 이동 속도
    public float MaxMoveSpeed => _runtimeData.MoveSpeed.Value;

    private float _moveAccelerationTime;    // 이동 가속 시간
    public float MoveAccelerationTime => _moveAccelerationTime;

    private float _moveDecelerationnTime;   // 이동 감속 시간
    public float MoveDecelerationnTime => _moveDecelerationnTime;

    private AnimationCurve _moveCurve;      // 이동 가속 커브
    public AnimationCurve MoveCurve => _moveCurve;

   // 최대 회전 속도
    public float MaxRotateSpeed => _runtimeData.RotateSpeed.Value;

    private float _rotateAccelerationTime;    // 회전 가속 시간
    public float RotateAccelerationTime => _rotateAccelerationTime;    

    private float _rotateDecelerationTime;   // 회전 감속 시간
    public float RotateDecationTime => _rotateDecelerationTime;

    private AnimationCurve _rotateCurve;      // 회전 가속 커브
    public AnimationCurve RotateCurve => _rotateCurve;

    private Vector3 _velocity;              // 현재 속도 (중력 포함)
    public Vector3 Velocity => _velocity;

    private float _currentMoveSpeed;            // 현재 이동 속도   
    public float CurrentMoveSpeed => _currentMoveSpeed;

    private float _currentRotationSpeed;        // 현재 회전 속도
    public float CurrentRotationSpeed => _currentRotationSpeed;

    private float _moveAccelTimer;              // 이동 가속 타이머
    private float _rotateAccelTimer;            // 회전 가속 타이머

    private float _lastDodgeEndTime;            // 마지막 회피 종료 시간

    [Header("Dodge Setting")]
    // 회피 설정
    public RuntimeDodgeConfig DodgeConfig => _runtimeData != null ? _runtimeData.RuntimeDodge : null;
    
    public bool CanDodge => DodgeConfig == null || Time.time >= _lastDodgeEndTime + DodgeConfig.Cooldown.Value;

    [SerializeField] private PlayerAbilityTagSO _invincibleSO;
    public PlayerAbilityTagSO InvincibleSO => _invincibleSO;

    [Header("ChargeMove Setting")]
    // 차지 이동 속도
    public float ChargeMoveSpeed => _runtimeData.ChargeMoveSpeed.Value;
    // 차지 이동 속도
    public float ChargeRoataeSpeed => _runtimeData.ChargeRotateSpeed.Value;

    [Header("Obstacle Detection")]
    [SerializeField] private LayerMask _obstacleMask = -1;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask _groundMask = -1;
    [SerializeField] private float _groundCheckDistance = 0.3f; // 바닥 감지 여유 거리
    [SerializeField] private float _groundCheckRadius = 0.25f;  // 바닥 감지 반경

    [Header("Drag Setting")]
    [SerializeField] private PlayerAbilityTagSO _dragSuperArmorSO; // 드래그 슈퍼 아머
    public PlayerAbilityTagSO DragSuperArmorSO => _dragSuperArmorSO;
    public event Action<bool> Dragged;

    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize(PlayerController player)
    {
        _characterController = GetComponent<CharacterController>();
        _camera = player.Camera;
        _events = player.Events;
        _runtimeData = player.RuntimeData;
        
        player.RegisterDisposable(this);    // 이벤트 해제 구독

        InitializeData(player.Data);
    }

    public void Dispose()
    {
        _characterController.enabled = false;   // 캐릭터 컨트롤러 비활성화
        DOTween.Kill(this);

        Dragged = null;
    }

    /// <summary>
    /// 데이터 초기화
    /// </summary>
    /// <param name="data">플레이어 데이터</param>
    private void InitializeData(PlayerDataSO data)
    {
        _moveAccelerationTime = data.MoveAccelerationTime;
        _moveDecelerationnTime = data.MoveDecelerationnTime;
        _moveCurve = data.MoveCurve;

        _rotateAccelerationTime = data.RotateAccelerationTime;
        _rotateDecelerationTime = data.RotateDecelerationTime;
        _rotateCurve = data.RotateCurve;
    }

    //==========================================================================================================================
    // Move ====================================================================================================================
    //==========================================================================================================================

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
        // bool isReversed = isInput && _currentMoveSpeed > (_maxMoveSpeed / 2f) &&
        //                  Vector3.Dot(moveInput, transform.forward) < -0.3f;
        
        bool isReversed = isInput && _currentMoveSpeed > (MaxMoveSpeed / 2f) &&
                         Vector3.Dot(moveInput, transform.forward) < -0.3f;

        if (isInput && !isReversed)
        {
            // 가속: 타이머 증가
            _moveAccelTimer += deltaTime / _moveAccelerationTime;
        }
        else
        {
            // 감속 혹은 방향 전환: 타이머 감소
            _moveAccelTimer -= deltaTime / _moveDecelerationnTime;
        }

        _moveAccelTimer = Mathf.Clamp01(_moveAccelTimer);

        // 커브와 이동 입력에 따른 스피드 계산
        // float speedRatio = _moveCurve.Evaluate(_moveAccelTimer);
        // _currentMoveSpeed = _maxMoveSpeed * speedRatio * moveInput.magnitude;
        
        float speedRatio = _moveCurve.Evaluate(_moveAccelTimer);
        _currentMoveSpeed = MaxMoveSpeed * speedRatio * moveInput.magnitude;

        // 카메라 방향 기준으로 이동 방향 계산
        Vector3 relativeDirection = GetRelativeVectorToCamera(moveInput);

        Move(relativeDirection, deltaTime);
    }

    /// <summary>
    /// 캐릭터가 지면에 있는지 확인합니다. (후한 판정)
    /// </summary>
    public bool IsGrounded()
    {
        if (_characterController == null) return false;
        if (_characterController.isGrounded) return true;

        // 약간 위에서 아래로 SphereCast를 쏘아 바닥을 감지
        Vector3 origin = transform.position + Vector3.up * _characterController.radius;
        float castDistance = _groundCheckDistance + _characterController.radius;
        
        return Physics.SphereCast(origin, _groundCheckRadius, -transform.up, out _, castDistance, _groundMask, QueryTriggerInteraction.Ignore);
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
            _velocity.y += Physics.gravity.y * 20 * Time.fixedDeltaTime;
        }

        // 최대 낙하 속도 제한
        if (_velocity.y < -30f)
        {
            _velocity.y = -30f;
        }
    }
    #endregion

    //==========================================================================================================================
    // Rotate ==================================================================================================================
    //==========================================================================================================================

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
            _rotateAccelTimer -= deltaTime / _rotateDecelerationTime;
        }
        else
        {
            // 회전 가속
            _rotateAccelTimer += deltaTime / _rotateAccelerationTime;
        }

        _rotateAccelTimer = Mathf.Clamp01(_rotateAccelTimer);

        // 커브를 이용한 회전 속도 계산
        // float rotationRatio = _rotateCurve.Evaluate(_rotateAccelTimer);
        // _currentRotationSpeed = _maxRotateSpeed * rotationRatio;
        
        float rotationRatio = _rotateCurve.Evaluate(_rotateAccelTimer);
        _currentRotationSpeed = MaxRotateSpeed * rotationRatio;

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
            _rotateAccelTimer -= deltaTime / _rotateDecelerationTime;
            _rotateAccelTimer = Mathf.Clamp01(_rotateAccelTimer);

            // 커브를 이용한 회전 속도 계산
            // _currentRotationSpeed = -_maxRotateSpeed * _rotateCurve.Evaluate(_rotateAccelTimer);
            _currentRotationSpeed = -MaxRotateSpeed * _rotateCurve.Evaluate(_rotateAccelTimer);
            return;
        }

        Rotate(velocity, deltaTime);
    }
    #endregion

    //==========================================================================================================================
    // Dodge ==================================================================================================================
    //==========================================================================================================================

    #region Dodge

    /// <summary>
    /// 회피 종료 시간을 기록합니다.
    /// </summary>
    public void SetLastDodgeEndTime()
    {
        _lastDodgeEndTime = Time.time;
    }

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
        float targetDistance = stepData.StepDistance;

        // 장애물 감지: 이동 방향에 장애물이 있으면 거리를 제한
        if (Physics.SphereCast(transform.position + _characterController.center, _characterController.radius, direction.normalized, out RaycastHit hit, targetDistance, _obstacleMask))
        {
            targetDistance = hit.distance;
        }

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
                    displacement = new Vector3(displacement.x, _velocity.y * Time.fixedDeltaTime, displacement.z);
                }
                // 캐릭터 컨트롤러 이동
                _characterController.Move(displacement);

                currentDistance = x;
            },
            targetDistance,
            stepData.StepDuration)
            .SetEase(stepData.StepCurve)
            .SetId(id)
            .SetUpdate(UpdateType.Fixed)
            .OnComplete(compeleteCallback);
    }


    /// <summary>
    /// 원하는 방향으로 스텝 데이터에 따라 스텝
    /// </summary>
    /// <param name="direction">스텝 방향</param>
    /// <param name="id">트윈 아이디</param>
    /// <param name="useGravity">중력 사용 여부</param>
    /// <param name="compeleteCallback">종료 시 콜백</param>
    public void Step(Vector3 direction, object id, bool useGravity = false, TweenCallback compeleteCallback = null)
    {
        float currentDistance = 0f;
        float targetDistance = DodgeConfig.MoveConfig.StepDistance;

        // 장애물 감지: 이동 방향에 장애물이 있으면 거리를 제한
        if (Physics.SphereCast(transform.position + _characterController.center, _characterController.radius, direction.normalized, out RaycastHit hit, targetDistance, _obstacleMask))
        {
            targetDistance = hit.distance;
        }

        DOTween.To(
            () => currentDistance,
            x =>
            {
                Vector3 moveDirection = direction == Vector3.zero ? transform.forward : direction;
                float deltaDistance = x - currentDistance;
                Vector3 displacement = moveDirection * deltaDistance;

                Rotate(moveDirection, DodgeConfig.MoveConfig.StepRotateSpeed, Time.fixedDeltaTime);

                if (useGravity)
                {
                    ApplyGravity();
                    displacement = new Vector3(displacement.x, _velocity.y * Time.fixedDeltaTime, displacement.z);
                }

                // 캐릭터 컨트롤러 이동
                _characterController.Move(displacement);

                currentDistance = x;
            },
            targetDistance,
            DodgeConfig.MoveConfig.StepDuration)
            .SetEase(DodgeConfig.MoveConfig.StepCurve)
            .SetId(id)
            .SetUpdate(UpdateType.Fixed)
            .OnComplete(compeleteCallback);
    }


    /// <summary>
    /// 정면으로 구르기 방향에 따라 구르기
    /// </summary>
    /// <param name="stepData">스텝 데이터</param>
    /// <param name="id">트윈 아이디</param>
    /// <param name="compeleteCallback">종료 시 콜백</param>
    public void Roll(StepData stepData, object id, TweenCallback compeleteCallback = null)
    {
        // 구르기 시작
        float currentDistance = 0f;

        DOTween.To(
            () => currentDistance,
            x =>
            {
                float deltaDistance = x - currentDistance;

                // 캐릭터 컨트롤러 이동
                Vector3 displacement = transform.forward * deltaDistance;
                displacement = new Vector3(displacement.x, _velocity.y * Time.fixedDeltaTime, displacement.z);
                CharacterControllerMove(displacement, 1);

                currentDistance = x;
            },
            stepData.StepDistance,
            stepData.StepDuration)
            .SetEase(stepData.StepCurve)
            .SetId(this)
            .SetUpdate(UpdateType.Fixed)
            .OnComplete(compeleteCallback);
    }

    /// <summary>
    /// 정면으로 구르기 방향에 따라 구르기
    /// </summary>
    /// <param name="id">트윈 아이디</param>
    /// <param name="compeleteCallback">종료 시 콜백</param>
    public void Roll(object id, TweenCallback compeleteCallback = null)
    {
        // 구르기 시작
        float currentDistance = 0f;

        DOTween.To(
            () => currentDistance,
            x =>
            {
                float deltaDistance = x - currentDistance;

                // 캐릭터 컨트롤러 이동
                Vector3 displacement = transform.forward * deltaDistance;
                displacement = new Vector3(displacement.x, _velocity.y * Time.fixedDeltaTime, displacement.z);
                CharacterControllerMove(displacement, 1);

                currentDistance = x;
            },
            DodgeConfig.MoveConfig.StepDistance,
            DodgeConfig.MoveConfig.StepDuration)
            .SetEase(DodgeConfig.MoveConfig.StepCurve)
            .SetId(this)
            .SetUpdate(UpdateType.Fixed)
            .OnComplete(compeleteCallback);
    }
    #endregion

    //==========================================================================================================================
    // Charge ====================================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// 차지 이동속도 설정
    /// </summary>
    public void SetChargeMoveSpeed(float speed)
    {
        // _chargeMoveSpeed = speed;
        // if (_runtimeData != null) _runtimeData.ChargeMoveSpeed.BaseValue = speed;
    }

    //==========================================================================================================================
    // Drag ====================================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// 플레이어 드래그 함수
    /// </summary>
    public void Drag()
    {
        Dragged?.Invoke(true);
    }

    /// <summary>
    /// 플레이어 드래그 취소 함수
    /// </summary>
    public void Drop()
    {
        Dragged?.Invoke(false);
    }

    //==========================================================================================================================
    // Util ====================================================================================================================
    //==========================================================================================================================

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
}