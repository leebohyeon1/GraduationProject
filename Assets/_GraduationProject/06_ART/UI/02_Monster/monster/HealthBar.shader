Shader "Custom/HealthBar"
{
   Properties
   {
       [MainTexture] _MainTex ("Fill Texture (Front/Back)", 2D) = "white" {}
       _BgTex ("Background Texture", 2D) = "white" {}

       _FrontFill ("Front Fill Amount", Range(0, 1)) = 1.0
       _BackFill ("Back Fill Amount (Trail)", Range(0, 1)) = 1.0

       _FrontColor ("Front Color (Current HP)", Color) = (1, 1, 1, 1)
       _BackColor ("Back Color (Damage Trail)", Color) = (1, 0, 0, 1)
       _BgColor ("Background Color", Color) = (0.2, 0.2, 0.2, 1)
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
           TEXTURE2D(_BgTex);
           SAMPLER(sampler_BgTex);

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
               half4 fillTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
               half4 bgTex = SAMPLE_TEXTURE2D(_BgTex, sampler_BgTex, IN.uv);

               // 마스크 생성
               float frontMask = step(IN.uv.x, _FrontFill);
               float backMask  = step(IN.uv.x, _BackFill);

               // 1. 기본 배경 (배경 텍스처 * 배경 색상)
               half4 finalColor = bgTex * _BgColor;

               // 2. 잔상(Back) 색상 덮어쓰기
               finalColor = lerp(finalColor, fillTex * _BackColor, backMask);

               // 3. 현재 체력(Front) 색상 덮어쓰기
               finalColor = lerp(finalColor, fillTex * _FrontColor, frontMask);

               // 4. 텍스처 모양대로 나오게 하기 (알파 마스크 적용)
               finalColor.a *= max(bgTex.a, fillTex.a);

               return finalColor;
           }
           ENDHLSL
       }
   }}