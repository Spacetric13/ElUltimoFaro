Shader "ElUltimoFaro/LowPolySea"
{
 Properties {
  _BaseColor("Water color",Color)=(.18,.29,.40,1)
 }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
  Pass {
   Name "ForwardLit"
   Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #pragma multi_compile_fog
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;};
   struct Varyings {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half fog:TEXCOORD1;half3 vertexLights:TEXCOORD2;};
   Varyings vert(Attributes v){
    Varyings o;float3 p=TransformObjectToWorld(v.positionOS.xyz);
    // LowPolyWater supplies animated vertices; do not displace them a second time.
    o.positionWS=p;o.positionCS=TransformWorldToHClip(p);o.fog=ComputeFogFactor(o.positionCS.z);o.vertexLights=VertexLighting(p,float3(0,1,0));return o;
   }
   half4 frag(Varyings i):SV_Target {
    // Flat face normals preserve the original faceted look without tessellation or normal maps.
    float3 n=normalize(cross(ddy(i.positionWS),ddx(i.positionWS)));if(n.y<0)n=-n;
    Light moon=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
    half3 light=SampleSH(n)+moon.color*saturate(dot(n,moon.direction))*moon.distanceAttenuation*moon.shadowAttenuation;
    #ifdef _ADDITIONAL_LIGHTS_VERTEX
    light+=i.vertexLights;
    #endif
    #ifdef _ADDITIONAL_LIGHTS
    uint count=GetAdditionalLightsCount();
    for(uint j=0;j<count;j++){Light l=GetAdditionalLight(j,i.positionWS);light+=l.color*saturate(dot(n,l.direction))*l.distanceAttenuation;}
    #endif
    float3 view=GetWorldSpaceNormalizeViewDir(i.positionWS);
    half spec=pow(saturate(dot(n,normalize(moon.direction+view))),28)*.18*moon.shadowAttenuation;
    half3 col=_BaseColor.rgb*light+moon.color*spec;
    return half4(MixFog(col,i.fog),1);
   }
   ENDHLSL
  }
 }
}
