using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ElUltimoFaro;

public static class FaroAndroidPreparation
{
    const string ScenePath = "Assets/Scenes/SampleScene_Android_01.unity";
    const string Generated = "Assets/ElUltimoFaro/Generated";
    const int GroundLayer = 8;
    static Transform[] All => SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();

    [MenuItem("El Último Faro/Etapa 1/Abrir recorrido Android")]
    public static void OpenTraversal() => EditorSceneManager.OpenScene(ScenePath);

    public static void Prepare()
    {
        EditorSceneManager.OpenScene(ScenePath);
        Directory.CreateDirectory(Generated);
        var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers=tags.FindProperty("layers");
        string previous=layers.GetArrayElementAtIndex(GroundLayer).stringValue;
        if(!string.IsNullOrEmpty(previous) && previous!="FaroGround") throw new Exception("Layer 8 is already used: "+previous);
        layers.GetArrayElementAtIndex(GroundLayer).stringValue="FaroGround";
        tags.ApplyModifiedPropertiesWithoutUndo();
        string[] groundNames={"polySurface32","polySurface32.001","polySurface32.002","polySurface32.003"};
        var ground=All.Where(t=>groundNames.Contains(t.name)).ToArray();
        foreach(string name in groundNames) if(!ground.Any(t=>t.name==name)) throw new Exception("Missing island: "+name);
        foreach(var t in ground)
        foreach(var mf in t.GetComponentsInChildren<MeshFilter>())
        {
            if(!mf.sharedMesh) continue;
            mf.gameObject.layer=GroundLayer;
            var col=mf.GetComponent<MeshCollider>(); if(!col) col=mf.gameObject.AddComponent<MeshCollider>();
            col.sharedMesh=mf.sharedMesh;
        }
        // Only building geometry is solid in this first block; portable props remain unchanged.
        foreach(var root in All.Where(t=>t.name=="El_Ultimo_Faro_LowPoly" || t.name=="Maquinaria_Antigua_Cuarzo").ToArray())
        foreach(var mf in root.GetComponentsInChildren<MeshFilter>())
        {
            if(!mf.sharedMesh || mf.name=="LightBeam" || mf.name=="LampGlow" || mf.name.Contains("Glass")) continue;
            var col=mf.GetComponent<MeshCollider>(); if(!col) col=mf.gameObject.AddComponent<MeshCollider>();
            col.sharedMesh=mf.sharedMesh;
        }
        Physics.SyncTransforms();
        var lighthouse=All.FirstOrDefault(t=>t.name=="El_Ultimo_Faro_LowPoly");
        if(!lighthouse) throw new Exception("Missing lighthouse");
        Vector3 spawn=FindSpawn(ground.Where(t=>t.name!="polySurface32.003").ToArray(),lighthouse.position);
        if(GameObject.Find("Player_Android")) throw new Exception("Player already exists. Do not run Prepare twice.");
        var player=new GameObject("Player_Android");
        player.transform.position=spawn;
        Vector3 facing=lighthouse.position-spawn; facing.y=0;
        if(facing.sqrMagnitude>0.01f) player.transform.rotation=Quaternion.LookRotation(facing);
        var controller=player.AddComponent<CharacterController>();
        controller.height=1.7f; controller.radius=0.2f; controller.center=new Vector3(0,0.85f,0);
        controller.skinWidth=0.025f; controller.stepOffset=0.25f; controller.slopeLimit=50; controller.minMoveDistance=0;
        var camera=Camera.main;
        if(!camera) throw new Exception("Missing Main Camera");
        camera.transform.SetParent(player.transform);
        camera.transform.localPosition=new Vector3(0,1.55f,0);
        camera.transform.localRotation=Quaternion.identity;
        camera.nearClipPlane=0.05f; camera.farClipPlane=200; camera.fieldOfView=65;
        var mobile=player.AddComponent<MobilePlayerController>();
        BuildUI(mobile,out var move,out var look);
        mobile.Configure(camera.transform,move,look,1<<GroundLayer);
        ConfigureAndroid();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        File.WriteAllText(Path.GetFullPath("../preparation.txt"),"Spawn="+spawn+"\nEye height=1.55m\nIsland colliders="+ground.Length+"\n");
        Debug.Log("FARO_PREPARE_OK spawn="+spawn);
    }

