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
[InitializeOnLoad]
public static class FaroQuestFirstBuild
{
    const string Root="Builds/Quest3S_Prueba01";
    const string Scene="Assets/Scenes/SampleScene_Quest.unity";
    static FaroQuestFirstBuild(){EditorApplication.update+=Poll;}
    static void Poll(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
        string request="Temp/FaroQuestFirst.request";
        if(!File.Exists(request))return;
        var command=File.ReadAllText(request).Trim();File.Delete(request);Directory.CreateDirectory(Root);
        try{
            if(command=="prepare")Prepare();
            else if(command=="build")Build();
            else throw new Exception("Unknown command");
        }catch(Exception ex){File.WriteAllText(Root+"/error.txt",ex.ToString());Debug.LogException(ex);}
    }
    static void Prepare(){
        var scene=EditorSceneManager.GetActiveScene();
        if(scene.path!=Scene)throw new Exception("Abre la escena Quest antes de preparar.");
        EditorSceneManager.SaveScene(scene,Root+"/Quest_before_changes.unity",true);
        FaroQuestSetup.Setup();
        var rig=UnityEngine.Object.FindAnyObjectByType<OVRCameraRig>();
        var bridge=UnityEngine.Object.FindAnyObjectByType<QuestPlayerBridge>();
        var grip=bridge.GetComponent<QuestGrip>();if(!grip)grip=bridge.gameObject.AddComponent<QuestGrip>();
        grip.player=bridge.player;grip.interaction=bridge.interaction;grip.socket=UnityEngine.Object.FindAnyObjectByType<QuartzSocketTrigger>();
        grip.leftHand=rig.transform.Find("TrackingSpace/LeftHandAnchor");grip.rightHand=rig.transform.Find("TrackingSpace/RightHandAnchor");
        if(!grip.leftHand||!grip.rightHand)throw new Exception("No se encuentran los anclajes de los mandos.");
        grip.leftMarker=Marker(grip.leftHand,"Mando_izquierdo_prueba",new Color(.12f,.7f,1));
        grip.rightMarker=Marker(grip.rightHand,"Mando_derecho_prueba",new Color(1,.55f,.15f));
        var camera=rig.transform.Find("TrackingSpace/CenterEyeAnchor").GetComponent<Camera>();
        if(!camera.enabled||UnityEngine.Object.FindObjectsByType<Camera>().Count(c=>c.enabled)!=1)throw new Exception("La escena debe tener una cámara activa.");
        if(!grip.interaction.Quartz||!grip.interaction.Lantern||!grip.socket)throw new Exception("Falta un objeto de interacción.");
        PlayerSettings.productName="El Último Faro VR";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.elultimofaro.quest");
        PlayerSettings.bundleVersion="0.7.1";PlayerSettings.Android.bundleVersionCode=71;
        EditorUserBuildSettings.buildAppBundle=false;
        EditorUtility.SetDirty(grip);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        File.WriteAllText(Root+"/preparation.txt","OK\nScene="+Scene+"\nCamera="+camera.name+"\nGrip anchors="+grip.leftHand.name+", "+grip.rightHand.name+"\nQuaternion tracking from OVRCameraRig; FloorLevel; scale 1/3\n");
    }
    static Renderer Marker(Transform hand,string name,Color color){
        var existing=hand.Find(name);if(existing)return existing.GetComponent<Renderer>();
        var go=GameObject.CreatePrimitive(PrimitiveType.Capsule);go.name=name;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(hand,false);go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.Euler(65,0,0);go.transform.localScale=new Vector3(.065f,.065f,.065f);
        var material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));material.SetColor("_BaseColor",color);
        string path="Assets/ElUltimoFaro/"+name+".mat";var saved=AssetDatabase.LoadAssetAtPath<Material>(path);if(!saved){AssetDatabase.CreateAsset(material,path);saved=material;}else UnityEngine.Object.DestroyImmediate(material);
        go.GetComponent<Renderer>().sharedMaterial=saved;return go.GetComponent<Renderer>();
    }
    public static void Build(){
        File.WriteAllText(Root+"/build-status.txt","Building Android ARM64 Quest APK");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Scene},locationPathName=Root+"/ElUltimoFaro_Quest3S_Prueba01.apk",target=BuildTarget.Android,options=BuildOptions.Development});
        File.WriteAllText(Root+"/build-status.txt",report.summary.result+"\nErrors="+report.summary.totalErrors+"\nBytes="+report.summary.totalSize+"\nDuration="+report.summary.totalTime);
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Quest build: "+report.summary.result);
    }
}

