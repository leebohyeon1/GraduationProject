using BH_Lib.Log;
using Pathfinding.Drawing;
using System;
using UnityEngine;

namespace player.Refactor
{
    public class PlayerCombat : MonoBehaviour, IAttacker
    {
        #region Private Fields

        /// <summary>
        /// 공격 범위의 중심점 위치
        /// </summary>
        private Vector3 _attackCenter;

        private bool _canCounterAttack;

        private PlayerCombatData _combatData;


        /// <summary>
        /// 나중에 지우기
        /// </summary>
        private bool _isDrawGizmos = false;
        #endregion

        #region Properties
        public bool CanCounterAttack => _canCounterAttack;


        #endregion

        public void Initialize(PlayerCombatData combatData)
        {
            _isDrawGizmos = true;
            _combatData = combatData;
        }


        #region Attack
        /// <summary>
        /// 공격 시작점 설정
        /// </summary>
        public void SetupAttackCenter()
        {
            _attackCenter = transform.position;
        }

        /// <summary>
        /// 공격 범위의 중심점 계산
        /// </summary>
        /// <returns>공격 범위 박스의 중심 위치</returns>
        private Vector3 GetAttackCenter(PlayerAttackData attackData)
        {
            return _attackCenter + transform.forward * (attackData.AttackRadius.z / 2);
        }

        /// <summary>
        /// 공통 공격 실행 로직 (일반/차지 공격 둘 다 사용)
        /// Physics.OverlapBox를 사용하여 박스 형태의 공격 범위에서 적을 감지합니다.
        /// </summary>
        public Collider[] ExecuteAttack(PlayerAttackData attackData)
        {
            // 공격 중심점과 범위 설정
            Vector3 attackCenter = GetAttackCenter(attackData);
            Vector3 halfExtents = attackData.AttackRadius / 2f;  // OverlapBox는 halfExtents를 사용

            Collider[] hitEnemies = Physics.OverlapBox(attackCenter, halfExtents, transform.rotation, _combatData.AttackLayerMask);

            Log.Print(hitEnemies.Length);   
            ProcessHitEnemies(attackData, hitEnemies);

            return hitEnemies;
        }

        /// <summary>
        /// 공격 범위 내 감지된 적들에게 피해 적용
        /// </summary>
        /// <param name="hitObjects">감지된 적들의 Collider 배열</param>
        private void ProcessHitEnemies(PlayerAttackData attackData, Collider[] hitObjects)
        {
            foreach (Collider obj in hitObjects)
            {
                IDamageable damageable = obj.GetComponent<IDamageable>();
                if (damageable != null && !damageable.IsDead)
                {
                    damageable.TakeDamage(attackData.AttackDamage, this);
                }
            }
        }
        #endregion

        #region RangedAttack
        public void FireProjectile(Transform firePoint)
        {
            if(_combatData.RangedAttackData.ProjectilePrefab == null)
            {
                return;
            }

            GameObject projectileObj = Instantiate(_combatData.RangedAttackData.ProjectilePrefab,
                firePoint.position, firePoint.rotation);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(_combatData.RangedAttackData.AttackDamage, 
                    _combatData.RangedAttackData.ProjectileSpeed, gameObject, _combatData.AttackLayerMask);
            }
        }
        #endregion

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if(!_isDrawGizmos)
            {
                return;
            }

            DrawAttackGizmo();
            DrawChargeAttackGizmo();
        }

        private void DrawAttackGizmo()
        {

            Vector3 attackCenter = transform.position + transform.forward * (_combatData.AttackDatas[0].AttackRadius.z / 2);
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(attackCenter, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, _combatData.AttackDatas[0].AttackRadius);
            Gizmos.matrix = Matrix4x4.identity;
        }

        private void DrawChargeAttackGizmo()
        {

            Vector3 attackCenter = transform.position + transform.forward * (_combatData.ChargeAttackData.AttackRadius.z / 2);
            Gizmos.color = Color.darkRed;
            Gizmos.matrix = Matrix4x4.TRS(attackCenter, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, _combatData.ChargeAttackData.AttackRadius);
            Gizmos.matrix = Matrix4x4.identity;
        }
#endif
    }

    public class PlayerCombatManager : IDisposable
    {
        [SerializeField] private PlayerCombat _combat;
        [SerializeField] private PlayerEvents _events;

        public PlayerCombatManager(PlayerCombat combat, PlayerEvents events) 
        {
            _combat = combat;
            _events = events;

            _events.OnRangedAttackStart += HandleRangedAttack;
        }

        public void Dispose()
        {
            _events.OnRangedAttackStart -= HandleRangedAttack;
        }

        private void HandleRangedAttack(Transform firePoint)
        {
            _combat.FireProjectile(firePoint);
        }
    }
}
