// RadialBlur.shader
Shader "PostEffect/RadialBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Center ("Blur Center", Vector) = (0.5, 0.5, 0, 0)
        _Strength ("Blur Strength", Range(0, 1)) = 0.1
        _Samples ("Sample Count", Range(2, 20)) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        Cull Off ZWrite Off ZTest Always

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
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float2 _Center;
            float _Strength;
            int _Samples;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 현재 픽셀에서 블러 중심까지의 방향 벡터 계산
                float2 direction = _Center - IN.uv;
                
                // 최종 색상을 저장할 변수 (현재 픽셀 색상으로 초기화)
                half4 finalColor = tex2D(_MainTex, IN.uv);
                
                // 샘플링 간격 설정
                float step = _Strength / _Samples;

                // 정해진 횟수(_Samples)만큼 반복해서 색상을 샘플링
                for (int i = 1; i < _Samples; i++)
                {
                    // 방향 벡터를 따라 조금씩 이동한 위치의 UV 좌표 계산
                    float2 offsetUV = IN.uv + direction * step * i;
                    // 해당 위치의 텍스처 색상을 읽어와서 더함
                    finalColor += tex2D(_MainTex, offsetUV);
                }
                
                // 샘플링한 색상들의 평균을 내서 최종 색상 결정
                return finalColor / _Samples;
            }
            ENDHLSL
        }
    }
}