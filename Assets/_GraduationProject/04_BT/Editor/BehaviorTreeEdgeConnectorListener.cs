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
            
            Vector2 screenPos = GUIUtility.GUIToScreenPoint(position);
            
            // Simplified position handling for search window
            // If window is null (shouldn't be), fallback to screen center logic
            
            // Just use screenPos for context menu, and pass local pos to CreateNode later
            // We need to calculate graph-local position for spawning
            Vector2 localPos = position; // This is usually local to graph view or window?
            // The position from OnDropOutsidePort is usually in GraphView coordinates already or needs conversion.
            // Let's rely on SearchWindowContext position for opening, and use a safe spawn point.
            
            searchWindow.Init(window, graphView, position); // Using position from event directly
            SearchWindow.Open(new SearchWindowContext(screenPos), searchWindow);
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            this.graphView.AddElement(edge);
        }
    }
}
