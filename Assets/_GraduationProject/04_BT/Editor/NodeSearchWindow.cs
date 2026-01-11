using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using BehaviorTree;

namespace BehaviorTree.Editor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private BehaviorTreeView graphView;
        private EditorWindow window;
        private Texture2D indentationIcon;
        private Port sourcePort;
        private Vector2 spawnPosition; 

        public void Init(EditorWindow window, BehaviorTreeView graphView, Vector2 spawnPosition, Port sourcePort = null)
        {
            this.window = window;
            this.graphView = graphView;
            this.sourcePort = sourcePort;
            this.spawnPosition = spawnPosition; 
            
            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
            };

            // 1. Action Nodes
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Action"), 1));
            var actionNodes = TypeCache.GetTypesDerivedFrom<ActionNode>();
            foreach (var type in actionNodes)
            {
                if (type.IsAbstract) continue;
                tree.Add(new SearchTreeEntry(new GUIContent(type.Name, indentationIcon))
                {
                    userData = type, 
                    level = 2
                });
            }

            // 2. Condition Nodes
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Condition"), 1));
            var conditionNodes = TypeCache.GetTypesDerivedFrom<ConditionNode>();
            foreach (var type in conditionNodes)
            {
                if (type.IsAbstract) continue;
                tree.Add(new SearchTreeEntry(new GUIContent(type.Name, indentationIcon))
                {
                    userData = type, 
                    level = 2
                });
            }

            // 3. Composite Nodes
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Composite"), 1));
            var compositeNodes = TypeCache.GetTypesDerivedFrom<CompositeNode>();
            foreach (var type in compositeNodes)
            {
                if (type.IsAbstract) continue;
                tree.Add(new SearchTreeEntry(new GUIContent(type.Name, indentationIcon))
                {
                    userData = type, 
                    level = 2
                });
            }

            // 4. Decorator Nodes (and others)
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Decorator"), 1));
            var allNodes = TypeCache.GetTypesDerivedFrom<Node>();
            foreach (var type in allNodes)
            {
                if (type.IsAbstract) continue;
                if (type.IsSubclassOf(typeof(ActionNode))) continue;
                if (type.IsSubclassOf(typeof(ConditionNode))) continue;
                if (type.IsSubclassOf(typeof(CompositeNode))) continue;
                if (type == typeof(Node)) continue; // Base class

                // This is likely a Decorator or specialized Node
                tree.Add(new SearchTreeEntry(new GUIContent(type.Name, indentationIcon))
                {
                    userData = type, 
                    level = 2
                });
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var type = SearchTreeEntry.userData as System.Type;
            
            if (graphView == null)
            {
                if (BehaviorTreeEditor.ActiveWindow != null)
                {
                    window = BehaviorTreeEditor.ActiveWindow;
                    graphView = BehaviorTreeEditor.ActiveWindow.TreeView;
                }
            }

            if (graphView == null)
            {
                var editors = Resources.FindObjectsOfTypeAll<BehaviorTreeEditor>();
                if (editors.Length > 0)
                {
                    var editor = editors[0];
                    window = editor;
                    graphView = editor.TreeView;
                }
            }

            if (graphView == null || window == null)
            {
                Debug.LogError("Could not find Behavior Tree Editor window. Please re-open the window.");
                return false;
            }
            
            Vector2 nodeCenterOffset = new Vector2(-75, -50); 
            var newNode = graphView.CreateNode(type, spawnPosition + nodeCenterOffset);
            
            if (newNode == null) return false;

            if (sourcePort != null)
            {
                graphView.ConnectPorts(sourcePort, newNode);
            }
            
            return true;
        }
    }
}
