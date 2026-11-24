using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace ElUltimoFaro
{
    public sealed class BoatController : MonoBehaviour
    {
        const float BoardingDistance = 2.2f;
        public Transform Seat => seat;
        public string LandingHint { get; private set; }
        [SerializeField] MobilePlayerController player;
        [SerializeField] Transform seat;
        [SerializeField] Transform[] landings;
        [SerializeField] Vector3 homePosition;
        [SerializeField] Button boardingButton;
        [SerializeField] Text boardingLabel, moveLabel, navigationStatus;
        [SerializeField] Vector2 minimum = new Vector2(-18, -23), maximum = new Vector2(8, 6);
        [SerializeField] float forwardSpeed = 1.5f, reverseSpeed = .65f, acceleration = 1.5f, turningSpeed = 75;
        [SerializeField] bool sampleHullWater;
        [SerializeField] float waterClearance = .10f;
        [SerializeField] LowPolyWater animatedWater;
        Vector2 input;
        float speed;
        bool focused = true;
        readonly Collider[] obstacles = new Collider[64];
        public bool HasPassenger => player && player.CurrentBoat == this;
        public float Speed => speed;
        public Transform[] Landings => landings;
        public Vector3 HomePosition => homePosition;
        public Vector2 Minimum => minimum;
        public Vector2 Maximum => maximum;
        public void Configure(MobilePlayerController controller, Transform playerSeat, Transform[] ports, Button button, Text movement, Text status)
        {
            player = controller; seat = playerSeat; landings = ports; homePosition = transform.position;
            boardingButton = button; boardingLabel = button.GetComponentInChildren<Text>(); moveLabel = movement; navigationStatus = status;
        }
        void Awake()
        {
            boardingButton.onClick.AddListener(Interact);
            // Water is queried for buoyancy but must not push the walking character off the dock.
            Physics.IgnoreLayerCollision(9, player.gameObject.layer, true);
        }
        public void SetInput(Vector2 value) { input = focused ? Vector2.ClampMagnitude(value, 1) : Vector2.zero; }
        public void Stop() { input = Vector2.zero; speed = 0; }
        void LateUpdate()
        {
            if (HasPassenger && focused) Simulate(Mathf.Min(Time.deltaTime, .05f));
            else
            {
                Stop();
                if (animatedWater)
                {
                    var rest = transform.position;
                    float height = WaterSupportHeight(rest, transform.rotation);
                    rest.y = Mathf.Max(height, Mathf.MoveTowards(rest.y, height, Time.deltaTime * .6f));
                    transform.position = rest;
                }
            }
            var port = AvailableLanding();
            bool nearShore = false;
            foreach (var landing in landings)
                if (HorizontalDistance(transform.position, landing.position) <= BoardingDistance) nearShore = true;
            boardingButton.interactable = HasPassenger ? port != null : CanBoard();
            boardingLabel.text = HasPassenger ? (port ? "DESEMBARCAR" : nearShore ? "DETÉN EL BOTE" : "ACÉRCATE A LA COSTA") : "EMBARCAR";
            moveLabel.text = HasPassenger ? "AVANZAR / GIRAR" : "MOVER";
            navigationStatus.text = HasPassenger ? (port ? "Aquí puedes desembarcar" : nearShore ? "Suelta el control y espera a detenerte" : "Arriba: avanzar · Abajo: retroceder · Lados: girar") : "";
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame) Interact();
#endif
        }
        public bool CanBoard()
        {
            if (!player || player.IsOnBoat || !player.HasGround(player.transform.position)) return false;
            if (HorizontalDistance(player.transform.position, transform.position) > BoardingDistance ||
                Mathf.Abs(player.transform.position.y - transform.position.y) > 1.5f || ClosestPortToPlayer() == null) return false;
            Vector3 origin = player.transform.position + Vector3.up * .45f;
            Vector3 delta = seat.position + Vector3.up * .45f - origin;
            foreach (var hit in Physics.RaycastAll(origin, delta.normalized, delta.magnitude, ~(1 << 9), QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(transform) && !hit.transform.IsChildOf(player.transform)) return false;
            return true;
        }
        Transform ClosestPortToPlayer()
        {
            foreach (var port in landings)
                if (HorizontalDistance(player.transform.position, port.position) < 1.25f) return port;
            return null;
        }
        public Transform AvailableLanding()
        {
            return TryLanding(out var port,out _) ? port : null;
        }
        public bool TryLanding(out Transform selected,out Vector3 feet)
        {
            selected=null;feet=Vector3.zero;LandingHint="Acércate al muelle o a la costa iluminada";
            bool near=false;
            foreach(var port in landings)
            {
                if(HorizontalDistance(transform.position,port.position)>BoardingDistance || Mathf.Abs(transform.position.y-port.position.y)>1.5f)continue;
                near=true;
                if(Mathf.Abs(speed)>.12f){LandingHint="Suelta los remos y espera a detenerte";continue;}
                // Ignore residual stroke input: a hull stopped at shore must allow exiting.
                if(player.TryLandingFeet(port.position,out feet)){selected=port;LandingHint="B · Desembarcar";return true;}
            }
            if(near && Mathf.Abs(speed)<=.12f)LandingHint="Acércate a la zona despejada del desembarque";
            return false;
        }
        public void Interact()
        {
            if (HasPassenger)
            {
                if(!TryLanding(out var port,out var feet))return;
                ReleasePassenger(); player.TeleportTo(feet, port.eulerAngles.y);
            }
            else if (CanBoard()) { Stop(); player.AttachToBoat(this, seat); }
        }
        public void ReleasePassenger()
        {
            Stop(); if (HasPassenger) player.LeaveBoatParent();
        }
        public void ResetForTest()
        {
            ReleasePassenger(); transform.SetPositionAndRotation(homePosition, Quaternion.identity);
            if (animatedWater) { var rest = transform.position; rest.y = WaterSupportHeight(rest, transform.rotation); transform.position = rest; }
            player.TeleportTo(landings[0].position, landings[0].eulerAngles.y);
        }
        public void Simulate(float dt)
        {
            if (!HasPassenger || !focused || dt <= 0) return;
            dt = Mathf.Min(dt, .05f);
            float desired = input.y * (input.y >= 0 ? forwardSpeed : reverseSpeed);
            Vector3 position = transform.position;
            float edge = Mathf.Min(position.x - minimum.x, maximum.x - position.x, position.z - minimum.y, maximum.y - position.z);
            Vector3 travel = transform.forward * Mathf.Sign(desired);
            Vector3 center = new Vector3((minimum.x + maximum.x) * .5f, position.y, (minimum.y + maximum.y) * .5f);
            if (Vector3.Dot(travel, position - center) > 0) desired *= Mathf.Lerp(.15f, 1, Mathf.Clamp01(edge / 2));
            speed = Mathf.MoveTowards(speed, desired, acceleration * dt);
            var rotation = transform.rotation * Quaternion.Euler(0, input.x * turningSpeed * dt, 0);
            if (Mathf.Abs(input.x) > .001f && !HullBlocked(position, rotation)) transform.rotation = rotation;
            Vector3 next = position + transform.forward * (speed * dt);
            // The whole hull stays inside a generous sea area; no visible wall or forced route.
            next.x = Mathf.Clamp(next.x, minimum.x + .65f, maximum.x - .65f);
            next.z = Mathf.Clamp(next.z, minimum.y + .65f, maximum.y - .65f);
            if (sampleHullWater)
            {
                float supportedHeight = WaterSupportHeight(next, transform.rotation);
                // Follow descending waves gently; never let an ascending crest enter the interior.
                next.y = Mathf.Max(supportedHeight, Mathf.MoveTowards(position.y, supportedHeight, dt * .6f));
            }
            else if (Physics.Raycast(new Vector3(next.x, 5, next.z), Vector3.down, out var water, 10, 1 << 9, QueryTriggerInteraction.Ignore))
                next.y = Mathf.MoveTowards(position.y, water.point.y + .055f, dt * .6f);
            if (!HullBlocked(next, transform.rotation)) transform.position = next;
            else speed = 0;
        }
        public bool HullBlocked(Vector3 position, Quaternion rotation)
        {
            if (sampleHullWater)
            {
                // A crest can lift the collision box above a low bank. Still reject dry ground below the hull.
                for (int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z++)
                {
                    var probe = position + rotation * new Vector3(x * .22f, 0, z * .45f); probe.y = 5;
                    if (Physics.Raycast(probe, Vector3.down, out var shore, 10, 1 << 8, QueryTriggerInteraction.Ignore) &&
                        Physics.Raycast(probe, Vector3.down, out var sea, 10, 1 << 9, QueryTriggerInteraction.Ignore) && shore.point.y > SeaHeight(sea) - .03f) return true;
                }
            }
            // Ignore water and passenger. Include the banks, dock and existing architecture.
            int count = Physics.OverlapBoxNonAlloc(position, new Vector3(.24f, .065f, .50f), obstacles, rotation, ~(1 << 9), QueryTriggerInteraction.Ignore);
            if (count == obstacles.Length) return true;
            for (int i = 0; i < count; i++)
                if (!obstacles[i].transform.IsChildOf(transform) && !obstacles[i].transform.IsChildOf(player.transform)) return true;
            return false;
        }
        public float WaterSupportHeight(Vector3 position, Quaternion rotation)
        {
            float highest = float.NegativeInfinity;
            for (int x = -1; x <= 1; x++)
            for (int z = -2; z <= 2; z++)
            {
                Vector3 point = position + rotation * new Vector3(x * .28f, 0, z * .30f);
                point.y = 5;
                if (Physics.Raycast(point, Vector3.down, out var water, 10, 1 << 9, QueryTriggerInteraction.Ignore)) highest = Mathf.Max(highest, SeaHeight(water));
            }
            return float.IsNegativeInfinity(highest) ? position.y : highest + waterClearance;
        }
        static float HorizontalDistance(Vector3 a, Vector3 b) { a.y = b.y = 0; return Vector3.Distance(a, b); }
        float SeaHeight(RaycastHit hit) => animatedWater && animatedWater.isActiveAndEnabled ? animatedWater.SurfaceHeight(hit) : hit.point.y;
        void OnApplicationFocus(bool value) { focused = value; if (!value) Stop(); }
        void OnApplicationPause(bool value) { focused = !value; if (value) Stop(); }
    }
}
