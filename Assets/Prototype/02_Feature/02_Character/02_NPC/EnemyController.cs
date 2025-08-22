using BH_Lib.DI;
using UnityEngine;

/// <summary>
/// 적 캐릭터의 AI 로직을 담당하는 컨트롤러
/// </summary>
[RequireComponent(typeof(IMovable), typeof(IAttacker))]
public class EnemyController : CharacterBase
{
    private IMovable _movable;
    private IAttacker _attacker;

    // AI가 제어할 변수들
    [Inject] private PlayerController _player;

    protected override void Awake()
    {
        _movable = GetComponent<IMovable>();
        _attacker = GetComponent<IAttacker>();
    }

    private void Update()
    {

    }
}
