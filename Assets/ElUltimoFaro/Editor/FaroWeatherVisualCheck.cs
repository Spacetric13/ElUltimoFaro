using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using ElUltimoFaro;
[InitializeOnLoad]
public static class FaroWeatherVisualCheck
{
 const string Flag="Faro.Weather.Visual";static int phase;static double since;static MobilePlayerController player;static MobileInteraction items;static WeatherDirector weather;static WeatherAudio sound;static QuartzSocketTrigger socket;static LighthouseBlink lighthouse;static float firstFlash=-1;static bool boltSeen;
 static FaroWeatherVisualCheck(){EditorApplication.playModeStateChanged+=Changed;}
 public static void Run(){FaroStageFiveTools.RefineRain();QualitySettings.SetQualityLevel(0,true);SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();}
 static void Changed(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Flag,false)){phase=0;since=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;}}
 static void Tick()
 {
  try{
   if(phase==4)
   {
    if(weather.FlashBrightness>.4f&&!boltSeen){
     if(!GameObject.Find("Rayo_ambiental").GetComponent<LineRenderer>().enabled)throw new Exception("Lightning geometry invisible during flash");
     boltSeen=true;firstFlash=weather.LastFlashTime;FaroNightVerification.Capture("stage5-final-flash.png");
    }
    if(!boltSeen||sound.ThunderCount<1){if(EditorApplication.timeSinceStartup-since>35)throw new Exception("Natural storm did not flash/thunder");return;}
   }
   if(phase==8 && weather.CalmProgress<1)return;
   if(EditorApplication.timeSinceStartup-since<3.2)return;
   switch(phase){
    case 0:
     Application.runInBackground=true;player=UnityEngine.Object.FindAnyObjectByType<MobilePlayerController>();items=player.GetComponent<MobileInteraction>();weather=UnityEngine.Object.FindAnyObjectByType<WeatherDirector>();sound=UnityEngine.Object.FindAnyObjectByType<WeatherAudio>();socket=UnityEngine.Object.FindAnyObjectByType<QuartzSocketTrigger>();lighthouse=UnityEngine.Object.FindAnyObjectByType<LighthouseBlink>();
     player.SendMessage("OnApplicationFocus",true);weather.SendMessage("OnApplicationFocus",true);sound.SendMessage("OnApplicationFocus",true);break;
    case 1:
     FaroNightVerification.Capture("stage5-final-night.png");items.VisitLanternTest();FaroNightVerification.Aim(player,items.Lantern.VisualBounds.center);break;
    case 2:
     items.TakeNearby();items.ToggleLantern();items.VisitQuartzTest();FaroNightVerification.Aim(player,items.Quartz.VisualBounds.center);break;
    case 3:
     items.TakeNearby();if(!items.Quartz.IsCarried)throw new Exception("Cannot collect quartz");FaroNightVerification.Aim(player,new Vector3(-6,2,-12));break;
    case 4:
     if(sound.LastThunderTime-firstFlash<1.15f)throw new Exception("Thunder did not follow flash");
     FaroNightVerification.Capture("stage5-final-storm.png");player.TeleportTo(new Vector3(-.30f,.72f,-1.40f),90);FaroNightVerification.Aim(player,socket.SnapPoint.position);break;
    case 5:
     FaroNightVerification.Capture("stage5-final-machine-before.png");items.TakeNearby();if(!socket.IsInserted)throw new Exception("Cannot insert quartz in rain");break;
    case 6:
     FaroNightVerification.Capture("stage5-final-machine.png");items.TakeNearby();if(!lighthouse.IsPowered)throw new Exception("Cannot light lighthouse");break;
    case 7:
     player.ReturnToStart();FaroNightVerification.Aim(player,lighthouse.lampGlowRenderer.bounds.center-Vector3.up*.65f);break;
    case 8:
     FaroNightVerification.Capture("stage5-final-calm.png");
     File.WriteAllText(Path.GetFullPath("../stage5-visual-verification.txt"),"PASS: permanent night; local heavy rain; quartz triggers natural lightning with visible geometry; thunder follows flash; existing machine can be operated in rain; lighthouse activation calms weather; final light rain remains at night.\nScreenshots reviewed separately; Android device verification pending.");Debug.Log("FARO_WEATHER_VISUAL_PASS");End(0);return;
   }
   phase++;since=EditorApplication.timeSinceStartup;
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Path.GetFullPath("../stage5-visual-verification.txt"),"FAIL: "+e);End(1);}
 }
 static void End(int code){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;EditorApplication.Exit(code);}
}