    static Vector3 FindSpawn(Transform[] ground,Vector3 target)
    {
        float best=float.PositiveInfinity;
        Vector3 result=default;
        foreach(var root in ground)
        foreach(var r in root.GetComponentsInChildren<Renderer>())
        {
            var b=r.bounds;
            for(float x=b.min.x+0.35f;x<b.max.x-0.35f;x+=0.35f)
            for(float z=b.min.z+0.35f;z<b.max.z-0.35f;z+=0.35f)
            {
                if(!Physics.Raycast(new Vector3(x,b.max.y+2,z),Vector3.down,out var hit,20,1<<GroundLayer)) continue;
                if(Vector3.Angle(hit.normal,Vector3.up)>30) continue;
                Vector3 feet=hit.point+Vector3.up*0.05f;
                if(Physics.CheckCapsule(feet+Vector3.up*0.3f,feet+Vector3.up*1.45f,0.24f,~(1<<GroundLayer),QueryTriggerInteraction.Ignore)) continue;
                bool safe=true;
                foreach(var offset in new[]{Vector3.right,Vector3.left,Vector3.forward,Vector3.back})
                    if(!Physics.Raycast(feet+offset*0.45f+Vector3.up*0.3f,Vector3.down,out var edge,0.7f,1<<GroundLayer) || Vector3.Angle(edge.normal,Vector3.up)>40) {safe=false;break;}
                if(!safe) continue;
                float distance=Vector3.Distance(feet,target);
                float score=Mathf.Abs(distance-2.5f)+Mathf.Max(0,target.z-feet.z)*5f+Mathf.Abs(feet.y)*0.1f;
                if(score<best) {best=score;result=feet;}
            }
        }
        if(float.IsPositiveInfinity(best)) throw new Exception("No safe spawn found; inspect island geometry.");
        return result;
    }

    public static void FinalizeTraversal()
    {
        EditorSceneManager.OpenScene(ScenePath);
        foreach(var t in All)
        {
            var mf=t.GetComponent<MeshFilter>();
            if(!mf || !mf.sharedMesh) continue;
            bool tree=t.name=="Tronco_marron" || t.name=="Raices_marron";
            bool architecture=t.root.name=="faro isla" && t.gameObject.layer!=GroundLayer &&
                t.name!="mar_low_poly" && t.name!="pCube22" && t.name!="pCube23" &&
                !t.name.StartsWith("calendula") && !t.name.StartsWith("polySurface3");
            if(!tree && !architecture) continue;
            var col=t.GetComponent<MeshCollider>(); if(!col) col=t.gameObject.AddComponent<MeshCollider>();
            col.sharedMesh=mf.sharedMesh;
        }
        var player=UnityEngine.Object.FindFirstObjectByType<MobilePlayerController>();
        var body=player.GetComponent<CharacterController>(); body.enabled=false;
        Physics.SyncTransforms();
        var lighthouse=All.First(t=>t.name=="El_Ultimo_Faro_LowPoly");
        var ground=All.Where(t=>new[]{"polySurface32","polySurface32.001","polySurface32.002"}.Contains(t.name)).ToArray();
        player.transform.position=FindSpawn(ground,lighthouse.position);
        var facing=lighthouse.position-player.transform.position;facing.y=0;
        player.transform.rotation=Quaternion.LookRotation(facing);
        body.enabled=true;
        player.Configure(Camera.main.transform,UnityEngine.Object.FindFirstObjectByType<TouchMovePad>(),UnityEngine.Object.FindFirstObjectByType<TouchLookArea>(),1<<GroundLayer);
        QualitySettings.SetQualityLevel(0,true);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("FARO_TRAVERSAL_FINALIZED "+player.transform.position);
    }

