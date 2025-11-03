Shader "Custom/DonutWave"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Alpha ("Alpha", Range(0,1)) = 0.6
        _TimeValue ("Time", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : NORMAL;
                float3 positionWS : TEXCOORD0;
            };

            float4 _BaseColor;
            float _Alpha;
            float _TimeValue;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 pos = IN.positionOS.xyz;

                // 흔들림 제거 (고정 평면)
                // pos 변형 없음

                OUT.positionHCS = TransformObjectToHClip(pos);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = mul(unity_ObjectToWorld, float4(pos,1)).xyz;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float light = saturate(dot(normalize(IN.normalWS), float3(0,1,0)));
                float4 col = _BaseColor;
                col.a *= _Alpha * light;
                return col;
            }
            ENDHLSL
        }
    }
}
