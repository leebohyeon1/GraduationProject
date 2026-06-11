Shader "SeeThrough/LeafSeeThroughShader"
{
    Properties
    {
        [ToggleUI]_EnableFlipNormal("EnableFlipNormal", Float) = 0
        _BaseGlancingAngleCut("Base Glancing Angle Cut", Range(0, 1)) = 1
        [NoScaleOffset]_BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_MetallicMap("MetallicMap", 2D) = "white" {}
        _Metallic("Metallic", Range(0, 1)) = 0
        _Smoothness("Smoothness", Range(0, 1)) = 0
        [NoScaleOffset]_OcclusionMap("OcclusionMap", 2D) = "white" {}
        [Normal][NoScaleOffset]_Normal_Map("Normal Map", 2D) = "bump" {}
        [NoScaleOffset]_Emission_Map("Emission Map", 2D) = "white" {}
        [HDR]_EmissionColor("EmissionColor", Color) = (0, 0, 0, 1)
        _SSS_Color("SSS Color", Color) = (1, 1, 1, 1)
        _SSS_Intensity("SSS Intensity", Range(0, 50)) = 6.3
        _SSS_Scattering("SSS Scattering", Range(0.1, 50)) = 0.1
        _SSS_Distortion("SSS Distortion", Vector, 3) = (-1, -1, 0, 0)
        _AOStrength("AOStrength", Float) = 0.5
        _Gradient_Offset("Gradient Offset", Float) = 0
        _GradientColor("GradientColor", Color) = (0, 0, 0, 1)
        _CutHeight("CutHeight", Float) = -2.47
        _EdgeSoftness("EdgeSoftness", Float) = 2.47
        _Dither("Dither", Range(0, 1)) = 0
        _LeadWindSpeed("LeadWindSpeed", Float) = 0.8
        _LeafWindScale("LeafWindScale", Float) = 5
        _BendSpeed("BendSpeed", Float) = 1.41
        _BendDirection("BendDirection", Vector, 4) = (0, 0, 0, 0)
        _Offset("Offset", Vector, 3) = (0, 0, 0, 0)
        [HideInInspector]_WorkflowMode("_WorkflowMode", Float) = 1
        [HideInInspector]_CastShadows("_CastShadows", Float) = 1
        [HideInInspector]_ReceiveShadows("_ReceiveShadows", Float) = 1
        [HideInInspector]_Surface("_Surface", Float) = 0
        [HideInInspector]_Blend("_Blend", Float) = 0
        [HideInInspector]_AlphaClip("_AlphaClip", Float) = 1
        [HideInInspector]_BlendModePreserveSpecular("_BlendModePreserveSpecular", Float) = 1
        [HideInInspector]_SrcBlend("_SrcBlend", Float) = 1
        [HideInInspector]_DstBlend("_DstBlend", Float) = 0
        [HideInInspector]_SrcBlendAlpha("_SrcBlendAlpha", Float) = 1
        [HideInInspector]_DstBlendAlpha("_DstBlendAlpha", Float) = 0
        [HideInInspector][ToggleUI]_ZWrite("_ZWrite", Float) = 1
        [HideInInspector]_ZWriteControl("_ZWriteControl", Float) = 0
        [HideInInspector]_ZTest("_ZTest", Float) = 4
        [HideInInspector]_Cull("_Cull", Float) = 0
        [HideInInspector]_AlphaToMask("_AlphaToMask", Float) = 1
        [HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "UniversalMaterialType" = "Lit"
            "Queue"="AlphaTest"
            "DisableBatching"="False"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalLitSubTarget"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
        
        // Render State
        Cull [_Cull]
        Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
        ZTest [_ZTest]
        ZWrite [_ZWrite]
        AlphaToMask [_AlphaToMask]
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
        #pragma multi_compile _ LIGHTMAP_BICUBIC_SAMPLING
        #pragma multi_compile _ REFLECTION_PROBE_ROTATION
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
        #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ _LIGHT_LAYERS
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_fragment _ _LIGHT_COOKIES
        #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
        #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
        #pragma shader_feature_fragment _ _SURFACE_TYPE_TRANSPARENT
        #pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON
        #pragma shader_feature_local_fragment _ _ALPHAMODULATE_ON
        #pragma shader_feature_local_fragment _ _ALPHATEST_ON
        #pragma shader_feature_local_fragment _ _SPECULAR_SETUP
        #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_COLOR
        #define GRAPH_VERTEX_USES_TIME_PARAMETERS_INPUT
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define VARYINGS_NEED_SHADOW_COORD
        #define VARYINGS_NEED_CULLFACE
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float4 color;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
             float4 probeOcclusion;
            #endif
             float4 fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
             float4 VertexColor;
             float FaceSign;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV : INTERP0;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV : INTERP1;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP2;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
             float4 probeOcclusion : INTERP3;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord : INTERP4;
            #endif
             float4 tangentWS : INTERP5;
             float4 texCoord0 : INTERP6;
             float4 color : INTERP7;
             float4 fogFactorAndVertexLight : INTERP8;
             float3 positionWS : INTERP9;
             float3 normalWS : INTERP10;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
            output.probeOcclusion = input.probeOcclusion;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
            output.probeOcclusion = input.probeOcclusion;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float _CutHeight;
        float _EdgeSoftness;
        float _Dither;
        float4 _Normal_Map_TexelSize;
        float4 _MetallicMap_TexelSize;
        float _Smoothness;
        float4 _OcclusionMap_TexelSize;
        float4 _Emission_Map_TexelSize;
        float4 _EmissionColor;
        float _Metallic;
        float _LeadWindSpeed;
        float _LeafWindScale;
        float _BendSpeed;
        float4 _BendDirection;
        float3 _Offset;
        float _BaseGlancingAngleCut;
        float _EnableFlipNormal;
        float _SSS_Scattering;
        float4 _SSS_Color;
        float _SSS_Intensity;
        float3 _SSS_Distortion;
        float _AOStrength;
        float _Gradient_Offset;
        float4 _GradientColor;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_Normal_Map);
        SAMPLER(sampler_Normal_Map);
        TEXTURE2D(_MetallicMap);
        SAMPLER(sampler_MetallicMap);
        TEXTURE2D(_OcclusionMap);
        SAMPLER(sampler_OcclusionMap);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
        {
             float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
             Out = lerp(Min, Max, randomno);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
        {
            float x; Hash_Tchou_2_1_float(p, x);
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }
        
        void Unity_GradientNoise_Deterministic_float (float2 UV, float3 Scale, out float Out)
        {
            float2 p = UV * Scale.xy;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        // unity-custom-func-begin
        void GetMainLight_float(out float3 Direction, out float3 Color, out float ShadowAtten){
            #if defined(SHADERGRAPH_PREVIEW)
            
                Direction = float3(0.5, 0.5, 0);
            
                Color = 1;
            
                ShadowAtten = 1;
            
            #else
            
                #if defined(UNIVERSAL_LIGHTING_INCLUDED)
            
                    Light mainLight = GetMainLight();
            
                    Direction = mainLight.direction;
            
                    Color = mainLight.color;
            
                    ShadowAtten = mainLight.shadowAttenuation;
            
                #else
            
                    Direction = float3(0.5, 0.5, 0);
            
                    Color = 1;
            
                    ShadowAtten = 1;
            
                #endif
            
            #endif
        }
        // unity-custom-func-end
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_Negate_float3(float3 In, out float3 Out)
        {
            Out = -1 * In;
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDX' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddx(In);
        }
        
        void Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDY' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddy(In);
        }
        
        void Unity_CrossProduct_float(float3 A, float3 B, out float3 Out)
        {
            Out = cross(A, B);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Split_e6ba90e06c194240888d9b07fce58a5d_R_1_Float = IN.ObjectSpacePosition[0];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float = IN.ObjectSpacePosition[1];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_B_3_Float = IN.ObjectSpacePosition[2];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_A_4_Float = 0;
            float _Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float = _BendSpeed;
            float3 _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3 = _Offset;
            float3 _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3;
            Unity_Add_float3(SHADERGRAPH_OBJECT_POSITION, _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3, _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3);
            float _Split_74566faebea64f928d3ab3395dea96c0_R_1_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[0];
            float _Split_74566faebea64f928d3ab3395dea96c0_G_2_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[1];
            float _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[2];
            float _Split_74566faebea64f928d3ab3395dea96c0_A_4_Float = 0;
            float2 _Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2 = float2(_Split_74566faebea64f928d3ab3395dea96c0_R_1_Float, _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float);
            float _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float;
            Unity_RandomRange_float(_Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2, float(0.1), float(1), _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float);
            float _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float;
            Unity_Multiply_float_float(_Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float, _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float);
            float _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.z, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float, _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float);
            float _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float;
            Unity_Divide_float(_Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float, float(100), _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float);
            float _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float;
            Unity_Multiply_float_float(_Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float, _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float, _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float);
            float _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float;
            Unity_Add_float(_Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float, float(1), _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float);
            float _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float;
            Unity_Multiply_float_float(_Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float);
            float _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float);
            float _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float;
            Unity_Subtract_float(_Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float);
            float4 _Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4 = _BendDirection;
            float4 _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4, (_RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4);
            float4 _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4, _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4);
            float _Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[0];
            float _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[1];
            float _Split_2e4561fe3a484551a112182bf2ae2954_B_3_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[2];
            float _Split_2e4561fe3a484551a112182bf2ae2954_A_4_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[3];
            float4 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4;
            float3 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3;
            float2 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2;
            Unity_Combine_float(_Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float, float(0), _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float, float(0), _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2);
            float3 _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3;
            Unity_Add_float3(IN.WorldSpacePosition, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3);
            float3 _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3;
            _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3 = TransformWorldToObject(_Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3.xyz);
            float _Split_86265132a9074281af78965592fb42d4_R_1_Float = IN.VertexColor[0];
            float _Split_86265132a9074281af78965592fb42d4_G_2_Float = IN.VertexColor[1];
            float _Split_86265132a9074281af78965592fb42d4_B_3_Float = IN.VertexColor[2];
            float _Split_86265132a9074281af78965592fb42d4_A_4_Float = IN.VertexColor[3];
            float4 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4;
            float3 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3;
            float2 _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2;
            Unity_Combine_float(_Split_86265132a9074281af78965592fb42d4_R_1_Float, _Split_86265132a9074281af78965592fb42d4_G_2_Float, float(0), float(0), _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2);
            float3 _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3;
            Unity_Lerp_float3(_Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3, IN.ObjectSpacePosition, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3);
            float _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float = _LeadWindSpeed;
            float _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float, _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float);
            float2 _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float.xx), _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2);
            float _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float = _LeafWindScale;
            float _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2, _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float, _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float);
            float3 _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            Unity_Lerp_float3(IN.ObjectSpacePosition, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3, (_GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float.xxx), _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3);
            description.Position = _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float3 Specular;
            float Smoothness;
            float Occlusion;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Split_20d61187fc734d028a2e0283ab610e2e_R_1_Float = IN.VertexColor[0];
            float _Split_20d61187fc734d028a2e0283ab610e2e_G_2_Float = IN.VertexColor[1];
            float _Split_20d61187fc734d028a2e0283ab610e2e_B_3_Float = IN.VertexColor[2];
            float _Split_20d61187fc734d028a2e0283ab610e2e_A_4_Float = IN.VertexColor[3];
            float _Property_deb326794dfb4908b3033da0b6291d6f_Out_0_Float = _AOStrength;
            float _Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float;
            Unity_Lerp_float(float(1), _Split_20d61187fc734d028a2e0283ab610e2e_A_4_Float, _Property_deb326794dfb4908b3033da0b6291d6f_Out_0_Float, _Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float);
            float4 _Property_30c5b5ed235e4330a6f9102cb3c56835_Out_0_Vector4 = _BaseColor;
            float4 _Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float.xxxx), _Property_30c5b5ed235e4330a6f9102cb3c56835_Out_0_Vector4, _Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4);
            float4 _Property_afb5f1e2fd284814bcf9c1ad4b9fc121_Out_0_Vector4 = _GradientColor;
            float4 _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4 = IN.uv0;
            float _Split_708de473eabc4313b3f7bc1c5d62096a_R_1_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[0];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_G_2_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[1];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_B_3_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[2];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_A_4_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[3];
            float _OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float;
            Unity_OneMinus_float(_Split_708de473eabc4313b3f7bc1c5d62096a_G_2_Float, _OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float);
            float _Property_77a6800940754a35be5fcef88a3a155e_Out_0_Float = _Gradient_Offset;
            float _Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float;
            Unity_Add_float(_OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float, _Property_77a6800940754a35be5fcef88a3a155e_Out_0_Float, _Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float);
            float _Saturate_6856939e57034af985ed339413123d4e_Out_1_Float;
            Unity_Saturate_float(_Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float, _Saturate_6856939e57034af985ed339413123d4e_Out_1_Float);
            float4 _Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4;
            Unity_Lerp_float4(_Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4, _Property_afb5f1e2fd284814bcf9c1ad4b9fc121_Out_0_Vector4, (_Saturate_6856939e57034af985ed339413123d4e_Out_1_Float.xxxx), _Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4);
            float _Property_d5903e624613450ca79764a0d69f2fdc_Out_0_Boolean = _EnableFlipNormal;
            UnityTexture2D _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Normal_Map);
            float4 _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.tex, _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.samplerstate, _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4);
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_R_4_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.r;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_G_5_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.g;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_B_6_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.b;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_A_7_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.a;
            float _IsFrontFace_19a764a852494650ab4318b8c36ff8d2_Out_0_Boolean = max(0, IN.FaceSign.x);
            float _Float_58e987145ca94b5e9adbc929e55b4b6b_Out_0_Float = float(-1);
            float _Multiply_78ea49f65ba140139105613218fe61a8_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_B_6_Float, _Float_58e987145ca94b5e9adbc929e55b4b6b_Out_0_Float, _Multiply_78ea49f65ba140139105613218fe61a8_Out_2_Float);
            float _Branch_ac9e2d676e0845dbabb82c7b506f7262_Out_3_Float;
            Unity_Branch_float(_IsFrontFace_19a764a852494650ab4318b8c36ff8d2_Out_0_Boolean, _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_B_6_Float, _Multiply_78ea49f65ba140139105613218fe61a8_Out_2_Float, _Branch_ac9e2d676e0845dbabb82c7b506f7262_Out_3_Float);
            float4 _Combine_5796639be6c5497d82a1e1a45094c6f6_RGBA_4_Vector4;
            float3 _Combine_5796639be6c5497d82a1e1a45094c6f6_RGB_5_Vector3;
            float2 _Combine_5796639be6c5497d82a1e1a45094c6f6_RG_6_Vector2;
            Unity_Combine_float(_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_R_4_Float, _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_G_5_Float, _Branch_ac9e2d676e0845dbabb82c7b506f7262_Out_3_Float, float(0), _Combine_5796639be6c5497d82a1e1a45094c6f6_RGBA_4_Vector4, _Combine_5796639be6c5497d82a1e1a45094c6f6_RGB_5_Vector3, _Combine_5796639be6c5497d82a1e1a45094c6f6_RG_6_Vector2);
            float4 _Branch_885bdab74c804361805e365abb412f7e_Out_3_Vector4;
            Unity_Branch_float4(_Property_d5903e624613450ca79764a0d69f2fdc_Out_0_Boolean, _Combine_5796639be6c5497d82a1e1a45094c6f6_RGBA_4_Vector4, _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4, _Branch_885bdab74c804361805e365abb412f7e_Out_3_Vector4);
            UnityTexture2D _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Emission_Map);
            float4 _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.tex, _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.samplerstate, _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_R_4_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.r;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_G_5_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.g;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_B_6_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.b;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_A_7_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.a;
            float4 _Property_ba712bb9090a4c52a43f38f92fd73591_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_EmissionColor) : _EmissionColor;
            float4 _Multiply_16c8957045404e8cb4dc99dec57c9601_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4, _Property_ba712bb9090a4c52a43f38f92fd73591_Out_0_Vector4, _Multiply_16c8957045404e8cb4dc99dec57c9601_Out_2_Vector4);
            float3 _Property_dcdf3d2648794cddb9212380f6973d15_Out_0_Vector3 = _SSS_Distortion;
            float _Split_3caa93318451401d87ad12e7a907097f_R_1_Float = _Property_dcdf3d2648794cddb9212380f6973d15_Out_0_Vector3[0];
            float _Split_3caa93318451401d87ad12e7a907097f_G_2_Float = _Property_dcdf3d2648794cddb9212380f6973d15_Out_0_Vector3[1];
            float _Split_3caa93318451401d87ad12e7a907097f_B_3_Float = _Property_dcdf3d2648794cddb9212380f6973d15_Out_0_Vector3[2];
            float _Split_3caa93318451401d87ad12e7a907097f_A_4_Float = 0;
            float3 _Multiply_22c18134d5924015ba2ec2a834e22739_Out_2_Vector3;
            Unity_Multiply_float3_float3(IN.WorldSpaceNormal, (_Split_3caa93318451401d87ad12e7a907097f_B_3_Float.xxx), _Multiply_22c18134d5924015ba2ec2a834e22739_Out_2_Vector3);
            float3 _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3;
            float3 _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Color_1_Vector3;
            float _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_ShadowAtten_2_Float;
            GetMainLight_float(_GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3, _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Color_1_Vector3, _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_ShadowAtten_2_Float);
            float _Split_8f9f81fe0bee433ca8736d7047af190c_R_1_Float = _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3[0];
            float _Split_8f9f81fe0bee433ca8736d7047af190c_G_2_Float = _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3[1];
            float _Split_8f9f81fe0bee433ca8736d7047af190c_B_3_Float = _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3[2];
            float _Split_8f9f81fe0bee433ca8736d7047af190c_A_4_Float = 0;
            float _Multiply_eb7953577efa42578ac8713bee265f5b_Out_2_Float;
            Unity_Multiply_float_float(_Split_3caa93318451401d87ad12e7a907097f_R_1_Float, _Split_8f9f81fe0bee433ca8736d7047af190c_R_1_Float, _Multiply_eb7953577efa42578ac8713bee265f5b_Out_2_Float);
            float _Multiply_008c4dc1817d4499bb015a203c3b9fc9_Out_2_Float;
            Unity_Multiply_float_float(_Split_3caa93318451401d87ad12e7a907097f_G_2_Float, _Split_8f9f81fe0bee433ca8736d7047af190c_G_2_Float, _Multiply_008c4dc1817d4499bb015a203c3b9fc9_Out_2_Float);
            float4 _Combine_6028bb2051e2436b810df848e038d344_RGBA_4_Vector4;
            float3 _Combine_6028bb2051e2436b810df848e038d344_RGB_5_Vector3;
            float2 _Combine_6028bb2051e2436b810df848e038d344_RG_6_Vector2;
            Unity_Combine_float(_Multiply_eb7953577efa42578ac8713bee265f5b_Out_2_Float, _Multiply_008c4dc1817d4499bb015a203c3b9fc9_Out_2_Float, _Split_8f9f81fe0bee433ca8736d7047af190c_B_3_Float, float(0), _Combine_6028bb2051e2436b810df848e038d344_RGBA_4_Vector4, _Combine_6028bb2051e2436b810df848e038d344_RGB_5_Vector3, _Combine_6028bb2051e2436b810df848e038d344_RG_6_Vector2);
            float3 _Add_be6bd9b0b67c4b6db449176e8ca930b2_Out_2_Vector3;
            Unity_Add_float3(_Multiply_22c18134d5924015ba2ec2a834e22739_Out_2_Vector3, _Combine_6028bb2051e2436b810df848e038d344_RGB_5_Vector3, _Add_be6bd9b0b67c4b6db449176e8ca930b2_Out_2_Vector3);
            float3 _Normalize_f3cd7974fb394a2cadf32d57b28d9261_Out_1_Vector3;
            Unity_Normalize_float3(_Add_be6bd9b0b67c4b6db449176e8ca930b2_Out_2_Vector3, _Normalize_f3cd7974fb394a2cadf32d57b28d9261_Out_1_Vector3);
            float3 _Negate_1793cf15ec9c407f8e406f36ca089de6_Out_1_Vector3;
            Unity_Negate_float3(_Normalize_f3cd7974fb394a2cadf32d57b28d9261_Out_1_Vector3, _Negate_1793cf15ec9c407f8e406f36ca089de6_Out_1_Vector3);
            float _DotProduct_b92b4de671844a4cb48bdf3543eb8209_Out_2_Float;
            Unity_DotProduct_float3(IN.WorldSpaceViewDirection, _Negate_1793cf15ec9c407f8e406f36ca089de6_Out_1_Vector3, _DotProduct_b92b4de671844a4cb48bdf3543eb8209_Out_2_Float);
            float _Saturate_ac2a94e9b1f44dc999b6a5f585e92ec5_Out_1_Float;
            Unity_Saturate_float(_DotProduct_b92b4de671844a4cb48bdf3543eb8209_Out_2_Float, _Saturate_ac2a94e9b1f44dc999b6a5f585e92ec5_Out_1_Float);
            float _Property_7f4e24b9dccf4656aa1db4f2b6d9c394_Out_0_Float = _SSS_Scattering;
            float _Power_6bffe28ecec947a4978b0e5497d49690_Out_2_Float;
            Unity_Power_float(_Saturate_ac2a94e9b1f44dc999b6a5f585e92ec5_Out_1_Float, _Property_7f4e24b9dccf4656aa1db4f2b6d9c394_Out_0_Float, _Power_6bffe28ecec947a4978b0e5497d49690_Out_2_Float);
            float _Power_bd15f48c24704cf38a062bb3163ede20_Out_2_Float;
            Unity_Power_float(_Power_6bffe28ecec947a4978b0e5497d49690_Out_2_Float, _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_ShadowAtten_2_Float, _Power_bd15f48c24704cf38a062bb3163ede20_Out_2_Float);
            float4 _Property_0c7582d4a9b7493287ae8b1cf4e99a8a_Out_0_Vector4 = _SSS_Color;
            float4 _Multiply_5fff722b6be44e2c8ac7eb45803009da_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Power_bd15f48c24704cf38a062bb3163ede20_Out_2_Float.xxxx), _Property_0c7582d4a9b7493287ae8b1cf4e99a8a_Out_0_Vector4, _Multiply_5fff722b6be44e2c8ac7eb45803009da_Out_2_Vector4);
            float _Property_1572ee45033348219e19d5ae51034997_Out_0_Float = _SSS_Intensity;
            float4 _Multiply_9b5ece35f1b94fd99f2d33c3e4f1d918_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_5fff722b6be44e2c8ac7eb45803009da_Out_2_Vector4, (_Property_1572ee45033348219e19d5ae51034997_Out_0_Float.xxxx), _Multiply_9b5ece35f1b94fd99f2d33c3e4f1d918_Out_2_Vector4);
            float3 _Multiply_66b508c9e81948b8a34cea6baffd763d_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Multiply_9b5ece35f1b94fd99f2d33c3e4f1d918_Out_2_Vector4.xyz), _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Color_1_Vector3, _Multiply_66b508c9e81948b8a34cea6baffd763d_Out_2_Vector3);
            float3 _Add_9595ed0b6087400383dcbc8cdce459f6_Out_2_Vector3;
            Unity_Add_float3((_Multiply_16c8957045404e8cb4dc99dec57c9601_Out_2_Vector4.xyz), _Multiply_66b508c9e81948b8a34cea6baffd763d_Out_2_Vector3, _Add_9595ed0b6087400383dcbc8cdce459f6_Out_2_Vector3);
            UnityTexture2D _Property_a48f9ee2e9074b1ba60bf202e28da2fd_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MetallicMap);
            float4 _SampleTexture2D_998830748c3540528d5f2a45292b994a_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_a48f9ee2e9074b1ba60bf202e28da2fd_Out_0_Texture2D.tex, _Property_a48f9ee2e9074b1ba60bf202e28da2fd_Out_0_Texture2D.samplerstate, _Property_a48f9ee2e9074b1ba60bf202e28da2fd_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_998830748c3540528d5f2a45292b994a_R_4_Float = _SampleTexture2D_998830748c3540528d5f2a45292b994a_RGBA_0_Vector4.r;
            float _SampleTexture2D_998830748c3540528d5f2a45292b994a_G_5_Float = _SampleTexture2D_998830748c3540528d5f2a45292b994a_RGBA_0_Vector4.g;
            float _SampleTexture2D_998830748c3540528d5f2a45292b994a_B_6_Float = _SampleTexture2D_998830748c3540528d5f2a45292b994a_RGBA_0_Vector4.b;
            float _SampleTexture2D_998830748c3540528d5f2a45292b994a_A_7_Float = _SampleTexture2D_998830748c3540528d5f2a45292b994a_RGBA_0_Vector4.a;
            float _IsFrontFace_f3f821f2fc6c405f9a50ce2534b7d5fe_Out_0_Boolean = max(0, IN.FaceSign.x);
            float _Property_2847d1c700f3424b97e54e3225b28133_Out_0_Float = _Metallic;
            float _Branch_ed088f94c2684895aaca8db87cbb4ab5_Out_3_Float;
            Unity_Branch_float(_IsFrontFace_f3f821f2fc6c405f9a50ce2534b7d5fe_Out_0_Boolean, _Property_2847d1c700f3424b97e54e3225b28133_Out_0_Float, float(0), _Branch_ed088f94c2684895aaca8db87cbb4ab5_Out_3_Float);
            float4 _Multiply_b725dae6dfc9431d91df366721b2ac8b_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_998830748c3540528d5f2a45292b994a_RGBA_0_Vector4, (_Branch_ed088f94c2684895aaca8db87cbb4ab5_Out_3_Float.xxxx), _Multiply_b725dae6dfc9431d91df366721b2ac8b_Out_2_Vector4);
            float _IsFrontFace_6e79567bd99b494698a501ec60fdc0b2_Out_0_Boolean = max(0, IN.FaceSign.x);
            float _Property_2f18b6082f284664b06b414e660ca29e_Out_0_Float = _Smoothness;
            float _Multiply_b547fe90c1f74b98b4b7ff44267f574f_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_998830748c3540528d5f2a45292b994a_A_7_Float, _Property_2f18b6082f284664b06b414e660ca29e_Out_0_Float, _Multiply_b547fe90c1f74b98b4b7ff44267f574f_Out_2_Float);
            float _Branch_1da4fbb515fc4aa3af6b097c6556ccec_Out_3_Float;
            Unity_Branch_float(_IsFrontFace_6e79567bd99b494698a501ec60fdc0b2_Out_0_Boolean, _Multiply_b547fe90c1f74b98b4b7ff44267f574f_Out_2_Float, float(0), _Branch_1da4fbb515fc4aa3af6b097c6556ccec_Out_3_Float);
            UnityTexture2D _Property_b4a63982b3a54373afafa669d09acd3b_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_OcclusionMap);
            float4 _SampleTexture2D_286b24c1b019408d8887fae901e34021_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_b4a63982b3a54373afafa669d09acd3b_Out_0_Texture2D.tex, _Property_b4a63982b3a54373afafa669d09acd3b_Out_0_Texture2D.samplerstate, _Property_b4a63982b3a54373afafa669d09acd3b_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_286b24c1b019408d8887fae901e34021_R_4_Float = _SampleTexture2D_286b24c1b019408d8887fae901e34021_RGBA_0_Vector4.r;
            float _SampleTexture2D_286b24c1b019408d8887fae901e34021_G_5_Float = _SampleTexture2D_286b24c1b019408d8887fae901e34021_RGBA_0_Vector4.g;
            float _SampleTexture2D_286b24c1b019408d8887fae901e34021_B_6_Float = _SampleTexture2D_286b24c1b019408d8887fae901e34021_RGBA_0_Vector4.b;
            float _SampleTexture2D_286b24c1b019408d8887fae901e34021_A_7_Float = _SampleTexture2D_286b24c1b019408d8887fae901e34021_RGBA_0_Vector4.a;
            UnityTexture2D _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.tex, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.samplerstate, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_R_4_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.r;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_G_5_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.g;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_B_6_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.b;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.a;
            float _Split_4f8f81177c174703b3b7216f7abcab96_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_4f8f81177c174703b3b7216f7abcab96_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_4f8f81177c174703b3b7216f7abcab96_A_4_Float = 0;
            float _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float = _CutHeight;
            float _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float;
            Unity_Subtract_float(_Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float, _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float, _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float);
            float _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float = _EdgeSoftness;
            float _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float;
            Unity_Divide_float(_Subtract_14c160049cc54391926538d185ebb749_Out_2_Float, _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float, _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float);
            float _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float;
            Unity_Saturate_float(_Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float, _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float);
            float _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float;
            Unity_OneMinus_float(_Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float);
            float _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float, _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float);
            float _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float);
            float _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            Unity_Subtract_float(_Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float, _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float, _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float);
            float3 _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3;
            Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(IN.WorldSpacePosition, _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3);
            float3 _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3;
            Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(IN.WorldSpacePosition, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3);
            float3 _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3;
            Unity_CrossProduct_float(_DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3, _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3);
            float3 _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3;
            Unity_Normalize_float3(_CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3, _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3);
            float _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float;
            Unity_DotProduct_float3(_Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3, IN.WorldSpaceViewDirection, _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float);
            float _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float;
            Unity_Absolute_float(_DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float, _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float);
            float _Property_406505a58d794e15861d235061da0712_Out_0_Float = _BaseGlancingAngleCut;
            float _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float;
            Unity_Multiply_float_float(_Property_406505a58d794e15861d235061da0712_Out_0_Float, 3, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float);
            float _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float;
            Unity_Lerp_float(float(1), _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float);
            float _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float, _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 1, _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float);
            surface.BaseColor = (_Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4.xyz);
            surface.NormalTS = (_Branch_885bdab74c804361805e365abb412f7e_Out_3_Vector4.xyz);
            surface.Emission = _Add_9595ed0b6087400383dcbc8cdce459f6_Out_2_Vector3;
            surface.Metallic = (_Multiply_b725dae6dfc9431d91df366721b2ac8b_Out_2_Vector4).x;
            surface.Specular = IsGammaSpace() ? float3(0.5, 0.5, 0.5) : SRGBToLinear(float3(0.5, 0.5, 0.5));
            surface.Smoothness = _Branch_1da4fbb515fc4aa3af6b097c6556ccec_Out_3_Float;
            surface.Occlusion = _SampleTexture2D_286b24c1b019408d8887fae901e34021_G_5_Float;
            surface.Alpha = _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.uv0 =                                        input.uv0;
            output.VertexColor =                                input.color;
            output.TimeParameters =                             _TimeParameters.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
            BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
            }
        
        // Render State
        Cull [_Cull]
        Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
        ZTest [_ZTest]
        ZWrite [_ZWrite]
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles3 glcore
        #pragma multi_compile_instancing
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
        #pragma multi_compile _ LIGHTMAP_BICUBIC_SAMPLING
        #pragma multi_compile _ REFLECTION_PROBE_ROTATION
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
        #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
        #pragma shader_feature_fragment _ _SURFACE_TYPE_TRANSPARENT
        #pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON
        #pragma shader_feature_local_fragment _ _ALPHAMODULATE_ON
        #pragma shader_feature_local_fragment _ _ALPHATEST_ON
        #pragma shader_feature_local_fragment _ _SPECULAR_SETUP
        #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_COLOR
        #define GRAPH_VERTEX_USES_TIME_PARAMETERS_INPUT
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define VARYINGS_NEED_SHADOW_COORD
        #define VARYINGS_NEED_CULLFACE
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_GBUFFER
        #define _FOG_FRAGMENT 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float4 color;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
             float4 probeOcclusion;
            #endif
             float4 fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
             float4 VertexColor;
             float FaceSign;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV : INTERP0;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV : INTERP1;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP2;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
             float4 probeOcclusion : INTERP3;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord : INTERP4;
            #endif
             float4 tangentWS : INTERP5;
             float4 texCoord0 : INTERP6;
             float4 color : INTERP7;
             float4 fogFactorAndVertexLight : INTERP8;
             float3 positionWS : INTERP9;
             float3 normalWS : INTERP10;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
            output.probeOcclusion = input.probeOcclusion;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(USE_APV_PROBE_OCCLUSION)
            output.probeOcclusion = input.probeOcclusion;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float _CutHeight;
        float _EdgeSoftness;
        float _Dither;
        float4 _Normal_Map_TexelSize;
        float4 _MetallicMap_TexelSize;
        float _Smoothness;
        float4 _OcclusionMap_TexelSize;
        float4 _Emission_Map_TexelSize;
        float4 _EmissionColor;
        float _Metallic;
        float _LeadWindSpeed;
        float _LeafWindScale;
        float _BendSpeed;
        float4 _BendDirection;
        float3 _Offset;
        float _BaseGlancingAngleCut;
        float _EnableFlipNormal;
        float _SSS_Scattering;
        float4 _SSS_Color;
        float _SSS_Intensity;
        float3 _SSS_Distortion;
        float _AOStrength;
        float _Gradient_Offset;
        float4 _GradientColor;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_Normal_Map);
        SAMPLER(sampler_Normal_Map);
        TEXTURE2D(_MetallicMap);
        SAMPLER(sampler_MetallicMap);
        TEXTURE2D(_OcclusionMap);
        SAMPLER(sampler_OcclusionMap);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
        {
             float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
             Out = lerp(Min, Max, randomno);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
        {
            float x; Hash_Tchou_2_1_float(p, x);
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }
        
        void Unity_GradientNoise_Deterministic_float (float2 UV, float3 Scale, out float Out)
        {
            float2 p = UV * Scale.xy;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        // unity-custom-func-begin
        void GetMainLight_float(out float3 Direction, out float3 Color, out float ShadowAtten){
            #if defined(SHADERGRAPH_PREVIEW)
            
                Direction = float3(0.5, 0.5, 0);
            
                Color = 1;
            
                ShadowAtten = 1;
            
            #else
            
                #if defined(UNIVERSAL_LIGHTING_INCLUDED)
            
                    Light mainLight = GetMainLight();
            
                    Direction = mainLight.direction;
            
                    Color = mainLight.color;
            
                    ShadowAtten = mainLight.shadowAttenuation;
            
                #else
            
                    Direction = float3(0.5, 0.5, 0);
            
                    Color = 1;
            
                    ShadowAtten = 1;
            
                #endif
            
            #endif
        }
        // unity-custom-func-end
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_Negate_float3(float3 In, out float3 Out)
        {
            Out = -1 * In;
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDX' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddx(In);
        }
        
        void Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDY' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddy(In);
        }
        
        void Unity_CrossProduct_float(float3 A, float3 B, out float3 Out)
        {
            Out = cross(A, B);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Split_e6ba90e06c194240888d9b07fce58a5d_R_1_Float = IN.ObjectSpacePosition[0];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float = IN.ObjectSpacePosition[1];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_B_3_Float = IN.ObjectSpacePosition[2];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_A_4_Float = 0;
            float _Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float = _BendSpeed;
            float3 _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3 = _Offset;
            float3 _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3;
            Unity_Add_float3(SHADERGRAPH_OBJECT_POSITION, _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3, _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3);
            float _Split_74566faebea64f928d3ab3395dea96c0_R_1_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[0];
            float _Split_74566faebea64f928d3ab3395dea96c0_G_2_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[1];
            float _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[2];
            float _Split_74566faebea64f928d3ab3395dea96c0_A_4_Float = 0;
            float2 _Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2 = float2(_Split_74566faebea64f928d3ab3395dea96c0_R_1_Float, _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float);
            float _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float;
            Unity_RandomRange_float(_Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2, float(0.1), float(1), _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float);
            float _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float;
            Unity_Multiply_float_float(_Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float, _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float);
            float _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.z, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float, _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float);
            float _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float;
            Unity_Divide_float(_Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float, float(100), _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float);
            float _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float;
            Unity_Multiply_float_float(_Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float, _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float, _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float);
            float _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float;
            Unity_Add_float(_Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float, float(1), _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float);
            float _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float;
            Unity_Multiply_float_float(_Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float);
            float _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float);
            float _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float;
            Unity_Subtract_float(_Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float);
            float4 _Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4 = _BendDirection;
            float4 _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4, (_RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4);
            float4 _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4, _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4);
            float _Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[0];
            float _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[1];
            float _Split_2e4561fe3a484551a112182bf2ae2954_B_3_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[2];
            float _Split_2e4561fe3a484551a112182bf2ae2954_A_4_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[3];
            float4 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4;
            float3 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3;
            float2 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2;
            Unity_Combine_float(_Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float, float(0), _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float, float(0), _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2);
            float3 _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3;
            Unity_Add_float3(IN.WorldSpacePosition, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3);
            float3 _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3;
            _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3 = TransformWorldToObject(_Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3.xyz);
            float _Split_86265132a9074281af78965592fb42d4_R_1_Float = IN.VertexColor[0];
            float _Split_86265132a9074281af78965592fb42d4_G_2_Float = IN.VertexColor[1];
            float _Split_86265132a9074281af78965592fb42d4_B_3_Float = IN.VertexColor[2];
            float _Split_86265132a9074281af78965592fb42d4_A_4_Float = IN.VertexColor[3];
            float4 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4;
            float3 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3;
            float2 _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2;
            Unity_Combine_float(_Split_86265132a9074281af78965592fb42d4_R_1_Float, _Split_86265132a9074281af78965592fb42d4_G_2_Float, float(0), float(0), _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2);
            float3 _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3;
            Unity_Lerp_float3(_Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3, IN.ObjectSpacePosition, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3);
            float _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float = _LeadWindSpeed;
            float _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float, _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float);
            float2 _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float.xx), _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2);
            float _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float = _LeafWindScale;
            float _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2, _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float, _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float);
            float3 _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            Unity_Lerp_float3(IN.ObjectSpacePosition, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3, (_GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float.xxx), _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3);
            description.Position = _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float3 Specular;
            float Smoothness;
            float Occlusion;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Split_20d61187fc734d028a2e0283ab610e2e_R_1_Float = IN.VertexColor[0];
            float _Split_20d61187fc734d028a2e0283ab610e2e_G_2_Float = IN.VertexColor[1];
            float _Split_20d61187fc734d028a2e0283ab610e2e_B_3_Float = IN.VertexColor[2];
            float _Split_20d61187fc734d028a2e0283ab610e2e_A_4_Float = IN.VertexColor[3];
            float _Property_deb326794dfb4908b3033da0b6291d6f_Out_0_Float = _AOStrength;
            float _Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float;
            Unity_Lerp_float(float(1), _Split_20d61187fc734d028a2e0283ab610e2e_A_4_Float, _Property_deb326794dfb4908b3033da0b6291d6f_Out_0_Float, _Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float);
            float4 _Property_30c5b5ed235e4330a6f9102cb3c56835_Out_0_Vector4 = _BaseColor;
            float4 _Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float.xxxx), _Property_30c5b5ed235e4330a6f9102cb3c56835_Out_0_Vector4, _Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4);
            float4 _Property_afb5f1e2fd284814bcf9c1ad4b9fc121_Out_0_Vector4 = _GradientColor;
            float4 _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4 = IN.uv0;
            float _Split_708de473eabc4313b3f7bc1c5d62096a_R_1_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[0];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_G_2_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[1];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_B_3_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[2];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_A_4_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[3];
            float _OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float;
            Unity_OneMinus_float(_Split_708de473eabc4313b3f7bc1c5d62096a_G_2_Float, _OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float);
            float _Property_77a6800940754a35be5fcef88a3a155e_Out_0_Float = _Gradient_Offset;
            float _Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float;
            Unity_Add_float(_OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float, _Property_77a6800940754a35be5fcef88a3a155e_Out_0_Float, _Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float);
            float _Saturate_6856939e57034af985ed339413123d4e_Out_1_Float;
            Unity_Saturate_float(_Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float, _Saturate_6856939e57034af985ed339413123d4e_Out_1_Float);
            float4 _Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4;
            Unity_Lerp_float4(_Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4, _Property_afb5f1e2fd284814bcf9c1ad4b9fc121_Out_0_Vector4, (_Saturate_6856939e57034af985ed339413123d4e_Out_1_Float.xxxx), _Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4);
            float _Property_d5903e624613450ca79764a0d69f2fdc_Out_0_Boolean = _EnableFlipNormal;
            UnityTexture2D _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Normal_Map);
            float4 _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.tex, _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.samplerstate, _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4);
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_R_4_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.r;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_G_5_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.g;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_B_6_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.b;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_A_7_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.a;
            float _IsFrontFace_19a764a852494650ab4318b8c36ff8d2_Out_0_Boolean = max(0, IN.FaceSign.x);
            float _Float_58e987145ca94b5e9adbc929e55b4b6b_Out_0_Float = float(-1);
            float _Multiply_78ea49f65ba140139105613218fe61a8_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_B_6_Float, _Float_58e987145ca94b5e9adbc929e55b4b6b_Out_0_Float, _Multiply_78ea49f65ba140139105613218fe61a8_Out_2_Float);
            float _Branch_ac9e2d676e0845dbabb82c7b506f7262_Out_3_Float;
            Unity_Branch_float(_IsFrontFace_19a764a852494650ab4318b8c36ff8d2_Out_0_Boolean, _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_B_6_Float, _Multiply_78ea49f65ba140139105613218fe61a8_Out_2_Float, _Branch_ac9e2d676e0845dbabb82c7b506f7262_Out_3_Float);
            float4 _Combine_5796639be6c5497d82a1e1a45094c6f6_RGBA_4_Vector4;
            float3 _Combine_5796639be6c5497d82a1e1a45094c6f6_RGB_5_Vector3;
            float2 _Combine_5796639be6c5497d82a1e1a45094c6f6_RG_6_Vector2;
            Unity_Combine_float(_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_R_4_Float, _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_G_5_Float, _Branch_ac9e2d676e0845dbabb82c7b506f7262_Out_3_Float, float(0), _Combine_5796639be6c5497d82a1e1a45094c6f6_RGBA_4_Vector4, _Combine_5796639be6c5497d82a1e1a45094c6f6_RGB_5_Vector3, _Combine_5796639be6c5497d82a1e1a45094c6f6_RG_6_Vector2);
            float4 _Branch_885bdab74c804361805e365abb412f7e_Out_3_Vector4;
            Unity_Branch_float4(_Property_d5903e624613450ca79764a0d69f2fdc_Out_0_Boolean, _Combine_5796639be6c5497d82a1e1a45094c6f6_RGBA_4_Vector4, _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4, _Branch_885bdab74c804361805e365abb412f7e_Out_3_Vector4);
            UnityTexture2D _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Emission_Map);
            float4 _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.tex, _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.samplerstate, _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_R_4_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.r;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_G_5_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.g;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_B_6_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.b;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_A_7_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.a;
            float4 _Property_ba712bb9090a4c52a43f38f92fd73591_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_EmissionColor) : _EmissionColor;
            float4 _Multiply_16c8957045404e8cb4dc99dec57c9601_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4, _Property_ba712bb9090a4c52a43f38f92fd73591_Out_0_Vector4, _Multiply_16c8957045404e8cb4dc99dec57c9601_Out_2_Vector4);
            float3 _Property_dcdf3d2648794cddb9212380f6973d15_Out_0_Vector3 = _SSS_Distortion;
            float _Split_3caa93318451401d87ad12e7a907097f_R_1_Float = _Property_dcdf3d2648794cddb9212380f6973d15_Out_0_Vector3[0];
            float _Split_3caa93318451401d87ad12e7a907097f_G_2_Float = _Property_dcdf3d2648794cddb9212380f6973d15_Out_0_Vector3[1];
            float _Split_3caa93318451401d87ad12e7a907097f_B_3_Float = _Property_dcdf3d2648794cddb9212380f6973d15_Out_0_Vector3[2];
            float _Split_3caa93318451401d87ad12e7a907097f_A_4_Float = 0;
            float3 _Multiply_22c18134d5924015ba2ec2a834e22739_Out_2_Vector3;
            Unity_Multiply_float3_float3(IN.WorldSpaceNormal, (_Split_3caa93318451401d87ad12e7a907097f_B_3_Float.xxx), _Multiply_22c18134d5924015ba2ec2a834e22739_Out_2_Vector3);
            float3 _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3;
            float3 _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Color_1_Vector3;
            float _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_ShadowAtten_2_Float;
            GetMainLight_float(_GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3, _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Color_1_Vector3, _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_ShadowAtten_2_Float);
            float _Split_8f9f81fe0bee433ca8736d7047af190c_R_1_Float = _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3[0];
            float _Split_8f9f81fe0bee433ca8736d7047af190c_G_2_Float = _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3[1];
            float _Split_8f9f81fe0bee433ca8736d7047af190c_B_3_Float = _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3[2];
            float _Split_8f9f81fe0bee433ca8736d7047af190c_A_4_Float = 0;
            float _Multiply_eb7953577efa42578ac8713bee265f5b_Out_2_Float;
            Unity_Multiply_float_float(_Split_3caa93318451401d87ad12e7a907097f_R_1_Float, _Split_8f9f81fe0bee433ca8736d7047af190c_R_1_Float, _Multiply_eb7953577efa42578ac8713bee265f5b_Out_2_Float);
            float _Multiply_008c4dc1817d4499bb015a203c3b9fc9_Out_2_Float;
            Unity_Multiply_float_float(_Split_3caa93318451401d87ad12e7a907097f_G_2_Float, _Split_8f9f81fe0bee433ca8736d7047af190c_G_2_Float, _Multiply_008c4dc1817d4499bb015a203c3b9fc9_Out_2_Float);
            float4 _Combine_6028bb2051e2436b810df848e038d344_RGBA_4_Vector4;
            float3 _Combine_6028bb2051e2436b810df848e038d344_RGB_5_Vector3;
            float2 _Combine_6028bb2051e2436b810df848e038d344_RG_6_Vector2;
            Unity_Combine_float(_Multiply_eb7953577efa42578ac8713bee265f5b_Out_2_Float, _Multiply_008c4dc1817d4499bb015a203c3b9fc9_Out_2_Float, _Split_8f9f81fe0bee433ca8736d7047af190c_B_3_Float, float(0), _Combine_6028bb2051e2436b810df848e038d344_RGBA_4_Vector4, _Combine_6028bb2051e2436b810df848e038d344_RGB_5_Vector3, _Combine_6028bb2051e2436b810df848e038d344_RG_6_Vector2);
            float3 _Add_be6bd9b0b67c4b6db449176e8ca930b2_Out_2_Vector3;
            Unity_Add_float3(_Multiply_22c18134d5924015ba2ec2a834e22739_Out_2_Vector3, _Combine_6028bb2051e2436b810df848e038d344_RGB_5_Vector3, _Add_be6bd9b0b67c4b6db449176e8ca930b2_Out_2_Vector3);
            float3 _Normalize_f3cd7974fb394a2cadf32d57b28d9261_Out_1_Vector3;
            Unity_Normalize_float3(_Add_be6bd9b0b67c4b6db449176e8ca930b2_Out_2_Vector3, _Normalize_f3cd7974fb394a2cadf32d57b28d9261_Out_1_Vector3);
            float3 _Negate_1793cf15ec9c407f8e406f36ca089de6_Out_1_Vector3;
            Unity_Negate_float3(_Normalize_f3cd7974fb394a2cadf32d57b28d9261_Out_1_Vector3, _Negate_1793cf15ec9c407f8e406f36ca089de6_Out_1_Vector3);
            float _DotProduct_b92b4de671844a4cb48bdf3543eb8209_Out_2_Float;
            Unity_DotProduct_float3(IN.WorldSpaceViewDirection, _Negate_1793cf15ec9c407f8e406f36ca089de6_Out_1_Vector3, _DotProduct_b92b4de671844a4cb48bdf3543eb8209_Out_2_Float);
            float _Saturate_ac2a94e9b1f44dc999b6a5f585e92ec5_Out_1_Float;
            Unity_Saturate_float(_DotProduct_b92b4de671844a4cb48bdf3543eb8209_Out_2_Float, _Saturate_ac2a94e9b1f44dc999b6a5f585e92ec5_Out_1_Float);
            float _Property_7f4e24b9dccf4656aa1db4f2b6d9c394_Out_0_Float = _SSS_Scattering;
            float _Power_6bffe28ecec947a4978b0e5497d49690_Out_2_Float;
            Unity_Power_float(_Saturate_ac2a94e9b1f44dc999b6a5f585e92ec5_Out_1_Float, _Property_7f4e24b9dccf4656aa1db4f2b6d9c394_Out_0_Float, _Power_6bffe28ecec947a4978b0e5497d49690_Out_2_Float);
            float _Power_bd15f48c24704cf38a062bb3163ede20_Out_2_Float;
            Unity_Power_float(_Power_6bffe28ecec947a4978b0e5497d49690_Out_2_Float, _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_ShadowAtten_2_Float, _Power_bd15f48c24704cf38a062bb3163ede20_Out_2_Float);
            float4 _Property_0c7582d4a9b7493287ae8b1cf4e99a8a_Out_0_Vector4 = _SSS_Color;
            float4 _Multiply_5fff722b6be44e2c8ac7eb45803009da_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Power_bd15f48c24704cf38a062bb3163ede20_Out_2_Float.xxxx), _Property_0c7582d4a9b7493287ae8b1cf4e99a8a_Out_0_Vector4, _Multiply_5fff722b6be44e2c8ac7eb45803009da_Out_2_Vector4);
            float _Property_1572ee45033348219e19d5ae51034997_Out_0_Float = _SSS_Intensity;
            float4 _Multiply_9b5ece35f1b94fd99f2d33c3e4f1d918_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_5fff722b6be44e2c8ac7eb45803009da_Out_2_Vector4, (_Property_1572ee45033348219e19d5ae51034997_Out_0_Float.xxxx), _Multiply_9b5ece35f1b94fd99f2d33c3e4f1d918_Out_2_Vector4);
            float3 _Multiply_66b508c9e81948b8a34cea6baffd763d_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Multiply_9b5ece35f1b94fd99f2d33c3e4f1d918_Out_2_Vector4.xyz), _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Color_1_Vector3, _Multiply_66b508c9e81948b8a34cea6baffd763d_Out_2_Vector3);
            float3 _Add_9595ed0b6087400383dcbc8cdce459f6_Out_2_Vector3;
            Unity_Add_float3((_Multiply_16c8957045404e8cb4dc99dec57c9601_Out_2_Vector4.xyz), _Multiply_66b508c9e81948b8a34cea6baffd763d_Out_2_Vector3, _Add_9595ed0b6087400383dcbc8cdce459f6_Out_2_Vector3);
            UnityTexture2D _Property_a48f9ee2e9074b1ba60bf202e28da2fd_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MetallicMap);
            float4 _SampleTexture2D_998830748c3540528d5f2a45292b994a_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_a48f9ee2e9074b1ba60bf202e28da2fd_Out_0_Texture2D.tex, _Property_a48f9ee2e9074b1ba60bf202e28da2fd_Out_0_Texture2D.samplerstate, _Property_a48f9ee2e9074b1ba60bf202e28da2fd_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_998830748c3540528d5f2a45292b994a_R_4_Float = _SampleTexture2D_998830748c3540528d5f2a45292b994a_RGBA_0_Vector4.r;
            float _SampleTexture2D_998830748c3540528d5f2a45292b994a_G_5_Float = _SampleTexture2D_998830748c3540528d5f2a45292b994a_RGBA_0_Vector4.g;
            float _SampleTexture2D_998830748c3540528d5f2a45292b994a_B_6_Float = _SampleTexture2D_998830748c3540528d5f2a45292b994a_RGBA_0_Vector4.b;
            float _SampleTexture2D_998830748c3540528d5f2a45292b994a_A_7_Float = _SampleTexture2D_998830748c3540528d5f2a45292b994a_RGBA_0_Vector4.a;
            float _IsFrontFace_f3f821f2fc6c405f9a50ce2534b7d5fe_Out_0_Boolean = max(0, IN.FaceSign.x);
            float _Property_2847d1c700f3424b97e54e3225b28133_Out_0_Float = _Metallic;
            float _Branch_ed088f94c2684895aaca8db87cbb4ab5_Out_3_Float;
            Unity_Branch_float(_IsFrontFace_f3f821f2fc6c405f9a50ce2534b7d5fe_Out_0_Boolean, _Property_2847d1c700f3424b97e54e3225b28133_Out_0_Float, float(0), _Branch_ed088f94c2684895aaca8db87cbb4ab5_Out_3_Float);
            float4 _Multiply_b725dae6dfc9431d91df366721b2ac8b_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_998830748c3540528d5f2a45292b994a_RGBA_0_Vector4, (_Branch_ed088f94c2684895aaca8db87cbb4ab5_Out_3_Float.xxxx), _Multiply_b725dae6dfc9431d91df366721b2ac8b_Out_2_Vector4);
            float _IsFrontFace_6e79567bd99b494698a501ec60fdc0b2_Out_0_Boolean = max(0, IN.FaceSign.x);
            float _Property_2f18b6082f284664b06b414e660ca29e_Out_0_Float = _Smoothness;
            float _Multiply_b547fe90c1f74b98b4b7ff44267f574f_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_998830748c3540528d5f2a45292b994a_A_7_Float, _Property_2f18b6082f284664b06b414e660ca29e_Out_0_Float, _Multiply_b547fe90c1f74b98b4b7ff44267f574f_Out_2_Float);
            float _Branch_1da4fbb515fc4aa3af6b097c6556ccec_Out_3_Float;
            Unity_Branch_float(_IsFrontFace_6e79567bd99b494698a501ec60fdc0b2_Out_0_Boolean, _Multiply_b547fe90c1f74b98b4b7ff44267f574f_Out_2_Float, float(0), _Branch_1da4fbb515fc4aa3af6b097c6556ccec_Out_3_Float);
            UnityTexture2D _Property_b4a63982b3a54373afafa669d09acd3b_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_OcclusionMap);
            float4 _SampleTexture2D_286b24c1b019408d8887fae901e34021_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_b4a63982b3a54373afafa669d09acd3b_Out_0_Texture2D.tex, _Property_b4a63982b3a54373afafa669d09acd3b_Out_0_Texture2D.samplerstate, _Property_b4a63982b3a54373afafa669d09acd3b_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_286b24c1b019408d8887fae901e34021_R_4_Float = _SampleTexture2D_286b24c1b019408d8887fae901e34021_RGBA_0_Vector4.r;
            float _SampleTexture2D_286b24c1b019408d8887fae901e34021_G_5_Float = _SampleTexture2D_286b24c1b019408d8887fae901e34021_RGBA_0_Vector4.g;
            float _SampleTexture2D_286b24c1b019408d8887fae901e34021_B_6_Float = _SampleTexture2D_286b24c1b019408d8887fae901e34021_RGBA_0_Vector4.b;
            float _SampleTexture2D_286b24c1b019408d8887fae901e34021_A_7_Float = _SampleTexture2D_286b24c1b019408d8887fae901e34021_RGBA_0_Vector4.a;
            UnityTexture2D _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.tex, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.samplerstate, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_R_4_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.r;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_G_5_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.g;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_B_6_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.b;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.a;
            float _Split_4f8f81177c174703b3b7216f7abcab96_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_4f8f81177c174703b3b7216f7abcab96_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_4f8f81177c174703b3b7216f7abcab96_A_4_Float = 0;
            float _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float = _CutHeight;
            float _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float;
            Unity_Subtract_float(_Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float, _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float, _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float);
            float _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float = _EdgeSoftness;
            float _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float;
            Unity_Divide_float(_Subtract_14c160049cc54391926538d185ebb749_Out_2_Float, _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float, _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float);
            float _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float;
            Unity_Saturate_float(_Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float, _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float);
            float _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float;
            Unity_OneMinus_float(_Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float);
            float _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float, _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float);
            float _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float);
            float _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            Unity_Subtract_float(_Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float, _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float, _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float);
            float3 _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3;
            Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(IN.WorldSpacePosition, _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3);
            float3 _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3;
            Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(IN.WorldSpacePosition, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3);
            float3 _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3;
            Unity_CrossProduct_float(_DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3, _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3);
            float3 _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3;
            Unity_Normalize_float3(_CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3, _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3);
            float _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float;
            Unity_DotProduct_float3(_Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3, IN.WorldSpaceViewDirection, _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float);
            float _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float;
            Unity_Absolute_float(_DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float, _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float);
            float _Property_406505a58d794e15861d235061da0712_Out_0_Float = _BaseGlancingAngleCut;
            float _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float;
            Unity_Multiply_float_float(_Property_406505a58d794e15861d235061da0712_Out_0_Float, 3, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float);
            float _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float;
            Unity_Lerp_float(float(1), _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float);
            float _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float, _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 1, _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float);
            surface.BaseColor = (_Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4.xyz);
            surface.NormalTS = (_Branch_885bdab74c804361805e365abb412f7e_Out_3_Vector4.xyz);
            surface.Emission = _Add_9595ed0b6087400383dcbc8cdce459f6_Out_2_Vector3;
            surface.Metallic = (_Multiply_b725dae6dfc9431d91df366721b2ac8b_Out_2_Vector4).x;
            surface.Specular = IsGammaSpace() ? float3(0.5, 0.5, 0.5) : SRGBToLinear(float3(0.5, 0.5, 0.5));
            surface.Smoothness = _Branch_1da4fbb515fc4aa3af6b097c6556ccec_Out_3_Float;
            surface.Occlusion = _SampleTexture2D_286b24c1b019408d8887fae901e34021_G_5_Float;
            surface.Alpha = _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.uv0 =                                        input.uv0;
            output.VertexColor =                                input.color;
            output.TimeParameters =                             _TimeParameters.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
            BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GBufferOutput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRGBufferPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
        
        // Render State
        Cull [_Cull]
        ZTest LEqual
        ZWrite On
        ColorMask 0
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
        #pragma shader_feature_local_fragment _ _ALPHATEST_ON
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define GRAPH_VERTEX_USES_TIME_PARAMETERS_INPUT
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SHADOWCASTER
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float3 positionWS : INTERP1;
             float3 normalWS : INTERP2;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float _CutHeight;
        float _EdgeSoftness;
        float _Dither;
        float4 _Normal_Map_TexelSize;
        float4 _MetallicMap_TexelSize;
        float _Smoothness;
        float4 _OcclusionMap_TexelSize;
        float4 _Emission_Map_TexelSize;
        float4 _EmissionColor;
        float _Metallic;
        float _LeadWindSpeed;
        float _LeafWindScale;
        float _BendSpeed;
        float4 _BendDirection;
        float3 _Offset;
        float _BaseGlancingAngleCut;
        float _EnableFlipNormal;
        float _SSS_Scattering;
        float4 _SSS_Color;
        float _SSS_Intensity;
        float3 _SSS_Distortion;
        float _AOStrength;
        float _Gradient_Offset;
        float4 _GradientColor;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_Normal_Map);
        SAMPLER(sampler_Normal_Map);
        TEXTURE2D(_MetallicMap);
        SAMPLER(sampler_MetallicMap);
        TEXTURE2D(_OcclusionMap);
        SAMPLER(sampler_OcclusionMap);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
        {
             float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
             Out = lerp(Min, Max, randomno);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
        {
            float x; Hash_Tchou_2_1_float(p, x);
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }
        
        void Unity_GradientNoise_Deterministic_float (float2 UV, float3 Scale, out float Out)
        {
            float2 p = UV * Scale.xy;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDX' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddx(In);
        }
        
        void Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDY' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddy(In);
        }
        
        void Unity_CrossProduct_float(float3 A, float3 B, out float3 Out)
        {
            Out = cross(A, B);
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Split_e6ba90e06c194240888d9b07fce58a5d_R_1_Float = IN.ObjectSpacePosition[0];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float = IN.ObjectSpacePosition[1];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_B_3_Float = IN.ObjectSpacePosition[2];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_A_4_Float = 0;
            float _Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float = _BendSpeed;
            float3 _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3 = _Offset;
            float3 _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3;
            Unity_Add_float3(SHADERGRAPH_OBJECT_POSITION, _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3, _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3);
            float _Split_74566faebea64f928d3ab3395dea96c0_R_1_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[0];
            float _Split_74566faebea64f928d3ab3395dea96c0_G_2_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[1];
            float _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[2];
            float _Split_74566faebea64f928d3ab3395dea96c0_A_4_Float = 0;
            float2 _Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2 = float2(_Split_74566faebea64f928d3ab3395dea96c0_R_1_Float, _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float);
            float _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float;
            Unity_RandomRange_float(_Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2, float(0.1), float(1), _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float);
            float _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float;
            Unity_Multiply_float_float(_Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float, _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float);
            float _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.z, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float, _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float);
            float _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float;
            Unity_Divide_float(_Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float, float(100), _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float);
            float _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float;
            Unity_Multiply_float_float(_Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float, _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float, _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float);
            float _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float;
            Unity_Add_float(_Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float, float(1), _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float);
            float _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float;
            Unity_Multiply_float_float(_Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float);
            float _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float);
            float _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float;
            Unity_Subtract_float(_Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float);
            float4 _Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4 = _BendDirection;
            float4 _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4, (_RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4);
            float4 _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4, _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4);
            float _Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[0];
            float _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[1];
            float _Split_2e4561fe3a484551a112182bf2ae2954_B_3_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[2];
            float _Split_2e4561fe3a484551a112182bf2ae2954_A_4_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[3];
            float4 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4;
            float3 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3;
            float2 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2;
            Unity_Combine_float(_Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float, float(0), _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float, float(0), _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2);
            float3 _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3;
            Unity_Add_float3(IN.WorldSpacePosition, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3);
            float3 _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3;
            _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3 = TransformWorldToObject(_Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3.xyz);
            float _Split_86265132a9074281af78965592fb42d4_R_1_Float = IN.VertexColor[0];
            float _Split_86265132a9074281af78965592fb42d4_G_2_Float = IN.VertexColor[1];
            float _Split_86265132a9074281af78965592fb42d4_B_3_Float = IN.VertexColor[2];
            float _Split_86265132a9074281af78965592fb42d4_A_4_Float = IN.VertexColor[3];
            float4 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4;
            float3 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3;
            float2 _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2;
            Unity_Combine_float(_Split_86265132a9074281af78965592fb42d4_R_1_Float, _Split_86265132a9074281af78965592fb42d4_G_2_Float, float(0), float(0), _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2);
            float3 _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3;
            Unity_Lerp_float3(_Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3, IN.ObjectSpacePosition, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3);
            float _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float = _LeadWindSpeed;
            float _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float, _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float);
            float2 _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float.xx), _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2);
            float _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float = _LeafWindScale;
            float _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2, _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float, _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float);
            float3 _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            Unity_Lerp_float3(IN.ObjectSpacePosition, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3, (_GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float.xxx), _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3);
            description.Position = _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.tex, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.samplerstate, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_R_4_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.r;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_G_5_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.g;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_B_6_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.b;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.a;
            float _Split_4f8f81177c174703b3b7216f7abcab96_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_4f8f81177c174703b3b7216f7abcab96_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_4f8f81177c174703b3b7216f7abcab96_A_4_Float = 0;
            float _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float = _CutHeight;
            float _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float;
            Unity_Subtract_float(_Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float, _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float, _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float);
            float _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float = _EdgeSoftness;
            float _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float;
            Unity_Divide_float(_Subtract_14c160049cc54391926538d185ebb749_Out_2_Float, _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float, _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float);
            float _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float;
            Unity_Saturate_float(_Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float, _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float);
            float _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float;
            Unity_OneMinus_float(_Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float);
            float _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float, _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float);
            float _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float);
            float _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            Unity_Subtract_float(_Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float, _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float, _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float);
            float3 _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3;
            Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(IN.WorldSpacePosition, _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3);
            float3 _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3;
            Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(IN.WorldSpacePosition, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3);
            float3 _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3;
            Unity_CrossProduct_float(_DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3, _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3);
            float3 _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3;
            Unity_Normalize_float3(_CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3, _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3);
            float _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float;
            Unity_DotProduct_float3(_Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3, IN.WorldSpaceViewDirection, _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float);
            float _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float;
            Unity_Absolute_float(_DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float, _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float);
            float _Property_406505a58d794e15861d235061da0712_Out_0_Float = _BaseGlancingAngleCut;
            float _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float;
            Unity_Multiply_float_float(_Property_406505a58d794e15861d235061da0712_Out_0_Float, 3, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float);
            float _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float;
            Unity_Lerp_float(float(1), _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float);
            float _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float, _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 1, _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float);
            surface.Alpha = _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.uv0 =                                        input.uv0;
            output.VertexColor =                                input.color;
            output.TimeParameters =                             _TimeParameters.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "MotionVectors"
            Tags
            {
                "LightMode" = "MotionVectors"
            }
        
        // Render State
        Cull [_Cull]
        ZTest LEqual
        ZWrite On
        ColorMask RG
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.5
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature_local_fragment _ _ALPHATEST_ON
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define GRAPH_VERTEX_USES_TIME_PARAMETERS_INPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_MOTION_VECTORS
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float3 positionWS : INTERP1;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float _CutHeight;
        float _EdgeSoftness;
        float _Dither;
        float4 _Normal_Map_TexelSize;
        float4 _MetallicMap_TexelSize;
        float _Smoothness;
        float4 _OcclusionMap_TexelSize;
        float4 _Emission_Map_TexelSize;
        float4 _EmissionColor;
        float _Metallic;
        float _LeadWindSpeed;
        float _LeafWindScale;
        float _BendSpeed;
        float4 _BendDirection;
        float3 _Offset;
        float _BaseGlancingAngleCut;
        float _EnableFlipNormal;
        float _SSS_Scattering;
        float4 _SSS_Color;
        float _SSS_Intensity;
        float3 _SSS_Distortion;
        float _AOStrength;
        float _Gradient_Offset;
        float4 _GradientColor;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_Normal_Map);
        SAMPLER(sampler_Normal_Map);
        TEXTURE2D(_MetallicMap);
        SAMPLER(sampler_MetallicMap);
        TEXTURE2D(_OcclusionMap);
        SAMPLER(sampler_OcclusionMap);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
        {
             float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
             Out = lerp(Min, Max, randomno);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
        {
            float x; Hash_Tchou_2_1_float(p, x);
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }
        
        void Unity_GradientNoise_Deterministic_float (float2 UV, float3 Scale, out float Out)
        {
            float2 p = UV * Scale.xy;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDX' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddx(In);
        }
        
        void Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDY' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddy(In);
        }
        
        void Unity_CrossProduct_float(float3 A, float3 B, out float3 Out)
        {
            Out = cross(A, B);
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Split_e6ba90e06c194240888d9b07fce58a5d_R_1_Float = IN.ObjectSpacePosition[0];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float = IN.ObjectSpacePosition[1];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_B_3_Float = IN.ObjectSpacePosition[2];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_A_4_Float = 0;
            float _Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float = _BendSpeed;
            float3 _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3 = _Offset;
            float3 _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3;
            Unity_Add_float3(SHADERGRAPH_OBJECT_POSITION, _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3, _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3);
            float _Split_74566faebea64f928d3ab3395dea96c0_R_1_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[0];
            float _Split_74566faebea64f928d3ab3395dea96c0_G_2_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[1];
            float _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[2];
            float _Split_74566faebea64f928d3ab3395dea96c0_A_4_Float = 0;
            float2 _Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2 = float2(_Split_74566faebea64f928d3ab3395dea96c0_R_1_Float, _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float);
            float _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float;
            Unity_RandomRange_float(_Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2, float(0.1), float(1), _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float);
            float _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float;
            Unity_Multiply_float_float(_Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float, _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float);
            float _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.z, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float, _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float);
            float _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float;
            Unity_Divide_float(_Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float, float(100), _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float);
            float _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float;
            Unity_Multiply_float_float(_Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float, _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float, _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float);
            float _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float;
            Unity_Add_float(_Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float, float(1), _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float);
            float _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float;
            Unity_Multiply_float_float(_Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float);
            float _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float);
            float _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float;
            Unity_Subtract_float(_Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float);
            float4 _Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4 = _BendDirection;
            float4 _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4, (_RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4);
            float4 _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4, _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4);
            float _Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[0];
            float _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[1];
            float _Split_2e4561fe3a484551a112182bf2ae2954_B_3_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[2];
            float _Split_2e4561fe3a484551a112182bf2ae2954_A_4_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[3];
            float4 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4;
            float3 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3;
            float2 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2;
            Unity_Combine_float(_Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float, float(0), _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float, float(0), _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2);
            float3 _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3;
            Unity_Add_float3(IN.WorldSpacePosition, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3);
            float3 _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3;
            _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3 = TransformWorldToObject(_Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3.xyz);
            float _Split_86265132a9074281af78965592fb42d4_R_1_Float = IN.VertexColor[0];
            float _Split_86265132a9074281af78965592fb42d4_G_2_Float = IN.VertexColor[1];
            float _Split_86265132a9074281af78965592fb42d4_B_3_Float = IN.VertexColor[2];
            float _Split_86265132a9074281af78965592fb42d4_A_4_Float = IN.VertexColor[3];
            float4 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4;
            float3 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3;
            float2 _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2;
            Unity_Combine_float(_Split_86265132a9074281af78965592fb42d4_R_1_Float, _Split_86265132a9074281af78965592fb42d4_G_2_Float, float(0), float(0), _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2);
            float3 _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3;
            Unity_Lerp_float3(_Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3, IN.ObjectSpacePosition, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3);
            float _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float = _LeadWindSpeed;
            float _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float, _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float);
            float2 _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float.xx), _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2);
            float _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float = _LeafWindScale;
            float _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2, _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float, _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float);
            float3 _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            Unity_Lerp_float3(IN.ObjectSpacePosition, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3, (_GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float.xxx), _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3);
            description.Position = _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.tex, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.samplerstate, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_R_4_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.r;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_G_5_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.g;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_B_6_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.b;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.a;
            float _Split_4f8f81177c174703b3b7216f7abcab96_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_4f8f81177c174703b3b7216f7abcab96_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_4f8f81177c174703b3b7216f7abcab96_A_4_Float = 0;
            float _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float = _CutHeight;
            float _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float;
            Unity_Subtract_float(_Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float, _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float, _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float);
            float _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float = _EdgeSoftness;
            float _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float;
            Unity_Divide_float(_Subtract_14c160049cc54391926538d185ebb749_Out_2_Float, _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float, _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float);
            float _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float;
            Unity_Saturate_float(_Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float, _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float);
            float _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float;
            Unity_OneMinus_float(_Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float);
            float _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float, _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float);
            float _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float);
            float _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            Unity_Subtract_float(_Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float, _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float, _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float);
            float3 _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3;
            Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(IN.WorldSpacePosition, _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3);
            float3 _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3;
            Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(IN.WorldSpacePosition, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3);
            float3 _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3;
            Unity_CrossProduct_float(_DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3, _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3);
            float3 _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3;
            Unity_Normalize_float3(_CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3, _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3);
            float _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float;
            Unity_DotProduct_float3(_Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3, IN.WorldSpaceViewDirection, _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float);
            float _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float;
            Unity_Absolute_float(_DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float, _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float);
            float _Property_406505a58d794e15861d235061da0712_Out_0_Float = _BaseGlancingAngleCut;
            float _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float;
            Unity_Multiply_float_float(_Property_406505a58d794e15861d235061da0712_Out_0_Float, 3, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float);
            float _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float;
            Unity_Lerp_float(float(1), _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float);
            float _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float, _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 1, _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float);
            surface.Alpha = _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.uv0 =                                        input.uv0;
            output.VertexColor =                                input.color;
            output.TimeParameters =                             _TimeParameters.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/MotionVectorPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }
        
        // Render State
        Cull [_Cull]
        ZTest LEqual
        ZWrite On
        ColorMask R
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature_local_fragment _ _ALPHATEST_ON
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define GRAPH_VERTEX_USES_TIME_PARAMETERS_INPUT
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float3 positionWS : INTERP1;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float _CutHeight;
        float _EdgeSoftness;
        float _Dither;
        float4 _Normal_Map_TexelSize;
        float4 _MetallicMap_TexelSize;
        float _Smoothness;
        float4 _OcclusionMap_TexelSize;
        float4 _Emission_Map_TexelSize;
        float4 _EmissionColor;
        float _Metallic;
        float _LeadWindSpeed;
        float _LeafWindScale;
        float _BendSpeed;
        float4 _BendDirection;
        float3 _Offset;
        float _BaseGlancingAngleCut;
        float _EnableFlipNormal;
        float _SSS_Scattering;
        float4 _SSS_Color;
        float _SSS_Intensity;
        float3 _SSS_Distortion;
        float _AOStrength;
        float _Gradient_Offset;
        float4 _GradientColor;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_Normal_Map);
        SAMPLER(sampler_Normal_Map);
        TEXTURE2D(_MetallicMap);
        SAMPLER(sampler_MetallicMap);
        TEXTURE2D(_OcclusionMap);
        SAMPLER(sampler_OcclusionMap);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
        {
             float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
             Out = lerp(Min, Max, randomno);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
        {
            float x; Hash_Tchou_2_1_float(p, x);
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }
        
        void Unity_GradientNoise_Deterministic_float (float2 UV, float3 Scale, out float Out)
        {
            float2 p = UV * Scale.xy;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDX' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddx(In);
        }
        
        void Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDY' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddy(In);
        }
        
        void Unity_CrossProduct_float(float3 A, float3 B, out float3 Out)
        {
            Out = cross(A, B);
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Split_e6ba90e06c194240888d9b07fce58a5d_R_1_Float = IN.ObjectSpacePosition[0];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float = IN.ObjectSpacePosition[1];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_B_3_Float = IN.ObjectSpacePosition[2];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_A_4_Float = 0;
            float _Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float = _BendSpeed;
            float3 _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3 = _Offset;
            float3 _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3;
            Unity_Add_float3(SHADERGRAPH_OBJECT_POSITION, _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3, _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3);
            float _Split_74566faebea64f928d3ab3395dea96c0_R_1_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[0];
            float _Split_74566faebea64f928d3ab3395dea96c0_G_2_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[1];
            float _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[2];
            float _Split_74566faebea64f928d3ab3395dea96c0_A_4_Float = 0;
            float2 _Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2 = float2(_Split_74566faebea64f928d3ab3395dea96c0_R_1_Float, _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float);
            float _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float;
            Unity_RandomRange_float(_Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2, float(0.1), float(1), _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float);
            float _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float;
            Unity_Multiply_float_float(_Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float, _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float);
            float _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.z, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float, _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float);
            float _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float;
            Unity_Divide_float(_Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float, float(100), _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float);
            float _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float;
            Unity_Multiply_float_float(_Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float, _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float, _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float);
            float _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float;
            Unity_Add_float(_Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float, float(1), _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float);
            float _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float;
            Unity_Multiply_float_float(_Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float);
            float _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float);
            float _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float;
            Unity_Subtract_float(_Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float);
            float4 _Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4 = _BendDirection;
            float4 _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4, (_RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4);
            float4 _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4, _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4);
            float _Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[0];
            float _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[1];
            float _Split_2e4561fe3a484551a112182bf2ae2954_B_3_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[2];
            float _Split_2e4561fe3a484551a112182bf2ae2954_A_4_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[3];
            float4 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4;
            float3 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3;
            float2 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2;
            Unity_Combine_float(_Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float, float(0), _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float, float(0), _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2);
            float3 _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3;
            Unity_Add_float3(IN.WorldSpacePosition, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3);
            float3 _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3;
            _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3 = TransformWorldToObject(_Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3.xyz);
            float _Split_86265132a9074281af78965592fb42d4_R_1_Float = IN.VertexColor[0];
            float _Split_86265132a9074281af78965592fb42d4_G_2_Float = IN.VertexColor[1];
            float _Split_86265132a9074281af78965592fb42d4_B_3_Float = IN.VertexColor[2];
            float _Split_86265132a9074281af78965592fb42d4_A_4_Float = IN.VertexColor[3];
            float4 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4;
            float3 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3;
            float2 _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2;
            Unity_Combine_float(_Split_86265132a9074281af78965592fb42d4_R_1_Float, _Split_86265132a9074281af78965592fb42d4_G_2_Float, float(0), float(0), _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2);
            float3 _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3;
            Unity_Lerp_float3(_Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3, IN.ObjectSpacePosition, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3);
            float _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float = _LeadWindSpeed;
            float _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float, _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float);
            float2 _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float.xx), _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2);
            float _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float = _LeafWindScale;
            float _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2, _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float, _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float);
            float3 _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            Unity_Lerp_float3(IN.ObjectSpacePosition, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3, (_GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float.xxx), _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3);
            description.Position = _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.tex, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.samplerstate, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_R_4_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.r;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_G_5_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.g;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_B_6_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.b;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.a;
            float _Split_4f8f81177c174703b3b7216f7abcab96_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_4f8f81177c174703b3b7216f7abcab96_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_4f8f81177c174703b3b7216f7abcab96_A_4_Float = 0;
            float _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float = _CutHeight;
            float _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float;
            Unity_Subtract_float(_Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float, _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float, _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float);
            float _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float = _EdgeSoftness;
            float _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float;
            Unity_Divide_float(_Subtract_14c160049cc54391926538d185ebb749_Out_2_Float, _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float, _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float);
            float _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float;
            Unity_Saturate_float(_Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float, _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float);
            float _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float;
            Unity_OneMinus_float(_Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float);
            float _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float, _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float);
            float _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float);
            float _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            Unity_Subtract_float(_Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float, _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float, _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float);
            float3 _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3;
            Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(IN.WorldSpacePosition, _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3);
            float3 _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3;
            Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(IN.WorldSpacePosition, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3);
            float3 _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3;
            Unity_CrossProduct_float(_DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3, _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3);
            float3 _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3;
            Unity_Normalize_float3(_CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3, _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3);
            float _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float;
            Unity_DotProduct_float3(_Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3, IN.WorldSpaceViewDirection, _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float);
            float _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float;
            Unity_Absolute_float(_DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float, _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float);
            float _Property_406505a58d794e15861d235061da0712_Out_0_Float = _BaseGlancingAngleCut;
            float _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float;
            Unity_Multiply_float_float(_Property_406505a58d794e15861d235061da0712_Out_0_Float, 3, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float);
            float _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float;
            Unity_Lerp_float(float(1), _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float);
            float _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float, _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 1, _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float);
            surface.Alpha = _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.uv0 =                                        input.uv0;
            output.VertexColor =                                input.color;
            output.TimeParameters =                             _TimeParameters.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }
        
        // Render State
        Cull [_Cull]
        ZTest LEqual
        ZWrite On
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature_local_fragment _ _ALPHATEST_ON
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_COLOR
        #define GRAPH_VERTEX_USES_TIME_PARAMETERS_INPUT
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_CULLFACE
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALS
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
             float FaceSign;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 tangentWS : INTERP0;
             float4 texCoord0 : INTERP1;
             float3 positionWS : INTERP2;
             float3 normalWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float _CutHeight;
        float _EdgeSoftness;
        float _Dither;
        float4 _Normal_Map_TexelSize;
        float4 _MetallicMap_TexelSize;
        float _Smoothness;
        float4 _OcclusionMap_TexelSize;
        float4 _Emission_Map_TexelSize;
        float4 _EmissionColor;
        float _Metallic;
        float _LeadWindSpeed;
        float _LeafWindScale;
        float _BendSpeed;
        float4 _BendDirection;
        float3 _Offset;
        float _BaseGlancingAngleCut;
        float _EnableFlipNormal;
        float _SSS_Scattering;
        float4 _SSS_Color;
        float _SSS_Intensity;
        float3 _SSS_Distortion;
        float _AOStrength;
        float _Gradient_Offset;
        float4 _GradientColor;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_Normal_Map);
        SAMPLER(sampler_Normal_Map);
        TEXTURE2D(_MetallicMap);
        SAMPLER(sampler_MetallicMap);
        TEXTURE2D(_OcclusionMap);
        SAMPLER(sampler_OcclusionMap);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
        {
             float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
             Out = lerp(Min, Max, randomno);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
        {
            float x; Hash_Tchou_2_1_float(p, x);
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }
        
        void Unity_GradientNoise_Deterministic_float (float2 UV, float3 Scale, out float Out)
        {
            float2 p = UV * Scale.xy;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDX' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddx(In);
        }
        
        void Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDY' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddy(In);
        }
        
        void Unity_CrossProduct_float(float3 A, float3 B, out float3 Out)
        {
            Out = cross(A, B);
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Split_e6ba90e06c194240888d9b07fce58a5d_R_1_Float = IN.ObjectSpacePosition[0];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float = IN.ObjectSpacePosition[1];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_B_3_Float = IN.ObjectSpacePosition[2];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_A_4_Float = 0;
            float _Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float = _BendSpeed;
            float3 _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3 = _Offset;
            float3 _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3;
            Unity_Add_float3(SHADERGRAPH_OBJECT_POSITION, _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3, _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3);
            float _Split_74566faebea64f928d3ab3395dea96c0_R_1_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[0];
            float _Split_74566faebea64f928d3ab3395dea96c0_G_2_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[1];
            float _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[2];
            float _Split_74566faebea64f928d3ab3395dea96c0_A_4_Float = 0;
            float2 _Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2 = float2(_Split_74566faebea64f928d3ab3395dea96c0_R_1_Float, _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float);
            float _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float;
            Unity_RandomRange_float(_Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2, float(0.1), float(1), _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float);
            float _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float;
            Unity_Multiply_float_float(_Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float, _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float);
            float _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.z, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float, _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float);
            float _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float;
            Unity_Divide_float(_Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float, float(100), _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float);
            float _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float;
            Unity_Multiply_float_float(_Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float, _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float, _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float);
            float _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float;
            Unity_Add_float(_Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float, float(1), _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float);
            float _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float;
            Unity_Multiply_float_float(_Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float);
            float _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float);
            float _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float;
            Unity_Subtract_float(_Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float);
            float4 _Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4 = _BendDirection;
            float4 _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4, (_RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4);
            float4 _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4, _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4);
            float _Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[0];
            float _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[1];
            float _Split_2e4561fe3a484551a112182bf2ae2954_B_3_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[2];
            float _Split_2e4561fe3a484551a112182bf2ae2954_A_4_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[3];
            float4 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4;
            float3 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3;
            float2 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2;
            Unity_Combine_float(_Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float, float(0), _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float, float(0), _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2);
            float3 _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3;
            Unity_Add_float3(IN.WorldSpacePosition, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3);
            float3 _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3;
            _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3 = TransformWorldToObject(_Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3.xyz);
            float _Split_86265132a9074281af78965592fb42d4_R_1_Float = IN.VertexColor[0];
            float _Split_86265132a9074281af78965592fb42d4_G_2_Float = IN.VertexColor[1];
            float _Split_86265132a9074281af78965592fb42d4_B_3_Float = IN.VertexColor[2];
            float _Split_86265132a9074281af78965592fb42d4_A_4_Float = IN.VertexColor[3];
            float4 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4;
            float3 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3;
            float2 _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2;
            Unity_Combine_float(_Split_86265132a9074281af78965592fb42d4_R_1_Float, _Split_86265132a9074281af78965592fb42d4_G_2_Float, float(0), float(0), _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2);
            float3 _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3;
            Unity_Lerp_float3(_Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3, IN.ObjectSpacePosition, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3);
            float _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float = _LeadWindSpeed;
            float _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float, _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float);
            float2 _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float.xx), _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2);
            float _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float = _LeafWindScale;
            float _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2, _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float, _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float);
            float3 _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            Unity_Lerp_float3(IN.ObjectSpacePosition, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3, (_GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float.xxx), _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3);
            description.Position = _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 NormalTS;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_d5903e624613450ca79764a0d69f2fdc_Out_0_Boolean = _EnableFlipNormal;
            UnityTexture2D _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Normal_Map);
            float4 _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.tex, _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.samplerstate, _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4);
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_R_4_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.r;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_G_5_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.g;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_B_6_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.b;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_A_7_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.a;
            float _IsFrontFace_19a764a852494650ab4318b8c36ff8d2_Out_0_Boolean = max(0, IN.FaceSign.x);
            float _Float_58e987145ca94b5e9adbc929e55b4b6b_Out_0_Float = float(-1);
            float _Multiply_78ea49f65ba140139105613218fe61a8_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_B_6_Float, _Float_58e987145ca94b5e9adbc929e55b4b6b_Out_0_Float, _Multiply_78ea49f65ba140139105613218fe61a8_Out_2_Float);
            float _Branch_ac9e2d676e0845dbabb82c7b506f7262_Out_3_Float;
            Unity_Branch_float(_IsFrontFace_19a764a852494650ab4318b8c36ff8d2_Out_0_Boolean, _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_B_6_Float, _Multiply_78ea49f65ba140139105613218fe61a8_Out_2_Float, _Branch_ac9e2d676e0845dbabb82c7b506f7262_Out_3_Float);
            float4 _Combine_5796639be6c5497d82a1e1a45094c6f6_RGBA_4_Vector4;
            float3 _Combine_5796639be6c5497d82a1e1a45094c6f6_RGB_5_Vector3;
            float2 _Combine_5796639be6c5497d82a1e1a45094c6f6_RG_6_Vector2;
            Unity_Combine_float(_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_R_4_Float, _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_G_5_Float, _Branch_ac9e2d676e0845dbabb82c7b506f7262_Out_3_Float, float(0), _Combine_5796639be6c5497d82a1e1a45094c6f6_RGBA_4_Vector4, _Combine_5796639be6c5497d82a1e1a45094c6f6_RGB_5_Vector3, _Combine_5796639be6c5497d82a1e1a45094c6f6_RG_6_Vector2);
            float4 _Branch_885bdab74c804361805e365abb412f7e_Out_3_Vector4;
            Unity_Branch_float4(_Property_d5903e624613450ca79764a0d69f2fdc_Out_0_Boolean, _Combine_5796639be6c5497d82a1e1a45094c6f6_RGBA_4_Vector4, _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4, _Branch_885bdab74c804361805e365abb412f7e_Out_3_Vector4);
            UnityTexture2D _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.tex, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.samplerstate, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_R_4_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.r;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_G_5_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.g;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_B_6_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.b;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.a;
            float _Split_4f8f81177c174703b3b7216f7abcab96_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_4f8f81177c174703b3b7216f7abcab96_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_4f8f81177c174703b3b7216f7abcab96_A_4_Float = 0;
            float _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float = _CutHeight;
            float _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float;
            Unity_Subtract_float(_Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float, _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float, _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float);
            float _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float = _EdgeSoftness;
            float _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float;
            Unity_Divide_float(_Subtract_14c160049cc54391926538d185ebb749_Out_2_Float, _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float, _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float);
            float _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float;
            Unity_Saturate_float(_Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float, _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float);
            float _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float;
            Unity_OneMinus_float(_Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float);
            float _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float, _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float);
            float _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float);
            float _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            Unity_Subtract_float(_Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float, _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float, _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float);
            float3 _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3;
            Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(IN.WorldSpacePosition, _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3);
            float3 _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3;
            Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(IN.WorldSpacePosition, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3);
            float3 _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3;
            Unity_CrossProduct_float(_DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3, _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3);
            float3 _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3;
            Unity_Normalize_float3(_CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3, _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3);
            float _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float;
            Unity_DotProduct_float3(_Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3, IN.WorldSpaceViewDirection, _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float);
            float _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float;
            Unity_Absolute_float(_DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float, _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float);
            float _Property_406505a58d794e15861d235061da0712_Out_0_Float = _BaseGlancingAngleCut;
            float _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float;
            Unity_Multiply_float_float(_Property_406505a58d794e15861d235061da0712_Out_0_Float, 3, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float);
            float _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float;
            Unity_Lerp_float(float(1), _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float);
            float _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float, _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 1, _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float);
            surface.NormalTS = (_Branch_885bdab74c804361805e365abb412f7e_Out_3_Vector4.xyz);
            surface.Alpha = _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.uv0 =                                        input.uv0;
            output.VertexColor =                                input.color;
            output.TimeParameters =                             _TimeParameters.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
            BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature _ EDITOR_VISUALIZATION
        #pragma shader_feature_local_fragment _ _ALPHATEST_ON
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define ATTRIBUTES_NEED_COLOR
        #define ATTRIBUTES_NEED_INSTANCEID
        #define GRAPH_VERTEX_USES_TIME_PARAMETERS_INPUT
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
        #define _FOG_FRAGMENT 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
             float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 texCoord1 : INTERP1;
             float4 texCoord2 : INTERP2;
             float4 color : INTERP3;
             float3 positionWS : INTERP4;
             float3 normalWS : INTERP5;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.texCoord1.xyzw = input.texCoord1;
            output.texCoord2.xyzw = input.texCoord2;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.texCoord1 = input.texCoord1.xyzw;
            output.texCoord2 = input.texCoord2.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float _CutHeight;
        float _EdgeSoftness;
        float _Dither;
        float4 _Normal_Map_TexelSize;
        float4 _MetallicMap_TexelSize;
        float _Smoothness;
        float4 _OcclusionMap_TexelSize;
        float4 _Emission_Map_TexelSize;
        float4 _EmissionColor;
        float _Metallic;
        float _LeadWindSpeed;
        float _LeafWindScale;
        float _BendSpeed;
        float4 _BendDirection;
        float3 _Offset;
        float _BaseGlancingAngleCut;
        float _EnableFlipNormal;
        float _SSS_Scattering;
        float4 _SSS_Color;
        float _SSS_Intensity;
        float3 _SSS_Distortion;
        float _AOStrength;
        float _Gradient_Offset;
        float4 _GradientColor;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_Normal_Map);
        SAMPLER(sampler_Normal_Map);
        TEXTURE2D(_MetallicMap);
        SAMPLER(sampler_MetallicMap);
        TEXTURE2D(_OcclusionMap);
        SAMPLER(sampler_OcclusionMap);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
        {
             float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
             Out = lerp(Min, Max, randomno);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
        {
            float x; Hash_Tchou_2_1_float(p, x);
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }
        
        void Unity_GradientNoise_Deterministic_float (float2 UV, float3 Scale, out float Out)
        {
            float2 p = UV * Scale.xy;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
        // unity-custom-func-begin
        void GetMainLight_float(out float3 Direction, out float3 Color, out float ShadowAtten){
            #if defined(SHADERGRAPH_PREVIEW)
            
                Direction = float3(0.5, 0.5, 0);
            
                Color = 1;
            
                ShadowAtten = 1;
            
            #else
            
                #if defined(UNIVERSAL_LIGHTING_INCLUDED)
            
                    Light mainLight = GetMainLight();
            
                    Direction = mainLight.direction;
            
                    Color = mainLight.color;
            
                    ShadowAtten = mainLight.shadowAttenuation;
            
                #else
            
                    Direction = float3(0.5, 0.5, 0);
            
                    Color = 1;
            
                    ShadowAtten = 1;
            
                #endif
            
            #endif
        }
        // unity-custom-func-end
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_Negate_float3(float3 In, out float3 Out)
        {
            Out = -1 * In;
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Power_float(float A, float B, out float Out)
        {
            Out = pow(A, B);
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDX' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddx(In);
        }
        
        void Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDY' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddy(In);
        }
        
        void Unity_CrossProduct_float(float3 A, float3 B, out float3 Out)
        {
            Out = cross(A, B);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Split_e6ba90e06c194240888d9b07fce58a5d_R_1_Float = IN.ObjectSpacePosition[0];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float = IN.ObjectSpacePosition[1];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_B_3_Float = IN.ObjectSpacePosition[2];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_A_4_Float = 0;
            float _Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float = _BendSpeed;
            float3 _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3 = _Offset;
            float3 _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3;
            Unity_Add_float3(SHADERGRAPH_OBJECT_POSITION, _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3, _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3);
            float _Split_74566faebea64f928d3ab3395dea96c0_R_1_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[0];
            float _Split_74566faebea64f928d3ab3395dea96c0_G_2_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[1];
            float _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[2];
            float _Split_74566faebea64f928d3ab3395dea96c0_A_4_Float = 0;
            float2 _Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2 = float2(_Split_74566faebea64f928d3ab3395dea96c0_R_1_Float, _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float);
            float _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float;
            Unity_RandomRange_float(_Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2, float(0.1), float(1), _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float);
            float _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float;
            Unity_Multiply_float_float(_Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float, _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float);
            float _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.z, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float, _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float);
            float _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float;
            Unity_Divide_float(_Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float, float(100), _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float);
            float _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float;
            Unity_Multiply_float_float(_Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float, _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float, _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float);
            float _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float;
            Unity_Add_float(_Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float, float(1), _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float);
            float _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float;
            Unity_Multiply_float_float(_Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float);
            float _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float);
            float _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float;
            Unity_Subtract_float(_Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float);
            float4 _Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4 = _BendDirection;
            float4 _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4, (_RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4);
            float4 _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4, _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4);
            float _Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[0];
            float _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[1];
            float _Split_2e4561fe3a484551a112182bf2ae2954_B_3_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[2];
            float _Split_2e4561fe3a484551a112182bf2ae2954_A_4_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[3];
            float4 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4;
            float3 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3;
            float2 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2;
            Unity_Combine_float(_Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float, float(0), _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float, float(0), _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2);
            float3 _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3;
            Unity_Add_float3(IN.WorldSpacePosition, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3);
            float3 _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3;
            _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3 = TransformWorldToObject(_Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3.xyz);
            float _Split_86265132a9074281af78965592fb42d4_R_1_Float = IN.VertexColor[0];
            float _Split_86265132a9074281af78965592fb42d4_G_2_Float = IN.VertexColor[1];
            float _Split_86265132a9074281af78965592fb42d4_B_3_Float = IN.VertexColor[2];
            float _Split_86265132a9074281af78965592fb42d4_A_4_Float = IN.VertexColor[3];
            float4 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4;
            float3 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3;
            float2 _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2;
            Unity_Combine_float(_Split_86265132a9074281af78965592fb42d4_R_1_Float, _Split_86265132a9074281af78965592fb42d4_G_2_Float, float(0), float(0), _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2);
            float3 _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3;
            Unity_Lerp_float3(_Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3, IN.ObjectSpacePosition, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3);
            float _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float = _LeadWindSpeed;
            float _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float, _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float);
            float2 _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float.xx), _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2);
            float _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float = _LeafWindScale;
            float _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2, _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float, _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float);
            float3 _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            Unity_Lerp_float3(IN.ObjectSpacePosition, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3, (_GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float.xxx), _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3);
            description.Position = _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 Emission;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Split_20d61187fc734d028a2e0283ab610e2e_R_1_Float = IN.VertexColor[0];
            float _Split_20d61187fc734d028a2e0283ab610e2e_G_2_Float = IN.VertexColor[1];
            float _Split_20d61187fc734d028a2e0283ab610e2e_B_3_Float = IN.VertexColor[2];
            float _Split_20d61187fc734d028a2e0283ab610e2e_A_4_Float = IN.VertexColor[3];
            float _Property_deb326794dfb4908b3033da0b6291d6f_Out_0_Float = _AOStrength;
            float _Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float;
            Unity_Lerp_float(float(1), _Split_20d61187fc734d028a2e0283ab610e2e_A_4_Float, _Property_deb326794dfb4908b3033da0b6291d6f_Out_0_Float, _Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float);
            float4 _Property_30c5b5ed235e4330a6f9102cb3c56835_Out_0_Vector4 = _BaseColor;
            float4 _Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float.xxxx), _Property_30c5b5ed235e4330a6f9102cb3c56835_Out_0_Vector4, _Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4);
            float4 _Property_afb5f1e2fd284814bcf9c1ad4b9fc121_Out_0_Vector4 = _GradientColor;
            float4 _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4 = IN.uv0;
            float _Split_708de473eabc4313b3f7bc1c5d62096a_R_1_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[0];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_G_2_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[1];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_B_3_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[2];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_A_4_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[3];
            float _OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float;
            Unity_OneMinus_float(_Split_708de473eabc4313b3f7bc1c5d62096a_G_2_Float, _OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float);
            float _Property_77a6800940754a35be5fcef88a3a155e_Out_0_Float = _Gradient_Offset;
            float _Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float;
            Unity_Add_float(_OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float, _Property_77a6800940754a35be5fcef88a3a155e_Out_0_Float, _Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float);
            float _Saturate_6856939e57034af985ed339413123d4e_Out_1_Float;
            Unity_Saturate_float(_Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float, _Saturate_6856939e57034af985ed339413123d4e_Out_1_Float);
            float4 _Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4;
            Unity_Lerp_float4(_Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4, _Property_afb5f1e2fd284814bcf9c1ad4b9fc121_Out_0_Vector4, (_Saturate_6856939e57034af985ed339413123d4e_Out_1_Float.xxxx), _Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4);
            UnityTexture2D _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Emission_Map);
            float4 _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.tex, _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.samplerstate, _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_R_4_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.r;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_G_5_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.g;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_B_6_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.b;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_A_7_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.a;
            float4 _Property_ba712bb9090a4c52a43f38f92fd73591_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_EmissionColor) : _EmissionColor;
            float4 _Multiply_16c8957045404e8cb4dc99dec57c9601_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4, _Property_ba712bb9090a4c52a43f38f92fd73591_Out_0_Vector4, _Multiply_16c8957045404e8cb4dc99dec57c9601_Out_2_Vector4);
            float3 _Property_dcdf3d2648794cddb9212380f6973d15_Out_0_Vector3 = _SSS_Distortion;
            float _Split_3caa93318451401d87ad12e7a907097f_R_1_Float = _Property_dcdf3d2648794cddb9212380f6973d15_Out_0_Vector3[0];
            float _Split_3caa93318451401d87ad12e7a907097f_G_2_Float = _Property_dcdf3d2648794cddb9212380f6973d15_Out_0_Vector3[1];
            float _Split_3caa93318451401d87ad12e7a907097f_B_3_Float = _Property_dcdf3d2648794cddb9212380f6973d15_Out_0_Vector3[2];
            float _Split_3caa93318451401d87ad12e7a907097f_A_4_Float = 0;
            float3 _Multiply_22c18134d5924015ba2ec2a834e22739_Out_2_Vector3;
            Unity_Multiply_float3_float3(IN.WorldSpaceNormal, (_Split_3caa93318451401d87ad12e7a907097f_B_3_Float.xxx), _Multiply_22c18134d5924015ba2ec2a834e22739_Out_2_Vector3);
            float3 _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3;
            float3 _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Color_1_Vector3;
            float _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_ShadowAtten_2_Float;
            GetMainLight_float(_GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3, _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Color_1_Vector3, _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_ShadowAtten_2_Float);
            float _Split_8f9f81fe0bee433ca8736d7047af190c_R_1_Float = _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3[0];
            float _Split_8f9f81fe0bee433ca8736d7047af190c_G_2_Float = _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3[1];
            float _Split_8f9f81fe0bee433ca8736d7047af190c_B_3_Float = _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Direction_0_Vector3[2];
            float _Split_8f9f81fe0bee433ca8736d7047af190c_A_4_Float = 0;
            float _Multiply_eb7953577efa42578ac8713bee265f5b_Out_2_Float;
            Unity_Multiply_float_float(_Split_3caa93318451401d87ad12e7a907097f_R_1_Float, _Split_8f9f81fe0bee433ca8736d7047af190c_R_1_Float, _Multiply_eb7953577efa42578ac8713bee265f5b_Out_2_Float);
            float _Multiply_008c4dc1817d4499bb015a203c3b9fc9_Out_2_Float;
            Unity_Multiply_float_float(_Split_3caa93318451401d87ad12e7a907097f_G_2_Float, _Split_8f9f81fe0bee433ca8736d7047af190c_G_2_Float, _Multiply_008c4dc1817d4499bb015a203c3b9fc9_Out_2_Float);
            float4 _Combine_6028bb2051e2436b810df848e038d344_RGBA_4_Vector4;
            float3 _Combine_6028bb2051e2436b810df848e038d344_RGB_5_Vector3;
            float2 _Combine_6028bb2051e2436b810df848e038d344_RG_6_Vector2;
            Unity_Combine_float(_Multiply_eb7953577efa42578ac8713bee265f5b_Out_2_Float, _Multiply_008c4dc1817d4499bb015a203c3b9fc9_Out_2_Float, _Split_8f9f81fe0bee433ca8736d7047af190c_B_3_Float, float(0), _Combine_6028bb2051e2436b810df848e038d344_RGBA_4_Vector4, _Combine_6028bb2051e2436b810df848e038d344_RGB_5_Vector3, _Combine_6028bb2051e2436b810df848e038d344_RG_6_Vector2);
            float3 _Add_be6bd9b0b67c4b6db449176e8ca930b2_Out_2_Vector3;
            Unity_Add_float3(_Multiply_22c18134d5924015ba2ec2a834e22739_Out_2_Vector3, _Combine_6028bb2051e2436b810df848e038d344_RGB_5_Vector3, _Add_be6bd9b0b67c4b6db449176e8ca930b2_Out_2_Vector3);
            float3 _Normalize_f3cd7974fb394a2cadf32d57b28d9261_Out_1_Vector3;
            Unity_Normalize_float3(_Add_be6bd9b0b67c4b6db449176e8ca930b2_Out_2_Vector3, _Normalize_f3cd7974fb394a2cadf32d57b28d9261_Out_1_Vector3);
            float3 _Negate_1793cf15ec9c407f8e406f36ca089de6_Out_1_Vector3;
            Unity_Negate_float3(_Normalize_f3cd7974fb394a2cadf32d57b28d9261_Out_1_Vector3, _Negate_1793cf15ec9c407f8e406f36ca089de6_Out_1_Vector3);
            float _DotProduct_b92b4de671844a4cb48bdf3543eb8209_Out_2_Float;
            Unity_DotProduct_float3(IN.WorldSpaceViewDirection, _Negate_1793cf15ec9c407f8e406f36ca089de6_Out_1_Vector3, _DotProduct_b92b4de671844a4cb48bdf3543eb8209_Out_2_Float);
            float _Saturate_ac2a94e9b1f44dc999b6a5f585e92ec5_Out_1_Float;
            Unity_Saturate_float(_DotProduct_b92b4de671844a4cb48bdf3543eb8209_Out_2_Float, _Saturate_ac2a94e9b1f44dc999b6a5f585e92ec5_Out_1_Float);
            float _Property_7f4e24b9dccf4656aa1db4f2b6d9c394_Out_0_Float = _SSS_Scattering;
            float _Power_6bffe28ecec947a4978b0e5497d49690_Out_2_Float;
            Unity_Power_float(_Saturate_ac2a94e9b1f44dc999b6a5f585e92ec5_Out_1_Float, _Property_7f4e24b9dccf4656aa1db4f2b6d9c394_Out_0_Float, _Power_6bffe28ecec947a4978b0e5497d49690_Out_2_Float);
            float _Power_bd15f48c24704cf38a062bb3163ede20_Out_2_Float;
            Unity_Power_float(_Power_6bffe28ecec947a4978b0e5497d49690_Out_2_Float, _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_ShadowAtten_2_Float, _Power_bd15f48c24704cf38a062bb3163ede20_Out_2_Float);
            float4 _Property_0c7582d4a9b7493287ae8b1cf4e99a8a_Out_0_Vector4 = _SSS_Color;
            float4 _Multiply_5fff722b6be44e2c8ac7eb45803009da_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Power_bd15f48c24704cf38a062bb3163ede20_Out_2_Float.xxxx), _Property_0c7582d4a9b7493287ae8b1cf4e99a8a_Out_0_Vector4, _Multiply_5fff722b6be44e2c8ac7eb45803009da_Out_2_Vector4);
            float _Property_1572ee45033348219e19d5ae51034997_Out_0_Float = _SSS_Intensity;
            float4 _Multiply_9b5ece35f1b94fd99f2d33c3e4f1d918_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Multiply_5fff722b6be44e2c8ac7eb45803009da_Out_2_Vector4, (_Property_1572ee45033348219e19d5ae51034997_Out_0_Float.xxxx), _Multiply_9b5ece35f1b94fd99f2d33c3e4f1d918_Out_2_Vector4);
            float3 _Multiply_66b508c9e81948b8a34cea6baffd763d_Out_2_Vector3;
            Unity_Multiply_float3_float3((_Multiply_9b5ece35f1b94fd99f2d33c3e4f1d918_Out_2_Vector4.xyz), _GetMainLightCustomFunction_20c63c44337547e3b5daa84f4a254644_Color_1_Vector3, _Multiply_66b508c9e81948b8a34cea6baffd763d_Out_2_Vector3);
            float3 _Add_9595ed0b6087400383dcbc8cdce459f6_Out_2_Vector3;
            Unity_Add_float3((_Multiply_16c8957045404e8cb4dc99dec57c9601_Out_2_Vector4.xyz), _Multiply_66b508c9e81948b8a34cea6baffd763d_Out_2_Vector3, _Add_9595ed0b6087400383dcbc8cdce459f6_Out_2_Vector3);
            UnityTexture2D _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.tex, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.samplerstate, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_R_4_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.r;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_G_5_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.g;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_B_6_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.b;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.a;
            float _Split_4f8f81177c174703b3b7216f7abcab96_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_4f8f81177c174703b3b7216f7abcab96_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_4f8f81177c174703b3b7216f7abcab96_A_4_Float = 0;
            float _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float = _CutHeight;
            float _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float;
            Unity_Subtract_float(_Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float, _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float, _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float);
            float _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float = _EdgeSoftness;
            float _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float;
            Unity_Divide_float(_Subtract_14c160049cc54391926538d185ebb749_Out_2_Float, _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float, _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float);
            float _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float;
            Unity_Saturate_float(_Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float, _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float);
            float _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float;
            Unity_OneMinus_float(_Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float);
            float _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float, _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float);
            float _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float);
            float _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            Unity_Subtract_float(_Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float, _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float, _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float);
            float3 _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3;
            Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(IN.WorldSpacePosition, _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3);
            float3 _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3;
            Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(IN.WorldSpacePosition, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3);
            float3 _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3;
            Unity_CrossProduct_float(_DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3, _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3);
            float3 _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3;
            Unity_Normalize_float3(_CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3, _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3);
            float _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float;
            Unity_DotProduct_float3(_Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3, IN.WorldSpaceViewDirection, _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float);
            float _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float;
            Unity_Absolute_float(_DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float, _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float);
            float _Property_406505a58d794e15861d235061da0712_Out_0_Float = _BaseGlancingAngleCut;
            float _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float;
            Unity_Multiply_float_float(_Property_406505a58d794e15861d235061da0712_Out_0_Float, 3, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float);
            float _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float;
            Unity_Lerp_float(float(1), _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float);
            float _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float, _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 1, _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float);
            surface.BaseColor = (_Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4.xyz);
            surface.Emission = _Add_9595ed0b6087400383dcbc8cdce459f6_Out_2_Vector3;
            surface.Alpha = _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.uv0 =                                        input.uv0;
            output.VertexColor =                                input.color;
            output.TimeParameters =                             _TimeParameters.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS = input.normalWS;
            const float renormFactor = 1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature_local_fragment _ _ALPHATEST_ON
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define GRAPH_VERTEX_USES_TIME_PARAMETERS_INPUT
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float3 positionWS : INTERP1;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float _CutHeight;
        float _EdgeSoftness;
        float _Dither;
        float4 _Normal_Map_TexelSize;
        float4 _MetallicMap_TexelSize;
        float _Smoothness;
        float4 _OcclusionMap_TexelSize;
        float4 _Emission_Map_TexelSize;
        float4 _EmissionColor;
        float _Metallic;
        float _LeadWindSpeed;
        float _LeafWindScale;
        float _BendSpeed;
        float4 _BendDirection;
        float3 _Offset;
        float _BaseGlancingAngleCut;
        float _EnableFlipNormal;
        float _SSS_Scattering;
        float4 _SSS_Color;
        float _SSS_Intensity;
        float3 _SSS_Distortion;
        float _AOStrength;
        float _Gradient_Offset;
        float4 _GradientColor;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_Normal_Map);
        SAMPLER(sampler_Normal_Map);
        TEXTURE2D(_MetallicMap);
        SAMPLER(sampler_MetallicMap);
        TEXTURE2D(_OcclusionMap);
        SAMPLER(sampler_OcclusionMap);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
        {
             float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
             Out = lerp(Min, Max, randomno);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
        {
            float x; Hash_Tchou_2_1_float(p, x);
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }
        
        void Unity_GradientNoise_Deterministic_float (float2 UV, float3 Scale, out float Out)
        {
            float2 p = UV * Scale.xy;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDX' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddx(In);
        }
        
        void Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDY' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddy(In);
        }
        
        void Unity_CrossProduct_float(float3 A, float3 B, out float3 Out)
        {
            Out = cross(A, B);
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Split_e6ba90e06c194240888d9b07fce58a5d_R_1_Float = IN.ObjectSpacePosition[0];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float = IN.ObjectSpacePosition[1];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_B_3_Float = IN.ObjectSpacePosition[2];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_A_4_Float = 0;
            float _Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float = _BendSpeed;
            float3 _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3 = _Offset;
            float3 _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3;
            Unity_Add_float3(SHADERGRAPH_OBJECT_POSITION, _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3, _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3);
            float _Split_74566faebea64f928d3ab3395dea96c0_R_1_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[0];
            float _Split_74566faebea64f928d3ab3395dea96c0_G_2_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[1];
            float _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[2];
            float _Split_74566faebea64f928d3ab3395dea96c0_A_4_Float = 0;
            float2 _Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2 = float2(_Split_74566faebea64f928d3ab3395dea96c0_R_1_Float, _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float);
            float _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float;
            Unity_RandomRange_float(_Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2, float(0.1), float(1), _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float);
            float _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float;
            Unity_Multiply_float_float(_Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float, _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float);
            float _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.z, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float, _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float);
            float _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float;
            Unity_Divide_float(_Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float, float(100), _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float);
            float _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float;
            Unity_Multiply_float_float(_Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float, _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float, _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float);
            float _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float;
            Unity_Add_float(_Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float, float(1), _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float);
            float _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float;
            Unity_Multiply_float_float(_Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float);
            float _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float);
            float _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float;
            Unity_Subtract_float(_Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float);
            float4 _Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4 = _BendDirection;
            float4 _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4, (_RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4);
            float4 _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4, _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4);
            float _Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[0];
            float _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[1];
            float _Split_2e4561fe3a484551a112182bf2ae2954_B_3_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[2];
            float _Split_2e4561fe3a484551a112182bf2ae2954_A_4_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[3];
            float4 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4;
            float3 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3;
            float2 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2;
            Unity_Combine_float(_Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float, float(0), _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float, float(0), _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2);
            float3 _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3;
            Unity_Add_float3(IN.WorldSpacePosition, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3);
            float3 _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3;
            _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3 = TransformWorldToObject(_Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3.xyz);
            float _Split_86265132a9074281af78965592fb42d4_R_1_Float = IN.VertexColor[0];
            float _Split_86265132a9074281af78965592fb42d4_G_2_Float = IN.VertexColor[1];
            float _Split_86265132a9074281af78965592fb42d4_B_3_Float = IN.VertexColor[2];
            float _Split_86265132a9074281af78965592fb42d4_A_4_Float = IN.VertexColor[3];
            float4 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4;
            float3 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3;
            float2 _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2;
            Unity_Combine_float(_Split_86265132a9074281af78965592fb42d4_R_1_Float, _Split_86265132a9074281af78965592fb42d4_G_2_Float, float(0), float(0), _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2);
            float3 _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3;
            Unity_Lerp_float3(_Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3, IN.ObjectSpacePosition, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3);
            float _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float = _LeadWindSpeed;
            float _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float, _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float);
            float2 _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float.xx), _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2);
            float _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float = _LeafWindScale;
            float _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2, _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float, _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float);
            float3 _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            Unity_Lerp_float3(IN.ObjectSpacePosition, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3, (_GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float.xxx), _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3);
            description.Position = _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.tex, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.samplerstate, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_R_4_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.r;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_G_5_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.g;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_B_6_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.b;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.a;
            float _Split_4f8f81177c174703b3b7216f7abcab96_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_4f8f81177c174703b3b7216f7abcab96_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_4f8f81177c174703b3b7216f7abcab96_A_4_Float = 0;
            float _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float = _CutHeight;
            float _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float;
            Unity_Subtract_float(_Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float, _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float, _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float);
            float _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float = _EdgeSoftness;
            float _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float;
            Unity_Divide_float(_Subtract_14c160049cc54391926538d185ebb749_Out_2_Float, _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float, _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float);
            float _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float;
            Unity_Saturate_float(_Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float, _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float);
            float _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float;
            Unity_OneMinus_float(_Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float);
            float _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float, _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float);
            float _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float);
            float _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            Unity_Subtract_float(_Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float, _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float, _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float);
            float3 _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3;
            Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(IN.WorldSpacePosition, _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3);
            float3 _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3;
            Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(IN.WorldSpacePosition, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3);
            float3 _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3;
            Unity_CrossProduct_float(_DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3, _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3);
            float3 _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3;
            Unity_Normalize_float3(_CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3, _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3);
            float _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float;
            Unity_DotProduct_float3(_Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3, IN.WorldSpaceViewDirection, _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float);
            float _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float;
            Unity_Absolute_float(_DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float, _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float);
            float _Property_406505a58d794e15861d235061da0712_Out_0_Float = _BaseGlancingAngleCut;
            float _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float;
            Unity_Multiply_float_float(_Property_406505a58d794e15861d235061da0712_Out_0_Float, 3, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float);
            float _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float;
            Unity_Lerp_float(float(1), _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float);
            float _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float, _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 1, _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float);
            surface.Alpha = _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.uv0 =                                        input.uv0;
            output.VertexColor =                                input.color;
            output.TimeParameters =                             _TimeParameters.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull [_Cull]
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature_local_fragment _ _ALPHATEST_ON
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define GRAPH_VERTEX_USES_TIME_PARAMETERS_INPUT
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
             float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 color : INTERP1;
             float3 positionWS : INTERP2;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float _CutHeight;
        float _EdgeSoftness;
        float _Dither;
        float4 _Normal_Map_TexelSize;
        float4 _MetallicMap_TexelSize;
        float _Smoothness;
        float4 _OcclusionMap_TexelSize;
        float4 _Emission_Map_TexelSize;
        float4 _EmissionColor;
        float _Metallic;
        float _LeadWindSpeed;
        float _LeafWindScale;
        float _BendSpeed;
        float4 _BendDirection;
        float3 _Offset;
        float _BaseGlancingAngleCut;
        float _EnableFlipNormal;
        float _SSS_Scattering;
        float4 _SSS_Color;
        float _SSS_Intensity;
        float3 _SSS_Distortion;
        float _AOStrength;
        float _Gradient_Offset;
        float4 _GradientColor;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_Normal_Map);
        SAMPLER(sampler_Normal_Map);
        TEXTURE2D(_MetallicMap);
        SAMPLER(sampler_MetallicMap);
        TEXTURE2D(_OcclusionMap);
        SAMPLER(sampler_OcclusionMap);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
        {
             float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
             Out = lerp(Min, Max, randomno);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
        {
            float x; Hash_Tchou_2_1_float(p, x);
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }
        
        void Unity_GradientNoise_Deterministic_float (float2 UV, float3 Scale, out float Out)
        {
            float2 p = UV * Scale.xy;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDX' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddx(In);
        }
        
        void Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDY' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddy(In);
        }
        
        void Unity_CrossProduct_float(float3 A, float3 B, out float3 Out)
        {
            Out = cross(A, B);
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Split_e6ba90e06c194240888d9b07fce58a5d_R_1_Float = IN.ObjectSpacePosition[0];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float = IN.ObjectSpacePosition[1];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_B_3_Float = IN.ObjectSpacePosition[2];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_A_4_Float = 0;
            float _Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float = _BendSpeed;
            float3 _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3 = _Offset;
            float3 _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3;
            Unity_Add_float3(SHADERGRAPH_OBJECT_POSITION, _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3, _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3);
            float _Split_74566faebea64f928d3ab3395dea96c0_R_1_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[0];
            float _Split_74566faebea64f928d3ab3395dea96c0_G_2_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[1];
            float _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[2];
            float _Split_74566faebea64f928d3ab3395dea96c0_A_4_Float = 0;
            float2 _Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2 = float2(_Split_74566faebea64f928d3ab3395dea96c0_R_1_Float, _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float);
            float _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float;
            Unity_RandomRange_float(_Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2, float(0.1), float(1), _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float);
            float _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float;
            Unity_Multiply_float_float(_Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float, _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float);
            float _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.z, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float, _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float);
            float _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float;
            Unity_Divide_float(_Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float, float(100), _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float);
            float _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float;
            Unity_Multiply_float_float(_Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float, _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float, _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float);
            float _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float;
            Unity_Add_float(_Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float, float(1), _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float);
            float _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float;
            Unity_Multiply_float_float(_Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float);
            float _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float);
            float _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float;
            Unity_Subtract_float(_Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float);
            float4 _Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4 = _BendDirection;
            float4 _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4, (_RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4);
            float4 _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4, _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4);
            float _Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[0];
            float _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[1];
            float _Split_2e4561fe3a484551a112182bf2ae2954_B_3_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[2];
            float _Split_2e4561fe3a484551a112182bf2ae2954_A_4_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[3];
            float4 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4;
            float3 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3;
            float2 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2;
            Unity_Combine_float(_Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float, float(0), _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float, float(0), _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2);
            float3 _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3;
            Unity_Add_float3(IN.WorldSpacePosition, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3);
            float3 _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3;
            _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3 = TransformWorldToObject(_Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3.xyz);
            float _Split_86265132a9074281af78965592fb42d4_R_1_Float = IN.VertexColor[0];
            float _Split_86265132a9074281af78965592fb42d4_G_2_Float = IN.VertexColor[1];
            float _Split_86265132a9074281af78965592fb42d4_B_3_Float = IN.VertexColor[2];
            float _Split_86265132a9074281af78965592fb42d4_A_4_Float = IN.VertexColor[3];
            float4 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4;
            float3 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3;
            float2 _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2;
            Unity_Combine_float(_Split_86265132a9074281af78965592fb42d4_R_1_Float, _Split_86265132a9074281af78965592fb42d4_G_2_Float, float(0), float(0), _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2);
            float3 _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3;
            Unity_Lerp_float3(_Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3, IN.ObjectSpacePosition, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3);
            float _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float = _LeadWindSpeed;
            float _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float, _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float);
            float2 _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float.xx), _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2);
            float _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float = _LeafWindScale;
            float _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2, _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float, _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float);
            float3 _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            Unity_Lerp_float3(IN.ObjectSpacePosition, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3, (_GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float.xxx), _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3);
            description.Position = _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Split_20d61187fc734d028a2e0283ab610e2e_R_1_Float = IN.VertexColor[0];
            float _Split_20d61187fc734d028a2e0283ab610e2e_G_2_Float = IN.VertexColor[1];
            float _Split_20d61187fc734d028a2e0283ab610e2e_B_3_Float = IN.VertexColor[2];
            float _Split_20d61187fc734d028a2e0283ab610e2e_A_4_Float = IN.VertexColor[3];
            float _Property_deb326794dfb4908b3033da0b6291d6f_Out_0_Float = _AOStrength;
            float _Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float;
            Unity_Lerp_float(float(1), _Split_20d61187fc734d028a2e0283ab610e2e_A_4_Float, _Property_deb326794dfb4908b3033da0b6291d6f_Out_0_Float, _Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float);
            float4 _Property_30c5b5ed235e4330a6f9102cb3c56835_Out_0_Vector4 = _BaseColor;
            float4 _Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float.xxxx), _Property_30c5b5ed235e4330a6f9102cb3c56835_Out_0_Vector4, _Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4);
            float4 _Property_afb5f1e2fd284814bcf9c1ad4b9fc121_Out_0_Vector4 = _GradientColor;
            float4 _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4 = IN.uv0;
            float _Split_708de473eabc4313b3f7bc1c5d62096a_R_1_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[0];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_G_2_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[1];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_B_3_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[2];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_A_4_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[3];
            float _OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float;
            Unity_OneMinus_float(_Split_708de473eabc4313b3f7bc1c5d62096a_G_2_Float, _OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float);
            float _Property_77a6800940754a35be5fcef88a3a155e_Out_0_Float = _Gradient_Offset;
            float _Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float;
            Unity_Add_float(_OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float, _Property_77a6800940754a35be5fcef88a3a155e_Out_0_Float, _Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float);
            float _Saturate_6856939e57034af985ed339413123d4e_Out_1_Float;
            Unity_Saturate_float(_Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float, _Saturate_6856939e57034af985ed339413123d4e_Out_1_Float);
            float4 _Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4;
            Unity_Lerp_float4(_Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4, _Property_afb5f1e2fd284814bcf9c1ad4b9fc121_Out_0_Vector4, (_Saturate_6856939e57034af985ed339413123d4e_Out_1_Float.xxxx), _Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4);
            UnityTexture2D _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.tex, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.samplerstate, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_R_4_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.r;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_G_5_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.g;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_B_6_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.b;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.a;
            float _Split_4f8f81177c174703b3b7216f7abcab96_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_4f8f81177c174703b3b7216f7abcab96_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_4f8f81177c174703b3b7216f7abcab96_A_4_Float = 0;
            float _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float = _CutHeight;
            float _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float;
            Unity_Subtract_float(_Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float, _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float, _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float);
            float _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float = _EdgeSoftness;
            float _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float;
            Unity_Divide_float(_Subtract_14c160049cc54391926538d185ebb749_Out_2_Float, _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float, _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float);
            float _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float;
            Unity_Saturate_float(_Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float, _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float);
            float _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float;
            Unity_OneMinus_float(_Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float);
            float _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float, _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float);
            float _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float);
            float _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            Unity_Subtract_float(_Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float, _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float, _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float);
            float3 _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3;
            Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(IN.WorldSpacePosition, _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3);
            float3 _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3;
            Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(IN.WorldSpacePosition, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3);
            float3 _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3;
            Unity_CrossProduct_float(_DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3, _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3);
            float3 _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3;
            Unity_Normalize_float3(_CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3, _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3);
            float _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float;
            Unity_DotProduct_float3(_Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3, IN.WorldSpaceViewDirection, _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float);
            float _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float;
            Unity_Absolute_float(_DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float, _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float);
            float _Property_406505a58d794e15861d235061da0712_Out_0_Float = _BaseGlancingAngleCut;
            float _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float;
            Unity_Multiply_float_float(_Property_406505a58d794e15861d235061da0712_Out_0_Float, 3, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float);
            float _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float;
            Unity_Lerp_float(float(1), _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float);
            float _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float, _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 1, _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float);
            surface.BaseColor = (_Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4.xyz);
            surface.Alpha = _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.uv0 =                                        input.uv0;
            output.VertexColor =                                input.color;
            output.TimeParameters =                             _TimeParameters.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Universal 2D"
            Tags
            {
                "LightMode" = "Universal2D"
            }
        
        // Render State
        Cull [_Cull]
        Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
        ZTest [_ZTest]
        ZWrite [_ZWrite]
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature_local_fragment _ _ALPHATEST_ON
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define GRAPH_VERTEX_USES_TIME_PARAMETERS_INPUT
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_2D
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceViewDirection;
             float3 WorldSpacePosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
             float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 color : INTERP1;
             float3 positionWS : INTERP2;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_TexelSize;
        float4 _BaseColor;
        float _CutHeight;
        float _EdgeSoftness;
        float _Dither;
        float4 _Normal_Map_TexelSize;
        float4 _MetallicMap_TexelSize;
        float _Smoothness;
        float4 _OcclusionMap_TexelSize;
        float4 _Emission_Map_TexelSize;
        float4 _EmissionColor;
        float _Metallic;
        float _LeadWindSpeed;
        float _LeafWindScale;
        float _BendSpeed;
        float4 _BendDirection;
        float3 _Offset;
        float _BaseGlancingAngleCut;
        float _EnableFlipNormal;
        float _SSS_Scattering;
        float4 _SSS_Color;
        float _SSS_Intensity;
        float3 _SSS_Distortion;
        float _AOStrength;
        float _Gradient_Offset;
        float4 _GradientColor;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_Normal_Map);
        SAMPLER(sampler_Normal_Map);
        TEXTURE2D(_MetallicMap);
        SAMPLER(sampler_MetallicMap);
        TEXTURE2D(_OcclusionMap);
        SAMPLER(sampler_OcclusionMap);
        TEXTURE2D(_Emission_Map);
        SAMPLER(sampler_Emission_Map);
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
        {
             float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
             Out = lerp(Min, Max, randomno);
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Combine_float(float R, float G, float B, float A, out float4 RGBA, out float3 RGB, out float2 RG)
        {
            RGBA = float4(R, G, B, A);
            RGB = float3(R, G, B);
            RG = float2(R, G);
        }
        
        void Unity_Lerp_float3(float3 A, float3 B, float3 T, out float3 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
        {
            float x; Hash_Tchou_2_1_float(p, x);
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }
        
        void Unity_GradientNoise_Deterministic_float (float2 UV, float3 Scale, out float Out)
        {
            float2 p = UV * Scale.xy;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }
        
        void Unity_Lerp_float(float A, float B, float T, out float Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDX' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddx(In);
        }
        
        void Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(float3 In, out float3 Out)
        {
            
                    #if defined(SHADER_STAGE_RAY_TRACING) && defined(RAYTRACING_SHADER_GRAPH_DEFAULT)
                    #error 'DDY' node is not supported in ray tracing, please provide an alternate implementation, relying for instance on the 'Raytracing Quality' keyword
                    #endif
            Out = ddy(In);
        }
        
        void Unity_CrossProduct_float(float3 A, float3 B, out float3 Out)
        {
            Out = cross(A, B);
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_DotProduct_float3(float3 A, float3 B, out float Out)
        {
            Out = dot(A, B);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Dither_float(float In, float4 ScreenPosition, out float Out)
        {
            float2 uv = ScreenPosition.xy * _ScreenParams.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            Out = In - DITHER_THRESHOLDS[index];
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Split_e6ba90e06c194240888d9b07fce58a5d_R_1_Float = IN.ObjectSpacePosition[0];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float = IN.ObjectSpacePosition[1];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_B_3_Float = IN.ObjectSpacePosition[2];
            float _Split_e6ba90e06c194240888d9b07fce58a5d_A_4_Float = 0;
            float _Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float = _BendSpeed;
            float3 _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3 = _Offset;
            float3 _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3;
            Unity_Add_float3(SHADERGRAPH_OBJECT_POSITION, _Property_ea8d1d01d91c48768b75e19aa0853098_Out_0_Vector3, _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3);
            float _Split_74566faebea64f928d3ab3395dea96c0_R_1_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[0];
            float _Split_74566faebea64f928d3ab3395dea96c0_G_2_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[1];
            float _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float = _Add_b1dbafd41a974e4684209abf88727f74_Out_2_Vector3[2];
            float _Split_74566faebea64f928d3ab3395dea96c0_A_4_Float = 0;
            float2 _Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2 = float2(_Split_74566faebea64f928d3ab3395dea96c0_R_1_Float, _Split_74566faebea64f928d3ab3395dea96c0_B_3_Float);
            float _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float;
            Unity_RandomRange_float(_Vector2_07201d61e0654cd6911b989b83e46415_Out_0_Vector2, float(0.1), float(1), _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float);
            float _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float;
            Unity_Multiply_float_float(_Property_882c8da6381d47efa1a0ca75b851a761_Out_0_Float, _RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float);
            float _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.z, _Multiply_778c92d245974c25bba6663e3b246d0f_Out_2_Float, _Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float);
            float _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float;
            Unity_Divide_float(_Multiply_f39f43316a6749e482ab29f0eae2468b_Out_2_Float, float(100), _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float);
            float _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float;
            Unity_Multiply_float_float(_Split_e6ba90e06c194240888d9b07fce58a5d_G_2_Float, _Divide_76126a9f9a0c4b05ae340692db5a6ba8_Out_2_Float, _Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float);
            float _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float;
            Unity_Add_float(_Multiply_a383754d814d4baabec43266c53e1e4b_Out_2_Float, float(1), _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float);
            float _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float;
            Unity_Multiply_float_float(_Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Add_a516f0f42673498b9106837df1e0312e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float);
            float _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float);
            float _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float;
            Unity_Subtract_float(_Multiply_edf3a77a577448dd827709d482a4930e_Out_2_Float, _Multiply_fc92f2e0190c47c88fe94c8fc5722642_Out_2_Float, _Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float);
            float4 _Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4 = _BendDirection;
            float4 _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Property_73d567aa83484deba2ad74f812b66fe2_Out_0_Vector4, (_RandomRange_94147062a53842d7897d99c4903bd685_Out_3_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4);
            float4 _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Subtract_140de6666c614c3aae2d57344e1bb9c5_Out_2_Float.xxxx), _Multiply_19014bfa9490414892512da40027fdca_Out_2_Vector4, _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4);
            float _Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[0];
            float _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[1];
            float _Split_2e4561fe3a484551a112182bf2ae2954_B_3_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[2];
            float _Split_2e4561fe3a484551a112182bf2ae2954_A_4_Float = _Multiply_a666989a64f94573b70a3bf3233ec63d_Out_2_Vector4[3];
            float4 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4;
            float3 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3;
            float2 _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2;
            Unity_Combine_float(_Split_2e4561fe3a484551a112182bf2ae2954_R_1_Float, float(0), _Split_2e4561fe3a484551a112182bf2ae2954_G_2_Float, float(0), _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGBA_4_Vector4, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RG_6_Vector2);
            float3 _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3;
            Unity_Add_float3(IN.WorldSpacePosition, _Combine_51b9d8d356824cc78f3d9b74b9ad43b5_RGB_5_Vector3, _Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3);
            float3 _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3;
            _Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3 = TransformWorldToObject(_Add_89639d8cf4744bb5a4873cc433edbc6e_Out_2_Vector3.xyz);
            float _Split_86265132a9074281af78965592fb42d4_R_1_Float = IN.VertexColor[0];
            float _Split_86265132a9074281af78965592fb42d4_G_2_Float = IN.VertexColor[1];
            float _Split_86265132a9074281af78965592fb42d4_B_3_Float = IN.VertexColor[2];
            float _Split_86265132a9074281af78965592fb42d4_A_4_Float = IN.VertexColor[3];
            float4 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4;
            float3 _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3;
            float2 _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2;
            Unity_Combine_float(_Split_86265132a9074281af78965592fb42d4_R_1_Float, _Split_86265132a9074281af78965592fb42d4_G_2_Float, float(0), float(0), _Combine_475f663663ae4a48b26df8ccb916e2c1_RGBA_4_Vector4, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Combine_475f663663ae4a48b26df8ccb916e2c1_RG_6_Vector2);
            float3 _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3;
            Unity_Lerp_float3(_Transform_63d03ce3c56f41569914b3957593f176_Out_1_Vector3, IN.ObjectSpacePosition, _Combine_475f663663ae4a48b26df8ccb916e2c1_RGB_5_Vector3, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3);
            float _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float = _LeadWindSpeed;
            float _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_3326daf0c4d84f98a9395f1f9845ddb1_Out_0_Float, _Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float);
            float2 _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_d3346c546a3d4ed8aadcb88d237be369_Out_2_Float.xx), _TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2);
            float _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float = _LeafWindScale;
            float _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float;
            Unity_GradientNoise_Deterministic_float(_TilingAndOffset_03f729529ffe441e931fac9f57de83aa_Out_3_Vector2, _Property_384102bcd4c446c9a58d0fe7d41722a3_Out_0_Float, _GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float);
            float3 _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            Unity_Lerp_float3(IN.ObjectSpacePosition, _Lerp_454333079a824ab78c7815f3531540c3_Out_3_Vector3, (_GradientNoise_620e3d0c6cdd45e994062f39eb53b8de_Out_2_Float.xxx), _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3);
            description.Position = _Lerp_6bda1669590c460cbe0670bf80223d00_Out_3_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Split_20d61187fc734d028a2e0283ab610e2e_R_1_Float = IN.VertexColor[0];
            float _Split_20d61187fc734d028a2e0283ab610e2e_G_2_Float = IN.VertexColor[1];
            float _Split_20d61187fc734d028a2e0283ab610e2e_B_3_Float = IN.VertexColor[2];
            float _Split_20d61187fc734d028a2e0283ab610e2e_A_4_Float = IN.VertexColor[3];
            float _Property_deb326794dfb4908b3033da0b6291d6f_Out_0_Float = _AOStrength;
            float _Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float;
            Unity_Lerp_float(float(1), _Split_20d61187fc734d028a2e0283ab610e2e_A_4_Float, _Property_deb326794dfb4908b3033da0b6291d6f_Out_0_Float, _Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float);
            float4 _Property_30c5b5ed235e4330a6f9102cb3c56835_Out_0_Vector4 = _BaseColor;
            float4 _Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Lerp_0bda863e630241798d2956b63f99ea37_Out_3_Float.xxxx), _Property_30c5b5ed235e4330a6f9102cb3c56835_Out_0_Vector4, _Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4);
            float4 _Property_afb5f1e2fd284814bcf9c1ad4b9fc121_Out_0_Vector4 = _GradientColor;
            float4 _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4 = IN.uv0;
            float _Split_708de473eabc4313b3f7bc1c5d62096a_R_1_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[0];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_G_2_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[1];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_B_3_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[2];
            float _Split_708de473eabc4313b3f7bc1c5d62096a_A_4_Float = _UV_100ea2f294ac482389c3f9aa22a0c224_Out_0_Vector4[3];
            float _OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float;
            Unity_OneMinus_float(_Split_708de473eabc4313b3f7bc1c5d62096a_G_2_Float, _OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float);
            float _Property_77a6800940754a35be5fcef88a3a155e_Out_0_Float = _Gradient_Offset;
            float _Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float;
            Unity_Add_float(_OneMinus_f80ad19bf6ce4882a43e6cc8d3c164c8_Out_1_Float, _Property_77a6800940754a35be5fcef88a3a155e_Out_0_Float, _Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float);
            float _Saturate_6856939e57034af985ed339413123d4e_Out_1_Float;
            Unity_Saturate_float(_Add_5f69767673384b2dbe5d8cdabf736450_Out_2_Float, _Saturate_6856939e57034af985ed339413123d4e_Out_1_Float);
            float4 _Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4;
            Unity_Lerp_float4(_Multiply_97382ea7fe7442f795e3643d649bda2e_Out_2_Vector4, _Property_afb5f1e2fd284814bcf9c1ad4b9fc121_Out_0_Vector4, (_Saturate_6856939e57034af985ed339413123d4e_Out_1_Float.xxxx), _Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4);
            UnityTexture2D _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.tex, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.samplerstate, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_R_4_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.r;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_G_5_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.g;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_B_6_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.b;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.a;
            float _Split_4f8f81177c174703b3b7216f7abcab96_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_4f8f81177c174703b3b7216f7abcab96_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_4f8f81177c174703b3b7216f7abcab96_A_4_Float = 0;
            float _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float = _CutHeight;
            float _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float;
            Unity_Subtract_float(_Split_4f8f81177c174703b3b7216f7abcab96_G_2_Float, _Property_50492eff20f644f0b1610b0b3e12d850_Out_0_Float, _Subtract_14c160049cc54391926538d185ebb749_Out_2_Float);
            float _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float = _EdgeSoftness;
            float _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float;
            Unity_Divide_float(_Subtract_14c160049cc54391926538d185ebb749_Out_2_Float, _Property_5e1d9d96f55b42888a293827e25768bd_Out_0_Float, _Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float);
            float _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float;
            Unity_Saturate_float(_Divide_a8550858610e43d3b2e102c9cbf21000_Out_2_Float, _Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float);
            float _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float;
            Unity_OneMinus_float(_Saturate_b10bb4156aec4a69af5e50378c45dafc_Out_1_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float);
            float _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _OneMinus_ed47b14dd9e842dab3061a1337eae602_Out_1_Float, _Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float);
            float _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float);
            float _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            Unity_Subtract_float(_Multiply_dd2f975ef9b646998800dba6c42e39b0_Out_2_Float, _Step_197fa31e6cb4407aa68fdef7419e513a_Out_2_Float, _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float);
            float3 _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3;
            Unity_DDX_333f7faeddc94e5db3b3c399ef301e7d_float3(IN.WorldSpacePosition, _DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3);
            float3 _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3;
            Unity_DDY_79ef38e2398249e4b0d5b3342d7b656d_float3(IN.WorldSpacePosition, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3);
            float3 _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3;
            Unity_CrossProduct_float(_DDX_333f7faeddc94e5db3b3c399ef301e7d_Out_1_Vector3, _DDY_79ef38e2398249e4b0d5b3342d7b656d_Out_1_Vector3, _CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3);
            float3 _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3;
            Unity_Normalize_float3(_CrossProduct_a6147b62719c4acc94e92ca001282135_Out_2_Vector3, _Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3);
            float _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float;
            Unity_DotProduct_float3(_Normalize_0a85beabde9a490ab31c11456d64cde8_Out_1_Vector3, IN.WorldSpaceViewDirection, _DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float);
            float _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float;
            Unity_Absolute_float(_DotProduct_a1466fd8db2d48af937c9d2cf574dbaf_Out_2_Float, _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float);
            float _Property_406505a58d794e15861d235061da0712_Out_0_Float = _BaseGlancingAngleCut;
            float _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float;
            Unity_Multiply_float_float(_Property_406505a58d794e15861d235061da0712_Out_0_Float, 3, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float);
            float _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float;
            Unity_Lerp_float(float(1), _Absolute_e5c6af29a2ca4facb895156b26b9abac_Out_1_Float, _Multiply_1ca2c120842649328c18dd73b9080c4b_Out_2_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float);
            float _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, _Lerp_b3f38069dd4c4c6dbd46420494acfb8a_Out_3_Float, _Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 1, _Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_2491cd00fb0a43c5adc76e5cf78b0194_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            Unity_Multiply_float_float(_Multiply_143555a29f364e3dbfb1a5500d9814e7_Out_2_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float);
            surface.BaseColor = (_Lerp_3fb35c0246c14927ba7ee49537426369_Out_3_Vector4.xyz);
            surface.Alpha = _Subtract_c274f67bfb744e39ab115a8b68ab2ac5_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_acd7d6df2c8c492eabfdb3ae9a27f707_Out_2_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.uv0 =                                        input.uv0;
            output.VertexColor =                                input.color;
            output.TimeParameters =                             _TimeParameters.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.WorldSpaceViewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
            output.VertexColor = input.color;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    CustomEditorForRenderPipeline "UnityEditor.ShaderGraphLitGUI" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
    FallBack "Hidden/Shader Graph/FallbackError"
}