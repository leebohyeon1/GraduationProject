using UnityEngine;
using System.Collections.Generic;

namespace BehaviorTree
{
    [CreateAssetMenu(fileName = "WeightedRandomSelector", menuName = "BehaviorTree/WeightedRandomSelector")]
    public class WeightedRandomSelector : CompositeNode
    {
        [Tooltip("각 자식 노드가 선택될 확률을 결정하는 가중치 리스트입니다. 노드 수보다 가중치 수가 적으면 나머지 노드는 기본 가중치 1로 간주됩니다.")]
        public List<float> weights = new List<float>();

        [System.NonSerialized]
        private int _runningChildIndex = -1;

        protected override NodeState OnUpdate()
        {
            UpdateServices();

            if (_runningChildIndex == -1)
            {
                _runningChildIndex = PickChildIndex();
                if (_runningChildIndex == -1) 
                {
                    return NodeState.FAILURE;
                }
                
            }

            NodeState state = nodes[_runningChildIndex].Evaluate();
            
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

            for (int i = 0; i < nodeCount; i++)
            {
                float w = (weights != null && i < weights.Count) ? weights[i] : 1.0f;
                totalWeight += Mathf.Max(0, w); 
            }

            if (totalWeight <= 0) 
            {
                return Random.Range(0, nodeCount);
            }

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

            return nodeCount - 1; 
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
            if (nodes != null && weights != null)
            {
                while (weights.Count < nodes.Length) weights.Add(1.0f);
            }
        }
    }
}
