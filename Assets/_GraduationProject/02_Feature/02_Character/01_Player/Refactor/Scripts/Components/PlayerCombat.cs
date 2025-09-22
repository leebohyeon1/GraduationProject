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
        /// ���� ������ �߽��� ��ġ
        /// </summary>
        private Vector3 _combatCenter;

        private bool _canCounterAttack;

        private PlayerCombatData _combatData;


        /// <summary>
        /// ���߿� �����
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
        
        /// <summary>
        /// ���� ������ ����
        /// </summary>
        public void SetupCombatCenter()
        {
            _combatCenter = transform.position;
        }

        #region Attack
        /// <summary>
        /// ���� ������ �߽��� ���
        /// </summary>
        /// <returns>���� ���� �ڽ��� �߽� ��ġ</returns>
        private Vector3 GetAttackCenter(PlayerAttackData attackData)
        {
            return _combatCenter + transform.forward * (attackData.AttackRadius.z / 2);
        }

        /// <summary>
        /// ���� ���� ���� ���� (�Ϲ�/���� ���� �� �� ���)
        /// Physics.OverlapBox�� ����Ͽ� �ڽ� ������ ���� �������� ���� �����մϴ�.
        /// </summary>
        /// <param name="attackData"> ���� ������ </param>
        /// <returns>Ÿ���� ������Ʈ��</returns>
        public Collider[] ExecuteAttack(PlayerAttackData attackData)
        {
            // ���� �߽����� ���� ����
            Vector3 attackCenter = GetAttackCenter(attackData);
            Vector3 halfExtents = attackData.AttackRadius / 2f;  // OverlapBox�� halfExtents�� ���

            Collider[] hitEnemies = Physics.OverlapBox(attackCenter, halfExtents, transform.rotation, _combatData.AttackLayerMask);

            Log.Print(hitEnemies.Length);   
            ProcessHitEnemies(attackData, hitEnemies);

            return hitEnemies;
        }

        /// <summary>
        /// ���� ���� �� ������ ���鿡�� ���� ����
        /// </summary>
        /// <param name="hitObjects">������ ������ Collider �迭</param>
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

        #region Parry
        /// <summary>
        /// �и� ���� ����
        /// </summary>
        /// <param name="parryRadius">�и� ����</param>
        /// <returns>�и��� ������Ʈ��</returns>
        public Collider[] ExcuteParry(Vector3 parryRadius)
        {
            // ���� �߽����� ���� ����
            Vector3 attackCenter = _combatCenter + transform.forward * (parryRadius.z / 2);
            Vector3 halfExtents = parryRadius / 2f;  // OverlapBox�� halfExtents�� ���

            Collider[] hitEnemies = Physics.OverlapBox(attackCenter, halfExtents, transform.rotation, _combatData.AttackLayerMask);

            ProcessParryEnemies(hitEnemies);

            return hitEnemies;
        }

        /// <summary>
        /// �и� ���� �� ������ ���鿡�� �и� ����
        /// </summary>
        /// <param name="hitObjects">������ ������ Collider �迭</param>
        private void ProcessParryEnemies( Collider[] hitObjects)
        {
            foreach (Collider obj in hitObjects)
            {
                IParryable parryable = obj.GetComponent<IParryable>();
                if (parryable != null && parryable.IsParryable)
                {
                    parryable.Parry(gameObject);
                }
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

            DrawActionGizmo(_combatData.AttackDatas[0].AttackRadius, Color.mediumVioletRed);
            DrawActionGizmo(_combatData.AttackDatas[1].AttackRadius, Color.orangeRed);
            DrawActionGizmo(_combatData.AttackDatas[2].AttackRadius, Color.darkRed);
            DrawActionGizmo(_combatData.ChargeAttackData.AttackRadius, Color.indianRed);
            DrawActionGizmo(_combatData.ParryRadius, Color.green);

        }

        private void DrawActionGizmo(Vector3 Radius, Color color)
        {

            Vector3 attackCenter = transform.position + transform.forward * (Radius.z / 2);
            Gizmos.color = color;
            Gizmos.matrix = Matrix4x4.TRS(attackCenter, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, Radius);
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
