using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
namespace ElUltimoFaro {
 [RequireComponent(typeof(CharacterController))]
 public sealed class HallPlayer : MonoBehaviour {
  public bool desktop; public Transform head;public bool CanMove=true;
  CharacterController body;float pitch,fall;bool turned;
  void Awake(){body=GetComponent<CharacterController>();Application.targetFrameRate=desktop?60:72;}
  void Update(){
   if(!CanMove||!Application.isFocused)return;Vector2 move=Vector2.zero;
   if(desktop){var k=Keyboard.current;if(k!=null)move=new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));
    if(Mouse.current!=null&&Mouse.current.rightButton.isPressed){var look=Mouse.current.delta.ReadValue()*.12f;transform.Rotate(0,look.x,0);pitch=Mathf.Clamp(pitch-look.y,-65,65);head.localRotation=Quaternion.Euler(pitch,0,0);}
   }else{InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis,out move);InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis,out Vector2 turn);if(Mathf.Abs(turn.x)<.25f)turned=false;if(Mathf.Abs(turn.x)>.7f&&!turned){transform.RotateAround(head.position,Vector3.up,Mathf.Sign(turn.x)*30);turned=true;}}
   Vector3 forward=Vector3.ProjectOnPlane(head.forward,Vector3.up).normalized;Vector3 step=Vector3.ClampMagnitude(forward*move.y+Vector3.Cross(Vector3.up,forward)*move.x,1)*2f;
   float dt=Mathf.Min(Time.deltaTime,.05f);fall=body.isGrounded?-2:Mathf.Max(-15,fall-15*dt);body.Move((step+Vector3.up*fall)*dt);
  }
 }
}
