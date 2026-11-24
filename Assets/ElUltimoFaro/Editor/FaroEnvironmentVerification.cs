using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using ElUltimoFaro;
[InitializeOnLoad]
public static class FaroEnvironmentVerification {
 const string Flag="Faro.Stage6.Environment";static int phase;static double since;
 static MobilePlayerController player;static MobileInteraction items;static LighthouseDoor door;static Transform tower;static TouchMovePad pad;static Vector3 target;
 static SeagullFlight[] birds;static Vector3[] birdStart;static VegetationSway[] plants;static Vector3[][] windStart;static float birdClock;static RainShelter shelter;static GameObject obstacle;
 static readonly StringBuilder report=new StringBuilder();
 static FaroEnvironmentVerification(){EditorApplication.playModeStateChanged+=Changed;}
 public static void Run(){FaroStageSixTools.RefineEntrance();QualitySettings.SetQualityLevel(0,true);SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();}
 static void Changed(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Flag,false)){phase=0;since=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;}}
 static void Check(bool value,string text){if(!value)throw new Exception(text);report.AppendLine("PASS: "+text);Debug.Log("STAGE6_ENV_PASS "+text);}
 static void Next(){phase++;since=EditorApplication.timeSinceStartup;}
 static void AimDoor(){door.SendMessage("OnApplicationFocus",true);FaroNightVerification.Aim(player,door.Leaf.GetComponent<Renderer>().bounds.center);}
 static void Walk(Vector3 destination){target=destination;pad.ResetInput();pad.OnPointerDown(new PointerEventData(EventSystem.current){pointerId=81,position=RectTransformUtility.WorldToScreenPoint(null,pad.transform.TransformPoint(new Vector3(0,65,0)))});}
 static bool Walking(){
  Vector3 d=target-player.transform.position;d.y=0;if(d.magnitude<.045f){pad.ResetInput();return false;}
  if(pad.Value==Vector2.zero){player.SendMessage("OnApplicationFocus",true);Walk(target);}
  player.transform.rotation=Quaternion.LookRotation(d);
  if(EditorApplication.timeSinceStartup-since>16){
   var body=player.GetComponent<CharacterController>();var center=player.transform.TransformPoint(body.center);var half=Vector3.up*(body.height*.5f-body.radius);var direction=d.normalized;
   Debug.Log("ENTRY_STATE local="+tower.InverseTransformPoint(player.transform.position)+" pad="+pad.Value+" groundedAhead="+player.HasGround(player.transform.position+direction*.1f)+" body="+body.height+" center="+body.center+" step3="+GameObject.Find("Step_3").GetComponent<Collider>().enabled);
   foreach(var hit in Physics.CapsuleCastAll(center-half,center+half,body.radius,direction,.15f,~(1<<9),QueryTriggerInteraction.Ignore))Debug.Log("ENTRY_CAPSULE "+hit.transform.name+" distance="+hit.distance+" normal="+hit.normal);
   foreach(var hit in Physics.RaycastAll(player.transform.position+direction*.1f+Vector3.up*.2f,Vector3.down,.4f,1<<8))Debug.Log("ENTRY_GROUND "+hit.transform.name+" normal="+hit.normal);
   FaroNightVerification.Capture("stage6-entry-blocked.png");
   throw new Exception("Interior walk blocked at "+player.transform.position+" target="+target);
  }
  return true;
 }
 static void Tick(){try{
  Application.runInBackground=true;
  if(phase==3||phase==7){if(Walking())return;Check(player.HasGround(player.transform.position),phase==3?"Walked through doorway into sheltered interior":"Walked out safely without teleport");Next();return;}
  if(EditorApplication.timeSinceStartup-since<3)return;
  if(door&&door.IsMoving&&phase!=9&&EditorApplication.timeSinceStartup-since<12)return;
  switch(phase){
   case 0:
    player=UnityEngine.Object.FindAnyObjectByType<MobilePlayerController>();items=player.GetComponent<MobileInteraction>();door=UnityEngine.Object.FindAnyObjectByType<LighthouseDoor>();tower=GameObject.Find("El_Ultimo_Faro_LowPoly").transform;pad=UnityEngine.Object.FindAnyObjectByType<TouchMovePad>();shelter=UnityEngine.Object.FindAnyObjectByType<RainShelter>();
    birds=UnityEngine.Object.FindObjectsByType<SeagullFlight>(FindObjectsSortMode.None);plants=UnityEngine.Object.FindObjectsByType<VegetationSway>(FindObjectsSortMode.None).Where(p=>p.name.StartsWith("Arbol")).ToArray();
    Check(birds.Length==2&&plants.Length==11,"Both existing gulls and all eleven existing trees configured");
    birdStart=birds.Select(b=>b.transform.position).ToArray();foreach(var bird in birds)bird.SendMessage("OnApplicationFocus",true);
    windStart=plants.Select(p=>p.Parts[0].filter.sharedMesh.vertices).ToArray();
    player.SendMessage("OnApplicationFocus",true);items.VisitLanternTest();FaroNightVerification.Aim(player,items.Lantern.VisualBounds.center);items.TakeNearby();items.ToggleLantern();Check(items.Lantern.IsLit,"Existing lantern lights the doorway and interior");player.TeleportTo(tower.TransformPoint(new Vector3(0,.70f,-4.6f)),tower.eulerAngles.y);AimDoor();break;
   case 1:
    Check(door.InReach(),"Door reachable from front steps");FaroNightVerification.Capture("stage6-door-closed.png");items.TakeNearby();break;
   case 2:
    Check(door.IsOpen&&door.Angle<=100.1f,"Context action opens original door smoothly to hinge limit; angle="+door.Angle+" blocked="+door.BlockedCount);AimDoor();FaroNightVerification.Capture("stage6-door-open.png");Walk(tower.TransformPoint(new Vector3(0,.64f,-.3f)));break;
   case 4:
    Check(shelter.IsSheltered,"Interior shelter detects player");
    var ps=shelter.GetComponent<ParticleSystem>();var drops=new ParticleSystem.Particle[ps.main.maxParticles];int count=ps.GetParticles(drops);
    Check(!drops.Take(count).Any(d=>d.remainingLifetime>0&&shelter.Contains(d.position)),"Rain is removed under the interior roof");
    AimDoor();FaroNightVerification.Capture("stage6-interior.png");Check(door.InReach(),"Door reachable from inside");items.TakeNearby();break;
   case 5:
    Check(door.Angle<.2f,"Door closes from inside");AimDoor();items.TakeNearby();break;
   case 6:
    Check(door.IsOpen,"Door reopens from inside without trapping player");Walk(tower.TransformPoint(new Vector3(0,.70f,-4.6f)));break;
   case 8:
    Check(!shelter.IsSheltered,"Leaving interior restores outdoor ambience");
    obstacle=new GameObject("Prueba_obstaculo_puerta");var capsule=obstacle.AddComponent<CapsuleCollider>();capsule.radius=.0667f;capsule.height=.5667f;obstacle.transform.position=tower.TransformPoint(new Vector3(.18f,1.955f,-2.68f));Physics.SyncTransforms();AimDoor();items.TakeNearby();break;
   case 9:
    Check(door.BlockedCount>0&&door.Angle>1,"Closing door stops before a person-sized obstacle");
    var leaf=door.Leaf.GetComponent<BoxCollider>();var cap=obstacle.GetComponent<CapsuleCollider>();
    Check(!Physics.ComputePenetration(leaf,leaf.transform.position,leaf.transform.rotation,cap,cap.transform.position,cap.transform.rotation,out _,out _),"Stopped door does not intersect the obstacle");
    UnityEngine.Object.DestroyImmediate(obstacle);Physics.SyncTransforms();AimDoor();items.TakeNearby();break;
   case 10:
    Check(door.Angle<.2f,"Door can close after removing obstruction");
    for(int j=0;j<birds.Length;j++){
     Check(Vector3.Distance(birdStart[j],birds[j].transform.position)>.3f,"Existing gull moves along its route: "+birds[j].name);
     Check(birds[j].CallCount>0,"Gull emits occasional positional sound: "+birds[j].name);
    }
    float largest=0;int fixedVertices=0;
    for(int j=0;j<plants.Length;j++){
     var part=plants[j].Parts[0];var now=part.filter.sharedMesh.vertices;
     for(int i=0;i<now.Length;i++){
      Vector3 a=part.filter.transform.TransformPoint(windStart[j][i]),b=part.filter.transform.TransformPoint(now[i]);largest=Mathf.Max(largest,Vector3.Distance(a,b));
      
     }
    }
    foreach(var plant in plants)foreach(var part in plant.Parts){var now=part.filter.sharedMesh.vertices;for(int i=0;i<now.Length;i++)if(part.filter.transform.TransformPoint(part.data.vertices[i]).y<plant.BaseY+.002f){if(Vector3.Distance(now[i],part.data.vertices[i])>.00001f)throw new Exception("Tree base moved");fixedVertices++;}}
    Check(largest>.002f,"Tree vertices sway; measured change="+largest);Check(fixedVertices>0,"Base vertices were sampled");
    player.ReturnToStart();FaroNightVerification.Aim(player,birds[0].transform.position);FaroNightVerification.Capture("stage6-environment.png");
    birds[0].SendMessage("OnApplicationPause",true);birdClock=birds[0].FlightClock;break;
   case 11:
    Check(Mathf.Abs(birdClock-birds[0].FlightClock)<.001f,"Gull freezes cleanly when app is paused");birds[0].SendMessage("OnApplicationPause",false);
    File.WriteAllText(Path.GetFullPath("../stage6-environment-verification.txt"),report+"\nEditor checks; Android device review pending.");End(0);return;
  }
  Next();
 }catch(Exception e){Debug.LogException(e);File.WriteAllText(Path.GetFullPath("../stage6-environment-verification.txt"),report+"\nFAIL: "+e);End(1);}}
 static void End(int code){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;EditorApplication.Exit(code);}
}
