using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 추가

public class BeamDamager : MonoBehaviour
{
    public int _tickDamage = 8; // 틱당 데미지
    public float _tickInterval = 0.2f; // 데미지 간격

    // 이미 데미지를 입은 캐릭터를 기록하여 중복 데미지를 방지
    // private List<CharacterBase> _hitCharacters = new List<CharacterBase>();
    private float _nextTickTime;
    //private IAttacker _attacker; // 누가 이 공격을 했는지 (Enemy)

    // 이 빔을 발사한 공격자(Enemy)를 설정하는 함수
    public void Initialize()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (Time.time < _nextTickTime)
        {
            return;
        }

        //if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
        //{
        //    CharacterBase character = damageable as CharacterBase;
        //    // 이 캐릭터가 이미 데미지를 입지 않았다면
        //    if (!_hitCharacters.Contains(character))
        //    {
        //        // // 데미지를 주고, 리스트에 추가합니다.
        //        // damageable.TakeDamage(_tickDamage, );
        //        // _hitCharacters.Add(character);

        //        // // 다음 틱 시간을 현재 시간 + 간격으로 설정
        //        // _nextTickTime = Time.time + _tickInterval;
        //    }
        //}
    }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.TryGetComponent<Character>(out Character character))
    //     {
    //         // 데미지 기록 리스트에서 제거
    //         if (_hitCharacters.Contains(character))
    //         {
    //             _hitCharacters.Remove(character);
    //         }
    //     }
    // }
}