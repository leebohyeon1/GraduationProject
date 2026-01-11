using UnityEngine.UIElements;
using UnityEditor;
using BehaviorTree;

namespace BehaviorTree.Editor
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

        UnityEditor.Editor editor;

        public InspectorView()
        {
        }

        public void UpdateSelection(NodeView nodeView)
        {
            Clear();
            UnityEngine.Object.DestroyImmediate(editor);
            
            if (nodeView == null) return;

            editor = UnityEditor.Editor.CreateEditor(nodeView.node);
            IMGUIContainer container = new IMGUIContainer(() => {
                if (editor != null && editor.target != null)
                {
                    editor.OnInspectorGUI();
                }
            });
            Add(container);
        }
    }
}
