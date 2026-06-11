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
        [Tooltip("泥댄겕??援ъ뿭 ID 由ъ뒪?몄엯?덈떎 (?? 1, 2, 3, 5)")]
        public List<int> targetZoneIds = new List<int>();

        protected override bool CheckCondition()
        {
            if (runner == null || runner.player == null) return false;

            // ?뚮젅?댁뼱?먭쾶 遺李⑸맂 Tracker 而댄룷?뚰듃瑜?媛?몄샃?덈떎.
            var tracker = runner.player.GetComponent<PlayerZoneTracker>();
            
            if (tracker == null)
            {
                // Tracker媛 ?녿떎硫??고??꾩뿉 異붽??섍굅??寃쎄퀬瑜??꾩슱 ???덉뒿?덈떎.
                // ?ш린?쒕뒗 ?쇰떒 false瑜?諛섑솚?⑸땲??
                return false;
            }
            // Debug.Log($"Current Zone ID: {tracker.CurrentZoneId}, Target Zone IDs: {string.Join(", ", targetZoneIds)}");
            // ?꾩옱 援ъ뿭???寃?援ъ뿭 由ъ뒪?몄뿉 ?ы븿?섏뼱 ?덈뒗吏 ?뺤씤?⑸땲??
            // ?뚮젅?댁뼱媛 諛잕퀬 ?덈뒗 援ъ뿭??以??寃?ID媛 ?섎굹?쇰룄 ?ы븿?섏뼱 ?덈뒗吏 ?뺤씤?⑸땲??
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
