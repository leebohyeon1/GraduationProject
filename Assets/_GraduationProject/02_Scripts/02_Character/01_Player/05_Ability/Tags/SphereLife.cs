using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SphereLife", menuName = "Project/Player/Ability/Tag/SphereLife")]
public class SphereLife : PlayerAbilityTagSO
{
    private PlayerController _player;

    public float _invincibleDuration = 2f; // 지속 시간 (초)
    public PlayerAbilityTagSO _invincibleTag;

    public override void Apply(PlayerController player)
    {
        _player = player;
        player.Health.OnHealthChanged += OnHealthChanged;
    }

    public override void Revert(PlayerController player)
    {
        player.Health.OnHealthChanged -= OnHealthChanged;
        _player = null;
    }

    private void OnHealthChanged(int previousHealth, int currentHealth)
    {
        if (currentHealth <= 0 && previousHealth > 0)
        {
            // 플레이어가 사망하기 직전에 체력이 0 이하로 떨어지는 경우
            // 사망을 막고 체력을 1로 설정

            _player.Health.ChangeHealth(1 - currentHealth); // 현재 체력을 1로 만들기 위해 필요한 회복량 계산
            Debug.Log("최후의 의지 작동");

            _player.StartCoroutine(StartInvincibility());
            _player.Combat.ResetCounterStack(); // 카운터 스택 초기화
        }
    }

    private IEnumerator StartInvincibility()
    {
        _player.Ability.AddTag(_invincibleTag); // 무적 태그 추가
        yield return new WaitForSeconds(_invincibleDuration); // 지속 시간 대기
        _player.Ability.RemoveTag(_invincibleTag.Id); // 무적 태그 제거
    }
}
