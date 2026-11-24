using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ElUltimoFaro;

public static class FaroStageSixTools
{
 public const string ScenePath="Assets/Scenes/SampleScene_Android_06.unity",Generated="Assets/ElUltimoFaro/Generated/Environment6";
 [MenuItem("El Último Faro/Etapa 6/Abrir entorno vivo Android")]
 public static void Open()=>EditorSceneManager.OpenScene(ScenePath);
 static void Ref(UnityEngine.Object o,string field,UnityEngine.Object value){var s=new SerializedObject(o);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
 static void Num(UnityEngine.Object o,string field,float value){var s=new SerializedObject(o);s.FindProperty(field).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
 static void Vec(UnityEngine.Object o,string field,Vector3 value){var s=new SerializedObject(o);s.FindProperty(field).vector3Value=value;s.ApplyModifiedPropertiesWithoutUndo();}
 static Transform Find(Transform r,string name)=>r.GetComponentsInChildren<Transform>(true).First(x=>x.name==name);
 static void Unpack(GameObject go){if(PrefabUtility.IsPartOfPrefabInstance(go))PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(go),PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);}
 static void Save(){EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();}
 static void RaiseEntranceWindow(Transform faro){foreach(string name in new[]{"Window_Lower_Front","Window_Lower_Front_Frame"})Find(faro,name).localPosition=new Vector3(0,.78f,0);}
 static void SmoothEntrance(Transform faro){
  foreach(string name in new[]{"Step_1","Step_2","Step_3"})foreach(var collider in Find(faro,name).GetComponents<Collider>())collider.enabled=false;
  string path=Generated+"/Paso_entrada.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);bool create=!mesh;if(create)mesh=new Mesh{name="Paso_continuo_entrada"};
  mesh.vertices=new[]{new Vector3(-.525f,.25f,-3.8f),new Vector3(.525f,.25f,-3.8f),new Vector3(-.525f,.73f,-2.25f),new Vector3(.525f,.73f,-2.25f),new Vector3(-.525f,.62f,-1.5f),new Vector3(.525f,.62f,-1.5f)};
  mesh.triangles=new[]{0,2,1,1,2,3,2,4,3,3,4,5};mesh.RecalculateBounds();mesh.RecalculateNormals();if(create)AssetDatabase.CreateAsset(mesh,path);else EditorUtility.SetDirty(mesh);
  var go=GameObject.Find("Paso_continuo_entrada");if(!go){go=new GameObject("Paso_continuo_entrada");go.transform.SetParent(faro,false);go.AddComponent<MeshCollider>();}go.layer=8;go.GetComponent<MeshCollider>().sharedMesh=mesh;
  var traversal=go.GetComponent<EntranceTraversal>();if(!traversal)traversal=go.AddComponent<EntranceTraversal>();Ref(traversal,"body",UnityEngine.Object.FindAnyObjectByType<MobilePlayerController>().GetComponent<CharacterController>());Ref(traversal,"lighthouse",faro);
 }
 public static void ProbeEntrance(){
  Open();var faro=GameObject.Find("El_Ultimo_Faro_LowPoly").transform;Find(faro,"Bisagra_puerta").localRotation=Quaternion.Euler(0,100,0);Physics.SyncTransforms();
  var feet=new Vector3(.55f,.76f,-.86f);var direction=(faro.TransformPoint(new Vector3(0,.64f,-.3f))-feet).normalized;
  for(int j=0;j<4;j++){var p=feet+direction*(j*.03f);foreach(var c in Physics.OverlapCapsule(p+Vector3.up*.067f,p+Vector3.up*.50f,.068f,~(1<<9),QueryTriggerInteraction.Ignore))Debug.Log("ENTRANCE_OVERLAP "+j+" "+c.name+" bounds="+c.bounds);}
  foreach(var hit in Physics.RaycastAll(feet+Vector3.up*.4f,direction,.8f,~(1<<9),QueryTriggerInteraction.Ignore))Debug.Log("ENTRANCE_RAY "+hit.transform.name+" distance="+hit.distance);
 }
 public static void RefineEntrance(){
  Open();var faro=GameObject.Find("El_Ultimo_Faro_LowPoly").transform;RaiseEntranceWindow(faro);
  var door=Find(faro,"Door");var mesh=door.GetComponent<MeshFilter>().sharedMesh;var b=mesh.bounds;var v=mesh.vertices;
  for(int i=0;i<v.Length;i++)v[i].y=Mathf.Lerp(.78f,3.27f,Mathf.InverseLerp(b.min.y,b.max.y,v[i].y));
  mesh.vertices=v;mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);door.GetComponent<BoxCollider>().center=mesh.bounds.center;door.GetComponent<BoxCollider>().size=mesh.bounds.size;
  var hinge=Find(faro,"Bisagra_puerta");door.SetParent(faro,true);hinge.localPosition=new Vector3(-.5f,.64f,-2.35f);door.SetParent(hinge,true);SmoothEntrance(faro);Save();
 }
 static AudioSource Audio(GameObject go,float min,float max){var a=go.AddComponent<AudioSource>();a.playOnAwake=false;a.spatialBlend=1;a.rolloffMode=AudioRolloffMode.Linear;a.minDistance=min;a.maxDistance=max;a.dopplerLevel=0;return a;}
 static AudioClip Clip(string name)=>AssetDatabase.LoadAssetAtPath<AudioClip>(Generated+"/Audio/"+name+".wav");
 static GameObject Box(Transform parent,string name,Vector3 center,Vector3 size,Material material,bool ground=false){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=center;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;if(ground)go.layer=8;return go;}
 struct Vertex {public Vector3 p,n;public Vector2 uv;public static Vertex Lerp(Vertex a,Vertex b,float t)=>new Vertex{p=Vector3.Lerp(a.p,b.p,t),n=Vector3.Lerp(a.n,b.n,t).normalized,uv=Vector2.Lerp(a.uv,b.uv,t)};}
 static List<Vertex> ClipPolygon(List<Vertex> input,Func<Vector3,float> distance,bool inside){
  var output=new List<Vertex>();if(input.Count==0)return output;Vertex previous=input[input.Count-1];float pd=distance(previous.p)*(inside?1:-1);
  foreach(var current in input){float cd=distance(current.p)*(inside?1:-1);if((pd>=0)!=(cd>=0))output.Add(Vertex.Lerp(previous,current,pd/(pd-cd)));if(cd>=0)output.Add(current);previous=current;pd=cd;}return output;
 }
 static Mesh EntranceMesh(Mesh source){
  var positions=source.vertices;var normals=source.normals;var uv=source.uv;var indices=source.triangles;
  var result=new List<Vertex>();var tris=new List<int>();
  Action<List<Vertex>> emit=poly=>{if(poly.Count<3)return;for(int k=1;k<poly.Count-1;k++){int at=result.Count;result.Add(poly[0]);result.Add(poly[k]);result.Add(poly[k+1]);tris.Add(at);tris.Add(at+1);tris.Add(at+2);}};
  Func<Vector3,float>[] planes={p=>p.x+.54f,p=>.54f-p.x,p=>p.y-.54f,p=>3.32f-p.y,p=>-1.5f-p.z};
  for(int i=0;i<indices.Length;i+=3){var poly=new List<Vertex>();for(int k=0;k<3;k++){int v=indices[i+k];poly.Add(new Vertex{p=positions[v],n=normals.Length==positions.Length?normals[v]:Vector3.up,uv=uv.Length==positions.Length?uv[v]:Vector2.zero});}
   foreach(var plane in planes){emit(ClipPolygon(poly,plane,false));poly=ClipPolygon(poly,plane,true);if(poly.Count==0)break;}
  }
  var mesh=new Mesh{name="Tower_Red_Lower_con_acceso"};mesh.vertices=result.Select(v=>v.p).ToArray();mesh.normals=result.Select(v=>v.n).ToArray();mesh.uv=result.Select(v=>v.uv).ToArray();mesh.triangles=tris.ToArray();mesh.RecalculateBounds();return mesh;
 }
 public static void Prepare(){
  if(File.Exists(ScenePath))throw new Exception("Stage 6 already exists; preserve edits");
  FaroStageFiveTools.Open();EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),ScenePath);Directory.CreateDirectory(Generated);AssetDatabase.Refresh();
  foreach(string file in Directory.GetFiles(Generated+"/Audio","*.wav")){var importer=(AudioImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));var settings=importer.defaultSampleSettings;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.65f;settings.loadType=AudioClipLoadType.CompressedInMemory;importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.SaveAndReimport();}
  var player=UnityEngine.Object.FindAnyObjectByType<MobilePlayerController>();var items=player.GetComponent<MobileInteraction>();var weather=UnityEngine.Object.FindAnyObjectByType<WeatherDirector>();
  var faro=GameObject.Find("El_Ultimo_Faro_LowPoly").transform;
  RaiseEntranceWindow(faro);
  var tower=Find(faro,"Tower_Red_Lower");var opening=EntranceMesh(tower.GetComponent<MeshFilter>().sharedMesh);AssetDatabase.CreateAsset(opening,Generated+"/Torre_con_acceso.asset");tower.GetComponent<MeshFilter>().sharedMesh=opening;tower.GetComponent<MeshCollider>().sharedMesh=opening;
  var frame=Find(faro,"Door_Frame");Material stone=frame.GetComponent<Renderer>().sharedMaterial;
  // Keep the original name, replacing the solid decorative panel with an actual three-sided frame.
  var combine=new List<CombineInstance>();
  foreach(var spec in new[]{new[]{new Vector3(-.62f,1.97f,-2.22f),new Vector3(.16f,2.86f,.23f)},new[]{new Vector3(.62f,1.97f,-2.22f),new Vector3(.16f,2.86f,.23f)},new[]{new Vector3(0,3.40f,-2.22f),new Vector3(1.4f,.16f,.23f)}}){var box=Box(faro,"Temporal_marco",spec[0],spec[1],stone);combine.Add(new CombineInstance{mesh=box.GetComponent<MeshFilter>().sharedMesh,transform=Matrix4x4.TRS(spec[0],Quaternion.identity,spec[1])});UnityEngine.Object.DestroyImmediate(box);}
  var frameMesh=new Mesh{name="Door_Frame_abierto"};frameMesh.CombineMeshes(combine.ToArray());AssetDatabase.CreateAsset(frameMesh,Generated+"/Marco_abierto.asset");frame.GetComponent<MeshFilter>().sharedMesh=frameMesh;frame.GetComponent<MeshCollider>().sharedMesh=frameMesh;
  var leaf=Find(faro,"Door");var original=leaf.GetComponent<MeshFilter>().sharedMesh;var doorMesh=UnityEngine.Object.Instantiate(original);doorMesh.name="Door_ajustada_al_acceso";var vertices=doorMesh.vertices;var bounds=original.bounds;
  for(int i=0;i<vertices.Length;i++){vertices[i].x=Mathf.Lerp(-.5f,.5f,Mathf.InverseLerp(bounds.min.x,bounds.max.x,vertices[i].x));vertices[i].y=Mathf.Lerp(.78f,3.27f,Mathf.InverseLerp(bounds.min.y,bounds.max.y,vertices[i].y));}
  doorMesh.vertices=vertices;doorMesh.RecalculateBounds();AssetDatabase.CreateAsset(doorMesh,Generated+"/Puerta_acceso.asset");leaf.GetComponent<MeshFilter>().sharedMesh=doorMesh;
  foreach(var c in leaf.GetComponents<Collider>())UnityEngine.Object.DestroyImmediate(c);var leafCollider=leaf.gameObject.AddComponent<BoxCollider>();leafCollider.center=doorMesh.bounds.center;leafCollider.size=doorMesh.bounds.size;
  var hinge=new GameObject("Bisagra_puerta").transform;hinge.SetParent(faro,false);hinge.localPosition=new Vector3(-.5f,.64f,-2.35f);leaf.SetParent(hinge,true);leaf.gameObject.isStatic=false;
  var door=hinge.gameObject.AddComponent<LighthouseDoor>();Ref(door,"hinge",hinge);Ref(door,"leaf",leafCollider);Ref(door,"player",player);Ref(door,"feedback",items);Ref(door,"view",Camera.main);
  var sound=Audio(hinge.gameObject,.5f,5);sound.volume=.30f;Ref(door,"sound",sound);Ref(door,"creak",Clip("Puerta_madera"));Ref(door,"latch",Clip("Cierre_puerta"));Ref(items,"door",door);
  var interior=new GameObject("Interior_sencillo").transform;interior.SetParent(faro,false);
  var interiorMat=new Material(stone){name="Piedra_interior"};interiorMat.SetColor("_BaseColor",new Color(.30f,.29f,.26f));AssetDatabase.CreateAsset(interiorMat,Generated+"/Piedra_interior.mat");
  Box(interior,"Suelo_interior",new Vector3(0,.57f,-.3f),new Vector3(2.5f,.10f,2.4f),interiorMat,true);
  Box(interior,"Suelo_umbral",new Vector3(0,.57f,-1.87f),new Vector3(1.08f,.10f,.74f),interiorMat,true);
  Box(interior,"Pared_fondo",new Vector3(0,2.10f,.94f),new Vector3(2.7f,3.06f,.16f),interiorMat);
  foreach(float sign in new[]{-1f,1f}){
   Box(interior,"Pared_lateral_"+sign,new Vector3(sign*1.33f,2.10f,-.3f),new Vector3(.16f,3.06f,2.4f),interiorMat);
   Box(interior,"Retorno_entrada_"+sign,new Vector3(sign*.925f,2.10f,-1.5f),new Vector3(.77f,3.06f,.13f),interiorMat);
   Box(interior,"Lateral_umbral_"+sign,new Vector3(sign*.59f,2.10f,-1.82f),new Vector3(.10f,3.06f,.67f),interiorMat);
  }
  Box(interior,"Techo_interior",new Vector3(0,3.61f,-.3f),new Vector3(2.7f,.12f,2.4f),interiorMat);
  Box(interior,"Techo_umbral",new Vector3(0,3.61f,-1.87f),new Vector3(1.18f,.12f,.74f),interiorMat);
  var shelterGo=new GameObject("Volumen_refugio");shelterGo.transform.SetParent(interior,false);var volume=shelterGo.AddComponent<BoxCollider>();volume.isTrigger=true;volume.center=new Vector3(0,2.1f,-.65f);volume.size=new Vector3(2.5f,3.1f,3.1f);
  var shelter=UnityEngine.Object.FindAnyObjectByType<RainField>().gameObject.AddComponent<RainShelter>();Ref(shelter,"interior",volume);Ref(shelter,"player",player.transform);Ref(shelter,"sound",UnityEngine.Object.FindAnyObjectByType<WeatherAudio>());
  var birds=SceneManager.GetActiveScene().GetRootGameObjects().Where(g=>g.name.StartsWith("Gaviota_en_vuelo",StringComparison.OrdinalIgnoreCase)).OrderBy(g=>g.name).ToArray();
  for(int i=0;i<birds.Length;i++){
   var bird=birds[i];Unpack(bird);foreach(var t in bird.GetComponentsInChildren<Transform>())t.gameObject.isStatic=false;
   var flight=bird.AddComponent<SeagullFlight>();Vec(flight,"center",i==0?new Vector3(-.1f,4.95f,-3.5f):new Vector3(-10.4f,2.4f,-15));
   var so=new SerializedObject(flight);so.FindProperty("circleRadius").vector2Value=i==0?new Vector2(4.6f,6.2f):new Vector2(3.1f,4);so.ApplyModifiedPropertiesWithoutUndo();Num(flight,"flightSpeed",i==0?1.25f:1.05f);Num(flight,"phase",i*2.6f);
   Ref(flight,"leftWing",Find(bird.transform,"Ala_izquierda_pivote"));Ref(flight,"rightWing",Find(bird.transform,"Ala_derecha_pivote"));Ref(flight,"weather",weather);Ref(flight,"voice",Audio(bird,3,19));
   so=new SerializedObject(flight);var calls=so.FindProperty("calls");calls.arraySize=2;for(int c=0;c<2;c++)calls.GetArrayElementAtIndex(c).objectReferenceValue=Clip("Gaviota_"+(c+1));so.ApplyModifiedPropertiesWithoutUndo();
  }
  var cache=new Dictionary<Mesh,WindMeshData>();int serial=0;
  var plants=SceneManager.GetActiveScene().GetRootGameObjects().Where(g=>g.name.StartsWith("Arbol_hoja_ancha")).Concat(GameObject.Find("faro isla").GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("calendula-flower")).Select(t=>t.gameObject)).ToArray();
  foreach(var plant in plants){
   bool tree=plant.name.StartsWith("Arbol");Unpack(plant);var filters=plant.GetComponentsInChildren<MeshFilter>().Where(f=>f.name!="Raices_marron").ToArray();
   var whole=filters[0].GetComponent<Renderer>().bounds;foreach(var f in filters)whole.Encapsulate(f.GetComponent<Renderer>().bounds);
   var sway=plant.AddComponent<VegetationSway>();Ref(sway,"weather",weather);Num(sway,"baseY",whole.min.y);Num(sway,"height",whole.size.y);Num(sway,"maxDisplacement",tree?.09f:.015f);Num(sway,"phase",serial*1.73f);
   var so=new SerializedObject(sway);var parts=so.FindProperty("parts");parts.arraySize=filters.Length;
   for(int i=0;i<filters.Length;i++){
    var f=filters[i];f.gameObject.isStatic=false;var mesh=f.sharedMesh;
    if(!cache.TryGetValue(mesh,out var data)){
     data=ScriptableObject.CreateInstance<WindMeshData>();data.vertices=mesh.vertices;data.normals=mesh.normals;data.uv=mesh.uv;data.submeshes=Enumerable.Range(0,mesh.subMeshCount).Select(s=>new WindMeshData.Submesh{triangles=mesh.GetTriangles(s)}).ToArray();AssetDatabase.CreateAsset(data,Generated+"/Vegetacion_"+cache.Count+".asset");cache.Add(mesh,data);
    }
    var part=parts.GetArrayElementAtIndex(i);part.FindPropertyRelative("filter").objectReferenceValue=f;part.FindPropertyRelative("data").objectReferenceValue=data;
   }
   so.ApplyModifiedPropertiesWithoutUndo();serial++;
  }
  GameObject.Find("Android_Controls").transform.Find("SafeArea/Title").GetComponent<Text>().text="EL ÚLTIMO FARO · LA ISLA DESPIERTA";
  SmoothEntrance(faro);Physics.SyncTransforms();Save();File.WriteAllText(Path.GetFullPath("../stage6-preparation.txt"),"Birds="+birds.Length+" Plants="+plants.Length+" Shared wind meshes="+cache.Count+"\nDoor, opening and sheltered interior prepared. Original models preserved.");
 }
 public static void BuildAndroid(){
  Open();FaroAndroidPreparation.ConfigureAndroid();PlayerSettings.bundleVersion="0.6.0";PlayerSettings.Android.bundleVersionCode=7;EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
  string output=Environment.GetEnvironmentVariable("FARO_APK_PATH");if(string.IsNullOrWhiteSpace(output))throw new Exception("FARO_APK_PATH required");
  var previous=PlayerSettings.GetAdditionalIl2CppArgs();BuildReport report;
  try{PlayerSettings.SetAdditionalIl2CppArgs(previous+" --jobs=2 --bee-jobs=2 --compiler-flags=-g0");report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=output,target=BuildTarget.Android,options=BuildOptions.None});}finally{PlayerSettings.SetAdditionalIl2CppArgs(previous);}
  File.WriteAllText(Path.GetFullPath("../stage6-build-result.txt"),report.summary.result+"\nErrors="+report.summary.totalErrors);if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Stage 6 build failed");
 }
 [MenuItem("El Último Faro/Etapa 6/Compilar APK Android")]
 public static void BuildFromMenu(){var path=EditorUtility.SaveFilePanel("Guardar APK",Path.GetFullPath(".."),"ElUltimoFaro_06_Entorno_Vivo","apk");if(string.IsNullOrEmpty(path))return;var previous=Environment.GetEnvironmentVariable("FARO_APK_PATH");try{Environment.SetEnvironmentVariable("FARO_APK_PATH",path);BuildAndroid();}finally{Environment.SetEnvironmentVariable("FARO_APK_PATH",previous);}}
}
