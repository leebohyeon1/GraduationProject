using UnityEngine;

namespace Caustics
{
    static class ShaderPropertyId
    {
        public static readonly int Texture = Shader.PropertyToID("_CausticsTexture");
        public static readonly int Strength = Shader.PropertyToID("_CausticsStrength");
        public static readonly int Split = Shader.PropertyToID("_CausticsSplit");
        public static readonly int Scale = Shader.PropertyToID("_CausticsScale");
        public static readonly int Speed = Shader.PropertyToID("_CausticsSpeed");
        public static readonly int FadeAmount = Shader.PropertyToID("_CausticsFadeAmount");
        public static readonly int FadeHardness = Shader.PropertyToID("_CausticsFadeHardness");
        public static readonly int LuminanceMaskStrength = Shader.PropertyToID("_CausticsSceneLuminanceMaskStrength");
        public static readonly int ShadowMaskStrength = Shader.PropertyToID("_CausticsShadowMaskStrength");
        public static readonly int FixedLightDirectionProperty = Shader.PropertyToID("_FixedLightDirection");
    }
}
