using UnityEngine;
using UnityEngine.XR;
namespace ElUltimoFaro
{
    // Direct proximity grabbing; each existing prop follows the tracked controller.
    public sealed class QuestGrip : MonoBehaviour
    {
        public Transform leftHand, rightHand;
        public MobileInteraction interaction;
        public MobilePlayerController player;
        public QuartzSocketTrigger socket;
        public Renderer leftMarker, rightMarker;
        public QuestRowing rowing;
        public bool LeftOccupied=>leftItem && leftItem.IsCarried;
        public bool RightOccupied=>rightItem && rightItem.IsCarried;
        PortableItem leftItem, rightItem;
        Vector3 leftReturn, rightReturn;
        Quaternion leftRotation, rightRotation;
        bool leftPressed, rightPressed;
        const float GrabDistance=.13f;
        void Update()
        {
            Handle(XRNode.LeftHand,leftHand,leftMarker,ref leftItem,ref leftPressed,ref leftReturn,ref leftRotation);
            Handle(XRNode.RightHand,rightHand,rightMarker,ref rightItem,ref rightPressed,ref rightReturn,ref rightRotation);
        }
        void Handle(XRNode node,Transform hand,Renderer marker,ref PortableItem held,ref bool pressed,ref Vector3 origin,ref Quaternion rotation)
        {
            if(!hand)return;
            var device=InputDevices.GetDeviceAtXRNode(node);
            bool tracked=device.TryGetFeatureValue(CommonUsages.isTracked,out bool t)&&t;
            if(marker)marker.enabled=tracked;
            if(!tracked || !Application.isFocused){pressed=false;return;}
            device.TryGetFeatureValue(CommonUsages.grip,out float grip);
            bool down=grip>.65f;
            if(held && held.IsInserted)held=null;
            if(down && !pressed && !held)
            {
                bool onOar=rowing&&(node==XRNode.LeftHand?rowing.LeftEngaged:rowing.RightEngaged);
                var item=onOar?null:Nearest(hand);
                if(item){
                    var bounds=item.VisualBounds;origin=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);rotation=item.transform.rotation;
                    if(item.Carry(hand)){held=item;device.SendHapticImpulse(0,.3f,.08f);interaction.ShowMessage("Suelta el agarre lateral para dejar el objeto.");}
                }
            }
            if(!down && pressed && held){
                if(held.Kind==PortableItem.ItemKind.Quartz && socket && socket.SnapPoint && Vector3.Distance(held.VisualBounds.center,socket.SnapPoint.position)<.12f && socket.TryInsert(held)){
                    interaction.ShowMessage("Cuarzo encajado. Máquina arrancando.");device.SendHapticImpulse(0,.5f,.15f);
                }else if(player.IsOnBoat && rowing){rowing.Stow(held);interaction.ShowMessage("Objeto guardado en el bote. Ya puedes remar.");}
                else{
                    // Do not lose mission objects in water: return to the pickup spot if no solid support exists.
                    Vector3 support=origin;
                    var hits=Physics.RaycastAll(hand.position+Vector3.up*.05f,Vector3.down,2f,~(1<<9),QueryTriggerInteraction.Ignore);
                    System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
                    foreach(var hit in hits)if(!hit.transform.IsChildOf(player.transform) && !hit.transform.IsChildOf(held.transform) && hit.normal.y>.6f){support=hit.point;break;}
                    held.ReleaseAt(support,rotation);
                }
                held=null;
            }
            pressed=down;
        }
        PortableItem Nearest(Transform hand)
        {
            PortableItem nearest=null;float distance=GrabDistance;
            foreach(var item in new[]{interaction.Quartz,interaction.Lantern}){
                if(!item || item.IsCarried || item.IsInserted)continue;
                Vector3 point=item.VisualBounds.ClosestPoint(hand.position);float d=Vector3.Distance(point,hand.position);
                if(d>distance)continue;
                bool blocked=false;
                foreach(var hit in Physics.RaycastAll(hand.position,(point-hand.position).normalized,d,~(1<<9),QueryTriggerInteraction.Ignore))
                    if(!hit.transform.IsChildOf(player.transform)&&!hit.transform.IsChildOf(item.transform)){blocked=true;break;}
                if(!blocked){nearest=item;distance=d;}
            }
            return nearest;
        }
    }
}
