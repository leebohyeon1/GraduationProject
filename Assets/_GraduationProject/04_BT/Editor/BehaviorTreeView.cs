using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using BehaviorTree;

namespace BehaviorTree.Editor
{
    public class BehaviorTreeView : GraphView
    {
        public Action<NodeView> OnNodeSelected;
        public ActionTree tree;

        public BehaviorTreeView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_GraduationProject/04_BT/Editor/BehaviorTreeEditor.uss");
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;
            
            // [Fix] Enable Spacebar Search Window
            this.nodeCreationRequest = (context) => OpenSearchWindow(context);
        }

        private void OpenSearchWindow(NodeCreationContext context)
        {
            var searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            var window = EditorWindow.GetWindow<BehaviorTreeEditor>();
            
            // SearchWindow needs Screen Position for the popup
            // But CreateNode needs Graph Local Position for spawning
            
            // The SearchWindow.Init expects 'spawnPosition'. 
            // We need to pass the LOCAL position corresponding to the mouse.
            // context.screenMousePosition is Screen Space.
            
            // Convert Screen -> Window -> Graph Local
            Vector2 windowMousePos = context.screenMousePosition - window.position.position;
            Vector2 graphMousePos = contentViewContainer.WorldToLocal(windowMousePos);

            // Pass the graph local position to Init so nodes spawn correctly
            searchWindow.Init(window, this, graphMousePos);
            
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void OnUndoRedo()
        {
            if(tree != null) PopulateView(tree);
            AssetDatabase.SaveAssets();
        }

        public void Init(EditorWindow window)
        {

        }

        public EditorWindow GetWindow() 
        { 
            return EditorWindow.GetWindow<BehaviorTreeEditor>(); 
        }

        NodeView FindNodeView(Node node)
        {
            return GetNodeByGuid(node.guid) as NodeView;
        }

        public void PopulateView(ActionTree tree)
        {
            this.tree = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            if (tree.rootNode == null)
            {
                // Root handling logic if needed
            }

            // Create Node Views
            if (tree.nodes != null)
            {
                tree.nodes.ForEach(n => 
                {
                    if(n != null) CreateNodeView(n);
                });
            }

            // Create Edges
            if (tree.nodes != null)
            {
                tree.nodes.ForEach(n => 
                {
                    if (n != null)
                    {
                        var children = tree.GetChildren(n);
                        children.ForEach(c => 
                        {
                            NodeView parentView = FindNodeView(n);
                            NodeView childView = FindNodeView(c);
                            
                            if (parentView != null && childView != null)
                            {
                                Edge edge = parentView.output.ConnectTo(childView.input);
                                AddElement(edge);
                            }
                        });
                    }
                });
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
            {
                if (startPort.direction == endPort.direction) return false;
                if (startPort.node == endPort.node) return false;
                return true;
            }).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is NodeView nodeView)
                    {
                        tree.DeleteNode(nodeView.node);
                    }
                    if (elem is Edge edge)
                    {
                        NodeView parentView = edge.output.node as NodeView;
                        NodeView childView = edge.input.node as NodeView;
                        tree.RemoveChild(parentView.node, childView.node);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    tree.AddChild(parentView.node, childView.node);
                });
            }
            
            if (graphViewChange.movedElements != null)
            {
                if (nodes != null)
                {
                    nodes.ForEach((n) => 
                    {
                        NodeView view = n as NodeView;
                        if(view != null) view.SortChildren();
                    });
                }
            }

            return graphViewChange;
        }

        // [Fix] Removed BuildContextualMenu to prioritize SearchWindow
        // If you want Right-Click menu too, you can keep BuildContextualMenu,
        // but SearchWindow is better for Spacebar.
        // I removed it to avoid conflict or dual systems. Spacebar will use OpenSearchWindow.

        public void CreateNode(System.Type type, Vector2 position)
        {
            if (tree == null) return;
            
            Node node = tree.CreateNode(type);
            node.position = position;
            CreateNodeView(node);
        }

        void CreateNodeView(Node node)
        {
            NodeView nodeView = new NodeView(node);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }

        public void UpdateNodeStates()
        {
            nodes.ForEach(n => 
            {
                NodeView view = n as NodeView;
                if(view != null) view.UpdateState();
            });
        }
    }
}
