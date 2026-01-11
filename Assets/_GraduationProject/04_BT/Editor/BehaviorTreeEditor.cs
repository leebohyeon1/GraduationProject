using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using BehaviorTree;

namespace BehaviorTree.Editor
{
    public class BehaviorTreeEditor : EditorWindow
    {
        BehaviorTreeView treeView;
        InspectorView inspectorView;

        [MenuItem("BehaviorTree/Graph Editor")]
        public static void OpenWindow()
        {
            BehaviorTreeEditor wnd = GetWindow<BehaviorTreeEditor>();
            wnd.titleContent = new GUIContent("BT Graph Editor");
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is ActionTree)
            {
                OpenWindow();
                return true;
            }
            return false;
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            // --- Programmatic Layout (No UXML dependency) ---
            
            // 1. Tree View (Main Graph Area)
            treeView = new BehaviorTreeView();
            treeView.style.flexGrow = 1; 
            root.Add(treeView);

            // 2. Inspector View (Side Panel)
            inspectorView = new InspectorView();
            inspectorView.style.width = 300;
            inspectorView.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            inspectorView.style.borderLeftColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            inspectorView.style.borderLeftWidth = 2;
            
            // Use absolute positioning to dock to right
            inspectorView.style.position = Position.Absolute;
            inspectorView.style.right = 0;
            inspectorView.style.top = 0;
            inspectorView.style.bottom = 0;
            
            root.Add(inspectorView);

            // --- Initialization ---
            treeView.Init(this);
            treeView.OnNodeSelected = inspectorView.UpdateSelection;

            OnSelectionChange();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            OnSelectionChange();
        }

        private void OnSelectionChange()
        {
            ActionTree tree = Selection.activeObject as ActionTree;
            if (!tree)
            {
                if (Selection.activeGameObject)
                {
                    // Handle Runner selection if needed
                }
            }

            if (tree && treeView != null)
            {
                treeView.PopulateView(tree);
            }
        }

        private void OnInspectorUpdate()
        {
            treeView?.UpdateNodeStates();
        }
    }
}
