using System;
using BH_Lib.DI;
using UnityEngine;

/// <summary>
/// 모든 캐릭터의 기본 클래스
/// IDamageable 인터페이스를 구현하여 체력과 피해 시스템을 제공
/// </summary>
public class CharacterBase : DIMonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
    }
}
