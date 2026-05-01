using UnityEditor;

namespace Caustics.Editor
{
    [CustomEditor(typeof(Caustics))]
    public class CausticsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("This renderer feature is used to pass the Main Light direction to the caustics shader.", MessageType.Info, true);
        }
    }
}