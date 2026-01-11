using UnityEngine;
using System.Collections.Generic;
using BehaviorTree;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace BehaviorTree
{
    [CreateAssetMenu(fileName = "ActionTree", menuName = "BehaviorTree/Action Tree")]
    public class ActionTree : ScriptableObject
    {
        [HideInInspector]
        public Node rootNode;
        public Node.NodeState treeState = Node.NodeState.RUNNING;
        
        // [Editor Support]
        public List<Node> nodes = new List<Node>();

        public Node.NodeState Update()
        {
            if (rootNode == null) return Node.NodeState.FAILURE;

            if (rootNode.Evaluate() == Node.NodeState.RUNNING)
            {
                treeState = Node.NodeState.RUNNING;
                return Node.NodeState.RUNNING;
            }
            treeState = Node.NodeState.SUCCESS;
            return Node.NodeState.SUCCESS;
        }

        public ActionTree Clone()
        {
            ActionTree newTree = Instantiate(this);
            if (rootNode != null)
            {
                newTree.rootNode = rootNode.Clone();
            }
            return newTree;
        }

        public void SetRunner(Enemy runner, AiBrain brain)
        {
            if (rootNode != null)
            {
                rootNode.SetRunner(runner, brain);
            }
        }

#if UNITY_EDITOR
        public Node CreateNode(System.Type type)
        {
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            
            Undo.RecordObject(this, "Behavior Tree (CreateNode)");
            nodes.Add(node);

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
            }
            
            Undo.RegisterCreatedObjectUndo(node, "Behavior Tree (CreateNode)");
            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(Node node)
        {
            Undo.RecordObject(this, "Behavior Tree (DeleteNode)");
            nodes.Remove(node);
            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
        }
        
        public void AddChild(Node parent, Node child)
        {
            if (parent is CompositeNode composite)
            {
                Undo.RecordObject(composite, "Behavior Tree (AddChild)");
                if (composite.nodes == null) composite.nodes = new Node[0];
                
                List<Node> list = new List<Node>(composite.nodes);
                list.Add(child);
                composite.nodes = list.ToArray();
                
                EditorUtility.SetDirty(composite);
            }
            else
            {
                var field = parent.GetType().GetField("child");
                if (field != null && field.FieldType == typeof(Node))
                {
                    Undo.RecordObject(parent, "Behavior Tree (AddChild)");
                    field.SetValue(parent, child);
                    EditorUtility.SetDirty(parent);
                }
            }
            AssetDatabase.SaveAssets();
            FindAndSetRoot();
        }
        
        public void RemoveChild(Node parent, Node child)
        {
             if (parent is CompositeNode composite)
            {
                Undo.RecordObject(composite, "Behavior Tree (RemoveChild)");
                if (composite.nodes == null) return;
                
                List<Node> list = new List<Node>(composite.nodes);
                list.Remove(child);
                composite.nodes = list.ToArray();
                
                EditorUtility.SetDirty(composite);
            }
            else
            {
                var field = parent.GetType().GetField("child");
                if (field != null && field.FieldType == typeof(Node))
                {
                    Undo.RecordObject(parent, "Behavior Tree (RemoveChild)");
                    field.SetValue(parent, null);
                    EditorUtility.SetDirty(parent);
                }
            }
            AssetDatabase.SaveAssets();
            FindAndSetRoot();
        }
        
        public List<Node> GetChildren(Node parent)
        {
            List<Node> children = new List<Node>();
            
            if (parent is CompositeNode composite)
            {
                if (composite.nodes != null)
                {
                    children.AddRange(composite.nodes);
                }
            }
            else
            {
                var field = parent.GetType().GetField("child");
                if (field != null && field.FieldType == typeof(Node))
                {
                    var child = field.GetValue(parent) as Node;
                    if (child != null)
                    {
                        children.Add(child);
                    }
                }
            }
            return children;
        }

        public void FindAndSetRoot()
        {
            if (nodes == null || nodes.Count == 0) return;

            HashSet<Node> allChildren = new HashSet<Node>();
            foreach (var node in nodes)
            {
                var kids = GetChildren(node);
                foreach (var kid in kids)
                {
                    allChildren.Add(kid);
                }
            }

            foreach (var node in nodes)
            {
                if (!allChildren.Contains(node))
                {
                    if (rootNode != node)
                    {
                        rootNode = node;
                        EditorUtility.SetDirty(this);
                    }
                    return;
                }
            }
        }
#endif
    }
}
