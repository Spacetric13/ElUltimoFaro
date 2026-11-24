Shader "ElUltimoFaro/NightSky"
{
 Properties {
  _Zenith("Zenith",Color)=(.008,.015,.032,1)
  _Horizon("Horizon",Color)=(.035,.055,.09,1)
  _Cloud("Cloud",Color)=(.015,.026,.047,1)
  _Flash("Lightning",Range(0,1))=0
 }
 SubShader {
  Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}
  Cull Off ZWrite Off
  Pass {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _Zenith, _Horizon, _Cloud;
   float _Flash;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;};
   struct Varyings {float4 positionCS:SV_POSITION;float3 direction:TEXCOORD0;};
   Varyings vert(Attributes v) {Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.direction=v.positionOS.xyz;return o;}
   half4 frag(Varyings i):SV_Target {
    float3 d=normalize(i.direction);float y=saturate(d.y);
    half3 sky=lerp(_Horizon.rgb,_Zenith.rgb,pow(y,.55));
    float bands=sin(d.x*12+d.z*6+_Time.y*.014)*sin(d.z*17-d.y*9-_Time.y*.009);
    float cloud=smoothstep(-.25,.5,bands)*smoothstep(-.06,.2,d.y);
    sky=lerp(sky,_Cloud.rgb,cloud*.65);
    sky+=_Flash*half3(.23,.28,.38)*(1-y*.35);
    return half4(sky,1);
   }
   ENDHLSL
  }
 }
}
