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
public static class FaroStageSixVerification
{
    const string Flag = "Faro.StageSix.Verify";
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
    static MachineInteraction machinery;
    static QuartzSocketTrigger socket;
    static MachineLever lever;
    static LighthouseBlink lighthouse;
    static bool sawDim, sawBright, sawFade;
    static bool sawStartupRamp;
    static Quaternion beamStart, gearStart, secondaryStart;
    static Transform mainGear, secondaryGear;
    static float rampTime;
    static WeatherDirector weather;
    static WeatherAudio weatherSound;
    static LowPolyWater water;
    static RainField rain;
    static float stormStart, boatLow, boatHigh, firstFlash=-1;
    static bool capturedFlash;
    static int waterChecks;
    static System.Text.StringBuilder results = new System.Text.StringBuilder();
    static FaroStageSixVerification() { EditorApplication.playModeStateChanged += Changed; }
    public static void Run()
    {
        FaroStageSixTools.Open(); QualitySettings.SetQualityLevel(0, true); EditorSettings.asyncShaderCompilation = false;
        SessionState.SetBool(Flag, true); EditorApplication.EnterPlaymode();
    }
    
    static void Changed(PlayModeStateChange state)
    {
        if (SessionState.GetBool(Flag, false) && state == PlayModeStateChange.EnteredPlayMode)
        { phase = 0; walkTrips = 0; since = EditorApplication.timeSinceStartup; EditorApplication.update += Tick; }
    }
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); Debug.Log("FARO_STAGE6_PASS " + message); results.AppendLine("PASS: " + message); }
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
            Debug.Log("FARO_WALK_STATE focus=" + typeof(MobilePlayerController).GetField("focused", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(player) + " dt=" + Time.deltaTime + " timeScale=" + Time.timeScale + " enabled=" + player.enabled);
            foreach (var c in Physics.OverlapCapsule(player.transform.position + Vector3.up * .07f, player.transform.position + Vector3.up * .50f, .075f, ~(1 << 9), QueryTriggerInteraction.Ignore)) Debug.Log("FARO_WALK_OVERLAP " + c.name);
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
            if (phase == 5)
            {
                boatLow=Mathf.Min(boatLow,boat.transform.position.y);boatHigh=Mathf.Max(boatHigh,boat.transform.position.y);
                if(weather.FlashCount>0 && firstFlash<0)firstFlash=weather.LastFlashTime;
                if(weather.FlashBrightness>.3f && !capturedFlash){capturedFlash=true;Capture("stage6-storm-flash.png");}
                if(boat.transform.position.y+.004f<boat.WaterSupportHeight(boat.transform.position,boat.transform.rotation))throw new Exception("Idle boat fell under animated sea support");
                if(Time.time-stormStart<12)return;
            }
            if(phase==10 && weather.CalmProgress<1)return;
            if (phase == 8)
            {
                sawDim |= lighthouse.CurrentBrightness < .05f;
                sawBright |= lighthouse.CurrentBrightness > .95f;
                sawFade |= lighthouse.CurrentBrightness > .1f && lighthouse.CurrentBrightness < .9f;
                if (Time.time - rampTime < 7) return;
            }
            if (phase == 7)
            {
                sawStartupRamp |= machinery.Machine.PowerLevel > .1f && machinery.Machine.PowerLevel < .9f && !machinery.Machine.IsPowered;
                if (Time.time - rampTime < 3.1f) return;
            }
            if (EditorApplication.timeSinceStartup - since < 1.2) return;
            switch (phase)
            {
                case 0:
                    player = UnityEngine.Object.FindAnyObjectByType<MobilePlayerController>(); items = player.GetComponent<MobileInteraction>();
                    boat = UnityEngine.Object.FindAnyObjectByType<BoatController>(); pad = UnityEngine.Object.FindAnyObjectByType<TouchMovePad>();
                    Application.runInBackground = true; InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    player.SendMessage("OnApplicationFocus", true); boat.SendMessage("OnApplicationFocus", true);
                    weather=UnityEngine.Object.FindAnyObjectByType<WeatherDirector>();weatherSound=UnityEngine.Object.FindAnyObjectByType<WeatherAudio>();water=UnityEngine.Object.FindAnyObjectByType<LowPolyWater>();rain=UnityEngine.Object.FindAnyObjectByType<RainField>();
                    weather.SendMessage("OnApplicationFocus",true);weatherSound.SendMessage("OnApplicationFocus",true);
                    Check(weather.Phase==WeatherDirector.WeatherPhase.HeavyRain && weather.FlashCount==0,"Starts at permanent night with heavy rain and no lightning");
                    Check(RenderSettings.sun.intensity<.5f && RenderSettings.skybox.shader.name=="ElUltimoFaro/NightSky","Night sky and low moonlight configured");
                    Check(water.OriginalMesh==water.GetComponent<MeshCollider>().sharedMesh && water.GetComponent<MeshFilter>().sharedMesh.vertexCount==7372 && water.Surface.vertexXZ.Length==7372,"Water retains original low poly mesh and topology");
                    Check(UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Length>0,"Scene behaviours loaded");
                    machinery = player.GetComponent<MachineInteraction>(); socket = UnityEngine.Object.FindAnyObjectByType<QuartzSocketTrigger>();
                    lever = UnityEngine.Object.FindAnyObjectByType<MachineLever>(); lighthouse = UnityEngine.Object.FindAnyObjectByType<LighthouseBlink>();
                    Check(Mathf.Abs(new SerializedObject(player).FindProperty("moveSpeed").floatValue - .9f) < .001f, "Walking speed increased 35 percent");
                    Check(!machinery.Machine.IsPowered && !lighthouse.IsPowered && !lighthouse.beamRenderer.enabled, "Machine and lighthouse start unpowered");
                    lever.PullLever(); Check(!lever.IsOn && !lighthouse.IsPowered, "Lever cannot light lighthouse without power");
                    Check(!socket.TryInsert(items.Lantern) && !socket.TryInsert(items.Quartz), "Socket rejects wrong item and uncollected quartz");
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
                    Capture("stage6-voyage.png");
                    boat.Interact(); Check(!player.IsOnBoat && player.HasGround(player.transform.position), "Disembark on secondary island safely");
                    Camera.main.transform.LookAt(items.Quartz.VisualBounds.center);
                    Check(items.CanTake(items.Quartz), "Quartz reachable from landing"); items.TakeNearby();
                    Check(items.Quartz.IsCarried, "Collect original quartz after voyage");
                    Check(weather.FlashCount==0,"No storm flashes before quartz pickup");
                    weather.SendMessage("Update");
                    Check(weather.Phase==WeatherDirector.WeatherPhase.Storm && weather.StormStrength<.2f,"Picking up original quartz starts gradual storm transition");
                    FaroNightVerification.Aim(player,new Vector3(-6,2,-12));
                    stormStart=Time.time;boatLow=boatHigh=boat.transform.position.y;break;
                case 5:
                    Check(System.Array.TrueForAll(UnityEngine.Object.FindObjectsByType<VegetationSway>(FindObjectsSortMode.None),p=>p.CurrentStrength>.95f),"Vegetation responds to the quartz storm");
                    Check(weather.StormStrength>.99f && rain.CurrentRate>1400 && water.Amplitude>.039f,"Storm intensifies rain, wind and small waves gradually");
                    Check(weather.FlashCount>=1 && capturedFlash,"Environmental lightning illuminates scene in spaced flashes");
                    Check(weatherSound.ThunderCount>=1 && weatherSound.LastThunderTime-firstFlash>=1.15f,"Thunder follows lightning with audible delay");
                    Check(boatHigh-boatLow>.003f,"Unoccupied boat follows animated water while player is ashore");
                    Check(rain.ActiveDrops>0 && rain.ActiveDrops<=1600,"Rain uses bounded local particle budget");
                    Check(water.OriginalMesh==water.GetComponent<MeshCollider>().sharedMesh && water.GetComponent<MeshFilter>().sharedMesh!=water.OriginalMesh,"Wave animation uses a private mesh and preserves the original collider");
                    Capture("stage6-storm-shore.png");
                    boat.Interact(); Check(player.IsOnBoat, "Reboard from secondary island");
                    // Reverse off the coast before turning in open water.
                    boat.SetInput(new Vector2(0, -1)); for (int i = 0; i < 35; i++) boat.Simulate(.05f); boat.Stop();
                    Drive(new Vector3(-10.2f, 0, -13.8f)); Drive(new Vector3(-6, 0, -9)); Drive(new Vector3(-5, 0, 3.5f)); Drive(new Vector3(-.5f, 0, 3.5f));
                    Drive(boat.HomePosition);
                    Check(boat.AvailableLanding() == boat.Landings[0], "Return to main dock by free navigation");
                    boat.Interact(); Check(!player.IsOnBoat && player.HasGround(player.transform.position), "Disembark at main dock safely");
                    Check(items.Quartz.IsCarried && items.Lantern.IsCarried && items.Lantern.IsLit, "Round trip preserves quartz and lit lantern");
                    break;
                case 6:
                    Capture("stage6-return.png");
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
                    PrepareMachineVantage();
                    mainGear = GameObject.Find("Pivot_MainGear").transform; secondaryGear = GameObject.Find("Pivot_SecondaryGear").transform;
                    gearStart = mainGear.localRotation; secondaryStart = secondaryGear.localRotation;
                    Check(machinery.InReach(), "Machine accessible from safe ground");
                    Check(machinery.ActionLabel == "INSERTAR CUARZO", "Context action offers insertion");
                    items.TakeNearby();
                    Check(socket.IsInserted && items.Quartz.IsInserted && !items.Quartz.IsCarried && items.Quartz.transform.parent == socket.SnapPoint, "Original quartz snaps into socket and leaves inventory hand");
                    Check(Vector3.Distance(items.Quartz.VisualBounds.center, socket.SnapPoint.position) < .002f, "Crystal centered precisely in socket");
                    Check(!items.CanTake(items.Quartz) && !items.Quartz.Carry(Camera.main.transform) && !socket.TryInsert(items.Quartz), "Inserted quartz is locked; duplicate insertion rejected");
                    Check(machinery.Machine.IsStarting && !machinery.Machine.IsPowered && machinery.Machine.PowerLevel == 0, "Startup begins at zero speed and energy");
                    lever.PullLever(); Check(!lever.IsOn, "Lever disabled during startup");
                    Capture("stage6-insertion.png"); rampTime = Time.time;
                    break;
                case 7:
                    Check(sawStartupRamp, "Startup passes through intermediate power before enabling lever");
                    Check(machinery.Machine.IsPowered && machinery.Machine.PowerLevel > .999f, "Machine gradually reaches energized state");
                    Check(Quaternion.Angle(gearStart, mainGear.localRotation) > 5 && Quaternion.Angle(secondaryStart, secondaryGear.localRotation) > 5, "Both existing gears animate");
                    Check(GameObject.Find("Luz_indicador").GetComponent<Light>().enabled && GameObject.Find("Luz_nucleo").GetComponent<Light>().enabled, "Indicator and energy core lights are active");
                    Check(machinery.Machine.GetComponent<AudioSource>().isPlaying && machinery.Machine.GetComponent<AudioSource>().volume > .2f, "Mechanical audio ramps to working volume");
                    Camera.main.transform.LookAt(socket.SnapPoint); items.TakeNearby();
                    Check(lever.IsOn && lighthouse.IsPowered, "Context action operates lever and lighthouse");
                    weather.SendMessage("Update");
                    Check(weather.Phase==WeatherDirector.WeatherPhase.Clearing && weather.CalmProgress<.2f,"First lighthouse activation starts gradual weather clearing");
                    beamStart = lighthouse.beamPivot.rotation; sawDim = sawBright = sawFade = false; rampTime = Time.time;
                    break;
                case 8:
                    Check(sawDim && sawBright && sawFade, "Lighthouse has bright, dark and smooth intermediate blink states");
                    Check(Quaternion.Angle(beamStart, lighthouse.beamPivot.rotation) > 15, "Existing beam slowly rotates about lamp center");
                    Check(machinery.MissionCompleted, "Mission completion follows machine and lever chain");
                    Camera.main.transform.LookAt(socket.SnapPoint); items.TakeNearby(); Check(!lighthouse.IsPowered && !lever.IsOn, "Lever switches lighthouse off again");
                    break;
                case 9:
                    Check(lighthouse.CurrentBrightness == 0 && !lighthouse.beamRenderer.enabled && !lighthouse.beaconLight.enabled, "Lighthouse fades completely off");
                    Check(socket.IsInserted && machinery.Machine.IsPowered, "Switching off preserves quartz and machinery energy");
                    Camera.main.transform.LookAt(socket.SnapPoint); items.TakeNearby(); Check(lighthouse.IsPowered, "Lighthouse can be reactivated");
                    break;
                case 10:
                    Capture("stage6-machine.png");
                    player.TeleportTo(new Vector3(-1.3f, .72f, -.7f), 0); Camera.main.transform.LookAt(lighthouse.lampGlowRenderer.bounds.center);
                    Check(!machinery.InReach(), "Machine cannot be operated remotely");
                    Check(items.Quartz.IsInserted && items.Quartz.transform.parent == socket.SnapPoint, "Returning or teleporting never removes locked crystal");
                    Capture("stage6-lighthouse.png");
                    Check(weather.MissionResolved && weather.Phase==WeatherDirector.WeatherPhase.Calm && Mathf.Abs(rain.CurrentRate-150)<1,"After the lighthouse, only light rain remains");
                    Check(System.Array.TrueForAll(UnityEngine.Object.FindObjectsByType<VegetationSway>(FindObjectsSortMode.None),p=>p.CurrentStrength<.22f),"Vegetation wind settles after the lighthouse");
                    Check(weather.FlashBrightness==0 && !GameObject.Find("Rayo_ambiental").GetComponent<LineRenderer>().enabled,"No lightning continues after mission completion");
                    Check(RenderSettings.sun.intensity<.5f && weatherSound.RainVolume<.06f,"Night remains while rain and wind audio subside");
                    int previousFlashes=weather.FlashCount;lever.PullLever();weather.SendMessage("Update");
                    Check(!lighthouse.IsPowered && weather.MissionResolved && weather.Phase==WeatherDirector.WeatherPhase.Calm && weather.FlashCount==previousFlashes,"Switching lighthouse off does not reset storm reward");
                    lever.PullLever();weather.SendMessage("OnApplicationPause",true);weather.SendMessage("Update");
                    Check(weather.FlashBrightness==0,"Pausing clears pending lightning");weather.SendMessage("OnApplicationPause",false);
                    results.AppendLine("Water clearance samples during voyage: " + waterChecks);
                    results.AppendLine("Editor automated checks only; physical Samsung A56 verification pending.");
                    File.WriteAllText(Path.GetFullPath("../stage6-verification.txt"), results.ToString());
                    End(0); return;
            }
            phase++; since = EditorApplication.timeSinceStartup;
        }
        catch (Exception ex) { Debug.LogException(ex); File.WriteAllText(Path.GetFullPath("../stage6-verification.txt"), "FAIL: " + ex); End(1); }
    }
    static void PrepareMachineVantage()
    {
        Vector3 target = socket.SnapPoint.position;
        for (float radius = .40f; radius < .95f; radius += .1f)
        for (int a = 0; a < 16; a++)
        {
            float angle = Mathf.PI * 2 * a / 16;
            Vector3 probe = target + new Vector3(-Mathf.Cos(angle) * radius, 4, Mathf.Sin(angle) * radius);
            if (!Physics.Raycast(probe, Vector3.down, out var hit, 8, 1 << 8) || Vector3.Angle(hit.normal, Vector3.up) > 40) continue;
            var feet = hit.point + Vector3.up * .03f;
            bool blocked = false;
            foreach (var c in Physics.OverlapCapsule(feet + Vector3.up * .1f, feet + Vector3.up * .48f, .07f, ~(1 << 9), QueryTriggerInteraction.Ignore)) if (!c.transform.IsChildOf(player.transform)) blocked = true;
            if (blocked) continue;
            player.TeleportTo(feet, 0); Camera.main.transform.LookAt(target); Physics.SyncTransforms();
            if (machinery.InReach()) { Debug.Log("FARO_MACHINE_VANTAGE " + feet.ToString("F4")); return; }
        }
        throw new Exception("No safe machine interaction position");
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
            if (boat.transform.position.y + .001f < boat.WaterSupportHeight(boat.transform.position, boat.transform.rotation)) throw new Exception("Hull below water support at " + boat.transform.position);
            waterChecks++;
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
