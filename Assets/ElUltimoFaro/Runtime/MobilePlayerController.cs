using UnityEngine;
using UnityEngine.InputSystem;

namespace ElUltimoFaro
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class MobilePlayerController : MonoBehaviour
    {
        [SerializeField] Transform view;
        [SerializeField] TouchMovePad movePad;
        [SerializeField] TouchLookArea lookArea;
        [SerializeField, Min(0.1f)] float moveSpeed = 2f;
        [SerializeField, Min(1f)] float lookSensitivity = 115f;
        [SerializeField] LayerMask groundMask;
        [SerializeField] Vector3 spawnPosition;
        [SerializeField] float spawnYaw;
        [SerializeField, Min(0.1f)] float traversalScale = 1f;
        CharacterController body;
        public bool externalBoatInput;
        Transform trackedHead, trackingOrigin;
        public void ConfigureTrackedView(Transform head, Transform origin) { trackedHead=head; trackingOrigin=origin; }
        // Recenter only on a transport transition; normal room-scale movement is preserved.
        public void CenterTrackedView()
        {
            if(!trackedHead || !trackingOrigin)return;
            float yaw=Mathf.DeltaAngle(trackedHead.eulerAngles.y,transform.eulerAngles.y);
            trackingOrigin.RotateAround(trackedHead.position,Vector3.up,yaw);
            Vector3 offset=transform.position-trackedHead.position;offset.y=0;
            trackingOrigin.position+=offset;
        }
        public bool TryLandingFeet(Vector3 desired,out Vector3 feet)
        {
            feet=desired;if(!body)body=GetComponent<CharacterController>();
            if(!Physics.Raycast(desired+Vector3.up*.45f,Vector3.down,out var hit,.9f,groundMask,QueryTriggerInteraction.Ignore) || Vector3.Angle(hit.normal,Vector3.up)>body.slopeLimit)return false;
            feet=hit.point+Vector3.up*.015f;
            float radius=body.radius;float height=body.height;
            var hits=Physics.OverlapCapsule(feet+Vector3.up*(radius+.025f),feet+Vector3.up*(height-radius),radius*.9f,~(1<<9),QueryTriggerInteraction.Ignore);
            foreach(var col in hits)if(!col.transform.IsChildOf(transform) && !(CurrentBoat && col.transform.IsChildOf(CurrentBoat.transform)))return false;
            return true;
        }
        float pitch, verticalSpeed, unsupportedTime;
        bool focused = true;
        public BoatController CurrentBoat { get; private set; }
        public bool IsOnBoat => CurrentBoat != null;

        public void Configure(Transform cameraTransform, TouchMovePad move, TouchLookArea look, int mask)
        {
            view = cameraTransform; movePad = move; lookArea = look; groundMask = mask;
            spawnPosition = transform.position; spawnYaw = transform.eulerAngles.y;
        }
        void Awake() { body = GetComponent<CharacterController>(); Application.targetFrameRate = 60; }
        public void ConfigureScale(float scale)
        {
            traversalScale = scale;
            moveSpeed = 2f * scale;
        }
        void Update()
        {
            if (!focused || !view) return;
            Vector2 move = movePad ? movePad.Value : Vector2.zero;
            Vector2 look = lookArea ? lookArea.ConsumeDelta() * lookSensitivity : Vector2.zero;
#if UNITY_EDITOR || UNITY_STANDALONE
            var keys = Keyboard.current;
            if (keys != null)
            {
                move += new Vector2((keys.dKey.isPressed ? 1 : 0) - (keys.aKey.isPressed ? 1 : 0),
                    (keys.wKey.isPressed ? 1 : 0) - (keys.sKey.isPressed ? 1 : 0));
                if (keys.rKey.wasPressedThisFrame) ReturnToStart();
            }
            if (Mouse.current != null && Mouse.current.rightButton.isPressed)
                look += Mouse.current.delta.ReadValue() * 0.12f;
#endif
            transform.Rotate(0, look.x, 0);
            pitch = Mathf.Clamp(pitch - look.y, -65f, 65f);
            view.localRotation = Quaternion.Euler(pitch, 0, 0);
            if (CurrentBoat)
            {
                if(!externalBoatInput)CurrentBoat.SetInput(move);
                return;
            }
            Vector3 direction = transform.TransformDirection(new Vector3(move.x, 0, move.y));
            direction = Vector3.ClampMagnitude(direction, 1);
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            Vector3 step = direction * (moveSpeed * dt);
            // A short probe beyond the capsule stops walking off an island without visible walls.
            if (step.sqrMagnitude > 0 && !HasGround(transform.position + step + direction.normalized * (body.radius + 0.06f * traversalScale)))
                step = Vector3.zero;
            verticalSpeed = body.isGrounded && verticalSpeed < 0 ? -2f : Mathf.Max(verticalSpeed - 18f * dt, -20f);
            body.Move(step + Vector3.up * (verticalSpeed * dt));
            unsupportedTime = HasGround(transform.position) ? 0 : unsupportedTime + dt;
            if (unsupportedTime > 1.25f || transform.position.y < spawnPosition.y - 4f) ReturnToStart();
        }
        public bool HasGround(Vector3 feet)
        {
            if (!body) body = GetComponent<CharacterController>();
            float probeUp = traversalScale < 1f ? 0.2f : 0.45f;
            float probeLength = traversalScale < 1f ? 0.4f : 1f;
            return Physics.Raycast(feet + Vector3.up * probeUp, Vector3.down, out var hit, probeLength,
                       groundMask, QueryTriggerInteraction.Ignore) && Vector3.Angle(hit.normal, Vector3.up) <= body.slopeLimit;
        }
        public void ReturnToStart()
        {
            TeleportTo(spawnPosition, spawnYaw);
        }
        public void TeleportTo(Vector3 position, float yaw)
        {
            if (CurrentBoat) CurrentBoat.ReleasePassenger();
            if (!body) body = GetComponent<CharacterController>();
            body.enabled = false;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            CenterTrackedView();
            body.enabled = true;
            pitch = verticalSpeed = unsupportedTime = 0;
            if (view) view.localRotation = Quaternion.identity;
            if (movePad) movePad.ResetInput();
            if (lookArea) lookArea.ResetInput();
        }
        public void AttachToBoat(BoatController boat, Transform seat)
        {
            if (CurrentBoat || !boat || !seat) return;
            if (!body) body = GetComponent<CharacterController>();
            body.enabled = false; CurrentBoat = boat;
            transform.SetParent(seat, false); transform.localPosition = Vector3.zero; transform.localRotation = Quaternion.identity;
            CenterTrackedView();
            pitch = verticalSpeed = unsupportedTime = 0; if(view)view.localRotation = Quaternion.identity;
            movePad.ResetInput(); lookArea.ResetInput();
        }
        public void LeaveBoatParent()
        {
            CurrentBoat = null; transform.SetParent(null, true);
        }
        void OnApplicationFocus(bool value) { focused = value; if (!value) { if(movePad) movePad.ResetInput(); if(lookArea) lookArea.ResetInput(); if(CurrentBoat) CurrentBoat.Stop(); } }
    }
}
