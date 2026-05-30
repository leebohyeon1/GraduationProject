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
        // currentHealth가 0 이하가 되었을 때 발동
        if (currentHealth <= 0 && previousHealth > 0)
        {
            Debug.Log($"최후의 의지 작동! 현재 체력: {currentHealth}");

            // 1. 체력을 1로 복구 (현재 체력이 -5라면 +6을 해서 1로 만듦)
            int recoverAmount = 1 - currentHealth;
            _player.Health.ChangeHealth(recoverAmount);

            // 2. 무적 코루틴 시작
            if (_player.gameObject.activeInHierarchy)
            {
                _player.StartCoroutine(StartInvincibility());
            }

            // 3. 카운터 스택 초기화
            _player.Combat.ResetCounterStack();
        }
    }

    private IEnumerator StartInvincibility()
    {
        _player.Ability.AddTag(_invincibleTag); // 무적 태그 추가
        yield return new WaitForSeconds(_invincibleDuration); // 지속 시간 대기
        _player.Ability.RemoveTag(_invincibleTag.Id); // 무적 태그 제거
    }
}
