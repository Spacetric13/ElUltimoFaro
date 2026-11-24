using UnityEngine;
namespace ElUltimoFaro {
 public sealed class LighthouseDoor:MonoBehaviour {
  [SerializeField] Transform hinge;[SerializeField] BoxCollider leaf;
  [SerializeField] MobilePlayerController player;[SerializeField] MobileInteraction feedback;[SerializeField] Camera view;
  [SerializeField] AudioSource sound;[SerializeField] AudioClip creak,latch;
  [SerializeField] float openAngle=100,reach=1.05f,openingSeconds=1.4f;
  readonly Collider[] nearby=new Collider[32];
  Vector3 centerInHinge,half;Quaternion leafRotation;
  float angle,target,velocity;bool targetOpen,suspended,wasMoving;
  public float Angle=>angle;
  public bool IsMoving=>Mathf.Abs(target-angle)>.15f;
  public bool IsOpen=>angle>openAngle-1;
  public int BlockedCount{get;private set;}
  public Transform Leaf=>leaf.transform;
  public string ActionLabel=>targetOpen?"CERRAR PUERTA":"ABRIR PUERTA";
  public float ViewAngle=>Vector3.Angle(view.transform.forward,leaf.bounds.center-view.transform.position);
  void Awake(){centerInHinge=hinge.InverseTransformPoint(leaf.transform.TransformPoint(leaf.center));leafRotation=Quaternion.Inverse(hinge.rotation)*leaf.transform.rotation;half=Vector3.Scale(leaf.size,leaf.transform.lossyScale)*.5f;half+=Vector3.one*.001f;}
  public bool InReach(){
   if(player.IsOnBoat)return false;Vector3 d=leaf.bounds.center-view.transform.position;
   if(d.magnitude>reach||ViewAngle>48)return false;
   foreach(var hit in Physics.RaycastAll(view.transform.position,d.normalized,d.magnitude,~(1<<9),QueryTriggerInteraction.Ignore))
    if(!hit.transform.IsChildOf(player.transform)&&!hit.transform.IsChildOf(hinge))return false;
   return true;
  }
  public void Interact(){if(!InReach()||suspended)return;targetOpen=!targetOpen;target=targetOpen?openAngle:0;velocity=0;sound.clip=creak;sound.loop=true;sound.Play();feedback.ShowMessage(targetOpen?"Abriendo la puerta del faro":"Cerrando la puerta del faro");}
  bool Blocked(float value){
   Quaternion rot=hinge.parent.rotation*Quaternion.Euler(0,value,0);
   Vector3 center=hinge.position+rot*Vector3.Scale(centerInHinge,hinge.lossyScale);
   int count=Physics.OverlapBoxNonAlloc(center,half,nearby,rot*leafRotation,~(1<<9),QueryTriggerInteraction.Ignore);
   if(count==nearby.Length)return true;
   for(int i=0;i<count;i++)if(!nearby[i].transform.IsChildOf(hinge)){
#if UNITY_EDITOR
    Debug.Log("DOOR_BLOCKED_BY "+nearby[i].name+" angle="+value);
#endif
    return true;
   }
   return false;
  }
  void Update(){
   if(suspended||!IsMoving)return;
   wasMoving=true;
   float next=Mathf.SmoothDamp(angle,target,ref velocity,Mathf.Max(.15f,openingSeconds*.25f),90,Mathf.Min(Time.deltaTime,.05f));
   if(Mathf.Abs(next-target)<.15f)next=target;
   // Small angular substeps stop the leaf before it pushes a character or intersects a wall.
   int steps=Mathf.Max(1,Mathf.CeilToInt(Mathf.Abs(next-angle)/1f));float from=angle;
   for(int i=1;i<=steps;i++){
    float candidate=Mathf.Lerp(from,next,(float)i/steps);
    if(Blocked(candidate)){target=angle;velocity=0;targetOpen=angle>openAngle*.5f;sound.Stop();BlockedCount++;feedback.ShowMessage("Puerta bloqueada. Apártate un poco para moverla.");return;}
    angle=candidate;hinge.localRotation=Quaternion.Euler(0,angle,0);
   }
   Physics.SyncTransforms();
   if(!IsMoving&&wasMoving){wasMoving=false;sound.Stop();sound.loop=false;if(angle<1)sound.PlayOneShot(latch);}
  }
  void Suspend(bool value){suspended=value;if(value)sound.Pause();else if(IsMoving)sound.UnPause();}
  void OnApplicationFocus(bool focus){Suspend(!focus);}
  void OnApplicationPause(bool paused){Suspend(paused);}
 }
}
