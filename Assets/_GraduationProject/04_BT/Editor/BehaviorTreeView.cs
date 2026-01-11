using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using BehaviorTree;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BehaviorTree.Editor
{
    public class BehaviorTreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BehaviorTreeView, GraphView.UxmlTraits> { }

        ActionTree tree;
        NodeSearchWindow searchWindow;
        EditorWindow window;
        
        public BehaviorTreeEdgeConnectorListener connectorListener;

        [Serializable]
        public class CopyPasteData
        {
            public List<string> nodeTypeNames = new List<string>();
            public List<string> jsonDatas = new List<string>();
        }

        public BehaviorTreeView()
        {
            this.focusable = true;

            Insert(0, new GridBackground());
            
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_GraduationProject/04_BT/Editor/BehaviorTreeEditor.uss");
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        // --- Public API for EditorWindow ---
        public void CopySelection()
        {
            if (selection.Count > 0)
            {
                string data = SerializeGraphElementsImpl(selection);
                GUIUtility.systemCopyBuffer = data;
                Debug.Log($"[BehaviorTreeView] Copied {selection.Count} nodes.");
            }
        }

        public void Paste()
        {
            string data = GUIUtility.systemCopyBuffer;
            if (!string.IsNullOrEmpty(data))
            {
                UnserializeAndPasteImpl("Paste", data);
                Debug.Log($"[BehaviorTreeView] Pasted nodes.");
            }
        }
        // -----------------------------------

        public void Init(EditorWindow editorWindow)
        {
            window = editorWindow;
            searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            
            searchWindow.Init(window, this, Vector2.zero);
            
            connectorListener = new BehaviorTreeEdgeConnectorListener(this, searchWindow);

            nodeCreationRequest = context => 
            {
                Vector2 windowMousePosition = context.screenMousePosition - window.position.position;
                Vector2 graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
                
                searchWindow.Init(window, this, graphMousePosition);
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
            };
        }

        string SerializeGraphElementsImpl(IEnumerable<ISelectable> elements)
        {
            CopyPasteData copyData = new CopyPasteData();
            
            foreach (var element in elements)
            {
                if (element is NodeView nodeView)
                {
                    copyData.nodeTypeNames.Add(nodeView.node.GetType().AssemblyQualifiedName);
                    copyData.jsonDatas.Add(JsonUtility.ToJson(nodeView.node));
                }
            }

            return JsonUtility.ToJson(copyData);
        }

        void UnserializeAndPasteImpl(string operationName, string data)
        {
            if (tree == null) return;
            
            CopyPasteData copyData;
            try
            {
                copyData = JsonUtility.FromJson<CopyPasteData>(data);
            }
            catch
            {
                return;
            }

            if (copyData == null || copyData.nodeTypeNames.Count == 0) return;

            ClearSelection();

            Vector2 pasteOffset = new Vector2(30, 30);

            for (int i = 0; i < copyData.nodeTypeNames.Count; i++)
            {
                string typeName = copyData.nodeTypeNames[i];
                string json = copyData.jsonDatas[i];
                
                System.Type type = System.Type.GetType(typeName);
                if (type == null) continue;

                Node newNode = tree.CreateNode(type);
                
                string newGuid = newNode.guid;

                JsonUtility.FromJsonOverwrite(json, newNode);

                newNode.guid = newGuid;
                newNode.position += pasteOffset;

                CreateNodeView(newNode);
                
                NodeView nodeView = FindNodeView(newNode);
                AddToSelection(nodeView);
            }
            
            AssetDatabase.SaveAssets();
        }

        public void UpdateNodeStates()
        {
            nodes.ForEach(n => {
                NodeView view = n as NodeView;
                view.UpdateState();
            });
        }

        private void OnUndoRedo()
        {
            if (tree)
            {
                PopulateView(tree);
                AssetDatabase.SaveAssets();
            }
        }

        public NodeView FindNodeView(Node node)
        {
            return GetNodeByGuid(node.guid) as NodeView;
        }

        public void PopulateView(ActionTree tree)
        {
            this.tree = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            // Handle Root Node Logic:
            // ActionTree relies on rootNode field. If null, user must start somewhere.
            // Unlike original editor, we don't auto-create a specific 'RootNode' type because Target system doesn't enforce it.
            // But we can check if nodes list is empty.
            if (tree.rootNode == null && tree.nodes.Count > 0)
            {
                // Try to find root
                tree.FindAndSetRoot();
            }

            // Create Node Views
            tree.nodes.ForEach(n => CreateNodeView(n));

            // Create Edges
            tree.nodes.ForEach(n => {
                var children = tree.GetChildren(n);
                children.ForEach(c => {
                    NodeView parentView = FindNodeView(n);
                    NodeView childView = FindNodeView(c);

                    if (parentView != null && childView != null)
                    {
                        Edge edge = parentView.output.ConnectTo(childView.input);
                        AddElement(edge);
                    }
                });
            });
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
            {
                if (endPort.direction == startPort.direction) return false;
                if (endPort.node == startPort.node) return false;
                return true;
            }).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(elem => {
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
                graphViewChange.edgesToCreate.ForEach(edge => {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    tree.AddChild(parentView.node, childView.node);
                });
            }
            
            if (graphViewChange.movedElements != null)
            {
                nodes.ForEach((n) => {
                    NodeView view = n as NodeView;
                    view.SortChildren();
                });
            }

            return graphViewChange;
        }
        
        public Node CreateNode(System.Type type)
        {
            if (tree == null) return null;
            Node node = tree.CreateNode(type);
            CreateNodeView(node);
            return node;
        }

        public Node CreateNode(System.Type type, Vector2 position)
        {
            if (tree == null) return null;
            Node node = tree.CreateNode(type);
            node.position = position;
            CreateNodeView(node);
            return node;
        }
        
        void CreateNodeView(Node node)
        {
            NodeView nodeView = new NodeView(node, this); 
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }
        
        public void ConnectPorts(Port sourcePort, Node targetNode)
        {
            NodeView targetView = FindNodeView(targetNode);
            if (targetView == null || targetView.input == null) return;

            Edge edge = sourcePort.ConnectTo(targetView.input);
            AddElement(edge);
            
            if (sourcePort.node is NodeView sourceView && sourceView.node != null && tree != null)
            {
                tree.AddChild(sourceView.node, targetNode);
            }
        }

        public Action<NodeView> OnNodeSelected;
        public EditorWindow GetWindow() { return window; }
    }
}
