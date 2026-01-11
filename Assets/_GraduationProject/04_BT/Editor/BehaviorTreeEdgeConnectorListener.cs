using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements; 
using BehaviorTree;

namespace BehaviorTree.Editor
{
    public class BehaviorTreeEdgeConnectorListener : IEdgeConnectorListener
    {
        private BehaviorTreeView graphView;
        private NodeSearchWindow searchWindow;

        public BehaviorTreeEdgeConnectorListener(BehaviorTreeView graphView, NodeSearchWindow searchWindow)
        {
            this.graphView = graphView;
            this.searchWindow = searchWindow;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            EditorWindow window = graphView.GetWindow();
            
            // 1. Calculate Absolute Screen Position from Event Position
            Vector2 screenPos = GUIUtility.GUIToScreenPoint(position);

            // 2. Calculate Window-Relative Position (Top-Left of the window content)
            Vector2 windowLocalPos = screenPos - window.position.position;
            
            // 3. Transform from Window Root -> GraphView Content Container (Handles Inspector offset + Zoom/Pan)
            Vector2 localPos = window.rootVisualElement.ChangeCoordinatesTo(graphView.contentViewContainer, windowLocalPos);

            searchWindow.Init(window, graphView, localPos, edge.output);
            
            SearchWindow.Open(new SearchWindowContext(screenPos), searchWindow);
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            this.graphView.AddElement(edge);
        }
    }
}
