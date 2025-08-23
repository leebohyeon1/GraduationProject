using BH_Lib.DI;
using UnityEngine;

/// <summary>
/// 적 캐릭터의 기본 클래스
/// </summary>
public class Enemy : CharacterBase
{
    [Inject] private Player _player;

    protected override void Awake()
    {

    }

    private void Update()
    {

    }
}
