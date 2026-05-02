Shader "Ameye/Water Caustics for URP"
{
    Properties
    {
        [NoScaleOffset] _CausticsTexture("_CausticsTexture", 2D) = "white" {}
        _CausticsStrength("_CausticsStrength", Range(0.0, 1.0)) = 0.1
        _CausticsSplit("_CausticsSplit", Range(0.0, 0.5)) = 0.0
        _CausticsScale("_CausticsScale", Range(0.01, 4.0)) = 2.0
        _CausticsSpeed("_CausticsSpeed", Range(0.0, 0.3)) = 0.1
        _CausticsNdotLMaskStrength("_CausticsNdotLMaskStrength", Range(0.0, 1.0)) = 0.0
        _CausticsSceneLuminanceMaskStrength("_CausticsSceneLuminanceMaskStrength", Range(0.0, 1.0)) = 0.0
        _CausticsShadowMaskStrength("_CausticsShadowMaskStrength", Range(0.0, 1.0)) = 0.0
        _CausticsFadeAmount("_CausticsFadeAmount", Range(0.0, 1.0)) = 0.5
        _CausticsFadeHardness("_CausticsFadeHardness", Range(0.5, 1.0)) = 1.0

        //[Header(Blending)]
        //[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("_ScrBlend", float) = 2.0
        //[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("_DstBlend", float) = 0.0
    }

    SubShader
    {
        // Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        // LOD 100

        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Front

            Blend One One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog
            #pragma multi_compile _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma shader_feature FIXED_LIGHT_DIRECTION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_CausticsTexture);
            SAMPLER(sampler_CausticsTexture);

            CBUFFER_START(UnityPerMaterial)
            half _CausticsScale;
            half _CausticsSpeed;
            half _CausticsSplit;
            half _CausticsNdotLMaskStrength;
            half _CausticsSceneLuminanceMaskStrength;
            half _CausticsShadowMaskStrength;
            half _CausticsStrength;
            half _CausticsFadeAmount;
            half _CausticsFadeHardness;
            CBUFFER_END

            half4x4 _MainLightDirection;
            half4x4 _FixedLightDirection;

            half2 Panner(half2 uv, half speed, half tiling)
            {
                half2 d = half2(1, 0);
                return (d * _Time.y * speed) + (uv * tiling);
            }

            half3 SampleCaustics(half2 uv, half split)
            {
                half2 uv1 = uv + half2(split, split);
                half2 uv2 = uv + half2(split, -split);
                half2 uv3 = uv + half2(-split, -split);

                half r = SAMPLE_TEXTURE2D_LOD(_CausticsTexture, sampler_CausticsTexture, uv1, 0).r;
                half g = SAMPLE_TEXTURE2D_LOD(_CausticsTexture, sampler_CausticsTexture, uv2, 0).r;
                half b = SAMPLE_TEXTURE2D_LOD(_CausticsTexture, sampler_CausticsTexture, uv3, 0).r;

                return half3(r, g, b);
            }

            float SampleMainLightShadows(float3 WorldPos)
            {
                float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
                return MainLightShadow(shadowCoord, WorldPos, half4(1, 1, 1, 1), _MainLightOcclusionProbes);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                // calculate position in screen-space coordinates
                float2 positionNDC = IN.positionCS.xy / _ScaledScreenParams.xy;
                //float2 positionNDC = GetNormalizedScreenSpaceUV(IN.positionCS.xy); // (alternative)

                // sample scene depth using screen-space coordinates
                #if UNITY_REVERSED_Z
                real depth = SampleSceneDepth(positionNDC);
                #else
                real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(positionNDC));
                #endif

                // calculate position in world-space coordinates
                float3 positionWS = ComputeWorldSpacePosition(positionNDC, depth, UNITY_MATRIX_I_VP);

                // calculate caustics texture UV coordinates (influenced by light direction)
                #if FIXED_LIGHT_DIRECTION
                half2 uv = mul(float4(positionWS.xyz, 1.0), _FixedLightDirection).xy;
                #else
                half2 uv = mul(float4(positionWS.xyz, 1.0), _MainLightDirection).xy;
                #endif

                // create panning UVs for the caustics
                half2 uv1 = Panner(uv, 0.75 * _CausticsSpeed, 1 / _CausticsScale);
                half2 uv2 = Panner(uv, 1 * _CausticsSpeed, -1 / _CausticsScale);

                // sample the caustics
                _CausticsSplit *= 0.015;
                half3 tex1 = SampleCaustics(uv1, _CausticsSplit);
                half3 tex2 = SampleCaustics(uv2, _CausticsSplit);

                // combine the caustics
                half3 caustics = min(tex1, tex2) * _CausticsStrength * 100;

                // calculate position in object-space coordinates
                float3 positionOS = TransformWorldToObject(positionWS);

                // create bounding box mask
                float boundingBoxMask = all(step(positionOS, 0.5) * (1 - step(positionOS, -0.5)));

                // edge fade mask
                half sphereMask = 1 - saturate(
                    (distance(positionOS, 0) - (1.0 - _CausticsFadeAmount)) / (1 - _CausticsFadeHardness));
                // 1 - saturate((distance(Coords, Center) - Radius) / (1 - Hardness));
                half edgeFadeMask = sphereMask; // = smoothstep(0, _CausticsFade, mask);

                // sample shadows
                half shadowMask = 1.0 - (1.0 - SampleMainLightShadows(positionWS)) * _CausticsShadowMaskStrength;

                // luminance mask
                half3 sceneColor = SampleSceneColor(positionNDC);
                half sceneLuminance = Luminance(sceneColor);
                half luminanceMask = smoothstep(_CausticsSceneLuminanceMaskStrength,
                                                _CausticsSceneLuminanceMaskStrength + 0.1, sceneLuminance);
                //half luminanceMask = lerp(1, sceneLuminance, _CausticsLuminanceMaskStrength);

                // NdotL mask
                //Light mainLight = GetMainLight();
                //float3 normal = SampleSceneNormals(positionNDC);
                //half ndotlmask = lerp(1.0, saturate(dot(normal, mainLight.direction)), _CausticsNdotLMaskStrength);

                // mask the caustics
                caustics *= boundingBoxMask;
                caustics *= edgeFadeMask;
                caustics *= luminanceMask;
                caustics *= shadowMask;
                //caustics *= ndotlmask;

                return half4(caustics, 1.0);
            }
            ENDHLSL
        }
    }
    //CustomEditor "WaterCausticsForURP.CausticsShaderGUI"
}