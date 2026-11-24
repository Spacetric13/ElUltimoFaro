using UnityEngine;
using UnityEngine.InputSystem;
namespace ElUltimoFaro {
 // Desktop harness for the same scene, item state and boat controller used on Quest.
 [DefaultExecutionOrder(200)]
 public sealed class DesktopPlaytest : MonoBehaviour {
  public MobilePlayerController player; public MobileInteraction items; public BoatController boat;
  public QuestRowing rowing; public Camera view; public MachineInteraction machine; public LighthouseDoor door;
  float leftPhase,rightPhase;bool showHelp=true;GUIStyle text,heading;
  void Awake(){player.enabled=true;player.externalBoatInput=true;Application.targetFrameRate=60;}
  System.Collections.IEnumerator Start(){
   // Optional automated launch check used for the desktop build, not a gameplay shortcut.
   string[] args=System.Environment.GetCommandLineArgs();int index=System.Array.IndexOf(args,"-capture-test");
   if(index>=0 && index+1<args.Length){yield return new WaitForSecondsRealtime(4);ScreenCapture.CaptureScreenshot(args[index+1]);yield return new WaitForSecondsRealtime(2);Application.Quit();}
  }
  void Update(){
   var k=Keyboard.current;if(k==null)return;
   if(k.hKey.wasPressedThisFrame)showHelp=!showHelp;
   if(k.escapeKey.wasPressedThisFrame){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
   if(k.digit1Key.wasPressedThisFrame)Drop(items.Quartz);
   if(k.digit2Key.wasPressedThisFrame)Drop(items.Lantern);
   if(k.f2Key.wasPressedThisFrame)boat.ResetForTest();
   bool free=!items.Quartz.IsCarried&&!items.Lantern.IsCarried;
   float l=Stroke(k.zKey.isPressed&&free&&player.IsOnBoat,ref leftPhase,Time.deltaTime);
   float r=Stroke(k.cKey.isPressed&&free&&player.IsOnBoat,ref rightPhase,Time.deltaTime);
   if(k.leftShiftKey.isPressed||k.rightShiftKey.isPressed){l=-l;r=-r;}
   if(player.IsOnBoat)boat.SetInput(QuestRowing.Mix(l,r));
   rowing.leftOar.localRotation=Quaternion.Euler(0,Mathf.Sin(leftPhase)*25,0);
   rowing.rightOar.localRotation=Quaternion.Euler(0,-Mathf.Sin(rightPhase)*25,0);
  }
  public static float Stroke(bool held,ref float phase,float dt){
   if(!held){phase=0;return 0;}float before=Mathf.Cos(phase)*.10f;phase+=dt*5;float after=Mathf.Cos(phase)*.10f;
   return QuestRowing.Pull(new Vector3(0,0,before),new Vector3(0,0,after),dt,true);
  }
  public void Drop(PortableItem item){
   if(!item.IsCarried)return;
   if(player.IsOnBoat){rowing.Stow(item);return;}
   var position=player.transform.position+Vector3.ProjectOnPlane(view.transform.forward,Vector3.up).normalized*.24f;
   if(Physics.Raycast(position+Vector3.up*.3f,Vector3.down,out var hit,.65f,1<<8,QueryTriggerInteraction.Ignore))item.ReleaseAt(hit.point,Quaternion.identity);
  }
  void OnGUI(){
   if(!showHelp)return;
   if(text==null){text=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true};text.normal.textColor=new Color(1,.95f,.83f);heading=new GUIStyle(text){fontSize=22,fontStyle=FontStyle.Bold};}
   float scale=Mathf.Clamp(Screen.height/800f,.7f,1.6f);GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
   GUI.Box(new Rect(15,15,560,190),"");GUI.Label(new Rect(30,25,530,30),"EL ÚLTIMO FARO · PRUEBA PC",heading);
   GUI.Label(new Rect(30,60,530,140),"WASD: caminar · Botón derecho + ratón: mirar\nE: recoger / activar · F: encender farol · B: embarcar / salir\nZ / C: remar izquierda / derecha (Shift + Z/C: retroceder)\n1: dejar cuarzo · 2: dejar farol · H: ocultar ayuda\nF2: volver al muelle (prueba) · R: volver al inicio",text);
   string action=player.IsOnBoat?(items.Quartz.IsCarried||items.Lantern.IsCarried?"Deja los objetos con 1 / 2 para liberar las manos":boat.AvailableLanding()?"B · DESEMBARCAR":boat.LandingHint):items.Candidate()?"E · RECOGER "+items.Candidate().DisplayName:items.DoorIsCandidate()?"E · "+door.ActionLabel:machine.InReach()?"E · "+machine.ActionLabel:boat.CanBoard()?"B · EMBARCAR":"Acércate y mira los objetos para interactuar";
   float bottom=Screen.height/scale-105;GUI.Box(new Rect(15,bottom,680,90),"");GUI.Label(new Rect(30,bottom+8,650,75),action+"\nCuarzo: "+(items.Quartz.IsInserted?"insertado":items.Quartz.IsCarried?"en mano":"en el mundo / bote")+" · Faro: "+(machine.MissionCompleted?"encendido":"pendiente"),text);
  }
 }
}
