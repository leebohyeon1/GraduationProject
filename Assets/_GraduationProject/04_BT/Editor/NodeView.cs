using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using BehaviorTree;
using System.Reflection;

namespace BehaviorTree.Editor
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Action<NodeView> OnNodeSelected;
        public BehaviorTree.Node node;
        public Port input;
        public Port output;
        
        private BehaviorTreeView treeView;

        public NodeView(BehaviorTree.Node node, BehaviorTreeView treeView) : base("Assets/_GraduationProject/04_BT/Editor/NodeView.uxml")
        {
            this.node = node;
            this.treeView = treeView;
            this.title = node.name;
            this.viewDataKey = node.guid;

            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            
            capabilities |= Capabilities.Snappable | Capabilities.Movable | Capabilities.Deletable;
        }

        private void SetupClasses()
        {
            if (node is ActionNode)
            {
                AddToClassList("action");
            }
            else if (node is ConditionNode) 
            {
                AddToClassList("condition");
            }
            else if (node is CompositeNode)
            {
                AddToClassList("composite");
            }
            else if (HasChildField(node))
            {
                AddToClassList("decorator");
            }
        }
        
        private bool HasChildField(Node node)
        {
             return node.GetType().GetField("child") != null;
        }
        
        public void UpdateState()
        {
            RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");

            if (Application.isPlaying)
            {
                // Use reflection to get protected isEntered
                var field = typeof(BehaviorTree.Node).GetField("isEntered", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    bool isEntered = (bool)field.GetValue(node);
                    if (isEntered) 
                    {
                        AddToClassList("running");
                    }
                }
            }
        }

        private void CreateInputPorts()
        {
            // All nodes can have parents
            input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));

            if (input != null)
            {
                input.portName = "";
                input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(input);
            }
        }

        private void CreateOutputPorts()
        {
            if (node is ActionNode || node is ConditionNode)
            {
                // Leaves have no output
            }
            else if (node is CompositeNode)
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
                output.style.flexDirection = FlexDirection.ColumnReverse;
                
                if (treeView != null && treeView.connectorListener != null)
                {
                    output.AddManipulator(new EdgeConnector<Edge>(treeView.connectorListener));
                }
                
                outputContainer.Add(output);
            }
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
                    System.Array.Sort(composite.nodes, (left, right) => left.position.x < right.position.x ? -1 : 1);
                }
            }
        }
    }
}
