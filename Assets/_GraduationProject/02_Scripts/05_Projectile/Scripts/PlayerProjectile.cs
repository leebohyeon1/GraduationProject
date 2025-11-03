using UnityEngine;

/// <summary>
/// 플레이어의 발사체 로직을 처리하는 클래스입니다.
/// </summary>
public class PlayerProjectile : Projectile
{
    /// <summary>
    /// 다른 오브젝트와 충돌했을 때 호출됩니다.
    /// </summary>
    protected override void OnTriggerEnter(Collider other)
    {
        // 발사체 소유자와 충돌한 경우 무시
        if (other.gameObject == p_owner) return;
        
        // 충돌한 오브젝트가 타겟 레이어에 속하는지 확인
        int layer = 1 << other.gameObject.layer;
        if ((p_targetLayerMask.value & layer) != 0)
        {
            // IDamageable 인터페이스를 구현하는지 확인
            if (other.TryGetComponent<IDamageable>(out var damageable) && !damageable.IsDead)
            {
                Attack(damageable); // 공격 실행
                
                // 원거리 공격 피격 이벤트 발생
                // p_owner.GetComponent<Player>().Events.TriggerRangedAttackAffect(other);

                // 충돌 시 파괴 옵션이 켜져 있으면 발사체 파괴
                if (p_destroyOnHit)
                {
                    DestroyProjectile();
                }
            }
        }
        // 타겟 레이어가 아니고, 소유자 레이어도 아닌 다른 오브젝트와 충돌 시 파괴
        else if (other.gameObject.layer != p_owner.layer)
        {
            DestroyProjectile();
        }
    }
}