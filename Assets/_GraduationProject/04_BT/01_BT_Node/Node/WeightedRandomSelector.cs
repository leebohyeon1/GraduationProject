using UnityEngine;
using System.Collections.Generic;

namespace BehaviorTree
{
    [CreateAssetMenu(fileName = "WeightedRandomSelector", menuName = "BehaviorTree/WeightedRandomSelector")]
    public class WeightedRandomSelector : CompositeNode
    {
        [Tooltip("각 자식 노드에 대응하는 가중치 리스트입니다. (예: 20, 25, 30...)")]
        public List<float> weights = new List<float>();

        [System.NonSerialized]
        private int _runningChildIndex = -1;

        protected override NodeState OnUpdate()
        {
            UpdateServices();

            // 현재 실행 중인 자식이 없다면 확률에 따라 하나를 선택합니다.
            if (_runningChildIndex == -1)
            {
                _runningChildIndex = PickChildIndex();
                if (_runningChildIndex == -1) 
                {
                    Debug.LogWarning($"[WeightedRandomSelector : {this.name}] 실행 가능한 자식 노드가 없습니다.");
                    return NodeState.FAILURE;
                }
                
                // Debug.Log($"[WeightedRandomSelector : {this.name}] {nodes[_runningChildIndex].name} 선택됨 (인덱스: {_runningChildIndex})");
            }

            // 선택된 자식 노드 실행
            NodeState state = nodes[_runningChildIndex].Evaluate();
            
            // 터미널 상태(SUCCESS/FAILURE)에 도달하면 인덱스를 초기화하여 다음 평가 때 새로 선택하도록 합니다.
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

            // 1. 전체 가중치 합 계산 (가중치 리스트가 노드 수보다 적으면 나머지는 기본값 1.0 사용)
            for (int i = 0; i < nodeCount; i++)
            {
                float w = (weights != null && i < weights.Count) ? weights[i] : 1.0f;
                totalWeight += Mathf.Max(0, w); // 음수 가중치는 0으로 처리
            }

            if (totalWeight <= 0) 
            {
                // 모든 가중치가 0인 경우 균등 확률로 선택
                return Random.Range(0, nodeCount);
            }

            // 2. 가중치에 따른 랜덤 선택 (Cumulative Sum Algorithm)
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

            return nodeCount - 1; // 부동소수점 오차 대비 마지막 인덱스 반환
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

        public override Node Clone()
        {
            WeightedRandomSelector newNode = (WeightedRandomSelector)base.Clone();
            // 가중치 리스트 깊은 복사
            newNode.weights = new List<float>(weights);
            return newNode;
        }

        private void OnValidate()
        {
            // 인스펙터에서 노드 수와 가중치 수 동기화 (편의 기능)
            if (nodes != null && weights != null)
            {
                while (weights.Count < nodes.Length) weights.Add(1.0f);
                // while (weights.Count > nodes.Length) weights.RemoveAt(weights.Count - 1);
            }
        }
    }
}
