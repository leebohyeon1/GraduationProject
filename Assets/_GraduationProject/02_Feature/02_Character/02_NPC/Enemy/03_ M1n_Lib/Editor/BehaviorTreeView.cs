// --- START OF FILE BehaviorTreeView.cs ---

using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using BehaviorTree;

public class BehaviorTreeView : GraphView
{
    private readonly BehaviorTreeEditorWindow _editorWindow;
    public readonly ActionTree _targetTree;

    public BehaviorTreeView(BehaviorTreeEditorWindow editorWindow, ActionTree targetTree)
    {
        _editorWindow = editorWindow;
        _targetTree = targetTree;
        style.flexGrow = 1;

        Insert(0, new GridBackground());
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var searchWindowProvider = ScriptableObject.CreateInstance<NodeSearchWindowProvider>();
        searchWindowProvider.Initialize(this);
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindowProvider);
        
        graphViewChanged = OnGraphViewChanged;
        
        // ★★★ 핵심 2: 그래프 배경을 클릭했을 때의 동작을 등록합니다. ★★★
        RegisterCallback<MouseDownEvent>(OnClickBackground);

        PopulateView(_targetTree);
    }

    // 배경 클릭 시 메인 ActionTree를 선택하여 인스펙터에 표시하는 함수
    private void OnClickBackground(MouseDownEvent e)
    {
        // 클릭 대상이 정확히 그래프 뷰 자신일 때만 (노드나 엣지가 아닐 때)
        if (e.target == this)
        {
            ClearSelection(); // 모든 노드 선택 해제
            Selection.activeObject = _targetTree; // 메인 트리를 선택 객체로 지정
        }
    }
    
    private void RemoveNodeFromAsset(BehaviorTree.Node nodeToRemove)
    {
        if (nodeToRemove == null) return;
        AssetDatabase.RemoveObjectFromAsset(nodeToRemove);
        AssetDatabase.SaveAssets();
    }
    
    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if (graphViewChange.elementsToRemove != null)
        {
            foreach (var element in graphViewChange.elementsToRemove)
            {
                if (element is NodeView nodeView) { RemoveNodeFromAsset(nodeView.node); }
            }
        }
        SaveChanges();
        return graphViewChange;
    }

    private void SaveChanges()
    {
        if (_targetTree == null) return;
        var nodeViews = nodes.OfType<NodeView>().ToList();
        nodeViews.ForEach(n => n.node.position = n.GetPosition().position);
        nodeViews.ForEach(parentView =>
        {
            if (parentView.node is CompositeNode composite)
            {
                var children = parentView.output.connections.Select(e => (e.input.node as NodeView)?.node).Where(n => n != null).ToList();
                children.Sort((a, b) => a.position.y.CompareTo(b.position.y));
                composite.nodes = children.ToArray();
            }
            else if (parentView.node is Decorator_Inverter inverter)
            {
                inverter.child = (parentView.output?.connections?.FirstOrDefault()?.input.node as NodeView)?.node;
            }
        });
        _targetTree.rootNode = nodeViews.FirstOrDefault(n => n.input.connections.Count() == 0)?.node;
        EditorUtility.SetDirty(_targetTree);
        nodeViews.ForEach(n => EditorUtility.SetDirty(n.node));
    }
    
    public void CreateNode(Type type, Vector2 screenMousePosition)
    {
        if (_targetTree == null) return;
        var node = ScriptableObject.CreateInstance(type) as BehaviorTree.Node;
        node.name = ObjectNames.NicifyVariableName(type.Name);
        AssetDatabase.AddObjectToAsset(node, _targetTree);
        AssetDatabase.SaveAssets();
        var nodeView = new NodeView(node);
        var windowMousePosition = _editorWindow.rootVisualElement.ChangeCoordinatesTo(_editorWindow.rootVisualElement.parent, screenMousePosition - _editorWindow.position.position);
        var graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
        nodeView.SetPosition(new Rect(graphMousePosition, new Vector2(150, 100)));
        AddElement(nodeView);
        EditorUtility.SetDirty(_targetTree);
        EditorUtility.SetDirty(node);
        SaveChanges();
    }

    public void PopulateView(ActionTree tree)
    {
        graphElements.ForEach(RemoveElement);
        if (tree == null) return;
        var treeNodes = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(tree)).OfType<BehaviorTree.Node>().ToList();
        treeNodes.ForEach(node => { AddElement(new NodeView(node)); });
        treeNodes.ForEach(node =>
        {
            var parentView = GetNodeByGuid(node.GetInstanceID().ToString()) as NodeView;
            if (parentView == null) return;
            if (node is CompositeNode composite)
            {
                foreach (var child in composite.nodes.Where(c => c != null))
                {
                    var childView = GetNodeByGuid(child.GetInstanceID().ToString()) as NodeView;
                    if (childView != null) { AddElement(parentView.output.ConnectTo(childView.input)); }
                }
            }
            else if (node is Decorator_Inverter inverter && inverter.child != null)
            {
                var childView = GetNodeByGuid(inverter.child.GetInstanceID().ToString()) as NodeView;
                if(childView != null) { AddElement(parentView.output.ConnectTo(childView.input)); }
            }
        });
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
    }
}
// --- END OF FILE BehaviorTreeView.cs ---