using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace ElUltimoFaro
{
    public sealed class MobileInteraction : MonoBehaviour
    {
        [SerializeField] Camera view;
        [SerializeField] MobilePlayerController player;
        [SerializeField] PortableItem quartz, lantern;
        [SerializeField] Transform quartzHand, lanternHand;
        [SerializeField] Button takeButton, lightButton;
        [SerializeField] Text takeLabel, lightLabel, inventory, feedback;
        [SerializeField] Vector3 quartzTestSpawn, lanternTestSpawn;
        [SerializeField] float reach = 1.6f;
        [SerializeField] MachineInteraction machinery;
        [SerializeField] LighthouseDoor door;
        bool DoorCandidate()=>door&&door.InReach()&&(!machinery||!machinery.InReach()||door.ViewAngle<machinery.ViewAngle);
        AudioSource sound;
        AudioClip pickupSound, switchSound;
        float messageUntil;
        public PortableItem Quartz => quartz;
        public bool DoorIsCandidate()=>DoorCandidate();
        public void ActivateNearby(){if(DoorCandidate())door.Interact();else if(machinery)machinery.Interact();}
        public PortableItem Lantern => lantern;
        public Vector3 QuartzTestSpawn => quartzTestSpawn;
        public Vector3 LanternTestSpawn => lanternTestSpawn;

        public void Configure(Camera camera, MobilePlayerController controller, PortableItem crystal, PortableItem lamp,
            Transform crystalHand, Transform lampHand, Button take, Button toggle, Text status, Text message,
            Vector3 crystalSpawn, Vector3 lampSpawn)
        {
            view = camera; player = controller; quartz = crystal; lantern = lamp;
            quartzHand = crystalHand; lanternHand = lampHand; takeButton = take; lightButton = toggle;
            takeLabel = take.GetComponentInChildren<Text>(); lightLabel = toggle.GetComponentInChildren<Text>();
            inventory = status; feedback = message; quartzTestSpawn = crystalSpawn; lanternTestSpawn = lampSpawn;
        }
        void Awake()
        {
            sound = gameObject.AddComponent<AudioSource>(); sound.playOnAwake = false; sound.spatialBlend = 0; sound.volume = 0.3f;
            pickupSound = Tone("Recogida", 700, 0.18f, true);
            switchSound = Tone("Interruptor", 160, 0.055f, false);
            takeButton.onClick.AddListener(TakeNearby);
            lightButton.onClick.AddListener(ToggleLantern);
            ShowMessage("Acércate a un objeto y míralo para recogerlo.");
        }
        void Update()
        {
            var item = Candidate();
            bool machineNearby = machinery && machinery.InReach();
            bool doorNearby=DoorCandidate();
            takeButton.interactable = item || doorNearby || (machineNearby && !machinery.Machine.IsStarting);
            takeLabel.text = item ? "RECOGER " + item.DisplayName : doorNearby ? door.ActionLabel : machineNearby ? machinery.ActionLabel : "ACÉRCATE Y MIRA";
            lightButton.gameObject.SetActive(lantern.IsCarried);
            lightLabel.text = lantern.IsLit ? "APAGAR FAROL" : "ENCENDER FAROL";
            inventory.text = (lantern.IsCarried ? "FAROL: " + (lantern.IsLit ? "ENCENDIDO" : "APAGADO") : "FAROL: PENDIENTE")
                + "   ·   " + (quartz.IsInserted ? "CUARZO: INSERTADO" : quartz.IsCarried ? "CUARZO: RECOGIDO" : "CUARZO: PENDIENTE");
            if (Time.unscaledTime > messageUntil) feedback.text = "";
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) TakeNearby();
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) ToggleLantern();
#endif
        }
        public bool CanTake(PortableItem item)
        {
            if (!item || item.IsCarried || item.IsInserted || !item.gameObject.activeInHierarchy) return false;
            Vector3 target = item.VisualBounds.center;
            Vector3 delta = target - view.transform.position;
            if (delta.magnitude > reach || Vector3.Angle(view.transform.forward, delta) > 55) return false;
            // Ignore the player's own capsule and triggers; solid scene geometry blocks collection.
            foreach (var hit in Physics.RaycastAll(view.transform.position, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(player.transform) && !hit.transform.IsChildOf(item.transform)) return false;
            return true;
        }
        public PortableItem Candidate()
        {
            if (CanTake(quartz)) return quartz;
            return CanTake(lantern) ? lantern : null;
        }
        public void TakeNearby()
        {
            var item = Candidate();
            if (!item) { if(DoorCandidate())door.Interact();else if (machinery) machinery.Interact(); return; }
            if (!item.Carry(item.Kind == PortableItem.ItemKind.Quartz ? quartzHand : lanternHand)) return;
            sound.PlayOneShot(pickupSound);
            ShowMessage(item.DisplayName + " recogido. Lo llevas contigo.");
        }
        public void ToggleLantern()
        {
            if (!lantern.ToggleLight()) return;
            sound.PlayOneShot(switchSound);
            ShowMessage(lantern.IsLit ? "Farol encendido" : "Farol apagado");
        }
        public void VisitQuartzTest() { Visit(quartzTestSpawn, quartz, "Prueba en la isla del cuarzo. Usa VOLVER AL INICIO para regresar."); }
        public void VisitLanternTest() { Visit(lanternTestSpawn, lantern, "Prueba junto al farol. Mira hacia abajo para recogerlo."); }
        void Visit(Vector3 position, PortableItem item, string message)
        {
            Vector3 direction = item.IsCarried ? Vector3.forward : item.VisualBounds.center - position;
            direction.y = 0;
            player.TeleportTo(position, Quaternion.LookRotation(direction).eulerAngles.y);
            ShowMessage(message);
        }
        public void ShowMessage(string message) { feedback.text = message; messageUntil = Time.unscaledTime + 5; }
        static AudioClip Tone(string name, float frequency, float duration, bool chime)
        {
            const int rate = 22050;
            var samples = new float[Mathf.CeilToInt(rate * duration)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate;
                float envelope = Mathf.Min(t * 150, 1) * Mathf.Pow(1 - t / duration, 2);
                samples[i] = (Mathf.Sin(2 * Mathf.PI * frequency * t) + (chime ? .3f * Mathf.Sin(2 * Mathf.PI * frequency * 1.5f * t) : 0)) * envelope * .5f;
            }
            var clip = AudioClip.Create(name, samples.Length, 1, rate, false); clip.SetData(samples, 0); return clip;
        }
        void OnDestroy() { if (pickupSound) Destroy(pickupSound); if (switchSound) Destroy(switchSound); }
    }
}
