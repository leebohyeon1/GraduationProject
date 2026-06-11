using UnityEditor;
using UnityEngine;

namespace Caustics.Editor
{
    public static class EditorUtils
    {
        public static class CommonStyles
        {
            public static readonly GUIContent MaterialGUIContent = EditorGUIUtility.TrTextContent("Material", "The caustics material to be used. You can change the material in the Mesh Renderer component.");
            public static readonly GUIContent TextureGUIContent = EditorGUIUtility.TrTextContent("Texture", "The caustics texture to be used.");
            public static readonly GUIContent IntensityGUIContent = EditorGUIUtility.TrTextContent("Intensity", "The intensity of caustics texture.");
            public static readonly GUIContent ScaleGUIContent = EditorGUIUtility.TrTextContent("Scale", "The scale of the caustics texture.");
            public static readonly GUIContent SpeedGUIContent = EditorGUIUtility.TrTextContent("Speed", "The movement speed of the caustics texture.");
            public static readonly GUIContent RgbSplitGUIContent = EditorGUIUtility.TrTextContent("RGB Split", "How much the light should fall out into the color spectrum.");
            public static readonly GUIContent LightDirectionGUIContent = EditorGUIUtility.TrTextContent("Light Direction", "The source of the light direction used for the caustics projection mapping.");
            public static readonly GUIContent FixedLightDirectionGUIContent = EditorGUIUtility.TrTextContent("Direction", "The fixed direction of the light used for the caustics projection mapping.");
            public static readonly GUIContent FadeAmountGUIContent = EditorGUIUtility.TrTextContent("Fade Amount", "How much to fade the caustics effect at the edges of the volume.");
            public static readonly GUIContent FadeHardnessGUIContent = EditorGUIUtility.TrTextContent("Fade Hardness", "How harsh the border of the fade should be.");
            public static readonly GUIContent SceneLuminanceMaskStrengthGUIContent = EditorGUIUtility.TrTextContent("Luminance Mask", "How much to mask the light based on the scene luminance.");
            public static readonly GUIContent ShadowMaskStrengthGUIContent = EditorGUIUtility.TrTextContent("Shadow Mask", "How much to mask the light based on the main light shadows.");
            public static readonly GUIContent DisplayDebugOverlayGUIContent = EditorGUIUtility.TrTextContent("Show Bounds", "Visualize the Caustics Volume Bounds in the Scene View.");
        }
    }
}