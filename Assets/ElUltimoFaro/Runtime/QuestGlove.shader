Shader "ElUltimoFaro/GuanteVR" {
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
  Pass {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
   struct Varyings { float4 positionCS:SV_POSITION; half4 color:COLOR; UNITY_VERTEX_OUTPUT_STEREO };
   Varyings vert(Attributes v){Varyings o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.positionCS=TransformObjectToHClip(v.positionOS.xyz);float3 n=TransformObjectToWorldNormal(v.normalOS);o.color=half4(v.color.rgb*(.60+.40*saturate(dot(n,normalize(float3(.3,1,.4))))),1);return o;}
   half4 frag(Varyings v):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(v);return v.color;}
   ENDHLSL
  }
 }
}
