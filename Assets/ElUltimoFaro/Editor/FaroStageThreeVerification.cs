using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using ElUltimoFaro;

[InitializeOnLoad]
public static class FaroStageThreeVerification
{
    const string Flag = "Faro.StageThree.Verify";
    static int phase;
    static int walkTrips;
    static double since;
    static MobilePlayerController player;
    static MobileInteraction items;
    static BoatController boat;
    static TouchMovePad pad;
    static Vector3 walkTarget;
    static Vector3 touchStart;
    static float helmYaw;
    static FaroStageThreeVerification() { EditorApplication.playModeStateChanged += Changed; }
    public static void Run()
    {
        FaroStageThreeTools.Open(); QualitySettings.SetQualityLevel(0, true); EditorSettings.asyncShaderCompilation = false;
        SessionState.SetBool(Flag, true); EditorApplication.EnterPlaymode();
    }
    public static void RefineAndRun() { FaroStageThreeTools.RefinePresentation(); Run(); }
    static void Changed(PlayModeStateChange state)
    {
        if (SessionState.GetBool(Flag, false) && state == PlayModeStateChange.EnteredPlayMode)
        { phase = 0; walkTrips = 0; since = EditorApplication.timeSinceStartup; EditorApplication.update += Tick; }
    }
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); Debug.Log("FARO_STAGE3_PASS " + message); }
    static float Distance(Vector3 a, Vector3 b) { a.y = b.y = 0; return Vector3.Distance(a, b); }
    static void StartWalk(Vector3 target)
    {
        walkTarget = target; since = EditorApplication.timeSinceStartup;
        pad.ResetInput();
        pad.OnPointerDown(new PointerEventData(EventSystem.current) {pointerId = 51, position = RectTransformUtility.WorldToScreenPoint(null, pad.transform.TransformPoint(new Vector3(0, 65, 0)))});
    }
    static bool WalkReached()
    {
        // A desktop focus change can legitimately release synthetic touches during a hidden batch test.
        if (pad.Value == Vector2.zero)
        {
            Debug.Log("FARO_TEST_FOREGROUND_RESTART"); player.SendMessage("OnApplicationFocus", true);
            pad.OnPointerDown(new PointerEventData(EventSystem.current) {pointerId = 51, position = RectTransformUtility.WorldToScreenPoint(null, pad.transform.TransformPoint(new Vector3(0, 65, 0)))});
        }
        if (Distance(player.transform.position, walkTarget) < .09f) { pad.ResetInput(); return true; }
        Vector3 direction = walkTarget - player.transform.position; direction.y = 0;
        player.transform.rotation = Quaternion.LookRotation(direction);
        if (EditorApplication.timeSinceStartup - since > 16)
        {
            var ahead = player.transform.position + direction.normalized * .10f;
            foreach (var hit in Physics.RaycastAll(ahead + Vector3.up * .2f, Vector3.down, .6f, 1 << 8))
                Debug.Log("FARO_DOCK_PROBE " + hit.transform.name + " distance=" + hit.distance + " normal=" + hit.normal);
            throw new Exception("Walking blocked at " + player.transform.position.ToString("F4") + " target " + walkTarget + " pad=" + pad.Value + " groundAhead=" + player.HasGround(ahead) + " velocity=" + player.GetComponent<CharacterController>().velocity);
        }
        return false;
    }
    static void Tick()
    {
        try
        {
            if (phase == 1 || phase == 2)
            {
                if (!WalkReached()) return;
                if (phase == 1)
                {
                    Check(player.transform.position.y < -.18f, "Walk down to dock without teleport");
                    StartWalk(new Vector3(-.42f, .665f, .84f)); phase = 2; return;
                }
                Check(player.transform.position.y > .57f, "Walk back up dock without teleport");
                walkTrips++;
                if (walkTrips < 3) { Time.captureFramerate = walkTrips == 1 ? 20 : 60; StartWalk(new Vector3(.70f, -.27f, 1.38f)); phase = 1; return; }
                Time.captureFramerate = 0;
                phase = 3; since = EditorApplication.timeSinceStartup; return;
            }
            if (EditorApplication.timeSinceStartup - since < 1.2) return;
            switch (phase)
            {
                case 0:
                    player = UnityEngine.Object.FindAnyObjectByType<MobilePlayerController>(); items = player.GetComponent<MobileInteraction>();
                    boat = UnityEngine.Object.FindAnyObjectByType<BoatController>(); pad = UnityEngine.Object.FindAnyObjectByType<TouchMovePad>();
                    Application.runInBackground = true; InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    player.SendMessage("OnApplicationFocus", true); boat.SendMessage("OnApplicationFocus", true);
                    Check(!boat.CanBoard(), "Cannot board from lighthouse spawn");
                    Check(Mathf.Abs(Camera.main.transform.localPosition.y - 1.55f / 3) < .001f, "Player height preserved");
                    player.TeleportTo(new Vector3(-.42f, .70f, .84f), 0);
                    StartWalk(new Vector3(.70f, -.27f, 1.38f)); phase = 1; return;
                case 3:
                    items.VisitLanternTest(); Camera.main.transform.LookAt(items.Lantern.VisualBounds.center); items.TakeNearby(); items.ToggleLantern();
                    Check(items.Lantern.IsCarried && items.Lantern.IsLit, "Lantern collected and switched on before voyage");
                    boat.ResetForTest();
                    Check(boat.CanBoard(), "Boarding enabled at existing dock");
                    GameObject.Find("Embarcar").GetComponent<Button>().onClick.Invoke();
                    Check(player.IsOnBoat && !player.GetComponent<CharacterController>().enabled, "Board button attaches player and suspends walking capsule");
                    touchStart = boat.transform.position; helmYaw = boat.transform.eulerAngles.y;
                    pad.OnPointerDown(new PointerEventData(EventSystem.current) {pointerId = 53, position = RectTransformUtility.WorldToScreenPoint(null, pad.transform.TransformPoint(new Vector3(0, 65, 0)))});
                    var look = UnityEngine.Object.FindAnyObjectByType<TouchLookArea>();
                    var drag = new PointerEventData(EventSystem.current) {pointerId = 54, delta = new Vector2(65, 10)};
                    look.OnPointerDown(drag); look.OnDrag(drag);
                    break;
                case 4:
                    pad.ResetInput(); boat.Stop();
                    UnityEngine.Object.FindAnyObjectByType<TouchLookArea>().ResetInput();
                    Check(Distance(touchStart, boat.transform.position) > .3f, "Actual touch joystick drives boat through player controller");
                    Check(Mathf.Abs(Mathf.DeltaAngle(helmYaw, boat.transform.eulerAngles.y)) < .1f && Mathf.Abs(player.transform.localEulerAngles.y) > 1, "Looking around does not steer boat");
                    var initial = boat.transform.position;
                    boat.SetInput(new Vector2(0, 1)); for (int i = 0; i < 20; i++) boat.Simulate(.05f);
                    Check(Distance(initial, boat.transform.position) > .35f, "Boat advances with throttle");
                    Check(boat.AvailableLanding() == null, "Cannot disembark while moving");
                    boat.Stop(); initial = boat.transform.position;
                    boat.SetInput(new Vector2(0, -1)); for (int i = 0; i < 12; i++) boat.Simulate(.05f);
                    Check(Vector3.Dot(boat.transform.position - initial, boat.transform.forward) < -.08f, "Boat can reverse"); boat.Stop();
                    Drive(new Vector3(-.5f, 0, 3.5f)); Drive(new Vector3(-5, 0, 3.5f));
                    Drive(new Vector3(-6, 0, -9)); Drive(new Vector3(-10.2f, 0, -14.5f)); Drive(new Vector3(-10.35f, 0, -15.05f));
                    Check(boat.AvailableLanding() == boat.Landings[1], "Secondary shore is reachable by free navigation");
                    Capture("stage3-voyage.png");
                    boat.Interact(); Check(!player.IsOnBoat && player.HasGround(player.transform.position), "Disembark on secondary island safely");
                    Camera.main.transform.LookAt(items.Quartz.VisualBounds.center);
                    Check(items.CanTake(items.Quartz), "Quartz reachable from landing"); items.TakeNearby();
                    Check(items.Quartz.IsCarried, "Collect original quartz after voyage");
                    boat.Interact(); Check(player.IsOnBoat, "Reboard from secondary island");
                    // Reverse off the coast before turning in open water.
                    boat.SetInput(new Vector2(0, -1)); for (int i = 0; i < 35; i++) boat.Simulate(.05f); boat.Stop();
                    Drive(new Vector3(-10.2f, 0, -13.8f)); Drive(new Vector3(-6, 0, -9)); Drive(new Vector3(-5, 0, 3.5f)); Drive(new Vector3(-.5f, 0, 3.5f));
                    Drive(boat.HomePosition);
                    Check(boat.AvailableLanding() == boat.Landings[0], "Return to main dock by free navigation");
                    boat.Interact(); Check(!player.IsOnBoat && player.HasGround(player.transform.position), "Disembark at main dock safely");
                    Check(items.Quartz.IsCarried && items.Lantern.IsCarried && items.Lantern.IsLit, "Round trip preserves quartz and lit lantern");
                    break;
                case 5:
                    Capture("stage3-return.png");
                    boat.ResetForTest(); boat.Interact();
                    boat.transform.SetPositionAndRotation(new Vector3(-8, -.3f, -8), Quaternion.identity); boat.Stop();
                    var barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    barrier.transform.position = boat.transform.position + Vector3.forward * 1.3f; barrier.transform.localScale = new Vector3(1, 3, .15f); Physics.SyncTransforms();
                    var beforeCollision = boat.transform.position;
                    boat.SetInput(Vector2.up); for (int i = 0; i < 60; i++) boat.Simulate(.05f);
                    Check(Distance(beforeCollision, boat.transform.position) < .9f && boat.Speed == 0, "Solid obstacle stops moving hull");
                    UnityEngine.Object.DestroyImmediate(barrier); Physics.SyncTransforms(); boat.Stop();
                    boat.transform.position = new Vector3(-17, -.35f, -21); boat.transform.rotation = Quaternion.Euler(0, 225, 0);
                    boat.SetInput(new Vector2(0, 1)); for (int i = 0; i < 500; i++) boat.Simulate(.05f);
                    Check(boat.transform.position.x >= boat.Minimum.x + .64f && boat.transform.position.z >= boat.Minimum.y + .64f, "Discrete sea limits contain the hull");
                    boat.transform.rotation = Quaternion.Euler(0, 45, 0); var edge = boat.transform.position;
                    boat.SetInput(new Vector2(0, 1)); for (int i = 0; i < 40; i++) boat.Simulate(.05f);
                    Check(Distance(edge, boat.transform.position) > .5f, "Player can steer away from boundary");
                    boat.SendMessage("OnApplicationFocus", false); Check(boat.Speed == 0, "Boat stops on lost application focus");
                    boat.SendMessage("OnApplicationFocus", true);
                    boat.ResetForTest(); Check(!player.IsOnBoat && boat.Speed == 0, "Test reset releases passenger and stops boat");
                    boat.Interact(); items.VisitQuartzTest();
                    Check(!player.IsOnBoat && player.GetComponent<CharacterController>().enabled && boat.Speed == 0, "Test teleport while aboard safely restores walking");
                    Check(items.Quartz.IsCarried && items.Lantern.IsCarried, "Test teleport preserves carried objects");
                    boat.ResetForTest();
                    File.WriteAllText(Path.GetFullPath("../stage3-verification.txt"), "PASS: dock descent/ascent with touch movement; boarding; actual touch driving and independent look; forward/reverse; safe secondary landing; quartz pickup; return voyage with lit lantern and quartz; main landing; solid hull obstacle; sea bounds and escape; focus loss; test reset/teleport state.\nDriving helper exists only in this Editor test, not in the game. Physical A56 validation pending.");
                    End(0); return;
            }
            phase++; since = EditorApplication.timeSinceStartup;
        }
        catch (Exception ex) { Debug.LogException(ex); File.WriteAllText(Path.GetFullPath("../stage3-verification.txt"), "FAIL: " + ex); End(1); }
    }
    // Test-only steering sends the same throttle/turn values as the touch controller. Runtime has no autopilot.
    static void Drive(Vector3 destination)
    {
        bool arrived = false;
        for (int i = 0; i < 4000; i++)
        {
            Vector3 delta = destination - boat.transform.position; delta.y = 0;
            if (delta.magnitude < .10f) { arrived = true; break; }
            float error = Vector3.SignedAngle(boat.transform.forward, delta, Vector3.up);
            boat.SetInput(new Vector2(Mathf.Clamp(error / 35, -1, 1), Mathf.Abs(error) < 18 ? Mathf.Clamp(delta.magnitude * .5f, .03f, .85f) : 0));
            boat.Simulate(.05f);
        }
        boat.SetInput(Vector2.zero); for (int i = 0; i < 30; i++) boat.Simulate(.05f);
        Check(arrived, "Steering reaches " + destination + "; actual=" + boat.transform.position.ToString("F4"));
    }
    static void End(int code) { SessionState.SetBool(Flag, false); EditorApplication.update -= Tick; EditorApplication.Exit(code); }
    static void Capture(string filename)
    {
        boat.SendMessage("LateUpdate"); items.SendMessage("Update");
        var camera = Camera.main; var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = .2f; Canvas.ForceUpdateCanvases();
        var target = new RenderTexture(1280, 720, 24); target.Create(); RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest {destination = target});
        var old = RenderTexture.active; RenderTexture.active = target;
        var png = new Texture2D(1280, 720, TextureFormat.RGB24, false); png.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); png.Apply(); File.WriteAllBytes(Path.GetFullPath("../" + filename), png.EncodeToPNG());
        RenderTexture.active = old; target.Release(); UnityEngine.Object.Destroy(target); UnityEngine.Object.Destroy(png); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }
}
