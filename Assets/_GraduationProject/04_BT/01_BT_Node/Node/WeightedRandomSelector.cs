using UnityEngine;
using System.Collections.Generic;

namespace BehaviorTree
{
    [CreateAssetMenu(fileName = "WeightedRandomSelector", menuName = "BehaviorTree/WeightedRandomSelector")]
    public class WeightedRandomSelector : CompositeNode
    {
        [Tooltip("媛??먯떇 ?몃뱶????묓븯??媛以묒튂 由ъ뒪?몄엯?덈떎. (?? 20, 25, 30...)")]
        public List<float> weights = new List<float>();

        [System.NonSerialized]
        private int _runningChildIndex = -1;

        protected override NodeState OnUpdate()
        {
            UpdateServices();

            // ?꾩옱 ?ㅽ뻾 以묒씤 ?먯떇???녿떎硫??뺣쪧???곕씪 ?섎굹瑜??좏깮?⑸땲??
            if (_runningChildIndex == -1)
            {
                _runningChildIndex = PickChildIndex();
                if (_runningChildIndex == -1) 
                {
                    // // Debug.LogWarning($"[WeightedRandomSelector : {this.name}] ?ㅽ뻾 媛?ν븳 ?먯떇 ?몃뱶媛 ?놁뒿?덈떎.");
                    return NodeState.FAILURE;
                }
                
            }

            // ?좏깮???먯떇 ?몃뱶 ?ㅽ뻾
            NodeState state = nodes[_runningChildIndex].Evaluate();
            
            // ?곕????곹깭(SUCCESS/FAILURE)???꾨떖?섎㈃ ?몃뜳?ㅻ? 珥덇린?뷀븯???ㅼ쓬 ?됯? ???덈줈 ?좏깮?섎룄濡??⑸땲??
            if (state != NodeState.RUNNING)
            {
                _runningChildIndex = -1;
            }
            
            return state;
        }

        private int PickChildIndex()
        {
            if (nodes == null || nodes.Length == 0) return -1;

            float totalWeight = 0;
            int nodeCount = nodes.Length;

            // 1. ?꾩껜 媛以묒튂 ??怨꾩궛 (媛以묒튂 由ъ뒪?멸? ?몃뱶 ?섎낫???곸쑝硫??섎㉧吏??湲곕낯媛?1.0 ?ъ슜)
            for (int i = 0; i < nodeCount; i++)
            {
                float w = (weights != null && i < weights.Count) ? weights[i] : 1.0f;
                totalWeight += Mathf.Max(0, w); // ?뚯닔 媛以묒튂??0?쇰줈 泥섎━
            }

            if (totalWeight <= 0) 
            {
                // 紐⑤뱺 媛以묒튂媛 0??寃쎌슦 洹좊벑 ?뺣쪧濡??좏깮
                return Random.Range(0, nodeCount);
            }

            // 2. 媛以묒튂???곕Ⅸ ?쒕뜡 ?좏깮 (Cumulative Sum Algorithm)
            float randomValue = Random.Range(0f, totalWeight);
            float currentSum = 0;

            for (int i = 0; i < nodeCount; i++)
            {
                float w = (weights != null && i < weights.Count) ? weights[i] : 1.0f;
                currentSum += Mathf.Max(0, w);
                
                if (randomValue <= currentSum)
                {
                    return i;
                }
            }

            return nodeCount - 1; // 遺?숈냼?섏젏 ?ㅼ감 ?鍮?留덉?留??몃뜳??諛섑솚
        }

        public override void initNode()
        {
            base.initNode();
            _runningChildIndex = -1;
        }

        public override void Abort()
        {
            if (isEntered)
            {
                if (_runningChildIndex != -1)
                {
                    nodes[_runningChildIndex].Abort();
                    _runningChildIndex = -1;
                }
                base.Abort();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            // [?듭떖 ?섏젙] ?몃뱶媛 醫낅즺(Success/Failure)?섎㈃ ?몃뜳?ㅻ? 由ъ뀑?섏뿬 		
            // ?ㅼ쓬???ㅼ떆 ?ㅼ뼱?????덈줈???뺣쪧 寃?щ? ?섑뻾?섎룄濡??⑸땲??
            _runningChildIndex = -1;
        }

        public override Node Clone()
        {
            WeightedRandomSelector newNode = (WeightedRandomSelector)base.Clone();
            newNode.weights = new List<float>(weights);
            return newNode;
        }

        private void OnValidate()
        {
            // ?몄뒪?숉꽣?먯꽌 ?몃뱶 ?섏? 媛以묒튂 ???숆린??(?몄쓽 湲곕뒫)
            if (nodes != null && weights != null)
            {
                while (weights.Count < nodes.Length) weights.Add(1.0f);
                // while (weights.Count > nodes.Length) weights.RemoveAt(weights.Count - 1);
            }
        }
    }
}
