using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using ElUltimoFaro;

[InitializeOnLoad]
public static class FaroStageOneVerification
{
    const string Flag="Faro.StageOne.Verify";
    static int phase;
    static double since;
    static MobilePlayerController player;
    static Vector3 start;
    static FaroStageOneVerification() { EditorApplication.playModeStateChanged+=Changed; }
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene_Android_01.unity");
        QualitySettings.SetQualityLevel(0,true);
        EditorSettings.asyncShaderCompilation=false;
        SessionState.SetBool(Flag,true);
        EditorApplication.EnterPlaymode();
    }
    static void Changed(PlayModeStateChange state)
    {
        if(!SessionState.GetBool(Flag,false)) return;
        if(state==PlayModeStateChange.EnteredPlayMode)
        { phase=0; since=EditorApplication.timeSinceStartup; EditorApplication.update+=Tick; }
    }
    static void Check(bool condition,string message) { if(!condition) throw new Exception(message); Debug.Log("FARO_CHECK_PASS "+message); }
    static void Tick()
    {
        try
        {
            if(EditorApplication.timeSinceStartup-since<1) return;
            switch(phase)
            {
                case 0:
                    player=UnityEngine.Object.FindFirstObjectByType<MobilePlayerController>();
                    Check(player,"Player exists");
                    Check(player.HasGround(player.transform.position),"Player starts on island ground");
                    Check(Mathf.Abs(Camera.main.transform.localPosition.y-1.55f)<0.001f,"Camera height 1.55m");
                    foreach(var n in new[]{"polySurface32","polySurface32.001","polySurface32.002","polySurface32.003"})
                        Check(GameObject.Find(n).GetComponent<MeshCollider>(),n+" has collider");
                    Check(GameObject.Find("Maquinaria_Antigua_Cuarzo"),"Existing machinery preserved");
                    Check(GameObject.Find("cuarzo"),"Existing quartz preserved");
                    var pad=UnityEngine.Object.FindFirstObjectByType<TouchMovePad>();
                    var e=new PointerEventData(EventSystem.current) {pointerId=10, position=RectTransformUtility.WorldToScreenPoint(null,pad.transform.TransformPoint(new Vector3(60,0,0)))};
                    pad.OnPointerDown(e);
                    Check(pad.Value.x>0.5f,"Touch joystick produces movement");
                    pad.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=11});
                    Check(pad.Value.x>0.5f,"Second finger cannot release joystick");
                    pad.OnPointerUp(e);
                    Check(pad.Value==Vector2.zero,"Joystick resets on release");
                    var look=UnityEngine.Object.FindFirstObjectByType<TouchLookArea>();
                    e.delta=new Vector2(40,20); look.OnPointerDown(e); look.OnDrag(e);
                    Check(look.ConsumeDelta().x>0,"Touch look accepts drag");
                    Check(look.ConsumeDelta()==Vector2.zero,"Touch look consumes each drag once");
                    look.OnPointerUp(e);
                    start=player.transform.position;
                    // Batch mode has no focused Game view. Simulate the foreground phone session explicitly.
                    Application.runInBackground=true;
                    InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                    player.SendMessage("OnApplicationFocus",true);
                    var moveEvent=new PointerEventData(EventSystem.current) {pointerId=20, position=RectTransformUtility.WorldToScreenPoint(null,pad.transform.TransformPoint(new Vector3(0,65,0)))};
                    pad.OnPointerDown(moveEvent);
                    Debug.Log("FARO_MOVEMENT_INPUT "+pad.Value+" groundAhead="+player.HasGround(start+player.transform.forward*0.3f));
                    break;
                case 1:
                    var activePad=UnityEngine.Object.FindFirstObjectByType<TouchMovePad>();
                    Debug.Log("FARO_MOVEMENT_AFTER value="+activePad.Value);
                    activePad.ResetInput();
                    Check(Vector3.Distance(start,player.transform.position)>0.08f,"Player moves with touch control: "+start+" -> "+player.transform.position+" dt="+Time.deltaTime);
                    player.ReturnToStart();
                    Check(Vector3.Distance(start,player.transform.position)<0.15f,"Return to start works");
                    var body=player.GetComponent<CharacterController>(); body.enabled=false;
                    player.transform.position=start+new Vector3(80,-10,80); body.enabled=true;
                    break;
                case 2:
                    Check(Vector3.Distance(start,player.transform.position)<0.2f,"Out of bounds recovery works");
                    File.WriteAllText(Path.GetFullPath("../play-verification.txt"),"PASS: ground, camera, preserved models, multitouch isolation, movement, reset, recovery.\nDevice touch latency and Android hardware remain untested.");
                    Capture();
                    SessionState.SetBool(Flag,false);
                    EditorApplication.update-=Tick;
                    EditorApplication.Exit(0);
                    return;
            }
            phase++; since=EditorApplication.timeSinceStartup;
        }
        catch(Exception ex)
        {
            Debug.LogException(ex); SessionState.SetBool(Flag,false); EditorApplication.update-=Tick;
            File.WriteAllText(Path.GetFullPath("../play-verification.txt"),"FAIL: "+ex);
            EditorApplication.Exit(1);
        }
    }
    static void Capture()
    {
        var camera=Camera.main;
        var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=0.3f;
        Canvas.ForceUpdateCanvases();
        var texture=new RenderTexture(1280,720,24);
        texture.Create();
        var request=new UniversalRenderPipeline.SingleCameraRequest {destination=texture};
        RenderPipeline.SubmitRenderRequest(camera,request);
        var old=RenderTexture.active; RenderTexture.active=texture;
        var png=new Texture2D(1280,720,TextureFormat.RGB24,false); png.ReadPixels(new Rect(0,0,1280,720),0,0); png.Apply();
        File.WriteAllBytes(Path.GetFullPath("../recorrido-preview.png"),png.EncodeToPNG());
        RenderTexture.active=old; texture.Release(); UnityEngine.Object.Destroy(texture); UnityEngine.Object.Destroy(png);
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;
    }
}
