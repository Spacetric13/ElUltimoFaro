using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ElUltimoFaro;
public static class FaroStageFiveTools
{
 public const string ScenePath="Assets/Scenes/SampleScene_Android_05.unity";
 public const string Generated="Assets/ElUltimoFaro/Generated/Weather5";
 [MenuItem("El Último Faro/Etapa 5/Abrir noche y tormenta Android")]
 public static void Open()=>EditorSceneManager.OpenScene(ScenePath);
 public static void PrepareNight()
 {
  if(File.Exists(ScenePath)) throw new Exception("Stage 5 exists; preserve edits");
  FaroStageFourTools.Open();EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),ScenePath);
  Directory.CreateDirectory(Generated);AssetDatabase.Refresh();
  var sky=new Material(Shader.Find("ElUltimoFaro/NightSky"));AssetDatabase.CreateAsset(sky,Generated+"/Cielo_noche.mat");
  RenderSettings.skybox=sky;RenderSettings.ambientMode=AmbientMode.Trilight;
  RenderSettings.ambientSkyColor=new Color(.13f,.17f,.25f);RenderSettings.ambientEquatorColor=new Color(.065f,.085f,.125f);RenderSettings.ambientGroundColor=new Color(.027f,.035f,.048f);
  RenderSettings.reflectionIntensity=.12f;
  RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.021f;RenderSettings.fogColor=new Color(.035f,.05f,.075f);
  foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
   if(light.type==LightType.Directional){light.color=new Color(.53f,.67f,.92f);light.intensity=.30f;light.transform.rotation=Quaternion.Euler(48,-38,0);light.shadows=LightShadows.Soft;RenderSettings.sun=light;}
  var items=UnityEngine.Object.FindAnyObjectByType<MobileInteraction>(); items.Lantern.LanternLight.intensity=.65f;items.Lantern.LanternLight.range=2.1f;
  foreach(string name in new[]{"Luz_guia_muelle","Luz_guia_isla"}) {var l=GameObject.Find(name).GetComponent<Light>();l.intensity=.35f;l.range=2.5f;}
  Camera.main.clearFlags=CameraClearFlags.Skybox;
  GameObject.Find("Android_Controls").transform.Find("SafeArea/Title").GetComponent<Text>().text="EL ÚLTIMO FARO · NOCHE DE TORMENTA";
  Save();
 }
 static void Save(){EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();}
 static void Ref(UnityEngine.Object obj,string field,UnityEngine.Object value){var so=new SerializedObject(obj);so.FindProperty(field).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
 static AudioSource Loop(Transform root,string name,string clip,float volume)
 {
  var go=new GameObject(name);go.transform.SetParent(root,false);var source=go.AddComponent<AudioSource>();source.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Generated+"/Audio/"+clip+".wav");source.loop=true;source.playOnAwake=false;source.spatialBlend=0;source.volume=volume;return source;
 }
 static Material Transparent(string name)
 {
  var material=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));material.SetColor("_BaseColor",Color.white);
  material.SetFloat("_Surface",1);material.SetFloat("_Blend",0);material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);material.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);material.SetFloat("_ZWrite",0);material.SetFloat("_Cull",(float)CullMode.Off);
  material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=3000;material.SetOverrideTag("RenderType","Transparent");AssetDatabase.CreateAsset(material,Generated+"/"+name+".mat");return material;
 }
 public static void AddWeather()
 {
  Open();if(GameObject.Find("Clima_Etapa5"))throw new Exception("Weather already exists; preserve edits");
  RenderSettings.ambientSkyColor=new Color(.30f,.36f,.45f);RenderSettings.ambientEquatorColor=new Color(.20f,.24f,.31f);RenderSettings.ambientGroundColor=new Color(.09f,.11f,.15f);
  var player=UnityEngine.Object.FindAnyObjectByType<MobilePlayerController>();var items=player.GetComponent<MobileInteraction>();var boat=UnityEngine.Object.FindAnyObjectByType<BoatController>();
  var seaObject=GameObject.Find("mar_low_poly");seaObject.isStatic=false;
  var mesh=seaObject.GetComponent<MeshFilter>().sharedMesh;
  if(seaObject.GetComponent<MeshCollider>().sharedMesh!=mesh)throw new Exception("Water collider and visible mesh must agree");
  var surface=ScriptableObject.CreateInstance<LowPolyWaterSurfaceData>();surface.localVertices=mesh.vertices;surface.vertexXZ=mesh.vertices.Select(v=>{var p=seaObject.transform.TransformPoint(v);return new Vector2(p.x,p.z);}).ToArray();surface.triangles=mesh.triangles;
  AssetDatabase.CreateAsset(surface,Generated+"/Mar_vertices_originales.asset");
  var waterRenderer=seaObject.GetComponent<Renderer>();var source=waterRenderer.sharedMaterial;
  var waterMaterial=new Material(Shader.Find("ElUltimoFaro/LowPolySea"));waterMaterial.SetColor("_BaseColor",source.GetColor("_BaseColor"));AssetDatabase.CreateAsset(waterMaterial,Generated+"/Mar_low_poly_animado.mat");waterRenderer.sharedMaterial=waterMaterial;
  var water=seaObject.AddComponent<LowPolyWater>();Ref(water,"surface",surface);Ref(boat,"animatedWater",water);
  var bounds=waterRenderer.localBounds;bounds.Expand(new Vector3(0,.12f/seaObject.transform.lossyScale.y,0));waterRenderer.localBounds=bounds;
  var root=new GameObject("Clima_Etapa5");var director=root.AddComponent<WeatherDirector>();
  var rainObject=new GameObject("Lluvia_local");rainObject.transform.SetParent(root.transform,false);rainObject.transform.position=player.transform.position+Vector3.up*3;
  var ps=rainObject.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
  var main=ps.main;main.loop=true;main.duration=5;main.startLifetime=new ParticleSystem.MinMaxCurve(.7f,.95f);main.startSpeed=0;main.startSize=new ParticleSystem.MinMaxCurve(.008f,.014f);main.startColor=new Color(.65f,.78f,.90f,.48f);main.maxParticles=1600;main.simulationSpace=ParticleSystemSimulationSpace.World;main.playOnAwake=true;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
  var shape=ps.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(9,.15f,9);
  var emission=ps.emission;emission.rateOverTime=1000;
  var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=.6f;velocity.y=-8;velocity.z=.3f;
  var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.08f),new GradientAlphaKey(.8f,.7f),new GradientAlphaKey(0,1)});
  var overLife=ps.colorOverLifetime;overLife.enabled=true;overLife.color=gradient;
  var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=1;renderer.velocityScale=.018f;renderer.cameraVelocityScale=0;renderer.sharedMaterial=Transparent("Gotas_lluvia");renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
  renderer.maxParticleSize=.035f;
  var rain=rainObject.AddComponent<RainField>();Ref(rain,"follow",player.transform);
  foreach(string file in Directory.GetFiles(Generated+"/Audio","*.wav"))
  {
   var importer=(AudioImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));var settings=importer.defaultSampleSettings;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.6f;settings.loadType=AudioClipLoadType.CompressedInMemory;importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.SaveAndReimport();
  }
  var audio=root.AddComponent<WeatherAudio>();Ref(audio,"player",player);Ref(audio,"rain",Loop(root.transform,"Audio_lluvia","Lluvia_fuerte",.25f));Ref(audio,"wind",Loop(root.transform,"Audio_viento","Viento_costero",.06f));Ref(audio,"sea",Loop(root.transform,"Audio_mar","Oleaje",.08f));
  var thunder=Loop(root.transform,"Audio_trueno","Trueno_1",.7f);thunder.loop=false;thunder.spatialBlend=1;thunder.rolloffMode=AudioRolloffMode.Linear;thunder.minDistance=3;thunder.maxDistance=75;thunder.dopplerLevel=0;Ref(audio,"thunder",thunder);
  var audioData=new SerializedObject(audio);var clips=audioData.FindProperty("thunderClips");clips.arraySize=3;for(int i=0;i<3;i++)clips.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(Generated+"/Audio/Trueno_"+(i+1)+".wav");audioData.ApplyModifiedPropertiesWithoutUndo();
  var boltObject=new GameObject("Rayo_ambiental");boltObject.transform.SetParent(root.transform,false);var bolt=boltObject.AddComponent<LineRenderer>();bolt.useWorldSpace=true;bolt.widthMultiplier=.045f;bolt.widthCurve=AnimationCurve.Linear(0,1,1,.2f);bolt.numCornerVertices=0;bolt.numCapVertices=0;bolt.sharedMaterial=Transparent("Rayo");bolt.shadowCastingMode=ShadowCastingMode.Off;bolt.enabled=false;
  Ref(director,"quartz",items.Quartz);Ref(director,"lighthouse",UnityEngine.Object.FindAnyObjectByType<LighthouseBlink>());Ref(director,"feedback",items);Ref(director,"view",Camera.main.transform);Ref(director,"moon",RenderSettings.sun);Ref(director,"rain",rain);Ref(director,"sound",audio);Ref(director,"water",water);Ref(director,"bolt",bolt);
  Save();File.WriteAllText(Path.GetFullPath("../stage5-weather-preparation.txt"),"Original water vertices="+mesh.vertexCount+" triangles="+mesh.triangles.Length/3+"\nRain budget=1600\nNight, rain, storm on quartz, calm after lighthouse configured.");
 }
 public static void RefineRain()
 {
  Open();GameObject.Find("Lluvia_local").GetComponent<ParticleSystemRenderer>().maxParticleSize=.035f;Save();
 }
 public static void BuildAndroid()
 {
  Open();FaroAndroidPreparation.ConfigureAndroid();PlayerSettings.bundleVersion="0.5.1";PlayerSettings.Android.bundleVersionCode=6;
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
  var path=Environment.GetEnvironmentVariable("FARO_APK_PATH");if(string.IsNullOrWhiteSpace(path))throw new Exception("FARO_APK_PATH required");
  // Keep native compilation practical on this machine. Debug symbols are not used by this APK.
  var previousArgs=PlayerSettings.GetAdditionalIl2CppArgs();BuildReport report;
  try
  {
   PlayerSettings.SetAdditionalIl2CppArgs(previousArgs+" --jobs=2 --bee-jobs=2 --compiler-flags=-g0");
   report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=path,target=BuildTarget.Android,options=BuildOptions.None});
  }
  finally {PlayerSettings.SetAdditionalIl2CppArgs(previousArgs);}
  File.WriteAllText(Path.GetFullPath("../stage5-build-result.txt"),report.summary.result+"\nErrors="+report.summary.totalErrors);
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Stage 5 Android build failed");
 }
 [MenuItem("El Último Faro/Etapa 5/Compilar APK Android")]
 public static void BuildFromMenu()
 {
  var path=EditorUtility.SaveFilePanel("Guardar APK",Path.GetFullPath(".."),"ElUltimoFaro_05_1_Mar_Animado","apk");if(string.IsNullOrEmpty(path))return;
  var previous=Environment.GetEnvironmentVariable("FARO_APK_PATH");try{Environment.SetEnvironmentVariable("FARO_APK_PATH",path);BuildAndroid();}finally{Environment.SetEnvironmentVariable("FARO_APK_PATH",previous);}
 }
}
