using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using ElUltimoFaro;
[InitializeOnLoad]
public static class FaroQuestCheck {
 static double until; static readonly StringBuilder errors=new StringBuilder();
 static FaroQuestCheck(){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
 static void Log(string message,string stack,LogType type){if(EditorApplication.isPlaying && (type==LogType.Error||type==LogType.Exception))errors.AppendLine(message);}
 static void Tick(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
  if(File.Exists("Temp/FaroQuestValidate.request")){
   File.Delete("Temp/FaroQuestValidate.request");var report=new StringBuilder();
   foreach(var group in new[]{BuildTargetGroup.Android,BuildTargetGroup.Standalone}){
    var issues=new System.Collections.Generic.List<UnityEngine.XR.OpenXR.Features.OpenXRFeature.ValidationRule>();
    var previous=EditorUserBuildSettings.selectedBuildTargetGroup; try { EditorUserBuildSettings.selectedBuildTargetGroup=group; UnityEditor.XR.OpenXR.OpenXRProjectValidation.GetCurrentValidationIssues(issues,group); } finally { EditorUserBuildSettings.selectedBuildTargetGroup=previous; }
    foreach(var issue in issues)report.AppendLine(group+" | "+(issue.error?"ERROR":"WARN")+" | "+issue.message+" | Fix: "+issue.fixItMessage);
   }
   File.WriteAllText("Temp/FaroQuestValidate.result",report.Length==0?"No validation issues":report.ToString());
  }
  if(File.Exists("Temp/FaroQuestCheck.request")&&!EditorApplication.isPlaying){
   File.Delete("Temp/FaroQuestCheck.request");SessionState.SetBool("FaroQuestCheck",true);EditorApplication.isPlaying=true;return;
  }
  if(!SessionState.GetBool("FaroQuestCheck",false)||!EditorApplication.isPlaying)return;
  if(until==0){until=EditorApplication.timeSinceStartup+12;return;}
  if(EditorApplication.timeSinceStartup<until)return;
  var bridge=UnityEngine.Object.FindAnyObjectByType<QuestPlayerBridge>();
  var sb=new StringBuilder();sb.AppendLine("Scene: "+UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
  sb.AppendLine("Bridge active: "+(bridge&&bridge.isActiveAndEnabled));
  sb.AppendLine("Cameras: "+string.Join(",",UnityEngine.Object.FindObjectsByType<Camera>().Where(c=>c.enabled).Select(c=>c.name)));
  sb.AppendLine("Audio listeners: "+UnityEngine.Object.FindObjectsByType<AudioListener>().Count(c=>c.enabled));
  sb.AppendLine("XR loaded: "+(XRGeneralSettings.Instance?.Manager?.activeLoader?.name??"No device/runtime"));
  foreach(var group in new[]{BuildTargetGroup.Android,BuildTargetGroup.Standalone}){
   var xr=OpenXRSettings.GetSettingsForBuildTargetGroup(group);
   sb.AppendLine(group+" features: "+string.Join(",",xr.GetFeatures().Where(f=>f.enabled).Select(f=>f.GetType().Name)));
  }
  sb.AppendLine("Runtime errors:\n"+errors);File.WriteAllText("Temp/FaroQuestCheck.result",sb.ToString());
  SessionState.SetBool("FaroQuestCheck",false);until=0;EditorApplication.isPlaying=false;
 }
}


