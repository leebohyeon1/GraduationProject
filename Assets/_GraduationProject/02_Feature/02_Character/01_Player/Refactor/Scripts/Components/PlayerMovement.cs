using UnityEngine;

namespace player.Refactor
{
    public class PlayerMovement : MonoBehaviour
    {
        #region Private Fields
        private CharacterController _characterController;
        private Camera _mainCamera;

        /// <summary>
        /// 현재 속도 벡터 (중력 포함)
        /// </summary>
        private Vector3 _velocity;
        /// <summary>
        /// 지면 접촉 상태
        /// </summary>
        private bool _isGrounded;
        /// <summary>
        /// 마지막 회피 시간 (쿨다운 계산용)
        /// </summary>
        private float _lastDodgeTime = -999f;
        #endregion

        #region Properties
        public float LastDodgeTime => _lastDodgeTime;

        #endregion


        public void Initialize(CharacterController characterController)
        {
            if (_characterController == null)
            {
                _characterController = characterController;
            }

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }


        /// <summary>
        /// 카메라 기준 이동 처리
        /// 입력 방향을 카메라 기준으로 변환하여 이동 및 회전 수행
        /// </summary>
        /// <param name="direction">이동 방향 (로컬 입력 좌표)</param>
        /// <param name="moveSpeed">이동 속도</param>
        public void Move(Vector3 direction, float moveSpeed, float rotateSpeed)
        {
            if (_characterController == null || _mainCamera == null)
            {
                return;
            }

            // 카메라 기준으로 방향 변환
            Vector3 cameraForward = _mainCamera.transform.forward;
            Vector3 cameraRight = _mainCamera.transform.right;

            // Y축 제거 (수평면 이동만)
            cameraForward.y = 0;
            cameraRight.y = 0;

            // 정규화
            cameraForward.Normalize();
            cameraRight.Normalize();

            // 입력에 따른 이동 방향 계산
            Vector3 moveVector = cameraForward * direction.z + cameraRight * direction.x;

            // 정규화 및 속도 적용
            if (moveVector.magnitude > 1f)
            {
                moveVector.Normalize();
            }

            Vector3 movement = moveVector * moveSpeed * Time.fixedDeltaTime;

            // 중력 적용
            movement.y = _velocity.y * Time.fixedDeltaTime;

            // 실제 이동
            _characterController.Move(movement);

            // 회전 (이동 방향으로)
            if (moveVector.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveVector);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotateSpeed * Time.fixedDeltaTime
                );
            }
        }

        /// <summary>
        /// 회피 이동 실행
        /// 입력 방향이 있으면 해당 방향으로, 없으면 전방으로 회피
        /// </summary>
        /// <param name="direction">회피 방향</param>
        /// <param name="dodgeSpeed">회피 속도</param>
        public void Dodge(Vector3 direction, float dodgeSpeed)
        {
            if (direction != Vector3.zero)
            {
                // 입력 방향으로 회피 (카메라 기준 변환 포함)
                Move(direction, dodgeSpeed, 360);
            }
            else
            {
                // 입력이 없으면 캐릭터 전진 방향으로 회피
                Vector3 moveVector = transform.forward;
                Vector3 movement = moveVector * dodgeSpeed * Time.fixedDeltaTime;
                movement.y = _velocity.y * Time.fixedDeltaTime;
                _characterController.Move(movement);
            }

            _lastDodgeTime = Time.time;
        }

        /// <summary>
        /// 지면 접촉 상태 체크
        /// CharacterController 하단에서 레이캐스트로 확인
        /// </summary>
        /// <param name="groundCheckDistance">지면 체크 거리</param>
        /// <param name="groundLayerMask">지면 레이어</param>
        public void CheckGrounded(float groundCheckDistance, LayerMask groundLayerMask)
        {
            // CharacterController의 아래쪽 경계에서 체크
            Vector3 rayOrigin = transform.position - new Vector3(0, _characterController.height / 2f, 0);
            _isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundLayerMask);
        }


        /// <summary>
        /// 중력 적용
        /// 지면 접촉 시 미세한 하향력 유지, 공중에서는 중력 가속도 적용
        /// </summary>
        /// <param name="gravityScale">중력 크기</param>
        public void ApplyGravity(float gravityScale)
        {
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f; // 약간의 하향력 유지하여 지면에 붙어있도록
            }
            else
            {
                _velocity.y += gravityScale * Time.fixedDeltaTime;
            }

            // 최대 낙하 속도 제한
            if (_velocity.y < -30f)
            {
                _velocity.y = -30f;
            }
        }

        #region Rotate
        /// <summary>
        /// 지정된 방향으로 즉시 회전
        /// 회피 시작 시 방향 설정 등에 사용
        /// </summary>
        /// <param name="direction">회전할 방향</param>
        public void RotateImmediately(Vector3 direction)
        {
            if (transform == null) return;

            // 카메라 기준으로 방향 변환
            Vector3 cameraForward = _mainCamera.transform.forward;
            Vector3 cameraRight = _mainCamera.transform.right;

            // Y축 제거 (수평면 이동만)
            cameraForward.y = 0;
            cameraRight.y = 0;

            // 정규화
            cameraForward.Normalize();
            cameraRight.Normalize();

            // 즉시 회전 적용
            transform.rotation = Quaternion.LookRotation(cameraForward * direction.z + cameraRight * direction.x);
        }

        /// <summary>
        /// 입력 기기에 따라 회전
        /// 키보드/마우스는 마우스 위치, 게임패드는 좌측 스틱 방향 사용
        /// </summary>
        /// <param name="deviceType">현재 사용 중인 입력 기기 타입</param>
        /// <param name="moveInput">게임패드 좌측 스틱 입력 벡터</param>
        /// <param name="mousePosition">마우스 스크린 좌표 위치</param>
        public void RotateToDirection(InputDeviceType deviceType, Vector2 moveInput, Vector2 mousePosition)
        {
            if (deviceType == InputDeviceType.KeyboardMouse)
            {
                RotatePlayerWithMouse(mousePosition);    // 마우스 위치 기반 회전
            }
            else // Gamepad
            {
                RotatePlayerWithGamepad(moveInput);      // 게임패드 스틱 방향 기반 회전
            }
        }

        /// <summary>
        /// 게임패드 좌측 스틱 입력으로 플레이어 회전
        /// 카메라 방향을 기준으로 스틱 입력을 월드 방향으로 변환합니다.
        /// </summary>
        /// <param name="moveInput">게임패드 우측 스틱의 2D 입력 벡터</param>
        private void RotatePlayerWithGamepad(Vector2 moveInput)
        {
            // 입력 강도가 최소 임계값 이하이거나 카메라가 없으면 무시
            if (moveInput.sqrMagnitude < 0.1f || Camera.main == null)
            {
                return;
            }

            // 스틱 입력을 카메라 기준 3D 방향으로 변환
            Vector3 lookDirection = CalculateLookDirection(moveInput);
            if (lookDirection.sqrMagnitude > 0.1f)
            {
                // 방향 벡터를 사용하여 즐시 회전 (수직 축은 고정)
                transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            }
        }

        /// <summary>
        /// 마우스 스크린 좌표로 플레이어 회전
        /// 마우스 스크린 위치를 월드 좌표로 변환하여 회전 방햦 계산
        /// Ray를 사용하여 마우스 포인터의 3D 월드 좌표를 구합니다.
        /// </summary>
        /// <param name="mousePosition">마우스의 스크린 좌표 (pixels)</param>
        private void RotatePlayerWithMouse(Vector2 mousePosition)
        {
            if (Camera.main == null) return;

            // 스크린 좌표를 3D 공간의 레이로 변환
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            // 플레이어와 같은 Y 높이의 수평 평면 생성 (지면 평면)
            Plane groundPlane = new Plane(Vector3.up, transform.position.y);

            // 레이가 지면 평면과 교차하는지 확인
            if (groundPlane.Raycast(ray, out float distance))
            {
                // 교차점에서 플레이어로의 방햦 계산
                Vector3 direction = GetMouseDirection(ray.GetPoint(distance));
                if (direction.sqrMagnitude > 0.1f)  // 최소 방햦 강도 체크
                {
                    // 계산된 방햦으로 즐시 회전
                    transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                }
            }
        }

        /// <summary>
        /// 게임패드 2D 스틱 입력을 카메라 기준 3D 월드 방향으로 변환
        /// 카메라의 전후좌우 방햦을 기준으로 스틱 입력을 3D 공간에 매핑합니다.
        /// </summary>
        /// <param name="lookInput">게임패드 우측 스틱의 2D 입력 (x: 좌우, y: 전후)</param>
        /// <returns>카메라 기준으로 정규화된 3D 방향 벡터</returns>
        private Vector3 CalculateLookDirection(Vector2 lookInput)
        {
            // 카메라의 전진 및 우측 방햦 벡터 추출
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;

            // Y축(vertical) 성분 제거하여 수평 면만 고려
            cameraForward.y = 0;
            cameraRight.y = 0;

            // 벡터 정규화
            cameraForward.Normalize();
            cameraRight.Normalize();

            // 스틱 입력을 카메라 기준 방햦으로 변환 및 정규화
            return (cameraRight * lookInput.x + cameraForward * lookInput.y).normalized;
        }

        /// <summary>
        /// 3D 월드 마우스 위치에서 플레이어로의 수평 방햦 계산
        /// Y축을 제거하여 수평면에서만의 방햦을 계산합니다.
        /// </summary>
        /// <param name="worldMousePosition">Ray가 지면과 교차한 3D 월드 좌표</param>
        /// <returns>플레이어에서 마우스 방햦으로의 정규화된 2D 방햦 벡터</returns>
        private Vector3 GetMouseDirection(Vector3 worldMousePosition)
        {
            // 마우스 위치에서 플레이어 위치로의 벡터 계산
            Vector3 direction = (worldMousePosition - transform.position).normalized;

            // Y축(수직) 성분 제거하여 수평 방햦만 사용
            direction.y = 0;

            return direction;
        }
        #endregion
    }
}


