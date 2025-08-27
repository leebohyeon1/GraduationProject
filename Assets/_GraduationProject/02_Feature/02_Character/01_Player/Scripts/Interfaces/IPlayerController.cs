using UnityEngine;

/// <summary>
/// 플레이어 입력 컨트롤러 인터페이스
/// Unity Input System으로부터 입력을 받아 게임 로직에서 사용할 수 있도록 제공합니다.
/// </summary>
public interface IPlayerController
{
    /// <summary>이동 입력 (WASD 또는 좌 스틱)</summary>
    Vector2 MoveInput { get; }
    
    /// <summary>공격 입력 (마우스 좌클릭 또는 버튼)</summary>
    bool AttackInput { get; }
    
    /// <summary>회피 입력 (스페이스바 또는 버튼)</summary>
    bool DodgeInput { get; }
    
    /// <summary>조준 입력 (게임패드 우 스틱)</summary>
    Vector2 LookInput { get; }
    
    /// <summary>마우스 스크린 위치</summary>
    Vector2 MousePosition { get; }
    
    /// <summary>프레임 종료 시 호출되어 일회성 입력 상태를 리셋</summary>
    void LateTick();
}
