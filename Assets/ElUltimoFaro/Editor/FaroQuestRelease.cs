using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using ElUltimoFaro;
public static class FaroQuestRelease {
 const string Output="Builds/Quest_Final";
 [MenuItem("El Último Faro/Hall/Compilar Quest final")]
 public static void Build(){
  Directory.CreateDirectory(Output);var report=new StringBuilder();
  var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
  if(xr==null||!xr.InitManagerOnStart||xr.Manager==null||!xr.Manager.activeLoaders.Any(l=>l&&l.GetType().Name.Contains("OpenXR")))throw new Exception("Android OpenXR startup is not configured");
  report.AppendLine("Android OpenXR automatic startup: OK");
  var settings=OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
  report.AppendLine("Enabled XR features: "+string.Join(", ",settings.GetFeatures().Where(f=>f&&f.enabled).Select(f=>f.GetType().Name)));
  string[] scenes={"Assets/Scenes/Hall_Quest.unity","Assets/Scenes/Isla_Quest.unity"};
  foreach(string path in scenes){
   var scene=EditorSceneManager.OpenScene(path);
   foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new Exception("Missing script: "+t.name);
   if(path.Contains("Hall_")){
    var hall=UnityEngine.Object.FindAnyObjectByType<HallSequence>();
    if(!hall||!hall.memoryVideo||hall.player.desktop||hall.destination!="Isla_Quest")throw new Exception("Invalid VR hall/video");
    if(UnityEngine.Object.FindObjectsByType<OVRManager>().Length!=1||!UnityEngine.Object.FindAnyObjectByType<HallXRSession>())throw new Exception("Hall XR session invalid");
    report.AppendLine("Hall: VR player, memory video, persistent XR session and destination OK");
   }else{
    if(UnityEngine.Object.FindObjectsByType<OVRManager>().Length!=0)throw new Exception("Duplicate island XR manager");
    if(!UnityEngine.Object.FindAnyObjectByType<QuestPlayerBridge>()||!UnityEngine.Object.FindAnyObjectByType<QuestGrip>()||!UnityEngine.Object.FindAnyObjectByType<QuestRowing>()||!UnityEngine.Object.FindAnyObjectByType<WeatherDirector>())throw new Exception("Missing VR gameplay component");
    report.AppendLine("Island: movement, grip, rowing, weather and single XR session OK");
   }
  }
  EditorBuildSettings.scenes=scenes.Select(p=>new EditorBuildSettingsScene(p,true)).ToArray();
  PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.elultimofaro.quest");
  PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
  PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
  PlayerSettings.bundleVersion="1.0.0";PlayerSettings.Android.bundleVersionCode=100;
  EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.development=false;EditorUserBuildSettings.allowDebugging=false;EditorUserBuildSettings.connectProfiler=false;
  File.WriteAllText(Output+"/preflight.txt",report.ToString());AssetDatabase.SaveAssets();
  var build=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=Output+"/ElUltimoFaro_Quest_Final.apk",target=BuildTarget.Android,options=BuildOptions.None});
  File.WriteAllText(Output+"/build-result.txt",build.summary.result+"\nErrors="+build.summary.totalErrors+"\nBytes="+build.summary.totalSize+"\nDuration="+build.summary.totalTime);
  if(build.summary.result!=BuildResult.Succeeded)throw new Exception("Quest final build failed");
 }
}
