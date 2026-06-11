// --- START OF FILE BehaviorTreeEditorWindow.cs ---

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using BehaviorTree;

public class BehaviorTreeEditorWindow : EditorWindow
{
    private BehaviorTreeView _graphView;

    [SerializeField]
    private ActionTree _targetTree;

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        var asset = EditorUtility.EntityIdToObject(instanceID) as ActionTree;
        if (asset == null && EditorUtility.EntityIdToObject(instanceID) is BehaviorTree.Node node)
        {
            string path = AssetDatabase.GetAssetPath(node);
            asset = AssetDatabase.LoadAssetAtPath<ActionTree>(path);
        }

        if (asset != null)
        {
            OpenWindow(asset);
            return true;
        }
        return false;
    }

    [MenuItem("Behavior Tree/Editor")]
    public static void OpenFromMenu()
    {
        BehaviorTreeEditorWindow window = GetWindow<BehaviorTreeEditorWindow>();
        window.titleContent = new GUIContent("Behavior Tree Editor");
    }

    public static void OpenWindow(ActionTree tree)
    {
        BehaviorTreeEditorWindow window = GetWindow<BehaviorTreeEditorWindow>();
        window.titleContent = new GUIContent($"BT Editor ({tree.name})");
        window.SetTargetTree(tree);
    }

    public void CreateGUI()
    {
        rootVisualElement.Clear();

        if (_targetTree != null)
        {
            _graphView = new BehaviorTreeView(this, _targetTree);
            rootVisualElement.Add(_graphView);
        }
        else
        {
            var label = new Label("Project 창에서 ActionTree 에셋을 선택하거나 더블클릭하여 여세요.");
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            rootVisualElement.Add(label);
        }
    }

    private void OnSelectionChange()
    {
        var selected = Selection.activeObject as ActionTree;
        if (selected != null)
        {
            if (_targetTree == null || _targetTree != selected) {
                SetTargetTree(selected);
            }
        }
    }
    
    private void SetTargetTree(ActionTree tree)
    {
        _targetTree = tree;
        if (_graphView != null)
        {
            rootVisualElement.Remove(_graphView);
            _graphView = null;
        }
        CreateGUI();
    }
}
// --- END OF FILE BehaviorTreeEditorWindow.cs ---