    static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 size,Vector2 position)
    {
        var go=new GameObject(name,typeof(RectTransform));
        var rt=go.GetComponent<RectTransform>(); rt.SetParent(parent,false);
        rt.anchorMin=min; rt.anchorMax=max; rt.sizeDelta=size; rt.anchoredPosition=position;
        return rt;
    }
    static Image Image(RectTransform r,Color color) {var image=r.gameObject.AddComponent<Image>();image.color=color;return image;}
    static Text Label(string name,string text,Transform parent,Vector2 anchor,Vector2 size,Vector2 pos,int fontSize)
    {
        var r=Rect(name,parent,anchor,anchor,size,pos);
        var t=r.gameObject.AddComponent<Text>(); t.text=text; t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize=fontSize; t.color=new Color(0.95f,0.95f,0.86f); t.alignment=TextAnchor.MiddleCenter; t.raycastTarget=false;
        return t;
    }
    static void BuildUI(MobilePlayerController player,out TouchMovePad move,out TouchLookArea look)
    {
        var canvasObject=new GameObject("Android_Controls",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=canvasObject.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=0.5f;
        var safe=Rect("SafeArea",canvas.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        safe.gameObject.AddComponent<MobileSafeArea>();
        var lookRect=Rect("Mirar",safe,new Vector2(0.45f,0),Vector2.one,Vector2.zero,Vector2.zero);
        Image(lookRect,new Color(0,0,0,0.001f)); look=lookRect.gameObject.AddComponent<TouchLookArea>();
        var pad=Rect("Movimiento",safe,Vector2.zero,Vector2.zero,new Vector2(190,190),new Vector2(145,145));
        Image(pad,new Color(0.04f,0.12f,0.15f,0.55f));
        move=pad.gameObject.AddComponent<TouchMovePad>();
        var thumb=Rect("Control",pad,new Vector2(0.5f,0.5f),new Vector2(0.5f,0.5f),new Vector2(65,65),Vector2.zero);
        Image(thumb,new Color(0.66f,0.83f,0.77f,0.8f)).raycastTarget=false; move.Configure(thumb);
        Label("MoveLabel","MOVER",safe,Vector2.zero,new Vector2(190,35),new Vector2(145,35),19);
        Label("LookLabel","DESLIZA PARA MIRAR",safe,Vector2.right,new Vector2(290,35),new Vector2(-190,35),19);
        Label("Title","EL ÚLTIMO FARO · PRUEBA DE RECORRIDO",safe,new Vector2(0.5f,1),new Vector2(680,40),new Vector2(0,-30),22);
        var reset=Rect("VolverAlInicio",safe,Vector2.one,Vector2.one,new Vector2(205,55),new Vector2(-125,-90));
        Image(reset,new Color(0.04f,0.12f,0.15f,0.8f));
        var button=reset.gameObject.AddComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick,player.ReturnToStart);
        Label("Label","VOLVER AL INICIO",reset,new Vector2(0.5f,0.5f),new Vector2(200,50),Vector2.zero,18);
        if(!UnityEngine.Object.FindFirstObjectByType<EventSystem>())
        {
            var events=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }
    public static void ConfigureAndroid()
    {
        PlayerSettings.productName="El Último Faro";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.elultimofaro.experiencia");
        PlayerSettings.bundleVersion="0.1.0";
        PlayerSettings.Android.bundleVersionCode=1;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)36;
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait=false; PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
        PlayerSettings.allowedAutorotateToLandscapeLeft=true; PlayerSettings.allowedAutorotateToLandscapeRight=true;
        PlayerSettings.Android.renderOutsideSafeArea=true;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});
        EditorUserBuildSettings.buildAppBundle=false;
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
    }
    public static void BuildAndroid()
    {
        ConfigureAndroid();
        var output=Environment.GetEnvironmentVariable("FARO_APK_PATH");
        if(string.IsNullOrWhiteSpace(output)) output=Path.GetFullPath("Builds/Android/ElUltimoFaro_01_Recorrido.apk");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{ScenePath},locationPathName=output,target=BuildTarget.Android,options=BuildOptions.None});
        File.WriteAllText(Path.GetFullPath("../android-build-result.txt"),report.summary.result+"\nErrors="+report.summary.totalErrors+"\nBytes="+report.summary.totalSize);
        if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Android build failed: "+report.summary.result);
        Debug.Log("FARO_APK_OK "+output);
    }
}
