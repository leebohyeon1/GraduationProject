// --- FILE: CompositeNode.cs ---

using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;

// 이 클래스는 직접 사용되지 않고 Selector, Sequence의 부모 역할을 합니다.
namespace BehaviorTree
{
    public abstract class CompositeNode : Node
    {
        public Node[] nodes;
        //이 노드에 부착된 서비스는 실행만 진행함
        public List<ServiceNode> services = new List<ServiceNode>();
        protected void UpdateServices()
        {
            if (services == null) return;

            for (int i = 0; i < services.Count; i++)
            {
                // 서비스의 Evaluate를 호출하여 OnEnter -> OnUpdate 사이클을 돌립니다.
                // 반환값(Success/Failure)은 무시합니다. (흐름에 영향 X)
                if(services[i] != null)
                {
                services[i].Evaluate();
            // // // BTDebug.Log($"runner: {runner.name}, Node: {this.name}, State: <color=green>{services[i].name}</color>");

                }
            }
        }
        public override Node Clone()
        {
            CompositeNode newNode = Instantiate(this);
            newNode.nodes = new Node[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] != null) newNode.nodes[i] = nodes[i].Clone();
            }
            newNode.services = new List<ServiceNode>();
            if (services != null)
            {
                foreach(var service in services)
                {
                    if(service != null)
                    {
                    newNode.services.Add((ServiceNode)service.Clone());
                        
                    }
                }
            }
                return newNode;
        }

        public override void initNode()
        {
            base.initNode();
            foreach (var node in nodes)
            {
                node.initNode();
            }
            foreach (var service in services)
            {
                if(service != null)
                {
                service.initNode();
                    
                }
            }
        }

        public override void SetRunner(Enemy runner, AiBrain brain)
        {
            base.SetRunner(runner, brain);
            if (nodes != null)
            {
            foreach (var node in nodes)
            {
                node.SetRunner(runner, brain);
            }
            }
            if(services != null)
            {
                foreach (var service in services)
                {
                    if(service != null)
                    {
                    service.SetRunner(runner, brain);
                        
                    }
                }
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            foreach (var service in services)
            {
                if(service != null)
                service.Abort();
            }
        }
    }
}