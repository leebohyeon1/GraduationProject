using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using BehaviorTree; // Changed namespace
using System.Reflection;

namespace BehaviorTree.Editor // Changed namespace
{
    public class BehaviorTreeEditor : EditorWindow
    {
        BehaviorTreeView treeView;
        InspectorView inspectorView;

        public static BehaviorTreeEditor ActiveWindow { get; private set; }
        public BehaviorTreeView TreeView => treeView;

        [MenuItem("AI/BehaviorTree Editor")]
        public static void OpenWindow()
        {
            BehaviorTreeEditor wnd = GetWindow<BehaviorTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviorTreeEditor");
            ActiveWindow = wnd;
            wnd.Focus();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is ActionTree) // Changed to ActionTree
            {
                OpenWindow();
                return true;
            }
            return false;
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_GraduationProject/04_BT/Editor/BehaviorTreeEditor.uxml");
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_GraduationProject/04_BT/Editor/BehaviorTreeEditor.uss");
            root.styleSheets.Add(styleSheet);

            treeView = root.Q<BehaviorTreeView>();
            inspectorView = root.Q<InspectorView>();
            
            treeView.Init(this);
            treeView.OnNodeSelected = inspectorView.UpdateSelection;

            OnSelectionChange();
        }

        // Standard Command Handling for Copy/Paste/Delete
        private void OnGUI()
        {
            Event e = Event.current;
            
            if (e.type == EventType.ValidateCommand)
            {
                if (e.commandName == "Copy" || e.commandName == "Paste" || e.commandName == "Delete" || e.commandName == "SoftDelete")
                {
                    e.Use();
                }
            }
            else if (e.type == EventType.ExecuteCommand)
            {
                if (e.commandName == "Copy")
                {
                    treeView.CopySelection();
                    e.Use();
                }
                else if (e.commandName == "Paste")
                {
                    treeView.Paste();
                    e.Use();
                }
                else if (e.commandName == "Delete" || e.commandName == "SoftDelete")
                {
                    treeView.DeleteSelection();
                    e.Use();
                }
            }
        }

        private void OnEnable()
        {
            ActiveWindow = this;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private void OnFocus()
        {
            ActiveWindow = this;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            if (ActiveWindow == this) ActiveWindow = null;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            OnSelectionChange();
        }

        private void OnSelectionChange()
        {
            ActionTree tree = Selection.activeObject as ActionTree; // Changed to ActionTree
            if (!tree)
            {
                if (Selection.activeGameObject)
                {
                    // Adapted for AiController
                    var runner = Selection.activeGameObject.GetComponent<AiController>();
                    if (runner)
                    {
                        // Reflect to get private _behaviorTree
                        FieldInfo field = typeof(AiController).GetField("_behaviorTree", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (field != null)
                        {
                            tree = field.GetValue(runner) as ActionTree;
                        }
                    }
                }
            }

            if (Application.isPlaying)
            {
                if (tree && treeView != null) 
                {
                    treeView.PopulateView(tree);
                }
            }
            else
            {
                if (tree && AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
                {
                    treeView?.PopulateView(tree);
                }
            }
        }

        private void OnInspectorUpdate()
        {
            treeView?.UpdateNodeStates();
        }
    }
}
