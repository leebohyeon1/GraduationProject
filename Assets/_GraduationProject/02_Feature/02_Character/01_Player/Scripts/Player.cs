using BH_Lib.DI;
using UnityEngine;

/// <summary>
/// 플레이어 캐릭터의 메인 클래스
/// CharacterBase를 상속받아 기본 체력 시스템을 구현하고
/// 각 기능 모듈들을 연결하는 역할
/// </summary>

[Register(LifetimeScope.Transient)]
public class Player : CharacterBase
{
    [Header("Player Components")]
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerAttack _playerAttack;

    protected override void Awake()
    {
        base.Awake();

        // PlayerStats가 설정되지 않았다면 CharacterStats를 PlayerStats로 캐스팅 시도
        if (_stats == null && base._stats is PlayerStats playerStats)
        {
            _stats = playerStats;
        }

        // 컴포넌트 자동 할당
        if (_playerMovement == null)
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        if (_playerController == null)
        {
            _playerController = GetComponent<PlayerController>();
        }

        if (_playerAttack == null)
        {
            _playerAttack = GetComponent<PlayerAttack>();
        }
    }

    protected override void Die()
    {
        base.Die();

        // 플레이어 사망 시 추가 로직
        Debug.Log("플레이어가 사망했습니다!");

        // 각 시스템 비활성화
        if (_playerController != null)
            _playerController.enabled = false;

        if (_playerMovement != null)
            _playerMovement.enabled = false;

        if (_playerAttack != null)
            _playerAttack.SetAttackEnabled(false);
    }

    public void TryAttack()
    {
        if (IsDead || _playerAttack == null) return;

        _playerAttack.TryAttack();
    }

    // 공개 프로퍼티들
    public PlayerStats PlayerStats => _stats as PlayerStats;
    public PlayerMovement PlayerMovement => _playerMovement;
    public PlayerController PlayerController => _playerController;
    public PlayerAttack PlayerAttack => _playerAttack;
    public bool IsAlive => !IsDead;
}
