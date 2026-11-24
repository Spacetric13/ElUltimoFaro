using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using ElUltimoFaro;
public static class FaroHallTools {
 public static void AssignVideo(){
  AssetDatabase.Refresh();var clip=AssetDatabase.LoadAssetAtPath<UnityEngine.Video.VideoClip>(Folder+"/Recuerdo.mp4");if(!clip)throw new Exception("Memory video missing");
  foreach(string scene in new[]{"Hall_PC","Hall_Quest"}){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");UnityEngine.Object.FindAnyObjectByType<HallSequence>().memoryVideo=clip;EditorSceneManager.SaveScene(SceneManager.GetActiveScene());}
  EditorSceneManager.OpenScene("Assets/Scenes/Hall_PC.unity");
 }
 public const string Output="Builds/Hall_Prueba04";
 const string Folder="Assets/ElUltimoFaro/Generated/Hall";
 static Material wood,edge,navy,wall,floor,cream,brass,linen,glass,photo;
 static Transform root;
 static Material Mat(string name,Color color,bool unlit=false){string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.12f);return m;}
 static GameObject Box(string name,Vector3 p,Vector3 size,Material material,bool solid=true){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root,false);g.transform.localPosition=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=material;if(!solid)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
 static Text Label(string name,string value,Vector3 p,Vector2 size,int font=34){var g=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(Text));g.transform.SetParent(root,false);g.transform.localPosition=p;g.transform.localScale=Vector3.one*.002f;var rt=g.GetComponent<RectTransform>();rt.sizeDelta=size;g.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;var text=g.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=font;text.alignment=TextAnchor.MiddleCenter;text.color=new Color(1,.86f,.64f);text.text=value;text.raycastTarget=false;return text;}
 static void SetLayer(GameObject g){foreach(var t in g.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;}
 static Mesh PhotoMesh(){string path=Folder+"/Foto_mentor.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh)return mesh;mesh=new Mesh{name="Fotografia del mentor"};mesh.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};mesh.uv=new[]{new Vector2(.310f,1-.481f),new Vector2(.451f,1-.470f),new Vector2(.441f,1-.291f),new Vector2(.303f,1-.294f)};mesh.triangles=new[]{0,2,1,0,3,2};mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;}
 [MenuItem("El Último Faro/Hall/Abrir hall PC")]
 public static void OpenPC(){FaroDesktopTools.CleanPC();ScenePair(false);EditorSceneManager.OpenScene("Assets/Scenes/Hall_PC.unity");}
 [MenuItem("El Último Faro/Hall/Abrir hall Quest")]
 public static void OpenQuest(){ScenePair(true);EditorSceneManager.OpenScene("Assets/Scenes/Hall_Quest.unity");}
 static void ScenePair(bool vr){EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/"+(vr?"Hall_Quest":"Hall_PC")+".unity",true),new EditorBuildSettingsScene("Assets/Scenes/"+(vr?"Isla_Quest":"SampleScene_PC")+".unity",true)};}
 static GameObject Coat(){
  string path=Folder+"/Abrigo_plegado.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  if(!mesh){mesh=new Mesh{name="Abrigo con pliegues"};var vertices=new System.Collections.Generic.List<Vector3>();var triangles=new System.Collections.Generic.List<int>();
   float[] heights={-.45f,-.15f,.18f,.40f,.46f};float[] widths={.32f,.38f,.35f,.30f,.12f};
   for(int row=0;row<4;row++)for(int col=0;col<4;col++){
    Vector3[] q=new Vector3[4];for(int corner=0;corner<4;corner++){int y=row+(corner>=2?1:0),x=col+((corner==1||corner==2)?1:0);q[corner]=new Vector3((x/4f*2-1)*widths[y],heights[y],x%2==0?0:-.075f);}
    foreach(int k in new[]{0,2,1,0,3,2}){triangles.Add(vertices.Count);vertices.Add(q[k]);}
   }mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);}
  var go=new GameObject("Abrigo_del_mentor",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root,false);go.transform.localPosition=new Vector3(1.15f,.92f,.43f);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=navy;return go;
 }
 public static void Prepare(){
  Directory.CreateDirectory(Output);AssetDatabase.Refresh();
  wood=Mat("Madera_miel",new Color(.37f,.16f,.07f));edge=Mat("Madera_oscura",new Color(.15f,.065f,.035f));navy=Mat("Tela_azul",new Color(.07f,.105f,.17f));wall=Mat("Pared_azul",new Color(.12f,.145f,.23f));floor=Mat("Suelo",new Color(.24f,.115f,.065f));cream=Mat("Papel",new Color(.84f,.70f,.49f));brass=Mat("Laton",new Color(.62f,.36f,.12f));linen=Mat("Ropa_cama",new Color(.38f,.42f,.43f));glass=Mat("Ventana_atardecer",new Color(1,.57f,.22f),true);photo=Mat("Foto",Color.white,true);photo.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/Referencia_habitacion.jpg"));
  EditorSceneManager.OpenScene("Assets/Scenes/SampleScene_Quest.unity");
  foreach(var manager in UnityEngine.Object.FindObjectsByType<OVRManager>())UnityEngine.Object.DestroyImmediate(manager);
  EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/Isla_Quest.unity");
  Create(false);Create(true);ScenePair(false);AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/Hall_PC.unity");
  File.WriteAllText(Output+"/prepared.txt","Hall_PC and Hall_Quest created. Photo UV uses original supplied reference. Separate scenes and additive cinematic island.\n");
 }
 static void Create(bool vr){
  EditorSceneManager.OpenScene("Assets/Scenes/SampleScene_Quest.unity");var original=UnityEngine.Object.FindAnyObjectByType<OVRCameraRig>();var sourceScene=SceneManager.GetActiveScene();
  var hall=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(hall);
  root=new GameObject("Habitacion_del_recuerdo").transform;root.position=new Vector3(100,0,100);var sequence=root.gameObject.AddComponent<HallSequence>();sequence.destination=vr?"Isla_Quest":"SampleScene_PC";
  Box("Suelo",new Vector3(0,-.1f,0),new Vector3(6,.2f,5.2f),floor);
  for(int i=0;i<15;i++)Box("Junta_tablas",new Vector3(-2.9f+i*.4f,.002f,0),new Vector3(.012f,.005f,5.2f),edge,false);
  Box("Pared_fondo",new Vector3(0,1.65f,2.6f),new Vector3(6,3.3f,.16f),wall);Box("Pared_izquierda",new Vector3(-3,1.65f,0),new Vector3(.16f,3.3f,5.2f),wall);Box("Pared_entrada",new Vector3(0,1.65f,-2.6f),new Vector3(6,3.3f,.16f),wall);Box("Techo",new Vector3(0,3.3f,0),new Vector3(6,.12f,5.2f),wall);
  Box("Pared_ventana_baja",new Vector3(3,.4f,0),new Vector3(.16f,.8f,5.2f),wood);Box("Pared_ventana_alta",new Vector3(3,3.02f,0),new Vector3(.16f,.56f,5.2f),wood);
  Box("Vidrio_atardecer",new Vector3(3.06f,1.77f,0),new Vector3(.04f,1.95f,5.2f),glass);
  for(int i=-2;i<=2;i++)Box("Marco_ventana",new Vector3(2.94f,1.77f,i*1.18f),new Vector3(.13f,2.05f,.10f),edge);
  Box("Travesano_ventana",new Vector3(2.92f,1.7f,0),new Vector3(.14f,.09f,5.2f),edge);Box("Alfeizar",new Vector3(2.85f,.82f,0),new Vector3(.40f,.1f,5.2f),wood);
  Box("Escritorio",new Vector3(-.35f,.91f,1.78f),new Vector3(2.5f,.12f,.85f),wood);
  foreach(float x in new[]{-1.45f,.75f})foreach(float z in new[]{1.45f,2.08f})Box("Pata_escritorio",new Vector3(x,.44f,z),new Vector3(.12f,.88f,.12f),edge);
  Box("Cajon",new Vector3(-.35f,.76f,1.5f),new Vector3(1.3f,.2f,.25f),wood);Box("Tirador",new Vector3(-.35f,.76f,1.35f),new Vector3(.25f,.035f,.035f),brass);
  var picture=new GameObject("Fotografia_mentor",typeof(MeshFilter),typeof(MeshRenderer));picture.transform.SetParent(root,false);picture.transform.localPosition=new Vector3(-.75f,1.28f,1.7f);picture.transform.localScale=new Vector3(.68f,.50f,1);picture.GetComponent<MeshFilter>().sharedMesh=PhotoMesh();picture.GetComponent<MeshRenderer>().sharedMaterial=photo;
  Box("Paspartu",new Vector3(-.75f,1.28f,1.72f),new Vector3(.77f,.59f,.025f),cream,false);
  Box("Marco_izq",new Vector3(-1.16f,1.28f,1.69f),new Vector3(.06f,.68f,.06f),edge);Box("Marco_der",new Vector3(-.34f,1.28f,1.69f),new Vector3(.06f,.68f,.06f),edge);foreach(float y in new[]{.96f,1.6f})Box("Marco_horizontal",new Vector3(-.75f,y,1.69f),new Vector3(.87f,.06f,.06f),wood);
  sequence.memoryVideo=AssetDatabase.LoadAssetAtPath<UnityEngine.Video.VideoClip>(Folder+"/Recuerdo.mp4");sequence.photo=picture.transform;sequence.photoMesh=picture.GetComponent<MeshFilter>();sequence.photoMaterial=photo;
  for(int i=0;i<3;i++){var book=Box("Libro",new Vector3(.37f,.99f+i*.05f,1.8f),new Vector3(.42f,.045f,.30f),i%2==0?navy:cream);book.transform.localRotation=Quaternion.Euler(0,i*12,0);}
  Box("Silla_asiento",new Vector3(1.15f,.5f,.90f),new Vector3(.65f,.1f,.64f),wood);Box("Silla_respaldo",new Vector3(1.15f,1,.63f),new Vector3(.65f,.85f,.075f),wood);
  foreach(float x in new[]{.90f,1.4f})foreach(float z in new[]{.65f,1.14f})Box("Pata_silla",new Vector3(x,.24f,z),new Vector3(.075f,.48f,.075f),edge);
  var coat=Coat();coat.transform.localRotation=Quaternion.Euler(-7,0,0);var sleeve=Box("Manga_abrigo",new Vector3(.77f,.87f,.50f),new Vector3(.18f,.75f,.18f),navy,false);sleeve.transform.localRotation=Quaternion.Euler(0,0,-12);Box("Solapa",new Vector3(1.13f,1.21f,.43f),new Vector3(.23f,.32f,.025f),linen,false).transform.localRotation=Quaternion.Euler(0,0,28);
  Box("Cama_base",new Vector3(-2,.26f,-.7f),new Vector3(1.35f,.45f,2.35f),edge);Box("Colchon",new Vector3(-2,.55f,-.7f),new Vector3(1.30f,.22f,2.3f),cream);Box("Manta",new Vector3(-2,.68f,-.90f),new Vector3(1.34f,.05f,1.8f),navy,false);Box("Almohada",new Vector3(-2,.73f,.16f),new Vector3(.95f,.16f,.45f),linen);Box("Cabecera",new Vector3(-2,.65f,.5f),new Vector3(1.4f,1.2f,.1f),wood);
  Box("Mochila",new Vector3(-1.1f,.29f,1),new Vector3(.42f,.58f,.24f),navy);Box("Bolsillo",new Vector3(-1.1f,.25f,.855f),new Vector3(.31f,.30f,.08f),wood,false);
  Box("Alfombra",new Vector3(.2f,.007f,-.7f),new Vector3(2.5f,.012f,2.15f),navy,false);Box("Puerta_cerrada",new Vector3(1.5f,1.06f,-2.49f),new Vector3(.95f,2.12f,.1f),wood);Box("Manilla",new Vector3(1.16f,1,-2.40f),new Vector3(.05f,.05f,.08f),brass);
  Label("Titulo","EL ÚLTIMO FARO",new Vector3(.1f,2.65f,2.49f),new Vector2(2000,200),82);
  Label("Historia","Tu mentor te enseñó a mantener la luz.\nAhora la isla te necesita.\nEncuentra el cuarzo y vuelve a encender el faro.",new Vector3(.15f,2.12f,2.48f),new Vector2(2000,310),44);
  sequence.prompt=Label("Inicio","Acércate a la fotografía",new Vector3(-.75f,1.83f,1.68f),new Vector2(1000,110),32);
  var controls=Label("Controles",vr?"PALANCA IZQUIERDA · CAMINAR\nPALANCA DERECHA · GIRO\nAGARRE LATERAL · TOMAR OBJETOS\nA · ACTIVAR     B · EMBARCAR\nREMOS · TIRA PARA AVANZAR\nGATILLOS + EMPUJAR · RETROCEDER":"WASD · CAMINAR\nBOTÓN DERECHO + RATÓN · MIRAR\nE · RECOGER / ACTIVAR\nB · EMBARCAR / DESEMBARCAR\nZ / C · REMAR     SHIFT · RETROCESO",new Vector3(2.1f,1.65f,2.48f),new Vector2(600,660),30);
  var lamp=new GameObject("Luz_de_atardecer",typeof(Light));lamp.transform.SetParent(root,false);lamp.transform.localPosition=new Vector3(2.3f,2.4f,.2f);lamp.transform.localRotation=Quaternion.Euler(28,-65,0);var light=lamp.GetComponent<Light>();light.type=LightType.Directional;light.spotAngle=115;light.range=9;light.intensity=1.6f;light.color=new Color(1,.66f,.34f);light.shadows=LightShadows.Soft;light.cullingMask=1<<30;
  var fill=new GameObject("Luz_suave",typeof(Light));fill.transform.SetParent(root,false);fill.transform.localPosition=new Vector3(-1,2.8f,-.8f);var fl=fill.GetComponent<Light>();fl.type=LightType.Point;fl.range=7;fl.intensity=.8f;fl.color=new Color(.42f,.52f,.8f);fl.cullingMask=1<<30;
  var person=new GameObject("Visitante",typeof(CharacterController),typeof(HallPlayer));person.transform.SetParent(root,false);person.transform.localPosition=new Vector3(.05f,.03f,-1.35f);var body=person.GetComponent<CharacterController>();body.height=1.75f;body.radius=.22f;body.center=Vector3.up*.875f;body.stepOffset=.2f;body.skinWidth=.02f;sequence.player=person.GetComponent<HallPlayer>();sequence.player.desktop=!vr;
  Camera camera;
  if(vr){var clone=UnityEngine.Object.Instantiate(original.gameObject,person.transform);clone.name="OVRCameraRig_Hall";clone.transform.localPosition=Vector3.zero;clone.transform.localRotation=Quaternion.identity;clone.transform.localScale=Vector3.one;var clonedManager=clone.GetComponent<OVRManager>();if(clonedManager)UnityEngine.Object.DestroyImmediate(clonedManager);
   var session=new GameObject("Sesion_XR",typeof(OVRManager),typeof(HallXRSession));EditorUtility.CopySerialized(original.GetComponent<OVRManager>(),session.GetComponent<OVRManager>());
   var rig=clone.GetComponent<OVRCameraRig>();camera=rig.centerEyeAnchor.GetComponent<Camera>();foreach(var c in clone.GetComponentsInChildren<Camera>(true))c.enabled=c==camera;sequence.player.head=rig.centerEyeAnchor;}
  else {var go=new GameObject("Camara_PC",typeof(Camera),typeof(AudioListener));go.transform.SetParent(person.transform,false);go.transform.localPosition=Vector3.up*1.6f;camera=go.GetComponent<Camera>();sequence.player.head=go.transform;}
  camera.tag="MainCamera";camera.nearClipPlane=.05f;camera.farClipPlane=35;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.02f,.025f,.04f);if(!vr)camera.stereoTargetEye=StereoTargetEyeMask.None;
  var overlay=new GameObject("Transicion",typeof(RectTransform),typeof(Canvas),typeof(Image));overlay.transform.SetParent(camera.transform,false);overlay.transform.localPosition=new Vector3(0,0,.12f);overlay.transform.localScale=Vector3.one*.01f;overlay.GetComponent<RectTransform>().sizeDelta=new Vector2(100,100);var canvas=overlay.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.sortingOrder=1000;sequence.fade=overlay.GetComponent<Image>();sequence.fade.color=Color.clear;sequence.fade.raycastTarget=false;
  sequence.ambience=root.gameObject.AddComponent<AudioSource>();sequence.ambience.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ElUltimoFaro/Generated/Weather5/Audio/Viento_costero.wav");sequence.ambience.loop=true;sequence.ambience.playOnAwake=true;sequence.ambience.volume=.055f;
  sequence.effects=root.gameObject.AddComponent<AudioSource>();sequence.effects.playOnAwake=false;sequence.thunder=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ElUltimoFaro/Generated/Weather5/Audio/Trueno_1.wav");sequence.rain=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ElUltimoFaro/Generated/Weather5/Audio/Lluvia_fuerte.wav");
  SetLayer(root.gameObject);RenderSettings.skybox=null;RenderSettings.fog=false;RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.42f,.38f,.37f);
  EditorSceneManager.SaveScene(hall,"Assets/Scenes/"+(vr?"Hall_Quest":"Hall_PC")+".unity");EditorSceneManager.CloseScene(sourceScene,true);
 }
 [MenuItem("El Último Faro/Hall/Compilar prueba PC")]
 public static void BuildPC(){FaroDesktopTools.CleanPC();PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.defaultIsNativeResolution=true;PlayerSettings.resizableWindow=true;Build(false);}
 [MenuItem("El Último Faro/Hall/Compilar APK Quest")]
 public static void BuildQuest(){Build(true);}
 static void Build(bool vr){Directory.CreateDirectory(Output);string folder=Output+(vr?"/ElUltimoFaro_Quest_Hall.apk":"/Juego_PC/ElUltimoFaro_PC.exe");if(!vr){string bindings=Output+"/Juego_PC/RuntimeActionBindings.json";if(File.Exists(bindings))File.Delete(bindings);PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);}
  PlayerSettings.bundleVersion="0.9.0";PlayerSettings.Android.bundleVersionCode=90;
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/"+(vr?"Hall_Quest":"Hall_PC")+".unity","Assets/Scenes/"+(vr?"Isla_Quest":"SampleScene_PC")+".unity"},locationPathName=folder,target=vr?BuildTarget.Android:BuildTarget.StandaloneWindows64,options=vr?BuildOptions.Development:BuildOptions.None});File.WriteAllText(Output+(vr?"/build-quest.txt":"/build-pc.txt"),report.summary.result+"\nErrors="+report.summary.totalErrors);if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed");}
}
