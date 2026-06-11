// --- FILE: CompositeNode.cs ---

using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;

// ???대옒?ㅻ뒗 吏곸젒 ?ъ슜?섏? ?딄퀬 Selector, Sequence??遺紐???븷???⑸땲??
namespace BehaviorTree
{
    public abstract class CompositeNode : Node
    {
        public Node[] nodes;
        //???몃뱶??遺李⑸맂 ?쒕퉬?ㅻ뒗 ?ㅽ뻾留?吏꾪뻾??
        public List<ServiceNode> services = new List<ServiceNode>();
        protected void UpdateServices()
        {
            if (services == null) return;

            for (int i = 0; i < services.Count; i++)
            {
                // ?쒕퉬?ㅼ쓽 Evaluate瑜??몄텧?섏뿬 OnEnter -> OnUpdate ?ъ씠?댁쓣 ?뚮┰?덈떎.
                // 諛섑솚媛?Success/Failure)? 臾댁떆?⑸땲?? (?먮쫫???곹뼢 X)
                if(services[i] != null)
                {
                services[i].Evaluate();

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
