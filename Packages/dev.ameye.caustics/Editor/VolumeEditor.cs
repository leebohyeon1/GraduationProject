using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Caustics.Editor
{
    [CustomEditor(typeof(Volume))]
    public class VolumeEditor : UnityEditor.Editor
    {
        private Volume volume;
        private new SerializedObject serializedObject;
        private GameObject selectedGameObject;
        
        private SerializedProperty materialProperty;
        private SerializedProperty textureProperty;
        private SerializedProperty intensityProperty;
        private SerializedProperty rgbSplitProperty;
        private SerializedProperty scaleProperty;
        private SerializedProperty speedProperty;
        private SerializedProperty sceneLuminanceMaskStrengthProperty;
        private SerializedProperty shadowMaskStrengthProperty;
        private SerializedProperty lightDirectionSourceProperty;
        private SerializedProperty fixedLightDirectionProperty;
        private SerializedProperty fadeAmountProperty;
        private SerializedProperty fadeHardnessProperty;
        private SerializedProperty displayDebugOverlayProperty;
        
        private static bool _textureSettingsFoldout, _visualsSettingsFoldout, _lightInfluenceFoldout, _fadeFoldout, _otherSettingsFoldout;
        private delegate void DrawSettingsMethod();
        
        public void OnEnable()
        {
            selectedGameObject = Selection.activeGameObject;

            if (!selectedGameObject) return;
            if (!volume) volume = selectedGameObject.GetComponent<Volume>();
            if (volume)
            {
                serializedObject = new SerializedObject(volume);
                
                // assign all serialized properties
                  FindAllProperties();
                //FindAllSerializedProperties();
                
                // hide mesh filter/renderer components
                // causticsVolume.HideMeshFilterRenderer();
            }
            Undo.undoRedoPerformed += ApplyChanges;
        }

        private void OnSceneGUI()
        {
            DrawSceneHandles();
        }

        private void DrawSceneHandles()
        {
            if (!displayDebugOverlayProperty.boolValue) return;

            var visible = Color.green;
            var occluded = Color.white;

            var v1 = new Vector3(-0.5f, -0.5f, -0.5f);
            var v2 = new Vector3(0.5f, 0.5f, 0.5f);
            var v3 = new Vector3(v1.x, v1.y, v2.z);
            var v4 = new Vector3(v1.x, v2.y, v1.z);
            var v5 = new Vector3(v2.x, v1.y, v1.z);
            var v6 = new Vector3(v1.x, v2.y, v2.z);
            var v7 = new Vector3(v2.x, v1.y, v2.z);
            var v8 = new Vector3(v2.x, v2.y, v1.z);

            Handles.matrix = volume.transform.localToWorldMatrix;
            
            Handles.color = GetVisibleOutlineColor(visible);
            Handles.zTest = CompareFunction.LessEqual;
            Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v1, v3, v7, v5, v1);
            Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v4, v6, v2, v8, v4);
            Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v7, v2, v6, v3, v7);
            Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v1, v4, v8, v5, v1);
            Handles.DrawSolidRectangleWithOutline(new[] {v7, v2, v6, v3}, GetVisibleFaceColor(visible),
                Color.clear);
            Handles.DrawSolidRectangleWithOutline(new[] {v1, v4, v8, v5}, GetVisibleFaceColor(visible),
                Color.clear);
            Handles.DrawSolidRectangleWithOutline(new[] {v1, v3, v7, v5}, GetVisibleFaceColor(visible),
                Color.clear);
            Handles.DrawSolidRectangleWithOutline(new[] {v4, v6, v2, v8}, GetVisibleFaceColor(visible),
                Color.clear);
            Handles.DrawSolidRectangleWithOutline(new[] {v5, v8, v2, v7}, GetVisibleFaceColor(visible),
                Color.clear);
            Handles.DrawSolidRectangleWithOutline(new[] {v3, v6, v4, v1}, GetVisibleFaceColor(visible),
                Color.clear);

            Handles.color = GetOccludedOutlineColor(occluded);
            Handles.zTest = CompareFunction.Greater;
            Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v1, v3, v7, v5, v1);
            Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v4, v6, v2, v8, v4);
            Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v7, v2, v6, v3, v7);
            Handles.DrawAAPolyLine(Texture2D.whiteTexture, 5f, v1, v4, v8, v5, v1);
            Handles.DrawSolidRectangleWithOutline(new[] {v7, v2, v6, v3}, GetOccludedFaceColor(occluded),
                Color.clear);
            Handles.DrawSolidRectangleWithOutline(new[] {v1, v4, v8, v5}, GetOccludedFaceColor(occluded),
                Color.clear);
            Handles.DrawSolidRectangleWithOutline(new[] {v1, v3, v7, v5}, GetOccludedFaceColor(occluded),
                Color.clear);
            Handles.DrawSolidRectangleWithOutline(new[] {v4, v6, v2, v8}, GetOccludedFaceColor(occluded),
                Color.clear);
            Handles.DrawSolidRectangleWithOutline(new[] {v5, v8, v2, v7}, GetOccludedFaceColor(occluded),
                Color.clear);
            Handles.DrawSolidRectangleWithOutline(new[] {v3, v6, v4, v1}, GetOccludedFaceColor(occluded),
                Color.clear);
        }

        private bool updateMaterialFromEditorChange;
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            volume.ReadMaterialProperties();
            updateMaterialFromEditorChange = false;
            using var check = new EditorGUI.ChangeCheckScope();
            DrawCausticsSettings();
            if (check.changed) updateMaterialFromEditorChange = true;
            ApplyChanges();
        }

        private void ApplyChanges()
        {
            if (serializedObject.targetObject) serializedObject.ApplyModifiedProperties();
            if (updateMaterialFromEditorChange)
            {
                volume.WriteMaterialProperties();
                volume.ReadMaterialProperties();
                updateMaterialFromEditorChange = false;
            }
        }

        private void DrawCausticsSettings()
        {
            using( new EditorGUI.DisabledScope(true) ) {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(materialProperty, EditorUtils.CommonStyles.MaterialGUIContent);
                EditorGUILayout.Space();
            }

            if (!volume.meshRenderer || !volume.meshRenderer.sharedMaterial || !volume.meshRenderer.enabled)
            {
                DrawWarningBox("GameObject has no active Mesh Renderer and/or assigned Material. Please add those first.");
                return;
            }

            if (volume.meshRenderer.sharedMaterial.shader.name != Volume.CausticsShaderName)
            {
                DrawWarningBox("Assigned Material is incompatible. You should add a Material that uses the \"Ameye/Water Caustics for URP\" shader.");
                return;
            }
            
            CoreEditorUtils.DrawSplitter();
            
            // foldouts
            _textureSettingsFoldout = CoreEditorUtils.DrawHeaderFoldout("Texture", _textureSettingsFoldout);
            DrawPropertiesInspector(_textureSettingsFoldout, DrawTextureSettings);
          
            _visualsSettingsFoldout = CoreEditorUtils.DrawHeaderFoldout("Visuals", _visualsSettingsFoldout);
            DrawPropertiesInspector(_visualsSettingsFoldout, DrawVisualsSettings);
            
            _lightInfluenceFoldout = CoreEditorUtils.DrawHeaderFoldout("Light Influence", _lightInfluenceFoldout);
            DrawPropertiesInspector(_lightInfluenceFoldout, DrawLightInfluenceSettings);
           
            _fadeFoldout = CoreEditorUtils.DrawHeaderFoldout("Edge Fading", _fadeFoldout);
            DrawPropertiesInspector(_fadeFoldout, DrawFadeSettings);
            
            _otherSettingsFoldout = CoreEditorUtils.DrawHeaderFoldout("Other Settings", _otherSettingsFoldout);
            DrawPropertiesInspector(_otherSettingsFoldout, DrawOtherSettings);
        }

        private void DrawTextureSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(textureProperty, EditorUtils.CommonStyles.TextureGUIContent);
            EditorGUILayout.PropertyField(scaleProperty,EditorUtils.CommonStyles. ScaleGUIContent);
            EditorGUILayout.PropertyField(speedProperty,EditorUtils.CommonStyles. SpeedGUIContent);
            EditorGUILayout.Space(); EditorGUILayout.Space();
        }

        private void DrawVisualsSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(intensityProperty,EditorUtils.CommonStyles. IntensityGUIContent);
            EditorGUILayout.PropertyField(rgbSplitProperty, EditorUtils.CommonStyles.RgbSplitGUIContent);
            EditorGUILayout.Space(); EditorGUILayout.Space();
        }

        private void DrawLightInfluenceSettings()
        {
            EditorGUILayout.Space();
            CoreEditorUtils.DrawPopup(EditorUtils.CommonStyles.LightDirectionGUIContent, lightDirectionSourceProperty, new[] { "Fixed", "Main Light" });
            if (lightDirectionSourceProperty.enumValueIndex == 0)
            {
                EditorGUILayout.PropertyField(fixedLightDirectionProperty, EditorUtils.CommonStyles.FixedLightDirectionGUIContent);
            }
            else
            {
                // note: make sure the main light direction is being written to
                EditorGUILayout.HelpBox(EditorGUIUtility.TrTextContent("Make sure that the CausticsLightDirection Renderer Feature is added to your renderer.").text, MessageType.Info, false);
                EditorGUILayout.Space();
            }
            EditorGUILayout.PropertyField(sceneLuminanceMaskStrengthProperty,EditorUtils.CommonStyles. SceneLuminanceMaskStrengthGUIContent);
            EditorGUILayout.PropertyField(shadowMaskStrengthProperty, EditorUtils.CommonStyles.ShadowMaskStrengthGUIContent);
            EditorGUILayout.Space(); EditorGUILayout.Space();
        }

        private void DrawFadeSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(fadeAmountProperty, EditorUtils.CommonStyles.FadeAmountGUIContent);
            EditorGUILayout.PropertyField(fadeHardnessProperty,EditorUtils.CommonStyles. FadeHardnessGUIContent);
            EditorGUILayout.Space(); EditorGUILayout.Space();
        }

        private void DrawOtherSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(displayDebugOverlayProperty, EditorUtils.CommonStyles.DisplayDebugOverlayGUIContent);
            EditorGUILayout.Space(); EditorGUILayout.Space();
        }
        
        private void DrawPropertiesInspector(bool active, DrawSettingsMethod drawProperties)
        {
            if (active)
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                drawProperties();
                if (EditorGUI.EndChangeCheck()) ApplyChanges();
                EditorGUI.indentLevel--;
            }
            CoreEditorUtils.DrawSplitter();
        }

        private static void DrawWarningBox(string message) {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(message, MessageType.Warning);
            EditorGUILayout.Space();
        }

        private void FindAllSerializedProperties()
        {
            if (!selectedGameObject) return;
            textureProperty = serializedObject.FindProperty("texture");
            intensityProperty = serializedObject.FindProperty("strength");
            rgbSplitProperty = serializedObject.FindProperty("rgbSplit");
            scaleProperty = serializedObject.FindProperty("scale");
            speedProperty = serializedObject.FindProperty("speed");
            sceneLuminanceMaskStrengthProperty = serializedObject.FindProperty("sceneLuminanceMaskStrength");
            shadowMaskStrengthProperty = serializedObject.FindProperty("shadowMaskStrength");
            lightDirectionSourceProperty = serializedObject.FindProperty("lightDirectionSource");
            fixedLightDirectionProperty = serializedObject.FindProperty("fixedLightDirection");
            fadeAmountProperty = serializedObject.FindProperty("fadeAmount");
            fadeHardnessProperty = serializedObject.FindProperty("fadeHardness");
        }
        
        void FindAllProperties() {
            IEnumerable<FieldInfo> GetFields( Type type ) {
                return type.GetFields( BindingFlags.Instance | BindingFlags.NonPublic )
                    .Where( x => x.FieldType == typeof(SerializedProperty) && x.Name.StartsWith( "m_" ) == false && x.GetValue( this ) == null );
            }

            IEnumerable<FieldInfo> fieldsBase = GetFields( GetType().BaseType );
            IEnumerable<FieldInfo> fieldsInherited = GetFields( GetType() );

            foreach( FieldInfo field in fieldsBase.Concat( fieldsInherited ) )
            {
                string fieldName = field.Name.Substring(0, field.Name.Length - 8);
                field.SetValue( this, serializedObject.FindProperty( fieldName ) );
                if( field.GetValue( this ) == null )
                    Debug.LogError( $"Failed to load {target.GetType()} property: {field.Name} !=> {fieldName}" );
            }
        }
        
        private static Color GetVisibleOutlineColor(Color baseColor)
        {
            baseColor.a = 0.7f;
            return baseColor;
        }

        private static Color GetVisibleFaceColor(Color baseColor)
        {
            baseColor.a = 0.0f;
            return baseColor;
        }

        private static Color GetOccludedOutlineColor(Color baseColor)
        {
            baseColor.a = 0.1f;
            return baseColor;
        }

        private static Color GetOccludedFaceColor(Color baseColor)
        {
            baseColor.a = .15f;
            return baseColor;
        }
    }
}