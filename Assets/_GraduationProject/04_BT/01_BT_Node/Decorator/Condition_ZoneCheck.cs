using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BehaviorTree;

namespace BehaviorTree
{
    [CreateAssetMenu(fileName = "Condition_ZoneCheck", menuName = "BehaviorTree/Condition/ZoneCheck")]
    public class Condition_ZoneCheck : ConditionNode
    {
        [Header("Zone Settings")]
        [Tooltip("체크할 구역 ID 리스트입니다 (예: 1, 2, 3, 5)")]
        public List<int> targetZoneIds = new List<int>();

        protected override bool CheckCondition()
        {
            if (runner == null || runner.player == null) return false;

            // 플레이어에게 부착된 Tracker 컴포넌트를 가져옵니다.
            var tracker = runner.player.GetComponent<PlayerZoneTracker>();
            
            if (tracker == null)
            {
                // Tracker가 없다면 런타임에 추가하거나 경고를 띄울 수 있습니다.
                // 여기서는 일단 false를 반환합니다.
                return false;
            }

            // 현재 구역이 타겟 구역 리스트에 포함되어 있는지 확인합니다.
            // 플레이어가 밟고 있는 구역들 중 타겟 ID가 하나라도 포함되어 있는지 확인합니다.
            // BTDebug.Log(" targetZoneIds.Any(id => tracker.CurrentZoneId == id) :"+  targetZoneIds.Any(id => tracker.CurrentZoneId == id));
            return targetZoneIds.Any(id => tracker.CurrentZoneId == id);
        }

        public override Node Clone()
        {
            Condition_ZoneCheck newNode = Instantiate(this);
            newNode.targetZoneIds = new List<int>(targetZoneIds);
            return newNode;
        }
    }
}
