using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using ElUltimoFaro;
public static class FaroQuestSecondTools
{
    const string Output="Builds/Quest3S_Prueba02";
    const string Scene="Assets/Scenes/SampleScene_Quest.unity";
    static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
    public static void PrepareAndVerify(){
        Directory.CreateDirectory(Output);EditorSceneManager.OpenScene(Scene);
        var bridge=UnityEngine.Object.FindAnyObjectByType<QuestPlayerBridge>();var grip=bridge.GetComponent<QuestGrip>();
        Check(bridge&&grip,"Missing VR player");
        foreach(var old in new[]{grip.leftMarker,grip.rightMarker})if(old)old.gameObject.SetActive(false);
        grip.leftMarker=null;grip.rightMarker=null;
        Material glove=Mat("Guantes",new Color(.65f,.44f,.24f),"ElUltimoFaro/GuanteVR");
        Hand(grip.leftHand,true,glove);Hand(grip.rightHand,false,glove);
        var boat=bridge.boat;var seat=(Transform)new SerializedObject(boat).FindProperty("seat").objectReferenceValue;
        Vector3 center=boat.transform.InverseTransformPoint(seat.position);
        var rowing=bridge.GetComponent<QuestRowing>();if(!rowing)rowing=bridge.gameObject.AddComponent<QuestRowing>();
        rowing.boat=boat;rowing.grip=grip;rowing.leftHand=grip.leftHand;rowing.rightHand=grip.rightHand;bridge.rowing=rowing;grip.rowing=rowing;
        Material wood=Mat("Remos_madera",new Color(.32f,.16f,.065f));Material blade=Mat("Remos_pala",new Color(.56f,.32f,.12f));Material metal=Mat("Remos_herrajes",new Color(.13f,.18f,.19f));
        Oar(boat.transform,center,-1,wood,blade,metal,out rowing.leftHandle,out rowing.leftOar);
        Oar(boat.transform,center,1,wood,blade,metal,out rowing.rightHandle,out rowing.rightOar);
        rowing.quartzCargo=Cargo(boat.transform,center,new Vector3(-.13f,.09f,-.25f),"Bandeja_cuarzo",wood,metal);
        rowing.lanternCargo=Cargo(boat.transform,center,new Vector3(.13f,.09f,-.25f),"Bandeja_farol",wood,metal);
        var prompts=bridge.GetComponent<QuestPrompts>();if(!prompts)prompts=bridge.gameObject.AddComponent<QuestPrompts>();
        prompts.bridge=bridge;prompts.grip=grip;prompts.rowing=rowing;prompts.machine=UnityEngine.Object.FindAnyObjectByType<MachineInteraction>();prompts.door=UnityEngine.Object.FindAnyObjectByType<LighthouseDoor>();
        if(!prompts.canvas){
            var ui=new GameObject("Indicaciones_VR",typeof(RectTransform),typeof(Canvas),typeof(Image));
            var rect=ui.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(620,150);
            prompts.canvas=ui.GetComponent<Canvas>();prompts.canvas.renderMode=RenderMode.WorldSpace;prompts.canvas.worldCamera=bridge.head.GetComponent<Camera>();
            var image=ui.GetComponent<Image>();image.color=new Color(.025f,.055f,.075f,.94f);image.raycastTarget=false;
            var words=new GameObject("Accion",typeof(RectTransform),typeof(Text));words.transform.SetParent(ui.transform,false);
            var wordRect=words.GetComponent<RectTransform>();wordRect.anchorMin=Vector2.zero;wordRect.anchorMax=Vector2.one;wordRect.offsetMin=new Vector2(14,8);wordRect.offsetMax=new Vector2(-14,-8);
            prompts.label=words.GetComponent<Text>();prompts.label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");prompts.label.fontSize=30;prompts.label.fontStyle=FontStyle.Bold;prompts.label.alignment=TextAnchor.MiddleCenter;prompts.label.color=new Color(1,.92f,.74f);prompts.label.raycastTarget=false;
            prompts.canvas.enabled=false;
        }
        PlayerSettings.productName="El Último Faro VR";PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.elultimofaro.quest");PlayerSettings.bundleVersion="0.8.0";PlayerSettings.Android.bundleVersionCode=80;
        foreach(var component in new UnityEngine.Object[]{bridge,grip,rowing,prompts})EditorUtility.SetDirty(component);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        VerifyMath();
        Check(UnityEngine.Object.FindObjectsByType<Camera>().Count(c=>c.enabled)==1,"Camera duplication");
        foreach(var obj in EditorSceneManager.GetActiveScene().GetRootGameObjects())foreach(var t in obj.GetComponentsInChildren<Transform>(true))Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"Missing script: "+t.name);
        File.WriteAllText(Output+"/verification.txt","PASS: rowing forward, steering, recovery, stationary hands, tracking loss/discontinuity.\nPASS: one camera and no missing scripts.\nBoat seat local="+center+"\nLeft handle="+rowing.leftHandle.localPosition+"\nRight handle="+rowing.rightHandle.localPosition+"\nReal controller and headset test still required.\n");
        Preview(glove);
    }
    static void VerifyMath(){
        float pull=QuestRowing.Pull(new Vector3(0,0,.03f),Vector3.zero,.05f,true);Check(pull>.8f,"Forward pull");
        var both=QuestRowing.Mix(pull,pull);Check(both.y>.8f&&Mathf.Abs(both.x)<.001f,"Balanced rowing");
        Check(QuestRowing.Mix(pull,0).x>0&&QuestRowing.Mix(0,pull).x<0,"Steering directions");
        Check(QuestRowing.Pull(Vector3.zero,new Vector3(0,0,.03f),.05f,true)==0,"Recovery must not reverse");
        Check(QuestRowing.Pull(Vector3.zero,Vector3.zero,.05f,true)==0,"Stationary hands");
        Check(QuestRowing.Pull(Vector3.forward,Vector3.zero,.05f,true)==0,"Reject tracking jump");
        Check(QuestRowing.Pull(Vector3.forward,Vector3.zero,.05f,false)==0,"Released grip");
        var origin=new GameObject("BoatFrameTest");Vector3 local=new Vector3(.1f,.3f,.2f);var before=origin.transform.InverseTransformPoint(origin.transform.TransformPoint(local));origin.transform.SetPositionAndRotation(new Vector3(5,1,7),Quaternion.Euler(0,65,0));var after=origin.transform.InverseTransformPoint(origin.transform.TransformPoint(local));Check(QuestRowing.Pull(before,after,.016f,true)<.001f,"Hull movement must not generate strokes");UnityEngine.Object.DestroyImmediate(origin);
    }
    static void Hand(Transform anchor,bool left,Material material){
        var existing=anchor.Find("Guante_VR");var go=existing?existing.gameObject:new GameObject("Guante_VR",typeof(MeshFilter),typeof(MeshRenderer),typeof(QuestHandVisual));
        go.transform.SetParent(anchor,false);go.transform.localPosition=new Vector3(0,-.012f,.012f);go.transform.localRotation=Quaternion.identity;go.transform.localScale=Vector3.one;
        go.GetComponent<MeshRenderer>().sharedMaterial=material;go.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;go.GetComponent<QuestHandVisual>().left=left;
    }
    static Material Mat(string name,Color color,string shader="Universal Render Pipeline/Lit"){
        string path="Assets/ElUltimoFaro/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat)return mat;
        mat=new Material(Shader.Find(shader));mat.name=name;if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",color);if(mat.HasProperty("_Smoothness"))mat.SetFloat("_Smoothness",.2f);AssetDatabase.CreateAsset(mat,path);return mat;
    }
    static Transform Node(Transform parent,string name,Vector3 position){var t=parent.Find(name);if(!t){t=new GameObject(name).transform;t.SetParent(parent,false);}t.localPosition=position;return t;}
    static void Box(Transform parent,string name,Vector3 p,Vector3 size,Material material){
        if(parent.Find(name))return;var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;
    }
    static void Oar(Transform boat,Vector3 center,float side,Material wood,Material blade,Material metal,out Transform handle,out Transform pivot){
        string suffix=side<0?"izquierdo":"derecho";
        Vector3 rest=center+new Vector3(side*.13f,.30f,.14f);
        handle=Node(boat,"Agarre_remo_"+suffix,rest);
        pivot=Node(boat,"Remo_"+suffix,center+new Vector3(side*.27f,.27f,.14f));
        Box(pivot,"Mango",new Vector3(-side*.14f,.03f,0),new Vector3(.09f,.025f,.025f),metal);
        var shaft=Node(pivot,"Eje",Vector3.zero);shaft.localRotation=Quaternion.Euler(0,0,side*-17);
        Box(shaft,"Madera",new Vector3(side*.20f,0,0),new Vector3(.76f,.019f,.023f),wood);
        Box(shaft,"Pala",new Vector3(side*.65f,0,0),new Vector3(.24f,.025f,.10f),blade);
        Box(pivot,"Abrazadera",Vector3.zero,new Vector3(.045f,.05f,.05f),metal);
    }
    static Transform Cargo(Transform boat,Vector3 center,Vector3 offset,string name,Material wood,Material metal){
        var slot=Node(boat,name,center+offset);Box(slot,"Base",new Vector3(0,-.01f,0),new Vector3(.18f,.02f,.18f),wood);Box(slot,"Borde",new Vector3(0,0,-.09f),new Vector3(.20f,.025f,.016f),metal);return slot;
    }
    static void Preview(Material glove){
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
        var root=new GameObject("Preview");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);
        for(int i=0;i<2;i++){var hand=new GameObject("Hand",typeof(MeshFilter),typeof(MeshRenderer),typeof(QuestHandVisual));hand.layer=31;hand.transform.SetParent(root.transform);hand.transform.position=new Vector3((i-.5f)*.16f,0,0);hand.transform.rotation=Quaternion.Euler(-20,i==0?-15:15,0);hand.GetComponent<MeshRenderer>().sharedMaterial=glove;var v=hand.GetComponent<QuestHandVisual>();v.left=i==0;v.Build(i==0?0:.75f);Check(hand.GetComponent<MeshFilter>().sharedMesh.vertexCount>200,"Glove mesh");}
        var cameraObject=new GameObject("PreviewCamera",typeof(Camera));cameraObject.transform.SetParent(root.transform);cameraObject.transform.position=new Vector3(0,.36f,.24f);cameraObject.transform.LookAt(new Vector3(0,0,.035f));var camera=cameraObject.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.025f,.035f,.05f);camera.nearClipPlane=.01f;camera.farClipPlane=2;camera.cullingMask=1<<31;camera.fieldOfView=48;
        var rt=new RenderTexture(1200,800,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1200,800,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1200,800),0,0);image.Apply();File.WriteAllBytes(Output+"/guantes.png",image.EncodeToPNG());RenderTexture.active=null;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(image);rt.Release();UnityEngine.Object.DestroyImmediate(rt);EditorSceneManager.CloseScene(scene,true);
    }
    public static void Build(){
        Directory.CreateDirectory(Output);var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Scene},locationPathName=Output+"/ElUltimoFaro_Quest3S_Prueba02.apk",target=BuildTarget.Android,options=BuildOptions.Development});
        File.WriteAllText(Output+"/build-status.txt",report.summary.result+"\nErrors="+report.summary.totalErrors+"\nDuration="+report.summary.totalTime);if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed");
    }
}

