using UnityEngine;
using UnityEngine.UI;
namespace ElUltimoFaro
{
    public sealed class QuestPrompts:MonoBehaviour
    {
        public QuestPlayerBridge bridge;
        public QuestGrip grip;
        public QuestRowing rowing;
        public MachineInteraction machine;
        public LighthouseDoor door;
        public Text label;
        public Canvas canvas;
        void LateUpdate(){
            var view=bridge.head;var items=bridge.interaction;
            Vector3 point=Vector3.zero;string text=null;
            if(bridge.player.IsOnBoat){
                var landing=bridge.boat.AvailableLanding();
                point=(rowing.leftHandle.position+rowing.rightHandle.position)*.5f+Vector3.up*.12f;
                text=landing?"B · DESEMBARCAR\nAgarre lateral · Tomar los remos":bridge.boat.LandingHint+"\nAgarra los remos y tira hacia ti";
                if(grip.LeftOccupied||grip.RightOccupied)text="DEJA LOS OBJETOS EN EL BOTE\nSuelta el agarre para liberar las manos";
                else if(!landing&&(rowing.LeftEngaged||rowing.RightEngaged))text="TIRA PARA AVANZAR\nRetroceder: gatillos + empujar los remos";
            }
            else if(items.DoorIsCandidate()){
                point=door.Leaf.GetComponent<Collider>().bounds.center+Vector3.up*.1f;
                text="A · "+door.ActionLabel;
            }
            else if(machine.InReach()){
                point=machine.PromptTarget.position+Vector3.up*.12f;
                text=machine.Machine.IsStarting?"MÁQUINA ARRANCANDO...":items.Quartz.IsCarried&&!grip.socket.IsInserted?"ACERCA EL CUARZO AL RECEPTÁCULO\nSuelta el agarre para encajarlo":"A · "+machine.ActionLabel;
            }
            else {
                PortableItem selected=null;float angle=50;
                foreach(var item in new[]{items.Quartz,items.Lantern}){
                    if(item.IsInserted)continue;
                    Vector3 p=item.VisualBounds.center;float a=Vector3.Angle(view.forward,p-view.position);
                    if(Vector3.Distance(view.position,p)<1.1f && a<angle && (item.IsCarried||items.CanTake(item))){selected=item;angle=a;}
                }
                if(selected){
                    point=selected.VisualBounds.center+Vector3.up*(selected.VisualBounds.extents.y+.065f);
                    if(selected.IsCarried)text=selected.Kind==PortableItem.ItemKind.Lantern?"X · "+(selected.IsLit?"APAGAR FAROL":"ENCENDER FAROL")+"\nSuelta el agarre para dejarlo":"CUARZO\nSuelta el agarre para dejarlo";
                    else text=selected.DisplayName+"\nAcerca la mano y mantén el agarre lateral";
                }
                else if(bridge.boat.CanBoard()){
                    point=(rowing.leftHandle.position+rowing.rightHandle.position)*.5f+Vector3.up*.12f;text="B · EMBARCAR";
                }
            }
            bool visible=text!=null && Vector3.Angle(view.forward,point-view.position)<65;
            canvas.enabled=visible;if(!visible)return;
            label.text=text;
            // World-space text stays next to the target, with real depth in both eyes.
            Vector3 toward=(view.position-point).normalized;
            canvas.transform.position=point+toward*.035f;
            canvas.transform.rotation=Quaternion.LookRotation(canvas.transform.position-view.position,Vector3.up);
            canvas.transform.localScale=Vector3.one*(.00055f*Mathf.Clamp(Vector3.Distance(view.position,point),.45f,1.3f));
        }
    }
}
