Shader "SKGames/Height Fog (Unity 6 URP)"
{
    Properties
    {
        [Header(Fog properties)]
        [Enum(World,1,Local,0)] _FogRelativeWorldOrLocal("Fog Simulation Space", Int) = 1
        _FogColor("Fog Color", Color) = (1,1,1,1)
        [HDR] _FogEmissionColor("Fog Emission Color", Color) = (1,1,1,1)
        _FogMin("Height Fog Min", Float) = -20
        _FogMax("Height Fog Max", Float) = 0
        _EmissionPower("Emission Power", Range(0, 1)) = 1
        [PowerSlider(3.0)] _FogEmissionPower("Fog Emission Power", Range(0, 100)) = 20
        [PowerSlider(3.0)] _FogEmissionFalloff("Fog Emission Falloff", Range(0.01, 20)) = 0.5
        [PowerSlider(3.0)] _FogFalloff("Fog Falloff", Range(0.01, 20)) = 1
        
        [Header(STANDARD fog properties overrides)]
        _STANDARD_FOG("Combine with STANDARD fog", Float) = 0
        _OVERRIDE_FOG_COLOR("Override STANDARD fog color", Float) = 0
        
        [Header(Fog animation properties)]
        _ANIMATION("Use fog animation", Float) = 0
        _FogWaveSpeedX("Fog Wave Speed X", Range(-50, 50)) = 2
        _FogWaveSpeedZ("Fog Wave Speed Z", Range(-50, 50)) = 2
        _FogWaveAmplitudeX("Fog Wave Amplitude X", Range(0, 1)) = 0.3
        _FogWaveAmplitudeZ("Fog Wave Amplitude Z", Range(0, 1)) = 0.3
        _FogWaveFreqX("Fog Frequency X", Range(0, 20)) = 0.5
        _FogWaveFreqZ("Fog Frequency Z", Range(0, 20)) = 0.5

        // Standard Lit Properties
        [HideInInspector] _WorkflowMode("WorkflowMode", Float) = 1.0
        _Color("Color", Color) = (0.5,0.5,0.5,1)
        _MainTex("Albedo", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}
        _SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0
        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}
        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
        _ReceiveShadows("Receive Shadows", Float) = 1.0

        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
    }

    SubShader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        LOD 300

        Pass
        {
            Name "StandardLit"
            Tags{"LightMode" = "UniversalForward"}

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            
            // Unity 6 (URP 17+) Shader Macros
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _FORWARD_PLUS
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICSPECGLOSSMAP
            #pragma shader_feature _OCCLUSIONMAP

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _FogColor, _FogEmissionColor, _MainTex_ST, _Color;
                float  _FogMin, _FogMax, _FogEmissionPower, _FogEmissionFalloff, _FogFalloff, _FogRelativeWorldOrLocal, _EmissionPower;
                float  _FogWaveSpeedX,_FogWaveSpeedZ,_FogWaveAmplitudeX,_FogWaveAmplitudeZ,_FogWaveFreqX,_FogWaveFreqZ, _ANIMATION, _STANDARD_FOG, _OVERRIDE_FOG_COLOR;
                
                float _Cutoff, _Glossiness, _GlossMapScale, _SmoothnessTextureChannel, _Metallic;
                float4 _SpecColor;
                float _BumpScale, _OcclusionStrength;
                float4 _EmissionColor;
            CBUFFER_END

            TEXTURE2D(_MainTex);          SAMPLER(sampler_MainTex);
            TEXTURE2D(_MetallicGlossMap); SAMPLER(sampler_MetallicGlossMap);
            TEXTURE2D(_SpecGlossMap);     SAMPLER(sampler_SpecGlossMap);
            TEXTURE2D(_BumpMap);          SAMPLER(sampler_BumpMap);
            TEXTURE2D(_OcclusionMap);     SAMPLER(sampler_OcclusionMap);
            TEXTURE2D(_EmissionMap);      SAMPLER(sampler_EmissionMap);

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
                float2 uvLM         : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                float2 uvLM                     : TEXCOORD1;
                float4 positionWSAndFogFactor   : TEXCOORD2; 
                float3 normalWS                 : TEXCOORD3;
                float3 positionOS               : TEXCOORD4;
                #if _NORMALMAP
                float3 tangentWS                : TEXCOORD5;
                float3 bitangentWS              : TEXCOORD6;
                #endif
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                float4 shadowCoord              : TEXCOORD7;
                #endif
                float4 positionCS               : SV_POSITION;
            };

            float3 waveCalc(float3 worldPos)
            {
                if (_ANIMATION > 0) {
                    float timeX = _Time.x * 20.0 * -_FogWaveSpeedX;
                    float timeZ = _Time.x * 20.0 * -_FogWaveSpeedZ;
                    float waveValueX = sin(timeX + worldPos.x * _FogWaveFreqX) * _FogWaveAmplitudeX;
                    float waveValueZ = sin(timeZ + worldPos.z * _FogWaveFreqZ) * _FogWaveAmplitudeZ;
                    float waveValue = (waveValueX + waveValueZ) * 0.5;
                    return float3(worldPos.x, worldPos.y + waveValue, worldPos.z);
                }
                return worldPos;
            }

            float3 DoHeightFogStuff(float3 color, Varyings input) {
                float3 localPos = waveCalc(input.positionOS);
                float3 wPos = waveCalc(input.positionWSAndFogFactor.xyz);
                
                float localWeight = saturate(1.0 - _FogRelativeWorldOrLocal);
                float worldWeight = saturate(_FogRelativeWorldOrLocal);
                float currentHeight = (localPos.y * localWeight) + (wPos.y * worldWeight);
                
                float lerpValue = saturate((currentHeight - _FogMin) / (_FogMax - _FogMin + 0.0001));
                lerpValue = 1.0 - pow(lerpValue, _FogFalloff);
                
                float3 emission = _FogColor.rgb + _FogEmissionColor.rgb * _FogEmissionPower;
                float3 fogEmissionColor = lerp(_FogColor.rgb, emission, pow(lerpValue, _FogEmissionFalloff));
                
                return lerp(color, fogEmissionColor, lerpValue);
            }

            Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                output.uvLM = input.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw;

                output.positionWSAndFogFactor = float4(vertexInput.positionWS, ComputeFogFactor(vertexInput.positionCS.z));
                output.normalWS = vertexNormalInput.normalWS;

                #if _NORMALMAP
                output.tangentWS = vertexNormalInput.tangentWS;
                output.bitangentWS = vertexNormalInput.bitangentWS;
                #endif

                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                output.shadowCoord = GetShadowCoord(vertexInput);
                #endif

                output.positionOS = input.positionOS.xyz;
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            half4 LitPassFragment(Varyings input) : SV_Target
            {
                half4 albedoAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedoAlpha.rgb * _Color.rgb;
                surfaceData.alpha = albedoAlpha.a * _Color.a;
                surfaceData.metallic = _Metallic;
                surfaceData.specular = _SpecColor.rgb;
                surfaceData.smoothness = _Glossiness;
                surfaceData.occlusion = _OcclusionStrength;
                
                #if _EMISSION
                surfaceData.emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb * _EmissionColor.rgb;
                #endif

                #if _NORMALMAP
                half4 sampleNormal = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv);
                half3 normalTS = UnpackNormalScale(sampleNormal, _BumpScale);
                half3 normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS, input.bitangentWS, input.normalWS));
                #else
                half3 normalWS = input.normalWS;
                #endif
                normalWS = SafeNormalize(normalWS);

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWSAndFogFactor.xyz;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = SafeNormalize(GetCameraPositionWS() - inputData.positionWS);
                
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                inputData.shadowCoord = input.shadowCoord;
                #else
                inputData.shadowCoord = float4(0,0,0,0);
                #endif

                #ifdef LIGHTMAP_ON
                inputData.bakedGI = SampleLightmap(input.uvLM, normalWS);
                #else
                inputData.bakedGI = SampleSH(normalWS);
                #endif

                inputData.fogCoord = input.positionWSAndFogFactor.w;
                inputData.shadowMask = half4(1,1,1,1);
                
                // Unity 6 PBR Lighting calculation
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                
                // Apply custom Height Fog
                color.rgb = DoHeightFogStuff(color.rgb, input);
                
                return color;
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
        UsePass "Universal Render Pipeline/Lit/Meta"
    }
    
    // Fallback to modern URP lit editor GUI
    FallBack "Hidden/Universal Render Pipeline/Lit"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.LitShader"
}