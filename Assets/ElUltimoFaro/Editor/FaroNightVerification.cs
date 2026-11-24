using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ElUltimoFaro;
[InitializeOnLoad]
public static class FaroNightVerification
{
 const string Flag="Faro.Night.Verify";static int phase;static double since;static MobilePlayerController player;static MobileInteraction items;
 static FaroNightVerification(){EditorApplication.playModeStateChanged+=Changed;}
 public static void Run(){FaroStageFiveTools.Open();QualitySettings.SetQualityLevel(0,true);SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();}
 static void Changed(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Flag,false)){phase=0;since=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;}}
 public static void Aim(MobilePlayerController p,Vector3 target)
 {
  var delta=target-Camera.main.transform.position;p.transform.rotation=Quaternion.Euler(0,Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg,0);
  var pitch=-Mathf.Atan2(delta.y,new Vector2(delta.x,delta.z).magnitude)*Mathf.Rad2Deg;
  typeof(MobilePlayerController).GetField("pitch",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(p,pitch);Camera.main.transform.localRotation=Quaternion.Euler(pitch,0,0);
 }
 static void Tick()
 {
  if(EditorApplication.timeSinceStartup-since<3)return;
  try{
   switch(phase){
    case 0:player=UnityEngine.Object.FindAnyObjectByType<MobilePlayerController>();items=player.GetComponent<MobileInteraction>();Application.runInBackground=true;player.SendMessage("OnApplicationFocus",true);break;
    case 1:Capture("stage5-night-start.png");items.VisitLanternTest();Aim(player,items.Lantern.VisualBounds.center);break;
    case 2:items.TakeNearby();items.ToggleLantern();if(!items.Lantern.IsLit)throw new Exception("Lantern unavailable at night");player.ReturnToStart();break;
    case 3:Capture("stage5-night-lantern.png");UnityEngine.Object.FindAnyObjectByType<BoatController>().ResetForTest();Aim(player,new Vector3(-5,0,-8));break;
    case 4:Capture("stage5-night-dock.png");Debug.Log("FARO_NIGHT_PASS");End(0);return;
   }
   phase++;since=EditorApplication.timeSinceStartup;
  }catch(Exception e){Debug.LogException(e);End(1);}
 }
 public static void Capture(string name)
 {
  var c=Camera.main;var canvas=UnityEngine.Object.FindAnyObjectByType<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=c;canvas.planeDistance=.2f;Canvas.ForceUpdateCanvases();
  var rt=new RenderTexture(1600,900,24);rt.Create();RenderPipeline.SubmitRenderRequest(c,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});
  var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(Path.GetFullPath("../"+name),tex.EncodeToPNG());
  RenderTexture.active=old;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(tex);canvas.renderMode=RenderMode.ScreenSpaceOverlay;
 }
 static void End(int code){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;EditorApplication.Exit(code);}
}
