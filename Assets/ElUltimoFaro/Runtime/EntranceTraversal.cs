using UnityEngine;
namespace ElUltimoFaro {
 [DefaultExecutionOrder(-400)]
 public sealed class EntranceTraversal:MonoBehaviour {
  [SerializeField] CharacterController body;
  [SerializeField] Transform lighthouse;
  float outdoorStep;bool initialized;
  void Awake(){outdoorStep=body.stepOffset;initialized=true;}
  void Update(){
   Vector3 p=lighthouse.InverseTransformPoint(body.transform.position);
   // The automatic outdoor step lift is too tall beneath this doorway's lintel.
   bool nearDoor=Mathf.Abs(p.x)<1.3f&&p.z>-3&&p.z<1.2f&&p.y>0&&p.y<3;
   float step=nearDoor?Mathf.Min(outdoorStep,.025f):outdoorStep;
   if(Mathf.Abs(body.stepOffset-step)>.001f)body.stepOffset=step;
  }
  void OnDisable(){if(body&&initialized)body.stepOffset=outdoorStep;}
 }
}
