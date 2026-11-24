using UnityEngine;
using UnityEngine.UI;

namespace ElUltimoFaro
{
    public sealed class MachineInteraction : MonoBehaviour
    {
        [SerializeField] MobilePlayerController player;
        [SerializeField] MobileInteraction items;
        [SerializeField] Camera view;
        [SerializeField] Transform machineRoot, target;
        [SerializeField] QuartzMachineController machine;
        [SerializeField] QuartzSocketTrigger socket;
        [SerializeField] MachineLever lever;
        [SerializeField] LighthouseBlink lighthouse;
        [SerializeField] Text status;
        [SerializeField] float reach = 1.1f;
        AudioSource sound;
        AudioClip insertion;
        public QuartzMachineController Machine => machine;
        public Transform PromptTarget => target;
        public float ViewAngle=>Vector3.Angle(view.transform.forward,target.position-view.transform.position);
        public bool MissionCompleted { get; private set; }
        public string ActionLabel => machine.IsStarting ? "ARRANCANDO..." : !socket.IsInserted && items.Quartz.IsCarried ? "INSERTAR CUARZO" : !machine.IsPowered ? "PROBAR PALANCA" : lever.IsOn ? "APAGAR FARO" : "ENCENDER FARO";
        void Awake()
        {
            sound = gameObject.AddComponent<AudioSource>(); sound.playOnAwake = false; sound.volume = .4f;
            insertion = MachineEffects.Sound("Encaje del cuarzo", false);
        }
        void Update()
        {
            if (lighthouse.IsPowered && !MissionCompleted) { MissionCompleted = true; items.ShowMessage("¡Faro encendido! Has devuelto la luz a la isla."); }
            status.text = lighthouse.IsPowered ? "FARO ENCENDIDO" : machine.IsPowered ? "MÁQUINA ENERGIZADA · Acciona la palanca" : machine.IsStarting ? "ARRANCANDO MAQUINARIA... " + Mathf.RoundToInt(machine.PowerLevel * 100) + "%" : items.Quartz.IsCarried ? "Lleva el cuarzo a la máquina junto al faro" : "Encuentra el cuarzo en la isla secundaria";
        }
        public bool InReach()
        {
            if (player.IsOnBoat) return false;
            Vector3 delta = target.position - view.transform.position;
            if (delta.magnitude > reach || Vector3.Angle(view.transform.forward, delta) > 55) return false;
            foreach (var hit in Physics.RaycastAll(view.transform.position, delta.normalized, delta.magnitude, ~(1 << 9), QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(player.transform) && !hit.transform.IsChildOf(machineRoot)) return false;
            return true;
        }
        public void Interact()
        {
            if (!InReach()) return;
            if (!socket.IsInserted && items.Quartz.IsCarried)
            {
                if (socket.TryInsert(items.Quartz)) { sound.PlayOneShot(insertion); items.ShowMessage("Cuarzo encajado. La máquina está arrancando..."); }
            }
            else if (machine.IsStarting) items.ShowMessage("Espera a que la máquina termine de arrancar.");
            else if (!machine.IsPowered) items.ShowMessage("Faro sin energía. Necesitas el cuarzo.");
            else { lever.PullLever(); items.ShowMessage(lever.IsOn ? "Faro encendido" : "Faro apagado. La máquina conserva su energía."); }
        }
        void OnDestroy() { if (insertion) Destroy(insertion); }
    }
}
