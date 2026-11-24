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
public static class FaroStageTwoVerification
{
    const string Flag = "Faro.StageTwo.Verify";
    static double since;
    static int phase;
    static MobilePlayerController player;
    static MobileInteraction interaction;
    static Vector3 start;
    static FaroStageTwoVerification() { EditorApplication.playModeStateChanged += Changed; }
    public static void Run()
    {
        FaroStageTwoTools.Open();
        QualitySettings.SetQualityLevel(0, true); EditorSettings.asyncShaderCompilation = false;
        SessionState.SetBool(Flag, true); EditorApplication.EnterPlaymode();
    }
    static void Changed(PlayModeStateChange state)
    {
        if (SessionState.GetBool(Flag, false) && state == PlayModeStateChange.EnteredPlayMode)
        { phase = 0; since = EditorApplication.timeSinceStartup; EditorApplication.update += Tick; }
    }
    static void Check(bool value, string message)
    { if (!value) throw new Exception(message); Debug.Log("FARO_STAGE2_PASS " + message); }
    static void Tick()
    {
        if (EditorApplication.timeSinceStartup - since < 1.2) return;
        try
        {
            switch (phase)
            {
                case 0:
                    player = UnityEngine.Object.FindAnyObjectByType<MobilePlayerController>();
                    interaction = player.GetComponent<MobileInteraction>(); start = player.transform.position;
                    Check(Mathf.Abs(Camera.main.transform.localPosition.y - 1.55f / 3) < .001f, "Eye height divided by three");
                    Check(Mathf.Abs(player.GetComponent<CharacterController>().height - 1.7f / 3) < .001f, "Capsule divided by three");
                    Check(player.HasGround(start), "Start supported at smaller scale");
                    Check(!interaction.CanTake(interaction.Quartz), "Distant quartz cannot be collected");
                    Check(!interaction.Lantern.ToggleLight(), "Lantern cannot be used before collection");
                    Application.runInBackground = true; InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    player.SendMessage("OnApplicationFocus", true);
                    var pad = UnityEngine.Object.FindAnyObjectByType<TouchMovePad>();
                    var move = new PointerEventData(EventSystem.current) {pointerId = 20, position = RectTransformUtility.WorldToScreenPoint(null, pad.transform.TransformPoint(new Vector3(0, 65, 0)))};
                    pad.OnPointerDown(move);
                    var look = UnityEngine.Object.FindAnyObjectByType<TouchLookArea>();
                    var drag = new PointerEventData(EventSystem.current) {pointerId = 21, delta = new Vector2(2, 1)};
                    look.OnPointerDown(drag); look.OnDrag(drag);
                    Check(pad.Value.y > .8f && look.ConsumeDelta().x > 0, "Two fingers move and look independently");
                    look.OnPointerUp(drag);
                    break;
                case 1:
                    UnityEngine.Object.FindAnyObjectByType<TouchMovePad>().ResetInput();
                    Check(Vector3.Distance(start, player.transform.position) > .05f, "Small player moves with touch joystick");
                    player.ReturnToStart();
                    Capture("stage2-height.png");
                    interaction.VisitLanternTest();
                    break;
                case 2:
                    Check(player.HasGround(player.transform.position), "Lantern test point is safe ground");
                    Camera.main.transform.LookAt(interaction.Lantern.VisualBounds.center);
                    Check(interaction.CanTake(interaction.Lantern), "Existing lantern reachable from ground");
                    var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.position = Vector3.Lerp(Camera.main.transform.position, interaction.Lantern.VisualBounds.center, .5f);
                    wall.transform.localScale = Vector3.one * .12f; Physics.SyncTransforms();
                    Check(!interaction.CanTake(interaction.Lantern), "Solid obstacle prevents pickup through walls");
                    UnityEngine.Object.DestroyImmediate(wall); Physics.SyncTransforms();
                    GameObject.Find("Recoger").GetComponent<Button>().onClick.Invoke();
                    Check(interaction.Lantern.IsCarried, "Context button collects existing lantern");
                    Check(!interaction.Lantern.Carry(Camera.main.transform), "Duplicate collection rejected");
                    interaction.ToggleLantern();
                    Check(interaction.Lantern.IsLit && interaction.Lantern.LanternLight.enabled, "Lantern lights on");
                    interaction.ToggleLantern();
                    Check(!interaction.Lantern.IsLit && !interaction.Lantern.LanternLight.enabled, "Lantern lights off");
                    interaction.VisitQuartzTest();
                    break;
                case 3:
                    Check(player.HasGround(player.transform.position), "Secondary island test point is safe ground");
                    Camera.main.transform.LookAt(interaction.Quartz.VisualBounds.center);
                    Check(interaction.CanTake(interaction.Quartz), "Existing quartz reachable on its original island");
                    GameObject.Find("Recoger").GetComponent<Button>().onClick.Invoke();
                    Check(interaction.Quartz.IsCarried, "Context button collects existing quartz");
                    Check(!interaction.CanTake(interaction.Quartz), "Carried quartz no longer a pickup target");
                    interaction.ToggleLantern();
                    player.ReturnToStart();
                    Check(interaction.Quartz.transform.IsChildOf(Camera.main.transform) && interaction.Lantern.transform.IsChildOf(Camera.main.transform), "Both original objects retained after returning");
                    break;
                case 4:
                    Capture("stage2-items.png");
                    player.TeleportTo(start + new Vector3(80, -10, 80), 0);
                    break;
                case 5:
                    Check(Vector3.Distance(player.transform.position, start) < .2f, "Out of bounds recovery works at new scale");
                    Check(interaction.Quartz.IsCarried && interaction.Lantern.IsCarried && interaction.Lantern.IsLit, "Recovery preserves inventory and lantern state");
                    File.WriteAllText(Path.GetFullPath("../stage2-verification.txt"), "PASS: one-third camera/capsule, safe spawn, touch movement and independent fingers, unreachable and obstructed pickup rejection, both contextual pickups, duplicate rejection, lantern toggle, safe island test transport, inventory through return and recovery.\nPhone performance/audio/rendering require Samsung A56 validation.");
                    End(0); return;
            }
            phase++; since = EditorApplication.timeSinceStartup;
        }
        catch (Exception ex) { Debug.LogException(ex); File.WriteAllText(Path.GetFullPath("../stage2-verification.txt"), "FAIL: " + ex); End(1); }
    }
    static void End(int exit) { SessionState.SetBool(Flag, false); EditorApplication.update -= Tick; EditorApplication.Exit(exit); }
    static void Capture(string filename)
    {
        var camera = Camera.main; var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = .2f;
        Canvas.ForceUpdateCanvases();
        var texture = new RenderTexture(1280, 720, 24); texture.Create();
        RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest {destination = texture});
        var old = RenderTexture.active; RenderTexture.active = texture;
        var png = new Texture2D(1280, 720, TextureFormat.RGB24, false); png.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); png.Apply();
        File.WriteAllBytes(Path.GetFullPath("../" + filename), png.EncodeToPNG());
        RenderTexture.active = old; texture.Release(); UnityEngine.Object.Destroy(texture); UnityEngine.Object.Destroy(png);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }
}
