using UnityEngine;
using UnityEngine.XR;
namespace ElUltimoFaro
{
    [DefaultExecutionOrder(150)]
    public sealed class QuestRowing : MonoBehaviour
    {
        public BoatController boat;
        public QuestGrip grip;
        public Transform leftHand, rightHand, leftHandle, rightHandle, leftOar, rightOar;
        public Transform quartzCargo, lanternCargo;
        public bool LeftEngaged { get; private set; }
        public bool RightEngaged { get; private set; }
        Vector3 lastLeft,lastRight;
        bool validLeft,validRight;
        float leftForce,rightForce;
        public static float Pull(Vector3 previous,Vector3 current,float dt,bool engaged)
        {
            if(!engaged || dt<=0 || dt>.1f || (current-previous).sqrMagnitude>.04f)return 0;
            // Recovery strokes do not push the boat backwards. Positions are boat-local,
            // so hull movement and waves cannot create artificial propulsion.
            return Mathf.Clamp01((-((current.z-previous.z)/dt)-.025f)*4f);
        }
        public static Vector2 Mix(float left,float right)=>new Vector2(Mathf.Clamp(left-right,-1,1),Mathf.Clamp((left+right)*.5f,-1,1));
        public static float StrokeForce(Vector3 previous,Vector3 current,float dt,bool engaged,bool reverse)
            =>reverse?-Pull(current,previous,dt,engaged):Pull(previous,current,dt,engaged);
        void Update()
        {
            bool ready=boat.HasPassenger && Application.isFocused;
            LeftEngaged=Sample(XRNode.LeftHand,leftHand,leftHandle,leftOar,-1,ready,grip.LeftOccupied,ref validLeft,ref lastLeft,ref leftForce,LeftEngaged);
            RightEngaged=Sample(XRNode.RightHand,rightHand,rightHandle,rightOar,1,ready,grip.RightOccupied,ref validRight,ref lastRight,ref rightForce,RightEngaged);
            if(boat.HasPassenger)boat.SetInput(Mix(leftForce,rightForce));
        }
        bool Sample(XRNode node,Transform hand,Transform handle,Transform oar,float side,bool ready,bool occupied,ref bool valid,ref Vector3 last,ref float force,bool engaged)
        {
            var device=InputDevices.GetDeviceAtXRNode(node);
            device.TryGetFeatureValue(CommonUsages.grip,out float squeeze);
            bool tracked=device.TryGetFeatureValue(CommonUsages.isTracked,out bool t)&&t;
            var point=boat.transform.InverseTransformPoint(hand.position);
            bool active=ready&&tracked&&!occupied&&squeeze>.65f&&(engaged||Vector3.Distance(hand.position,handle.position)<.15f);
            device.TryGetFeatureValue(CommonUsages.trigger,out float reverse);
            // The index trigger selects a reverse power stroke, so recovery does not cancel propulsion.
            float target=valid?StrokeForce(last,point,Time.deltaTime,active,reverse>.65f):0;
            force=active?Mathf.Lerp(force,target,1-Mathf.Exp(-Time.deltaTime*10)):0;
            valid=active;last=point;
            // Oar pivots remain attached to the gunwales. The inner handles follow the stroke.
            Vector3 rest=boat.transform.InverseTransformPoint(handle.position);
            float sweep=active?Mathf.Clamp((point.z-rest.z)*220,-35,35):0;
            float lift=active?Mathf.Clamp((point.y-rest.y)*160,-18,18):0;
            oar.localRotation=Quaternion.Slerp(oar.localRotation,Quaternion.Euler(side*lift,side*sweep,0),1-Mathf.Exp(-Time.deltaTime*16));
            return active;
        }
        public void Stow(PortableItem item)
        {
            var slot=item.Kind==PortableItem.ItemKind.Quartz?quartzCargo:lanternCargo;
            if(item.ReleaseAt(slot.position,slot.rotation))item.transform.SetParent(slot,true);
        }
        void OnDisable(){leftForce=rightForce=0;validLeft=validRight=false;LeftEngaged=RightEngaged=false;if(boat)boat.SetInput(Vector2.zero);}
    }
}
