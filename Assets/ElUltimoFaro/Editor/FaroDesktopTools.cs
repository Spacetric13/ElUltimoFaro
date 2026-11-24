using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using ElUltimoFaro;
[InitializeOnLoad]
public static class FaroDesktopTools {
 const string Output="Builds/Quest_PC_Prueba03";
 const string PC="Assets/Scenes/SampleScene_PC.unity";
 static double until;static StringBuilder log=new StringBuilder();static BoatController boat;
 static FaroDesktopTools(){EditorApplication.update+=Tick;}
 static void Ref(UnityEngine.Object obj,string name,UnityEngine.Object value){var s=new SerializedObject(obj);s.FindProperty(name).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
 [MenuItem("El Último Faro/Prueba PC/Abrir escena de prueba")]
 public static void Open(){var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);if(xr){xr.InitManagerOnStart=false;EditorUtility.SetDirty(xr);AssetDatabase.SaveAssets();}EditorSceneManager.OpenScene(PC);}
 public static void Prepare(){
  Directory.CreateDirectory(Output);EditorSceneManager.OpenScene("Assets/Scenes/SampleScene_Quest.unity");
  var bridge=UnityEngine.Object.FindAnyObjectByType<QuestPlayerBridge>();var p=bridge.player;
  // Keep the native Quest scene; build a separate desktop entry point from the same content.
  EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),PC);
  var rig=bridge.head.GetComponentInParent<OVRCameraRig>();
  var camera=new GameObject("Camara_PC",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.transform.SetParent(p.transform,false);camera.transform.localPosition=new Vector3(0,.516667f,0);
  EditorUtility.CopySerialized(bridge.head.GetComponent<Camera>(),camera.GetComponent<Camera>());camera.GetComponent<Camera>().enabled=true;camera.GetComponent<Camera>().stereoTargetEye=StereoTargetEyeMask.None;
  var extra=camera.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();var oldExtra=bridge.head.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();if(oldExtra)EditorUtility.CopySerialized(oldExtra,extra);
  foreach(var listener in UnityEngine.Object.FindObjectsByType<AudioListener>())listener.enabled=listener.gameObject==camera;
  var items=bridge.interaction;var serial=new SerializedObject(items);
  foreach(var pair in new[]{("quartzHand",-.12f),("lanternHand",.12f)}){var anchor=(Transform)serial.FindProperty(pair.Item1).objectReferenceValue;anchor.SetParent(camera.transform,false);anchor.localPosition=new Vector3(pair.Item2,-.12f,.24f);anchor.localScale=Vector3.one;anchor.localRotation=Quaternion.identity;}
  Ref(p,"view",camera.transform);Ref(items,"view",camera.GetComponent<Camera>());
  var ps=new SerializedObject(p);ps.FindProperty("moveSpeed").floatValue=.9f;ps.ApplyModifiedPropertiesWithoutUndo();p.enabled=true;p.externalBoatInput=true;
  var machine=UnityEngine.Object.FindAnyObjectByType<MachineInteraction>();var door=UnityEngine.Object.FindAnyObjectByType<LighthouseDoor>();Ref(machine,"view",camera.GetComponent<Camera>());Ref(door,"view",camera.GetComponent<Camera>());
  var row=bridge.rowing;row.enabled=false;bridge.enabled=false;bridge.GetComponent<QuestGrip>().enabled=false;
  var prompts=bridge.GetComponent<QuestPrompts>();prompts.canvas.enabled=false;prompts.enabled=false;rig.gameObject.SetActive(false);
  var test=p.gameObject.AddComponent<DesktopPlaytest>();test.player=p;test.items=items;test.boat=bridge.boat;test.rowing=row;test.view=camera.GetComponent<Camera>();test.machine=machine;test.door=door;
  var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);if(xr){xr.InitManagerOnStart=false;EditorUtility.SetDirty(xr);}
  EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
  SessionState.SetBool("FaroDesktopTest",true);EditorApplication.EnterPlaymode();
 }
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);log.AppendLine("PASS: "+message);}
 static void Tick(){
  if(!SessionState.GetBool("FaroDesktopTest",false)||!EditorApplication.isPlaying)return;
  if(until==0){until=EditorApplication.timeSinceStartup+5;return;}if(EditorApplication.timeSinceStartup<until)return;
  SessionState.SetBool("FaroDesktopTest",false);bool pass=false;
  try{
   var pc=UnityEngine.Object.FindAnyObjectByType<DesktopPlaytest>();boat=pc.boat;var p=pc.player;boat.SendMessage("OnApplicationFocus",true);p.SendMessage("OnApplicationFocus",true);
   Check(Camera.main==pc.view && UnityEngine.Object.FindObjectsByType<Camera>().Count(c=>c.isActiveAndEnabled)==1,"One desktop camera, no headset required");
   // Emulate an off-centre headset with a yawed tracking origin at the real miniature scale.
   var origin=new GameObject("SyntheticTrackingOrigin").transform;origin.SetParent(p.transform,false);origin.localScale=Vector3.one/3;var head=new GameObject("SyntheticHead").transform;head.SetParent(origin,false);head.localPosition=new Vector3(.9f,1.6f,-.45f);head.localRotation=Quaternion.Euler(0,42,0);p.ConfigureTrackedView(head,origin);
   boat.ResetForTest();boat.Interact();Check(p.IsOnBoat,"Board at main dock");
   Check(Vector3.ProjectOnPlane(head.position-boat.Seat.position,Vector3.up).magnitude<.001f,"Off-centre tracked head is aligned with seat");Check(Mathf.Abs(Mathf.DeltaAngle(head.eulerAngles.y,boat.Seat.eulerAngles.y))<.01f,"Tracked heading aligned on boarding");p.ConfigureTrackedView(null,null);UnityEngine.Object.DestroyImmediate(origin.gameObject);
   float phase=0,total=0;for(int i=0;i<120;i++)total+=DesktopPlaytest.Stroke(true,ref phase,1f/60);Check(total>10,"Keyboard stroke simulator produces real rowing input");Check(DesktopPlaytest.Stroke(false,ref phase,.016f)==0,"Release stops simulated rowing");
   Drive(new Vector3(-.5f,0,3.5f));Drive(new Vector3(-5,0,3.5f));Drive(new Vector3(-6,0,-9));Drive(new Vector3(-10.2f,0,-14.5f));Drive(new Vector3(-10.35f,0,-15.05f));
   log.AppendLine("Secondary port="+boat.Landings[1].position+" boat="+boat.transform.position+" hint="+boat.LandingHint);
   boat.SetInput(Vector2.up);Check(boat.AvailableLanding()==boat.Landings[1],"Stopped hull can disembark despite residual oar input");
   boat.Interact();Check(!p.IsOnBoat&&p.HasGround(p.transform.position),"Disembark on grounded secondary island");
   pc.view.transform.LookAt(pc.items.Quartz.VisualBounds.center);pc.items.TakeNearby();Check(pc.items.Quartz.IsCarried,"Collect original quartz on secondary island");
   Check(boat.CanBoard(),"Can reboard from secondary island");boat.Interact();pc.Drop(pc.items.Quartz);Check(pc.items.Quartz.transform.parent==pc.rowing.quartzCargo,"Store quartz in boat using desktop controls");
   // Turn in place, using the existing collision protection, then row back out.
   Drive(new Vector3(-10.2f,0,-13.8f));Drive(new Vector3(-6,0,-9));Drive(new Vector3(-5,0,3.5f));Drive(new Vector3(-.5f,0,3.5f));Drive(boat.HomePosition);
   Check(boat.AvailableLanding()==boat.Landings[0],"Return to main dock without teleport");
   pc.view.transform.LookAt(pc.items.Quartz.VisualBounds.center);pc.items.TakeNearby();Check(pc.items.Quartz.IsCarried,"Retrieve cargo by ordinary interaction");boat.Interact();Check(!p.IsOnBoat&&p.HasGround(p.transform.position),"Disembark at main dock with quartz");
   // A solid obstacle at the landing must still be rejected.
   var barrier=GameObject.CreatePrimitive(PrimitiveType.Cube);barrier.transform.position=boat.Landings[0].position+Vector3.up*.3f;barrier.transform.localScale=Vector3.one*.5f;Physics.SyncTransforms();Check(!p.TryLandingFeet(boat.Landings[0].position,out _),"Obstructed landing rejected");UnityEngine.Object.DestroyImmediate(barrier);
   pass=true;
  }catch(Exception e){log.AppendLine("FAIL: "+e);}
  File.WriteAllText(Output+"/verification.txt",log.ToString());EditorApplication.Exit(pass?0:1);
 }
 static void Drive(Vector3 destination){
  bool arrived=false;
  for(int i=0;i<5000;i++){
   Vector3 d=destination-boat.transform.position;d.y=0;if(d.magnitude<.1f){arrived=true;break;}
   float error=Vector3.SignedAngle(boat.transform.forward,d,Vector3.up);boat.SetInput(new Vector2(Mathf.Clamp(error/35,-1,1),Mathf.Abs(error)<18?Mathf.Clamp(d.magnitude*.5f,.03f,.85f):0));boat.Simulate(.05f);Physics.SyncTransforms();
  }
  boat.Stop();Check(arrived,"Free navigation reaches "+destination+" actual="+boat.transform.position);
 }
 public static void CleanPC(){
  // Embedded package test features must never enter a player build.
  foreach(var group in new[]{BuildTargetGroup.Standalone,BuildTargetGroup.Android}){
   var settings=UnityEngine.XR.OpenXR.OpenXRSettings.GetSettingsForBuildTargetGroup(group);if(!settings)continue;
   var serialized=new SerializedObject(settings);var features=serialized.FindProperty("features");
   for(int i=features.arraySize-1;i>=0;i--){var feature=features.GetArrayElementAtIndex(i).objectReferenceValue;
    if(!feature || feature.GetType().Namespace.Contains(".Tests")){features.GetArrayElementAtIndex(i).objectReferenceValue=null;features.DeleteArrayElementAtIndex(i);}
   }serialized.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(settings);
  }
  var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
  if(xr){xr.InitManagerOnStart=false;xr.Manager.TrySetLoaders(new System.Collections.Generic.List<XRLoader>());EditorUtility.SetDirty(xr.Manager);EditorUtility.SetDirty(xr);}
  AssetDatabase.SaveAssets();

  }
 public static void BuildPC(){
  CleanPC();string bindings=Output+"/Juego_PC/RuntimeActionBindings.json";if(File.Exists(bindings))File.Delete(bindings);
  PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{PC},target=BuildTarget.StandaloneWindows64,locationPathName=Output+"/Juego_PC/ElUltimoFaro_PC.exe",options=BuildOptions.Development});File.WriteAllText(Output+"/build-status.txt",report.summary.result+"\nErrors="+report.summary.totalErrors);if(report.summary.result!=BuildResult.Succeeded)throw new Exception("PC build failed");
 }
}
