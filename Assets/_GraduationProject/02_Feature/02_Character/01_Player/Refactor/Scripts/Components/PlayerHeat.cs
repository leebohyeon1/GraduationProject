using BH_Lib.Log;
using System;
using UnityEngine;

namespace player.Refactor
{
    public class PlayerHeat : HeatSystem
    {
        public void Initialize(SourceMapDatabaseSO sourceMapDatabaseSO, TierStatDatabaseSO tierStatDatabaseSO)
        {
            p_sourceMapDataBase = sourceMapDatabaseSO;
            p_tierStatDatabase = tierStatDatabaseSO;
        }

        public void IncreaseHeatOnCharge(SourceMap sourceMap , float chargeGuage) 
        {
            if (chargeGuage >= CurrentHeat)
            {
                SetHeat(Mathf.FloorToInt(chargeGuage)); ;
            }
        }

        public void IncreaseHeatOnAttack(Collider collider)
        {
            IHeatable heatable = collider.GetComponent<IHeatable>();

            if (heatable != null && !heatable.IsHeatLock)
            {
                SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnMeleeHit", heatable.ActorType, -1);
                int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;

                heatable.ChangeHeat(deltaHeat);

                Log.PrintColor(Color.red, $"target: {collider.gameObject.name}, 열기 변화량: {deltaHeat}");
            }
        }

        public void IncreaseHeatOnChargeAttack(Collider collider)
        {
            IHeatable heatable = collider.GetComponent<IHeatable>();
            if (heatable != null && !heatable.IsHeatLock)
            {
                SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnChargeAttack", heatable.ActorType, CurrentTier);
                int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
                heatable.ChangeHeat(deltaHeat);

                Log.PrintColor(Color.red, $"target: {collider.gameObject.name}, 열기 변화량: {deltaHeat}");
            }
        }

        public void DecreaseHeatOnRangeAttack(Collider collider)
        {
            IHeatable heatable = collider.GetComponent<IHeatable>();
            if (heatable != null)
            {
                SourceMap sourceMap;
                sourceMap = p_sourceMapDataBase.GetSourceMap("OnIceBallSuccess", heatable.ActorType, -1);

                int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;

                Log.PrintColor(Color.red, $"target: {heatable.ActorType}, 열기 변화량: {deltaHeat}");
                heatable.ChangeHeat(deltaHeat);
            }
        }

        public void IncreaseHeatOnParrySuccess()
        {
            SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnParrySuccess", -1);
            int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
            ChangeHeat(deltaHeat);
        }
    }

    /// <summary>
    /// 플레이어 열기를 관리하는 클래스
    /// </summary>
    public class PlayerHeatManager: IDisposable
    {
        private PlayerHeat _heat;
        private PlayerEvents _events;
        private bool _disposed = false; // 중복 호출 방지

        public PlayerHeatManager(PlayerHeat heat, PlayerEvents events)
        {
            _heat = heat;
            _events = events;

            _heat.OnHeatChanged += HandleHeatChanged;
            _events.OnAttackAffect += HandleAttackAffect;
            _events.OnChargeAttackAffect += HandleChargeAttackAffect;
            _events.OnParryAffect += HandleParryAffect;
            _events.OnRangedAttackAffect += HandleRangedAttackAffect;
        }

        // IDisposable 인터페이스의 Dispose 메서드
        public void Dispose()
        {
            // 이미 Dispose가 호출되었다면 아무것도 하지 않음
            if (_disposed) return;

            // 이벤트 구독 해제
            _heat.OnHeatChanged -= HandleHeatChanged;
            _events.OnAttackAffect -= HandleAttackAffect;
            _events.OnChargeAttackAffect -= HandleChargeAttackAffect;
            _events.OnParryAffect -= HandleParryAffect;
            _events.OnRangedAttackAffect -= HandleRangedAttackAffect;

            _disposed = true;
        }

        /// <summary>
        /// 열기 관련 이벤트 핸들 함수
        /// </summary>
        /// <param name="previousHeat">이전 열기 티어</param>
        /// <param name="currentHeat">현재 열기 티어</param>
        private void HandleHeatChanged(int previousHeat, int currentHeat)
        {
            // 열기 티어가 올라갔는지 내려갔는지 확인           
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
        /// 근접 공격 시 열기 효과 핸들 함수
        /// </summary>
        /// <param name="collider">효과 대상</param>
        private void HandleAttackAffect(Collider collider)
        {
            _heat.IncreaseHeatOnAttack(collider);
        }

        /// <summary>
        /// 차징 공격 시 열기 효과 핸들 함수
        /// </summary>
        /// <param name="collider">효과 대상</param>
        private void HandleChargeAttackAffect(Collider collider)
        {
            _heat.IncreaseHeatOnChargeAttack(collider);
        }

        /// <summary>
        /// 패링 성공 시 효과 핸들 함수
        /// </summary>
        /// <param name="collider">패링한 오브젝트</param>
        private void HandleParryAffect(Collider collider) 
        {
            _heat.IncreaseHeatOnParrySuccess();
        }

        private void HandleRangedAttackAffect(Collider collider) 
        {
            _heat.DecreaseHeatOnRangeAttack(collider);
        }
    }
}