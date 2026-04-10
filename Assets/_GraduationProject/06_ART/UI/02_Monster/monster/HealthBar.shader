Shader "Custom/HealthBar"
{
   Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        
        _FrontFill ("Front Fill Amount", Range(0, 1)) = 1.0
        _BackFill ("Back Fill Amount (Trail)", Range(0, 1)) = 1.0
        
        _FrontColor ("Front Color (Current HP)", Color) = (1, 1, 1, 1) // 초록색
        _BackColor ("Back Color (Damage Trail)", Color) = (0, 0, 0, 1) // 빨간색
        _BgColor ("Background Color", Color) = (0, 0, 0, 0)    // 어두운 회색
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _FrontFill;
                float _BackFill;
                half4 _FrontColor;
                half4 _BackColor;
                half4 _BgColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                
                // 마스크 생성
                float frontMask = step(IN.uv.x, _FrontFill);
                float backMask  = step(IN.uv.x, _BackFill);
                
                // 1. 기본 배경색
                half4 finalColor = _BgColor;
                // 2. 배경 위에 잔상(Back) 색상 덮어쓰기
                finalColor = lerp(finalColor, _BackColor * texColor, backMask);
                // 3. 잔상 위에 현재 체력(Front) 색상 덮어쓰기
                finalColor = lerp(finalColor, _FrontColor * texColor, frontMask);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}