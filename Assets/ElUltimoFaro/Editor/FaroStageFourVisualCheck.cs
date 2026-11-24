using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using ElUltimoFaro;
[InitializeOnLoad]
public static class FaroStageFourVisualCheck
{
    const string Flag = "Faro.Stage4.Visual";
    static int phase;
    static double since;
    static MobilePlayerController player;
    static QuartzSocketTrigger socket;
    static MobileInteraction items;
    static LighthouseBlink lighthouse;
    static FaroStageFourVisualCheck() { EditorApplication.playModeStateChanged += Changed; }
    public static void Run() { FaroStageFourTools.Open(); QualitySettings.SetQualityLevel(0, true); SessionState.SetBool(Flag, true); EditorApplication.EnterPlaymode(); }
    static void Changed(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Flag, false)) { phase = 0; since = EditorApplication.timeSinceStartup; EditorApplication.update += Tick; }
    }
    static void Aim(Vector3 target)
    {
        Vector3 delta = target - Camera.main.transform.position;
        player.transform.rotation = Quaternion.Euler(0, Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg, 0);
        float pitch = -Mathf.Atan2(delta.y, new Vector2(delta.x, delta.z).magnitude) * Mathf.Rad2Deg;
        typeof(MobilePlayerController).GetField("pitch", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, pitch);
        Camera.main.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }
    static void Tick()
    {
        if (EditorApplication.timeSinceStartup - since < 3) return;
        try
        {
            switch (phase)
            {
                case 0:
                    Application.runInBackground = true;
                    player = UnityEngine.Object.FindAnyObjectByType<MobilePlayerController>(); player.SendMessage("OnApplicationFocus", true);
                    socket = UnityEngine.Object.FindAnyObjectByType<QuartzSocketTrigger>(); items = player.GetComponent<MobileInteraction>(); lighthouse = UnityEngine.Object.FindAnyObjectByType<LighthouseBlink>();
                    items.VisitQuartzTest(); Aim(items.Quartz.VisualBounds.center); items.TakeNearby();
                    if (!items.Quartz.IsCarried) throw new Exception("Visual setup pickup failed");
                    player.TeleportTo(new Vector3(-.30f, .72f, -1.40f), 90); Aim(socket.SnapPoint.position);
                    break;
                case 1:
                    if (!player.GetComponent<MachineInteraction>().InReach()) throw new Exception("Visual vantage unreachable");
                    Capture("stage4-before-insertion.png"); items.TakeNearby();
                    if (!socket.IsInserted) throw new Exception("Visual insertion failed");
                    break;
                case 2:
                    Aim(socket.SnapPoint.position); Capture("stage4-machine-close.png"); items.TakeNearby(); break;
                case 3:
                    Capture("stage4-lever-on.png");
                    player.TeleportTo(new Vector3(-1.3f, .72f, -.7f), 90); Aim(lighthouse.lampGlowRenderer.bounds.center - Vector3.up * .8f);
                    break;
                case 4:
                    Capture("stage4-lighthouse-wide.png");
                    Debug.Log("FARO_VISUAL_PASS"); End(0); return;
            }
            phase++; since = EditorApplication.timeSinceStartup;
        }
        catch (Exception e) { Debug.LogException(e); End(1); }
    }
    static void Capture(string name)
    {
        Canvas.ForceUpdateCanvases(); var camera = Camera.main; var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = .2f; Canvas.ForceUpdateCanvases();
        var rt = new RenderTexture(1600, 900, 24); rt.Create(); RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = rt });
        var old = RenderTexture.active; RenderTexture.active = rt; var texture = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); texture.Apply(); File.WriteAllBytes(Path.GetFullPath("../" + name), texture.EncodeToPNG());
        RenderTexture.active = old; rt.Release(); UnityEngine.Object.Destroy(rt); UnityEngine.Object.Destroy(texture); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }
    static void End(int code) { SessionState.SetBool(Flag, false); EditorApplication.update -= Tick; EditorApplication.Exit(code); }
}
