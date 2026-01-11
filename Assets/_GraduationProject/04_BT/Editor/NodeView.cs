using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using BehaviorTree;

namespace BehaviorTree.Editor
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Action<NodeView> OnNodeSelected;
        public Node node;
        public Port input;
        public Port output;

        public NodeView(Node node) : base("Assets/_GraduationProject/04_BT/Editor/NodeView.uxml")
        {
            this.node = node;
            this.title = node.name;
            this.viewDataKey = node.guid;

            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
        }

        private void SetupClasses()
        {
            if (node is CompositeNode)
            {
                AddToClassList("composite");
            }
            else if (node is Decorator_Inverter)
            {
                AddToClassList("decorator");
            }
            else
            {
                AddToClassList("action");
            }
        }

        private void CreateInputPorts()
        {
            // All nodes get an input port
            input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
            if (input != null)
            {
                input.portName = "";
                inputContainer.Add(input);
            }
        }

        private void CreateOutputPorts()
        {
            if (node is CompositeNode)
            {
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            }
            else if (HasChildField(node)) 
            {
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
            }

            if (output != null)
            {
                output.portName = "";
                outputContainer.Add(output);
            }
        }

        private bool HasChildField(Node node)
        {
            // Decorator_Inverter has 'child' field
            return node is Decorator_Inverter;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(node, "Behavior Tree (Set Position)");
            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;
            EditorUtility.SetDirty(node);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if (OnNodeSelected != null)
            {
                OnNodeSelected.Invoke(this);
            }
        }

        public void SortChildren()
        {
            if (node is CompositeNode composite)
            {
                if (composite.nodes != null)
                {
                    composite.nodes = composite.nodes.OrderBy(c => c.position.x).ToArray();
                }
            }
        }

        public void UpdateState()
        {
            RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");
        }
    }
}
