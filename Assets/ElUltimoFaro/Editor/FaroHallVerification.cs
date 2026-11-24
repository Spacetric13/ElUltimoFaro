using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using ElUltimoFaro;
[InitializeOnLoad]
public static class FaroHallVerification {
 static StringBuilder report=new StringBuilder();static double started,waitUntil,stageSince;static int phase;static string seenStage;static BoatController boat;static HallSequence hall;static bool capturedPhoto,capturedIsland;
 const string Out="Builds/Hall_Prueba04";
 static FaroHallVerification(){EditorApplication.update+=Tick;Application.logMessageReceived+=(m,s,t)=>{if(SessionState.GetBool("HallVerify",false)&&EditorApplication.isPlaying&&(t==LogType.Exception||t==LogType.Error)&&s.Contains("Assets/ElUltimoFaro/Runtime"))report.AppendLine("RUNTIME ERROR: "+m+"\n"+s);};}
 public static void Run(){
 EditorSceneManager.OpenScene("Assets/Scenes/Hall_Quest.unity");var session=UnityEngine.Object.FindAnyObjectByType<HallXRSession>();if(!session||!session.GetComponent<OVRManager>()||UnityEngine.Object.FindObjectsByType<OVRManager>().Length!=1)throw new Exception("Hall must have exactly one persistent XR manager");
 if(UnityEngine.Object.FindAnyObjectByType<HallSequence>().destination!="Isla_Quest")throw new Exception("Wrong Quest destination");
 EditorSceneManager.OpenScene("Assets/Scenes/Isla_Quest.unity");if(UnityEngine.Object.FindObjectsByType<OVRManager>().Length!=0||!UnityEngine.Object.FindAnyObjectByType<QuestPlayerBridge>())throw new Exception("Quest island must reuse session manager");File.WriteAllText(Out+"/xr-preflight.txt","PASS: one persistent manager in Quest hall, no duplicate in island, Quest bridge and destination present. Hardware tracking test remains pending.");
EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Hall_PC.unity",true),new EditorBuildSettingsScene("Assets/Scenes/SampleScene_PC.unity",true)};EditorSceneManager.OpenScene("Assets/Scenes/SampleScene_PC.unity");SessionState.SetBool("HallVerify",true);EditorApplication.EnterPlaymode();}
 static void Check(bool value,string message){if(!value)throw new Exception(message);report.AppendLine("PASS: "+message);File.WriteAllText(Out+"/verification-progress.txt",report.ToString());}
 static void Tick(){if(!SessionState.GetBool("HallVerify",false)||!EditorApplication.isPlaying)return;
  if(started==0){started=EditorApplication.timeSinceStartup;waitUntil=started+3;return;}if(EditorApplication.timeSinceStartup-started>120){Finish(new Exception("Timed out at phase "+phase+" / "+seenStage));return;}if(EditorApplication.timeSinceStartup<waitUntil)return;
  try{
   if(phase==0){
    var pc=UnityEngine.Object.FindAnyObjectByType<DesktopPlaytest>();boat=pc.boat;boat.SendMessage("OnApplicationFocus",true);pc.player.SendMessage("OnApplicationFocus",true);boat.ResetForTest();boat.Interact();Check(pc.player.IsOnBoat,"Board for coast regression");
    Drive(new Vector3(-.5f,0,3.5f));Drive(new Vector3(-5,0,3.5f));Drive(new Vector3(-6,0,-9));Drive(new Vector3(-10.2f,0,-14.5f));Drive(new Vector3(-10.35f,0,-15.05f));
    boat.SetInput(Vector2.up);for(int i=0;i<120;i++){boat.Simulate(.05f);Physics.SyncTransforms();}Check(Mathf.Abs(boat.Speed)<.01f,"Hull stops against actual quartz island coast");
    Vector3 pressed=boat.transform.position,forward=boat.transform.forward;boat.SetInput(QuestRowing.Mix(-1,-1));for(int i=0;i<70;i++){boat.Simulate(.05f);Physics.SyncTransforms();}boat.Stop();
    Check(Vector3.Dot(boat.transform.position-pressed,forward)<-.65f,"Reverse strokes free hull from coast without teleport or disabling collisions");float yaw=boat.transform.eulerAngles.y;boat.SetInput(Vector2.right);for(int i=0;i<24;i++)boat.Simulate(.05f);boat.Stop();Check(Mathf.Abs(Mathf.DeltaAngle(yaw,boat.transform.eulerAngles.y))>45,"Boat can turn after backing away");
    Check(QuestRowing.StrokeForce(Vector3.zero,Vector3.forward*.03f,.05f,true,true)<-.8f,"Quest reverse requires pushing oar away with trigger");Check(QuestRowing.StrokeForce(Vector3.forward*.03f,Vector3.zero,.05f,true,true)==0,"Reverse recovery does not propel forward");Check(QuestRowing.StrokeForce(Vector3.zero,Vector3.forward*.03f,.05f,false,true)==0,"Released oars produce no reverse force");
    phase=1;SceneManager.LoadScene("Hall_PC");waitUntil=EditorApplication.timeSinceStartup+2;return;
   }
   if(phase==1){hall=UnityEngine.Object.FindAnyObjectByType<HallSequence>();Check(hall!=null,"Hall loaded");Check(!hall.CanStart,"Photo cannot activate from room entrance");Capture("hall-entrada.png",hall.player.head.GetComponent<Camera>());
    var body=hall.player.GetComponent<CharacterController>();for(int i=0;i<19;i++)body.Move(Vector3.forward*.12f);hall.player.head.LookAt(hall.photo.position);Check(hall.CanStart,"Approach and look at photograph enables start");Capture("hall-foto.png",hall.player.head.GetComponent<Camera>());hall.introDuration=4;hall.Begin();hall.Begin();Check(hall.IsRunning&&!hall.player.CanMove,"One activation locks locomotion while preserving head tracking");phase=2;return;
   }
   if(phase==2){
    string stage=hall?hall.Stage:"Descargado";if(stage!=seenStage){seenStage=stage;stageSince=EditorApplication.timeSinceStartup;report.AppendLine("STAGE: "+stage);}
    if(stage=="Video"&&!capturedPhoto&&EditorApplication.timeSinceStartup-stageSince>1){Capture("hall-video.png",hall.player.head.GetComponent<Camera>());capturedPhoto=true;}
    if(stage=="Isla"&&!hall.VideoCompleted)throw new Exception("Video did not finish successfully"); if(stage=="Isla"&&!capturedIsland&&EditorApplication.timeSinceStartup-stageSince>1){Capture("hall-cinematica.png",hall.player.head.GetComponent<Camera>());capturedIsland=true;}
    if(!SceneManager.GetSceneByName("Hall_PC").isLoaded){var pc=UnityEngine.Object.FindAnyObjectByType<DesktopPlaytest>();Check(pc&&pc.player.enabled,"Gameplay controls restored after introduction");Check(UnityEngine.Object.FindObjectsByType<Camera>().Count(c=>c.isActiveAndEnabled)==1,"Exactly one gameplay camera after transition");Check(UnityEngine.Object.FindObjectsByType<AudioListener>().Count(c=>c.isActiveAndEnabled)==1,"Exactly one active audio listener");Check(UnityEngine.Object.FindAnyObjectByType<WeatherDirector>().Phase==WeatherDirector.WeatherPhase.HeavyRain,"Preview storm does not alter quest progression");Check(!pc.items.Quartz.IsCarried&&!pc.items.Quartz.IsInserted,"Quartz still collectible after introduction");Check(capturedPhoto&&capturedIsland,"Memory video completed and island cinematic rendered");Capture("hall-juego.png",pc.view);Finish(null);}
   }
  }catch(Exception e){Finish(e);}
 }
 static void Drive(Vector3 destination){bool arrived=false;for(int i=0;i<5000;i++){var delta=destination-boat.transform.position;delta.y=0;if(delta.magnitude<.1f){arrived=true;break;}float error=Vector3.SignedAngle(boat.transform.forward,delta,Vector3.up);boat.SetInput(new Vector2(Mathf.Clamp(error/35,-1,1),Mathf.Abs(error)<18?Mathf.Clamp(delta.magnitude*.5f,.03f,.85f):0));boat.Simulate(.05f);Physics.SyncTransforms();}boat.Stop();Check(arrived,"Navigation reaches "+destination);}
 static void Capture(string name,Camera camera){float oldAspect=camera.aspect;camera.aspect=1280f/720f;foreach(var fit in UnityEngine.Object.FindObjectsByType<DesktopVideoFit>())if(fit.view==camera)fit.Fit();var rt=new RenderTexture(1280,720,24);rt.Create();RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Out+"/"+name,image.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);rt.Release();UnityEngine.Object.DestroyImmediate(rt);camera.aspect=oldAspect;}
 static void Finish(Exception error){if(error!=null)report.AppendLine("FAIL: "+error);SessionState.SetBool("HallVerify",false);File.WriteAllText(Out+"/verification.txt",report.ToString());EditorApplication.Exit(error==null&&!report.ToString().Contains("RUNTIME ERROR")?0:1);}
}
