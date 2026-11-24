using UnityEngine;
using UnityEngine.XR;

namespace ElUltimoFaro
{
    // Quest-only adapter. Existing phone components remain intact in the Android scene.
    [DefaultExecutionOrder(100)]
    public sealed class QuestPlayerBridge : MonoBehaviour
    {
        public MobilePlayerController player;
        public Transform head;
        public MobileInteraction interaction;
        public BoatController boat;
        public QuestRowing rowing;
        public float speed = .9f;
        CharacterController body;
        float vertical, unsupported;
        bool turned, actionHeld, boatHeld, lightHeld;
        void Awake() { body = player.GetComponent<CharacterController>(); var rig=head.GetComponentInParent<OVRCameraRig>(); if(rig)player.ConfigureTrackedView(head,rig.transform); player.enabled = false; Application.targetFrameRate = 72; }
        void Update()
        {
            if(!Application.isFocused){if(boat)boat.Stop();return;}
            var left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            left.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 move);
            right.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 turn);
            right.TryGetFeatureValue(CommonUsages.primaryButton, out bool action);
            right.TryGetFeatureValue(CommonUsages.secondaryButton, out bool boarding);
            left.TryGetFeatureValue(CommonUsages.primaryButton, out bool lamp);
            if(action && !actionHeld) interaction.ActivateNearby();
            if(boarding && !boatHeld) boat.Interact();
            if(lamp && !lightHeld) interaction.ToggleLantern();
            actionHeld=action; boatHeld=boarding; lightHeld=lamp;
            if(Mathf.Abs(turn.x)<.25f) turned=false;
            if(Mathf.Abs(turn.x)>.7f && !turned && !player.IsOnBoat) {
                Vector3 pivot=head.position;
                player.transform.RotateAround(pivot,Vector3.up,Mathf.Sign(turn.x)*30); turned=true;
            }
            if(player.CurrentBoat) { if(!rowing)player.CurrentBoat.SetInput(move); return; }
            Vector3 forward=Vector3.ProjectOnPlane(head.forward,Vector3.up).normalized;
            Vector3 direction=Vector3.ClampMagnitude(forward*move.y+Vector3.Cross(Vector3.up,forward)*move.x,1);
            float dt=Mathf.Min(Time.deltaTime,.05f);
            Vector3 step=direction*speed*dt;
            if(step.sqrMagnitude>0 && !player.HasGround(player.transform.position+step+direction.normalized*(body.radius+.02f))) step=Vector3.zero;
            vertical=body.isGrounded && vertical<0 ? -2 : Mathf.Max(vertical-18*dt,-20);
            if(body.enabled) body.Move(step+Vector3.up*vertical*dt);
            unsupported=player.HasGround(player.transform.position)?0:unsupported+dt;
            if(unsupported>1.25f) {player.ReturnToStart();unsupported=vertical=0;}
        }
        void OnApplicationFocus(bool focused) { if(!focused && boat)boat.Stop(); }
    }
}

