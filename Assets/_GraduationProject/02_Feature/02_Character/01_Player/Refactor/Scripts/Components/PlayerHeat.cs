using BH_Lib.Log;
using System;
using UnityEngine;

namespace player.Refactor
{
    public class PlayerHeat : HeatSystem
    {
        /// <summary>
        /// 열기 시스템 초기화
        /// </summary>
        /// <param name="sourceMapDatabaseSO">소스맵 데이터베이스</param>
        /// <param name="tierStatDatabaseSO">티어 스탯 데이터베이스</param>
        public void Initialize(SourceMapDatabaseSO sourceMapDatabaseSO, TierStatDatabaseSO tierStatDatabaseSO)
        {
            p_sourceMapDataBase = sourceMapDatabaseSO;
            p_tierStatDatabase = tierStatDatabaseSO;
        }

        /// <summary>
        /// 근접 공격 시 열기 증가 처리
        /// </summary>
        /// <param name="collider">타격 대상 콜라이더</param>
        public void IncreaseHeatOnAttack(Collider collider)
        {
            IHeatable heatable = collider.GetComponent<IHeatable>();

            if (heatable != null && !heatable.IsHeatLock)
            {
                SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnMeleeHit", heatable.ActorType, -1);
                int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;

                heatable.ChangeHeat(deltaHeat);

                Log.PrintColor(Color.red, $"대상: {collider.gameObject.name}, 열기 변화량: {deltaHeat}");
            }
        }

        /// <summary>
        /// 차지 게이지에 따른 열기 증가 처리
        /// </summary>
        /// <param name="sourceMap">소스맵 데이터</param>
        /// <param name="chargeGuage">차지 게이지 값</param>
        public void IncreaseHeatOnCharge(SourceMap sourceMap, float chargeGuage)
        {
            if (chargeGuage >= CurrentHeat)
            {
                SetHeat(Mathf.FloorToInt(chargeGuage));
            }
        }

        /// <summary>
        /// 차지 공격 시 열기 증가 처리
        /// </summary>
        /// <param name="collider">타격 대상 콜라이더</param>
        public void IncreaseHeatOnChargeAttack(Collider collider)
        {
            IHeatable heatable = collider.GetComponent<IHeatable>();
            if (heatable != null && !heatable.IsHeatLock)
            {
                SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnChargeAttack", heatable.ActorType, CurrentTier);
                int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
                heatable.ChangeHeat(deltaHeat);

                Log.PrintColor(Color.red, $"대상: {collider.gameObject.name}, 열기 변화량: {deltaHeat}");
            }
        }

        /// <summary>
        /// 원거리 공격 시 열기 감소 처리
        /// </summary>
        /// <param name="collider">타격 대상 콜라이더</param>
        public void DecreaseHeatOnRangeAttack(Collider collider)
        {
            IHeatable heatable = collider.GetComponent<IHeatable>();
            if (heatable != null)
            {
                SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnIceBallSuccess", heatable.ActorType, -1);
                int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;

                Log.PrintColor(Color.red, $"대상: {heatable.ActorType}, 열기 변화량: {deltaHeat}");
                heatable.ChangeHeat(deltaHeat);
            }
        }

        /// <summary>
        /// 패리 성공 시 열기 증가 처리
        /// </summary>
        public void IncreaseHeatOnParrySuccess()
        {
            SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnParrySuccess", -1);
            int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
            ChangeHeat(deltaHeat);
        }
    }

    /// <summary>
    /// 플레이어 열기 시스템을 관리하는 클래스
    /// </summary>
    public class PlayerHeatManager : IDisposable
    {
        private PlayerHeat _heat;
        private PlayerEvents _events;
        private bool _disposed = false; // 중복 Dispose 방지

        public PlayerHeatManager(PlayerHeat heat, PlayerEvents events)
        {
            _heat = heat;
            _events = events;

            // 이벤트 구독
            _events.OnAttackAffect += HandleAttackAffect;
            _heat.OnHeatChanged += HandleHeatChanged;
            _events.OnChargeAttackAffect += HandleChargeAttackAffect;
            _events.OnParryAffect += HandleParryAffect;
            _events.OnRangedAttackAffect += HandleRangedAttackAffect;
        }

        /// <summary>
        /// 리소스 정리 및 이벤트 구독 해제
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            // 이벤트 구독 해제
            _events.OnAttackAffect -= HandleAttackAffect;
            _heat.OnHeatChanged -= HandleHeatChanged;
            _events.OnChargeAttackAffect -= HandleChargeAttackAffect;
            _events.OnParryAffect -= HandleParryAffect;
            _events.OnRangedAttackAffect -= HandleRangedAttackAffect;

            _disposed = true;
        }

        /// <summary>
        /// 열기 변화 이벤트 처리
        /// </summary>
        /// <param name="previousHeat">이전 열기 값</param>
        /// <param name="currentHeat">현재 열기 값</param>
        private void HandleHeatChanged(int previousHeat, int currentHeat)
        {
            // 열기 티어 변경 여부 확인
            if (currentHeat > previousHeat)
            {
                _events.TriggerTierUp(_heat.CurrentTier);
            }
            else
            {
                _events.TriggerTierDown(_heat.CurrentTier);
            }
        }

        /// <summary>
        /// 근접 공격 시 열기 효과 처리
        /// </summary>
        /// <param name="collider">타격 대상 콜라이더</param>
        private void HandleAttackAffect(Collider collider)
        {
            _heat.IncreaseHeatOnAttack(collider);
        }

        /// <summary>
        /// 차지 공격 시 열기 효과 처리
        /// </summary>
        /// <param name="collider">타격 대상 콜라이더</param>
        private void HandleChargeAttackAffect(Collider collider)
        {
            _heat.IncreaseHeatOnChargeAttack(collider);
        }

        /// <summary>
        /// 패리 성공 시 열기 효과 처리
        /// </summary>
        /// <param name="collider">패리 대상 콜라이더</param>
        private void HandleParryAffect(Collider collider)
        {
            _heat.IncreaseHeatOnParrySuccess();
        }

        /// <summary>
        /// 원거리 공격 시 열기 효과 처리
        /// </summary>
        /// <param name="collider">타격 대상 콜라이더</param>
        private void HandleRangedAttackAffect(Collider collider)
        {
            _heat.DecreaseHeatOnRangeAttack(collider);
        }
    }
}