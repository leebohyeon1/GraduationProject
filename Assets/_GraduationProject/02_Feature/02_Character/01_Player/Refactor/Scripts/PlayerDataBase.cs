using System;
using System.Collections.Generic;
using UnityEngine;

namespace player.Refactor
{
    /// <summary>
    /// 런타임 데이터를 담을 플레이어 데이터 클래스
    /// </summary>
    [Serializable]
    public class PlayerRuntimeData
    {
        [Header("Stats")]
        public int MaxHealth;
        public int MaxMana;

        [Header("Movement")]
        public LayerMask GroundLayerMask = 1 << 3;
        public float MoveSpeed;
        public float RotateSpeed;
        public float Gravity = -9.81f;
        public float GroundCheckDistance = 0.1f;

        [Header("Combat")]
        public PlayerCombatData CombatData;

        public void Initialize(PlayerBaseDatasSO baseData)
        {
            MaxHealth = baseData.MaxHealth;
            MaxMana = baseData.MaxMana;
            MoveSpeed = baseData.MoveSpeed;
            RotateSpeed = baseData.RotateSpeed;
            CombatData = baseData.CombatData;
        }
    }

    /// <summary>
    /// 플레이어의 데이터를 관리하는 Glue 컴포넌트
    /// </summary>
    public class PlayerDataBase : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private PlayerBaseDatasSO _baseDatasSO;
        [SerializeField] private TierStatDatabaseSO _tierStatDatabaseSO;
        [SerializeField] private SourceMapDatabaseSO _sourceMapDatabaseSO;

   
        private PlayerRuntimeData _runtimeData;
        #endregion

        #region Properties
        public PlayerRuntimeData RuntimeData => _runtimeData;
        public TierStatDatabaseSO TierStatData => _tierStatDatabaseSO;
        public SourceMapDatabaseSO SourceMapData => _sourceMapDatabaseSO;
        #endregion

        public void Initialize()
        {
            _runtimeData = new PlayerRuntimeData();
            _runtimeData.Initialize(_baseDatasSO);
        }
    }

    /// <summary>
    /// 플레이어 전투관련한 데이터
    /// </summary>
    [Serializable]
    public class PlayerCombatData
    {
        // Todo: 구르기, 방어, 패링, 공격, 피격 등등 전투 관련 데이터 추가   
        [Header("Dodge")]
        public float DodgeSpeed = 8f;
        public float DodgeCooldown = 2f;

        [Header("Damaged")]
        public float DefendDamageReductionRate = 0.7f;
        public float HitStunDuration = 0.1f;

        [Header("Attack")]
        public LayerMask AttackLayerMask = 1 << 8;
        public PlayerAttackData[] AttackDatas;
        public float LastAttackDelay = 0.3f;

        [Header("ChargeAttack")]
        public PlayerAttackData ChargeAttackData;

        [Header("RangedAttack")]
        public RangedAttackData RangedAttackData;

        [Header("Parry")]
        public Vector3 ParryRadius;
    }

    /// <summary>
    /// 플레이어 근접 공격 관련 데이터
    /// 공격 시 전진 이동, 공격력, 범위 등을 정의
    /// </summary>
    [Serializable]
    public class PlayerAttackData
    {
        [Header("Attack Movement")]
        [Tooltip("공격 시 전진할 거리")]
        public float AttackMoveDistance = 2f;

        [Tooltip("전진 이동 지속 시간")]
        public float AttackMoveDuration = 0.3f;

        [Tooltip("전진 이동 애니메이션 곡선")]
        public AnimationCurve AttackMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Attack Stats")]
        [Tooltip("공격 데미지")]
        public int AttackDamage = 10;

        [Tooltip("공격 범위 반지름")]
        public Vector3 AttackRadius = Vector3.one;

        [Header("Attack Timing")]
        [Tooltip("공격후 딜레이 시간")]
        public float AttackDelay = 0.2f;
    }

    /// <summary>
    /// 플레이어 원거리 공격 관련 데이터
    /// 차징 시간, 투사체 속도, 데미지 등을 정의
    /// </summary>
    [Serializable]
    public class RangedAttackData
    {
        [Header("Charge Stats")]
        [Tooltip("원거리 공격 차징 시간")]
        public float ChargeTime;

        [Header("Attack Stats")]
        [Tooltip("원거리 공격 데미지")]
        public int AttackDamage = 10;

        [Header("Projectile")]
        public GameObject ProjectilePrefab;
        [Tooltip("투사체 이동 속도")]
        public float ProjectileSpeed;
        [Tooltip("투사체 이동 애니메이션 곡선 (현재 미사용)")]
        public AnimationCurve ProjectileMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Attack Timing")]
        public float AttackDelay = 0.2f;
    }
}
