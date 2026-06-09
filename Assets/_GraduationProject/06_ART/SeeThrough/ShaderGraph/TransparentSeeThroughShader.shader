Shader "SeeThrough/TransparentSeeThroughShader"
{
    Properties
    {
        [NoScaleOffset]_BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_MetallicMap("MetallicMap", 2D) = "white" {}
        _Metallic("Metallic", Range(0, 1)) = 0
        _Smoothness("Smoothness", Range(0, 1)) = 0
        [NoScaleOffset]_OcclusionMap("OcclusionMap", 2D) = "white" {}
        [Normal][NoScaleOffset]_Normal_Map("Normal Map", 2D) = "bump" {}
        [NoScaleOffset]_Emission_Map("Emission Map", 2D) = "white" {}
        [HDR]_EmissionColor("EmissionColor", Color) = (0, 0, 0, 1)
        _CutHeight("CutHeight", Float) = 1000
        _EdgeSoftness("EdgeSoftness", Float) = 2.47
        _Section_Color("Section Color", Color) = (0, 0, 0, 1)
        _Dither("Dither", Range(0, 1)) = 0
        [HideInInspector]_WorkflowMode("_WorkflowMode", Float) = 1
        [HideInInspector]_CastShadows("_CastShadows", Float) = 1
        [HideInInspector]_ReceiveShadows("_ReceiveShadows", Float) = 1
        [HideInInspector]_Surface("_Surface", Float) = 1
        [HideInInspector]_Blend("_Blend", Float) = 0
        [HideInInspector]_AlphaClip("_AlphaClip", Float) = 1
        [HideInInspector]_BlendModePreserveSpecular("_BlendModePreserveSpecular", Float) = 1
        [HideInInspector]_SrcBlend("_SrcBlend", Float) = 1
        [HideInInspector]_DstBlend("_DstBlend", Float) = 0
        [HideInInspector]_SrcBlendAlpha("_SrcBlendAlpha", Float) = 1
        [HideInInspector]_DstBlendAlpha("_DstBlendAlpha", Float) = 0
        [HideInInspector][ToggleUI]_ZWrite("_ZWrite", Float) = 0
        [HideInInspector]_ZWriteControl("_ZWriteControl", Float) = 1
        [HideInInspector]_ZTest("_ZTest", Float) = 4
        [HideInInspector]_Cull("_Cull", Float) = 0
        [HideInInspector]_AlphaToMask("_AlphaToMask", Float) = 0
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
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
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
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
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
             float3 TangentSpaceNormal;
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
             float4 fogFactorAndVertexLight : INTERP7;
             float3 positionWS : INTERP8;
             float3 normalWS : INTERP9;
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
        float4 _Section_Color;
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
        // GraphIncludes: <None>
        
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
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
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
            description.Position = IN.ObjectSpacePosition;
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
            float _IsFrontFace_d0464ac9fc6b4eef8f5adbc6ea3de0d9_Out_0_Boolean = max(0, IN.FaceSign.x);
            UnityTexture2D _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.tex, _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.samplerstate, _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_R_4_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.r;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_G_5_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.g;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_B_6_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.b;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_A_7_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.a;
            float4 _Property_3c8a011c055c469fbcf7423655053914_Out_0_Vector4 = _BaseColor;
            float4 _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_SampleTexture2D_7078fd4747b44483adf78f31afb414f0_A_7_Float.xxxx), _Property_3c8a011c055c469fbcf7423655053914_Out_0_Vector4, _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4);
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_A_4_Float = 0;
            float _Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float;
            Unity_Subtract_float(_Split_6200e33bff1e47168ca8e9349d8c7e31_G_2_Float, float(-0.17), _Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float);
            float _Property_745cfc6a2a5e48b9b2210dcfcab2fd10_Out_0_Float = _EdgeSoftness;
            float _Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float;
            Unity_Divide_float(_Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float, _Property_745cfc6a2a5e48b9b2210dcfcab2fd10_Out_0_Float, _Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float);
            float4 _Property_5c51ec591c9d48cc8c1de2159356d7d6_Out_0_Vector4 = _Section_Color;
            float4 _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float.xxxx), _Property_5c51ec591c9d48cc8c1de2159356d7d6_Out_0_Vector4, _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4);
            float4 _Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4;
            Unity_Branch_float4(_IsFrontFace_d0464ac9fc6b4eef8f5adbc6ea3de0d9_Out_0_Boolean, _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4, _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4, _Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4);
            UnityTexture2D _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Normal_Map);
            float4 _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.tex, _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.samplerstate, _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4);
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_R_4_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.r;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_G_5_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.g;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_B_6_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.b;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_A_7_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.a;
            UnityTexture2D _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Emission_Map);
            float4 _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.tex, _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.samplerstate, _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_R_4_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.r;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_G_5_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.g;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_B_6_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.b;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_A_7_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.a;
            float4 _Property_ba712bb9090a4c52a43f38f92fd73591_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_EmissionColor) : _EmissionColor;
            float4 _Multiply_0d1eec5c365043d2b3baa196887d059a_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4, _Property_ba712bb9090a4c52a43f38f92fd73591_Out_0_Vector4, _Multiply_0d1eec5c365043d2b3baa196887d059a_Out_2_Vector4);
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
            float _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, 2, _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float);
            float _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float);
            float _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            Unity_Subtract_float(_Multiply_d6178352605049c2941aebf23849321e_Out_2_Float, _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float, _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float);
            float _Split_915e41711d524817aa5be576b8bbef01_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_915e41711d524817aa5be576b8bbef01_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_915e41711d524817aa5be576b8bbef01_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_915e41711d524817aa5be576b8bbef01_A_4_Float = 0;
            float _Property_ab0c325aed474096b0398879a622744a_Out_0_Float = _CutHeight;
            float _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float;
            Unity_Subtract_float(_Split_915e41711d524817aa5be576b8bbef01_G_2_Float, _Property_ab0c325aed474096b0398879a622744a_Out_0_Float, _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float);
            float _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float = _EdgeSoftness;
            float _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float;
            Unity_Divide_float(_Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float, _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float, _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float);
            float _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float;
            Unity_Saturate_float(_Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float, _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 3, _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float);
            surface.BaseColor = (_Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4.xyz);
            surface.NormalTS = (_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.xyz);
            surface.Emission = (_Multiply_0d1eec5c365043d2b3baa196887d059a_Out_2_Vector4.xyz);
            surface.Metallic = (_Multiply_b725dae6dfc9431d91df366721b2ac8b_Out_2_Vector4).x;
            surface.Specular = IsGammaSpace() ? float3(0.5, 0.5, 0.5) : SRGBToLinear(float3(0.5, 0.5, 0.5));
            surface.Smoothness = _Branch_1da4fbb515fc4aa3af6b097c6556ccec_Out_3_Float;
            surface.Occlusion = _SampleTexture2D_286b24c1b019408d8887fae901e34021_G_5_Float;
            surface.Alpha = _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
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
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
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
             float3 TangentSpaceNormal;
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
             float4 fogFactorAndVertexLight : INTERP7;
             float3 positionWS : INTERP8;
             float3 normalWS : INTERP9;
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
        float4 _Section_Color;
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
        // GraphIncludes: <None>
        
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
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
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
            description.Position = IN.ObjectSpacePosition;
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
            float _IsFrontFace_d0464ac9fc6b4eef8f5adbc6ea3de0d9_Out_0_Boolean = max(0, IN.FaceSign.x);
            UnityTexture2D _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.tex, _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.samplerstate, _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_R_4_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.r;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_G_5_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.g;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_B_6_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.b;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_A_7_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.a;
            float4 _Property_3c8a011c055c469fbcf7423655053914_Out_0_Vector4 = _BaseColor;
            float4 _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_SampleTexture2D_7078fd4747b44483adf78f31afb414f0_A_7_Float.xxxx), _Property_3c8a011c055c469fbcf7423655053914_Out_0_Vector4, _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4);
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_A_4_Float = 0;
            float _Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float;
            Unity_Subtract_float(_Split_6200e33bff1e47168ca8e9349d8c7e31_G_2_Float, float(-0.17), _Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float);
            float _Property_745cfc6a2a5e48b9b2210dcfcab2fd10_Out_0_Float = _EdgeSoftness;
            float _Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float;
            Unity_Divide_float(_Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float, _Property_745cfc6a2a5e48b9b2210dcfcab2fd10_Out_0_Float, _Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float);
            float4 _Property_5c51ec591c9d48cc8c1de2159356d7d6_Out_0_Vector4 = _Section_Color;
            float4 _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float.xxxx), _Property_5c51ec591c9d48cc8c1de2159356d7d6_Out_0_Vector4, _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4);
            float4 _Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4;
            Unity_Branch_float4(_IsFrontFace_d0464ac9fc6b4eef8f5adbc6ea3de0d9_Out_0_Boolean, _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4, _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4, _Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4);
            UnityTexture2D _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Normal_Map);
            float4 _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.tex, _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.samplerstate, _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4);
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_R_4_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.r;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_G_5_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.g;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_B_6_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.b;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_A_7_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.a;
            UnityTexture2D _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Emission_Map);
            float4 _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.tex, _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.samplerstate, _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_R_4_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.r;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_G_5_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.g;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_B_6_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.b;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_A_7_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.a;
            float4 _Property_ba712bb9090a4c52a43f38f92fd73591_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_EmissionColor) : _EmissionColor;
            float4 _Multiply_0d1eec5c365043d2b3baa196887d059a_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4, _Property_ba712bb9090a4c52a43f38f92fd73591_Out_0_Vector4, _Multiply_0d1eec5c365043d2b3baa196887d059a_Out_2_Vector4);
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
            float _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, 2, _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float);
            float _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float);
            float _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            Unity_Subtract_float(_Multiply_d6178352605049c2941aebf23849321e_Out_2_Float, _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float, _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float);
            float _Split_915e41711d524817aa5be576b8bbef01_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_915e41711d524817aa5be576b8bbef01_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_915e41711d524817aa5be576b8bbef01_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_915e41711d524817aa5be576b8bbef01_A_4_Float = 0;
            float _Property_ab0c325aed474096b0398879a622744a_Out_0_Float = _CutHeight;
            float _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float;
            Unity_Subtract_float(_Split_915e41711d524817aa5be576b8bbef01_G_2_Float, _Property_ab0c325aed474096b0398879a622744a_Out_0_Float, _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float);
            float _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float = _EdgeSoftness;
            float _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float;
            Unity_Divide_float(_Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float, _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float, _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float);
            float _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float;
            Unity_Saturate_float(_Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float, _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 3, _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float);
            surface.BaseColor = (_Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4.xyz);
            surface.NormalTS = (_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.xyz);
            surface.Emission = (_Multiply_0d1eec5c365043d2b3baa196887d059a_Out_2_Vector4.xyz);
            surface.Metallic = (_Multiply_b725dae6dfc9431d91df366721b2ac8b_Out_2_Vector4).x;
            surface.Specular = IsGammaSpace() ? float3(0.5, 0.5, 0.5) : SRGBToLinear(float3(0.5, 0.5, 0.5));
            surface.Smoothness = _Branch_1da4fbb515fc4aa3af6b097c6556ccec_Out_3_Float;
            surface.Occlusion = _SampleTexture2D_286b24c1b019408d8887fae901e34021_G_5_Float;
            surface.Alpha = _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
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
        float4 _Section_Color;
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
        // GraphIncludes: <None>
        
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
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
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
            description.Position = IN.ObjectSpacePosition;
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
            float _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, 2, _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float);
            float _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float);
            float _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            Unity_Subtract_float(_Multiply_d6178352605049c2941aebf23849321e_Out_2_Float, _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float, _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float);
            float _Split_915e41711d524817aa5be576b8bbef01_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_915e41711d524817aa5be576b8bbef01_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_915e41711d524817aa5be576b8bbef01_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_915e41711d524817aa5be576b8bbef01_A_4_Float = 0;
            float _Property_ab0c325aed474096b0398879a622744a_Out_0_Float = _CutHeight;
            float _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float;
            Unity_Subtract_float(_Split_915e41711d524817aa5be576b8bbef01_G_2_Float, _Property_ab0c325aed474096b0398879a622744a_Out_0_Float, _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float);
            float _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float = _EdgeSoftness;
            float _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float;
            Unity_Divide_float(_Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float, _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float, _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float);
            float _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float;
            Unity_Saturate_float(_Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float, _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 3, _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float);
            surface.Alpha = _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
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
             float3 WorldSpacePosition;
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpacePosition;
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
        float4 _Section_Color;
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
        // GraphIncludes: <None>
        
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
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
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
            description.Position = IN.ObjectSpacePosition;
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
            float _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, 2, _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float);
            float _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float);
            float _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            Unity_Subtract_float(_Multiply_d6178352605049c2941aebf23849321e_Out_2_Float, _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float, _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float);
            float _Split_915e41711d524817aa5be576b8bbef01_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_915e41711d524817aa5be576b8bbef01_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_915e41711d524817aa5be576b8bbef01_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_915e41711d524817aa5be576b8bbef01_A_4_Float = 0;
            float _Property_ab0c325aed474096b0398879a622744a_Out_0_Float = _CutHeight;
            float _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float;
            Unity_Subtract_float(_Split_915e41711d524817aa5be576b8bbef01_G_2_Float, _Property_ab0c325aed474096b0398879a622744a_Out_0_Float, _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float);
            float _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float = _EdgeSoftness;
            float _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float;
            Unity_Divide_float(_Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float, _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float, _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float);
            float _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float;
            Unity_Saturate_float(_Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float, _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 3, _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float);
            surface.Alpha = _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
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
        float4 _Section_Color;
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
        // GraphIncludes: <None>
        
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
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
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
            description.Position = IN.ObjectSpacePosition;
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
            float _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, 2, _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float);
            float _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float);
            float _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            Unity_Subtract_float(_Multiply_d6178352605049c2941aebf23849321e_Out_2_Float, _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float, _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float);
            float _Split_915e41711d524817aa5be576b8bbef01_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_915e41711d524817aa5be576b8bbef01_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_915e41711d524817aa5be576b8bbef01_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_915e41711d524817aa5be576b8bbef01_A_4_Float = 0;
            float _Property_ab0c325aed474096b0398879a622744a_Out_0_Float = _CutHeight;
            float _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float;
            Unity_Subtract_float(_Split_915e41711d524817aa5be576b8bbef01_G_2_Float, _Property_ab0c325aed474096b0398879a622744a_Out_0_Float, _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float);
            float _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float = _EdgeSoftness;
            float _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float;
            Unity_Divide_float(_Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float, _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float, _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float);
            float _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float;
            Unity_Saturate_float(_Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float, _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 3, _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float);
            surface.Alpha = _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
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
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
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
        float4 _Section_Color;
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
        // GraphIncludes: <None>
        
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
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
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
            description.Position = IN.ObjectSpacePosition;
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
            UnityTexture2D _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Normal_Map);
            float4 _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.tex, _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.samplerstate, _Property_04f86e22c000404e9c8f36e9d6be981f_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.rgb = UnpackNormal(_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4);
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_R_4_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.r;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_G_5_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.g;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_B_6_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.b;
            float _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_A_7_Float = _SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.a;
            UnityTexture2D _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.tex, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.samplerstate, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_R_4_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.r;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_G_5_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.g;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_B_6_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.b;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.a;
            float _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, 2, _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float);
            float _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float);
            float _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            Unity_Subtract_float(_Multiply_d6178352605049c2941aebf23849321e_Out_2_Float, _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float, _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float);
            float _Split_915e41711d524817aa5be576b8bbef01_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_915e41711d524817aa5be576b8bbef01_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_915e41711d524817aa5be576b8bbef01_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_915e41711d524817aa5be576b8bbef01_A_4_Float = 0;
            float _Property_ab0c325aed474096b0398879a622744a_Out_0_Float = _CutHeight;
            float _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float;
            Unity_Subtract_float(_Split_915e41711d524817aa5be576b8bbef01_G_2_Float, _Property_ab0c325aed474096b0398879a622744a_Out_0_Float, _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float);
            float _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float = _EdgeSoftness;
            float _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float;
            Unity_Divide_float(_Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float, _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float, _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float);
            float _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float;
            Unity_Saturate_float(_Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float, _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 3, _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float);
            surface.NormalTS = (_SampleTexture2D_83e6bb9563d54c3691d83ce9f06fc5c3_RGBA_0_Vector4.xyz);
            surface.Alpha = _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
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
        #define ATTRIBUTES_NEED_INSTANCEID
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define VARYINGS_NEED_CULLFACE
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
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
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
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 texCoord1 : INTERP1;
             float4 texCoord2 : INTERP2;
             float3 positionWS : INTERP3;
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
            output.texCoord1 = input.texCoord1.xyzw;
            output.texCoord2 = input.texCoord2.xyzw;
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
        float4 _Section_Color;
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
        // GraphIncludes: <None>
        
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
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
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
            description.Position = IN.ObjectSpacePosition;
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
            float _IsFrontFace_d0464ac9fc6b4eef8f5adbc6ea3de0d9_Out_0_Boolean = max(0, IN.FaceSign.x);
            UnityTexture2D _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.tex, _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.samplerstate, _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_R_4_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.r;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_G_5_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.g;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_B_6_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.b;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_A_7_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.a;
            float4 _Property_3c8a011c055c469fbcf7423655053914_Out_0_Vector4 = _BaseColor;
            float4 _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_SampleTexture2D_7078fd4747b44483adf78f31afb414f0_A_7_Float.xxxx), _Property_3c8a011c055c469fbcf7423655053914_Out_0_Vector4, _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4);
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_A_4_Float = 0;
            float _Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float;
            Unity_Subtract_float(_Split_6200e33bff1e47168ca8e9349d8c7e31_G_2_Float, float(-0.17), _Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float);
            float _Property_745cfc6a2a5e48b9b2210dcfcab2fd10_Out_0_Float = _EdgeSoftness;
            float _Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float;
            Unity_Divide_float(_Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float, _Property_745cfc6a2a5e48b9b2210dcfcab2fd10_Out_0_Float, _Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float);
            float4 _Property_5c51ec591c9d48cc8c1de2159356d7d6_Out_0_Vector4 = _Section_Color;
            float4 _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float.xxxx), _Property_5c51ec591c9d48cc8c1de2159356d7d6_Out_0_Vector4, _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4);
            float4 _Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4;
            Unity_Branch_float4(_IsFrontFace_d0464ac9fc6b4eef8f5adbc6ea3de0d9_Out_0_Boolean, _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4, _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4, _Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4);
            UnityTexture2D _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Emission_Map);
            float4 _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.tex, _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.samplerstate, _Property_b032915d7aa644ed84a34a8cad1f3ffe_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_R_4_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.r;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_G_5_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.g;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_B_6_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.b;
            float _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_A_7_Float = _SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4.a;
            float4 _Property_ba712bb9090a4c52a43f38f92fd73591_Out_0_Vector4 = IsGammaSpace() ? LinearToSRGB(_EmissionColor) : _EmissionColor;
            float4 _Multiply_0d1eec5c365043d2b3baa196887d059a_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleTexture2D_b64f90083fbc45bdb2d81b9ca08978b6_RGBA_0_Vector4, _Property_ba712bb9090a4c52a43f38f92fd73591_Out_0_Vector4, _Multiply_0d1eec5c365043d2b3baa196887d059a_Out_2_Vector4);
            UnityTexture2D _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.tex, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.samplerstate, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_R_4_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.r;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_G_5_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.g;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_B_6_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.b;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.a;
            float _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, 2, _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float);
            float _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float);
            float _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            Unity_Subtract_float(_Multiply_d6178352605049c2941aebf23849321e_Out_2_Float, _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float, _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float);
            float _Split_915e41711d524817aa5be576b8bbef01_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_915e41711d524817aa5be576b8bbef01_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_915e41711d524817aa5be576b8bbef01_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_915e41711d524817aa5be576b8bbef01_A_4_Float = 0;
            float _Property_ab0c325aed474096b0398879a622744a_Out_0_Float = _CutHeight;
            float _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float;
            Unity_Subtract_float(_Split_915e41711d524817aa5be576b8bbef01_G_2_Float, _Property_ab0c325aed474096b0398879a622744a_Out_0_Float, _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float);
            float _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float = _EdgeSoftness;
            float _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float;
            Unity_Divide_float(_Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float, _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float, _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float);
            float _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float;
            Unity_Saturate_float(_Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float, _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 3, _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float);
            surface.BaseColor = (_Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4.xyz);
            surface.Emission = (_Multiply_0d1eec5c365043d2b3baa196887d059a_Out_2_Vector4.xyz);
            surface.Alpha = _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
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
        float4 _Section_Color;
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
        // GraphIncludes: <None>
        
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
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
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
            description.Position = IN.ObjectSpacePosition;
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
            float _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, 2, _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float);
            float _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float);
            float _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            Unity_Subtract_float(_Multiply_d6178352605049c2941aebf23849321e_Out_2_Float, _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float, _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float);
            float _Split_915e41711d524817aa5be576b8bbef01_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_915e41711d524817aa5be576b8bbef01_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_915e41711d524817aa5be576b8bbef01_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_915e41711d524817aa5be576b8bbef01_A_4_Float = 0;
            float _Property_ab0c325aed474096b0398879a622744a_Out_0_Float = _CutHeight;
            float _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float;
            Unity_Subtract_float(_Split_915e41711d524817aa5be576b8bbef01_G_2_Float, _Property_ab0c325aed474096b0398879a622744a_Out_0_Float, _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float);
            float _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float = _EdgeSoftness;
            float _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float;
            Unity_Divide_float(_Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float, _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float, _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float);
            float _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float;
            Unity_Saturate_float(_Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float, _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 3, _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float);
            surface.Alpha = _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
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
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_CULLFACE
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
        float4 _Section_Color;
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
        // GraphIncludes: <None>
        
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
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
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
            description.Position = IN.ObjectSpacePosition;
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
            float _IsFrontFace_d0464ac9fc6b4eef8f5adbc6ea3de0d9_Out_0_Boolean = max(0, IN.FaceSign.x);
            UnityTexture2D _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.tex, _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.samplerstate, _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_R_4_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.r;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_G_5_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.g;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_B_6_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.b;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_A_7_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.a;
            float4 _Property_3c8a011c055c469fbcf7423655053914_Out_0_Vector4 = _BaseColor;
            float4 _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_SampleTexture2D_7078fd4747b44483adf78f31afb414f0_A_7_Float.xxxx), _Property_3c8a011c055c469fbcf7423655053914_Out_0_Vector4, _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4);
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_A_4_Float = 0;
            float _Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float;
            Unity_Subtract_float(_Split_6200e33bff1e47168ca8e9349d8c7e31_G_2_Float, float(-0.17), _Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float);
            float _Property_745cfc6a2a5e48b9b2210dcfcab2fd10_Out_0_Float = _EdgeSoftness;
            float _Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float;
            Unity_Divide_float(_Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float, _Property_745cfc6a2a5e48b9b2210dcfcab2fd10_Out_0_Float, _Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float);
            float4 _Property_5c51ec591c9d48cc8c1de2159356d7d6_Out_0_Vector4 = _Section_Color;
            float4 _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float.xxxx), _Property_5c51ec591c9d48cc8c1de2159356d7d6_Out_0_Vector4, _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4);
            float4 _Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4;
            Unity_Branch_float4(_IsFrontFace_d0464ac9fc6b4eef8f5adbc6ea3de0d9_Out_0_Boolean, _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4, _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4, _Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4);
            UnityTexture2D _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.tex, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.samplerstate, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_R_4_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.r;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_G_5_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.g;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_B_6_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.b;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.a;
            float _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, 2, _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float);
            float _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float);
            float _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            Unity_Subtract_float(_Multiply_d6178352605049c2941aebf23849321e_Out_2_Float, _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float, _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float);
            float _Split_915e41711d524817aa5be576b8bbef01_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_915e41711d524817aa5be576b8bbef01_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_915e41711d524817aa5be576b8bbef01_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_915e41711d524817aa5be576b8bbef01_A_4_Float = 0;
            float _Property_ab0c325aed474096b0398879a622744a_Out_0_Float = _CutHeight;
            float _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float;
            Unity_Subtract_float(_Split_915e41711d524817aa5be576b8bbef01_G_2_Float, _Property_ab0c325aed474096b0398879a622744a_Out_0_Float, _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float);
            float _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float = _EdgeSoftness;
            float _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float;
            Unity_Divide_float(_Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float, _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float, _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float);
            float _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float;
            Unity_Saturate_float(_Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float, _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 3, _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float);
            surface.BaseColor = (_Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4.xyz);
            surface.Alpha = _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
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
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_CULLFACE
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
        float4 _Section_Color;
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
        // GraphIncludes: <None>
        
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
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Step_float(float Edge, float In, out float Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
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
            description.Position = IN.ObjectSpacePosition;
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
            float _IsFrontFace_d0464ac9fc6b4eef8f5adbc6ea3de0d9_Out_0_Boolean = max(0, IN.FaceSign.x);
            UnityTexture2D _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.tex, _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.samplerstate, _Property_88fcf5092b134d8bb5edccf5936bf718_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_R_4_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.r;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_G_5_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.g;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_B_6_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.b;
            float _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_A_7_Float = _SampleTexture2D_7078fd4747b44483adf78f31afb414f0_RGBA_0_Vector4.a;
            float4 _Property_3c8a011c055c469fbcf7423655053914_Out_0_Vector4 = _BaseColor;
            float4 _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4;
            Unity_Multiply_float4_float4((_SampleTexture2D_7078fd4747b44483adf78f31afb414f0_A_7_Float.xxxx), _Property_3c8a011c055c469fbcf7423655053914_Out_0_Vector4, _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4);
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_6200e33bff1e47168ca8e9349d8c7e31_A_4_Float = 0;
            float _Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float;
            Unity_Subtract_float(_Split_6200e33bff1e47168ca8e9349d8c7e31_G_2_Float, float(-0.17), _Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float);
            float _Property_745cfc6a2a5e48b9b2210dcfcab2fd10_Out_0_Float = _EdgeSoftness;
            float _Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float;
            Unity_Divide_float(_Subtract_cf1dbae0ea5c44d28cd02e610cf2900f_Out_2_Float, _Property_745cfc6a2a5e48b9b2210dcfcab2fd10_Out_0_Float, _Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float);
            float4 _Property_5c51ec591c9d48cc8c1de2159356d7d6_Out_0_Vector4 = _Section_Color;
            float4 _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4;
            Unity_Multiply_float4_float4((_Divide_7a5e03df9ceb449287bd282544eb0949_Out_2_Float.xxxx), _Property_5c51ec591c9d48cc8c1de2159356d7d6_Out_0_Vector4, _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4);
            float4 _Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4;
            Unity_Branch_float4(_IsFrontFace_d0464ac9fc6b4eef8f5adbc6ea3de0d9_Out_0_Boolean, _Multiply_83a36ecc91c8492d9224cc4f5ef9d75d_Out_2_Vector4, _Multiply_6f0fc655c2614fdaa1e348ed2a2ba773_Out_2_Vector4, _Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4);
            UnityTexture2D _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_BaseMap);
            float4 _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.tex, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.samplerstate, _Property_009b1290fec84924a0333efa8bee677a_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_R_4_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.r;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_G_5_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.g;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_B_6_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.b;
            float _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float = _SampleTexture2D_527cdfc8979b4043961dfaa503f65423_RGBA_0_Vector4.a;
            float _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float;
            Unity_Multiply_float_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, 2, _Multiply_d6178352605049c2941aebf23849321e_Out_2_Float);
            float _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float;
            Unity_Step_float(_SampleTexture2D_527cdfc8979b4043961dfaa503f65423_A_7_Float, float(0.5), _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float);
            float _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            Unity_Subtract_float(_Multiply_d6178352605049c2941aebf23849321e_Out_2_Float, _Step_b2acd661a00d4ba0906243b5b10ae7af_Out_2_Float, _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float);
            float _Split_915e41711d524817aa5be576b8bbef01_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_915e41711d524817aa5be576b8bbef01_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_915e41711d524817aa5be576b8bbef01_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_915e41711d524817aa5be576b8bbef01_A_4_Float = 0;
            float _Property_ab0c325aed474096b0398879a622744a_Out_0_Float = _CutHeight;
            float _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float;
            Unity_Subtract_float(_Split_915e41711d524817aa5be576b8bbef01_G_2_Float, _Property_ab0c325aed474096b0398879a622744a_Out_0_Float, _Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float);
            float _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float = _EdgeSoftness;
            float _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float;
            Unity_Divide_float(_Subtract_d367e935dffc45689e08fa24e229c591_Out_2_Float, _Property_c56104964e294ca0ac2550141fe6219e_Out_0_Float, _Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float);
            float _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float;
            Unity_Saturate_float(_Divide_0c0d84172c774b9b8a2b821e25ebc3df_Out_2_Float, _Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float);
            float _Property_a0b0c9860b594939a505835320f41848_Out_0_Float = _Dither;
            float _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float;
            Unity_Multiply_float_float(_Property_a0b0c9860b594939a505835320f41848_Out_0_Float, 3, _Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float);
            float _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float;
            Unity_Dither_float(_Multiply_4314d65b170a45eab8954749ed5a968c_Out_2_Float, float4(IN.NDCPosition.xy, 0, 0), _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float);
            float _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
            Unity_Multiply_float_float(_Saturate_d51b70dbcfe3498aa847229b233127e1_Out_1_Float, _Dither_0d505fe77f084492ba1731ecc1510994_Out_2_Float, _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float);
            surface.BaseColor = (_Branch_2517752a68b04a11ae084d45bcb48f03_Out_3_Vector4.xyz);
            surface.Alpha = _Subtract_2f3140eb3124497d99c25233baa85168_Out_2_Float;
            surface.AlphaClipThreshold = _Multiply_5e7b36b7a4154620ba339d081c49fd5e_Out_2_Float;
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