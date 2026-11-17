Shader "SOOM/PlayerBuffParticle"
{
 Properties { _Tint("Tint",Color)=(1,1,1,1) _Mode("Shape: Flame, Spark, Ring",Float)=0 }
 SubShader {
 Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
 Pass {
 Blend SrcAlpha OneMinusSrcAlpha
 ZWrite Off
 Cull Off
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float4 _Tint; float _Mode;
 CBUFFER_END
 struct Attributes {float4 positionOS:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 struct Varyings {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 Varyings Vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;o.color=v.color;return o;}
 half4 Frag(Varyings i):SV_Target {
 float2 uv=i.uv;float alpha;
 if(_Mode<.5){
  float bend=sin(uv.y*9+_Time.y*8)*.07*uv.y;
  float width=lerp(.42,.025,uv.y);
  alpha=(1-smoothstep(width-.1,width,abs(uv.x-.5+bend)))*smoothstep(0,.12,uv.y)*(1-smoothstep(.7,1,uv.y));
 }else if(_Mode<1.5){alpha=pow(saturate(1-length((uv-.5)*2)),2);}
 else {float radius=length((uv-.5)*2);alpha=(1-smoothstep(.045,.11,abs(radius-.75)));}
 half4 color=i.color*_Tint;color.a*=alpha;
 clip(color.a-.002);return color;
 }
 ENDHLSL
 }
 }
}
