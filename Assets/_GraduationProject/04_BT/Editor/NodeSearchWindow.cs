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
        private Vector2 spawnPosition; 

        public void Init(EditorWindow window, BehaviorTreeView graphView, Vector2 spawnPosition)
        {
            this.window = window;
            this.graphView = graphView;
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

            // 1. Composites
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Composites"), 1));
            var compositeNodes = TypeCache.GetTypesDerivedFrom<CompositeNode>();
            foreach (var type in compositeNodes)
            {
                if (type.IsAbstract) continue;
                tree.Add(new SearchTreeEntry(new GUIContent(type.Name, indentationIcon)) { userData = type, level = 2 });
            }

            // 2. Decorators
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Decorators"), 1));
            tree.Add(new SearchTreeEntry(new GUIContent("Inverter", indentationIcon)) { userData = typeof(Decorator_Inverter), level = 2 });
            // Add more if needed

            // 3. Actions (ConditionNode is leaf/action here)
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Actions"), 1));
            var allNodes = TypeCache.GetTypesDerivedFrom<Node>();
            foreach (var type in allNodes)
            {
                if (type.IsAbstract) continue;
                if (type.IsSubclassOf(typeof(CompositeNode))) continue;
                if (type == typeof(Decorator_Inverter)) continue;
                if (type == typeof(ActionTree)) continue;
                if (type.Name == "ConditionNode") continue; 

                tree.Add(new SearchTreeEntry(new GUIContent(type.Name, indentationIcon)) { userData = type, level = 2 });
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var type = SearchTreeEntry.userData as System.Type;
            graphView.CreateNode(type, spawnPosition); 
            return true;
        }
    }
}
