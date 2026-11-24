using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEditor.XR.OpenXR.Features;
using ElUltimoFaro;

[InitializeOnLoad]
public static class FaroQuestSetup
{
    const string ScenePath="Assets/Scenes/SampleScene_Quest.unity";
    static FaroQuestSetup(){EditorApplication.update+=Poll;}
    static void Poll(){
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists("Temp/FaroQuestSetup.request"))return;
        File.Delete("Temp/FaroQuestSetup.request");
        try{Setup();}catch(Exception ex){File.WriteAllText("Temp/FaroQuestSetup.result",ex.ToString());Debug.LogException(ex);}
    }
    [MenuItem("El Último Faro/Quest/Configurar escena Quest")]
    public static void Setup(){
        var scene=EditorSceneManager.GetActiveScene();
        if(scene.path!=ScenePath)throw new Exception("Abre SampleScene_Quest antes de configurar.");
        var player=UnityEngine.Object.FindFirstObjectByType<MobilePlayerController>();
        var rig=UnityEngine.Object.FindFirstObjectByType<OVRCameraRig>();
        if(!player || !rig)throw new Exception("Falta Player o Camera Rig.");
        var oldCamera=player.GetComponentsInChildren<Camera>(true).FirstOrDefault(c=>!c.GetComponentInParent<OVRCameraRig>());
        if(!oldCamera)oldCamera=player.GetComponentsInChildren<Camera>(true).FirstOrDefault(c=>c.name=="Main Camera");
        var head=rig.centerEyeAnchor;
        if(!head)head=rig.transform.Find("TrackingSpace/CenterEyeAnchor");
        var camera=head.GetComponent<Camera>();
        rig.transform.SetParent(player.transform,false);
        rig.transform.localPosition=Vector3.zero; rig.transform.localRotation=Quaternion.identity;rig.transform.localScale=Vector3.one/3f;
        rig.GetComponent<OVRManager>().trackingOriginType=OVRManager.TrackingOrigin.FloorLevel;
        camera.enabled=true;camera.tag="MainCamera";camera.nearClipPlane=.015f;camera.farClipPlane=300;
        // Retain phone camera's carry anchors and references, now following the tracked head.
        if(oldCamera && oldCamera!=camera){
            oldCamera.enabled=false;oldCamera.tag="Untagged";
            oldCamera.transform.SetParent(head,false);
            oldCamera.transform.localPosition=Vector3.zero;oldCamera.transform.localRotation=Quaternion.identity;oldCamera.transform.localScale=Vector3.one*3;
            var listener=oldCamera.GetComponent<AudioListener>();if(listener)listener.enabled=false;
        }
        foreach(var listener in UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include,FindObjectsSortMode.None))listener.enabled=listener.transform==head;
        if(!head.GetComponent<AudioListener>())head.gameObject.AddComponent<AudioListener>();
        foreach(var behaviour in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
            if(!behaviour || behaviour is MobilePlayerController)continue;
            var so=new SerializedObject(behaviour);var p=so.GetIterator();bool changed=false;
            while(p.NextVisible(true))if(p.propertyType==SerializedPropertyType.ObjectReference && oldCamera && p.objectReferenceValue==oldCamera){p.objectReferenceValue=camera;changed=true;}
            if(changed)so.ApplyModifiedPropertiesWithoutUndo();
        }
        player.enabled=false;
        var bridge=player.GetComponent<QuestPlayerBridge>();if(!bridge)bridge=player.gameObject.AddComponent<QuestPlayerBridge>();
        bridge.player=player;bridge.head=head;bridge.interaction=UnityEngine.Object.FindFirstObjectByType<MobileInteraction>();bridge.boat=UnityEngine.Object.FindFirstObjectByType<BoatController>();
        // Keep UI objects alive for existing logic but do not draw phone overlays in VR.
        var controls=GameObject.Find("Android_Controls");if(controls)foreach(var canvas in controls.GetComponentsInChildren<Canvas>(true))canvas.enabled=false;
        ConfigureXR(BuildTargetGroup.Android);ConfigureXR(BuildTargetGroup.Standalone);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel32;
        EditorUtility.SetDirty(bridge);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        string result="OK: "+scene.path+"\nRig="+rig.name+"; scale="+rig.transform.localScale+"; camera="+camera.name+"; phone controller disabled="+!player.enabled;
        result+="\nEnabled cameras="+string.Join(",",UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Where(c=>c.enabled).Select(c=>c.name));
        File.WriteAllText("Temp/FaroQuestSetup.result",result);Debug.Log(result);
    }
    static void ConfigureXR(BuildTargetGroup group){
        if(!EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(XRGeneralSettings.k_SettingsKey,out var perTarget)){
            Directory.CreateDirectory("Assets/XR");AssetDatabase.Refresh();perTarget=ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(perTarget,"Assets/XR/XRGeneralSettingsPerBuildTarget.asset");EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey,perTarget,true);
        }
        if(!perTarget.HasManagerSettingsForBuildTarget(group))perTarget.CreateDefaultManagerSettingsForBuildTarget(group);
        var settings=perTarget.SettingsForBuildTarget(group);settings.InitManagerOnStart=true;
        const string path="Assets/XR/FaroOpenXRLoader.asset";
        var loader=AssetDatabase.LoadAssetAtPath<OpenXRLoader>(path);
        if(!loader){loader=ScriptableObject.CreateInstance<OpenXRLoader>();AssetDatabase.CreateAsset(loader,path);}
        if(!settings.Manager.activeLoaders.Contains(loader) && !settings.Manager.TryAddLoader(loader))throw new Exception("No se pudo agregar OpenXR a "+group);
        FeatureHelpers.RefreshFeatures(group);
        var open=OpenXRSettings.GetSettingsForBuildTargetGroup(group);
        foreach(var feature in open.GetFeatures()){
            string name=feature.GetType().Name;
            if(name=="OculusTouchControllerProfile" || (group==BuildTargetGroup.Android && name=="MetaQuestFeature")){feature.enabled=true;EditorUtility.SetDirty(feature);}
        }
        EditorUtility.SetDirty(open);EditorUtility.SetDirty(settings);EditorUtility.SetDirty(settings.Manager);EditorUtility.SetDirty(perTarget);
    }
}
