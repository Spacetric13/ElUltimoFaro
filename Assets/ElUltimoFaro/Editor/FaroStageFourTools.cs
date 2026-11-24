using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ElUltimoFaro;

public static class FaroStageFourTools
{
    public const string ScenePath = "Assets/Scenes/SampleScene_Android_04.unity";
    [MenuItem("El Último Faro/Etapa 4/Abrir máquina y faro Android")]
    public static void Open() => EditorSceneManager.OpenScene(ScenePath);
    static Transform Find(Transform root, string name) => root.GetComponentsInChildren<Transform>(true).First(t => t.name == name);
    static void Ref(UnityEngine.Object obj, string field, UnityEngine.Object value) { var so = new SerializedObject(obj); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    static void Number(UnityEngine.Object obj, string field, float value) { var so = new SerializedObject(obj); so.FindProperty(field).floatValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    static void Unpack(GameObject root) { if (PrefabUtility.IsPartOfPrefabInstance(root)) PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(root), PrefabUnpackMode.Completely, InteractionMode.AutomatedAction); }
    static Transform Pivot(Transform root, string name, Vector3 position, params Transform[] parts)
    {
        var pivot = new GameObject(name).transform; pivot.position = position; pivot.SetParent(root, true);
        foreach (var part in parts)
        {
            part.SetParent(pivot, true);
            foreach (var t in part.GetComponentsInChildren<Transform>()) t.gameObject.isStatic = false;
            foreach (var collider in part.GetComponentsInChildren<Collider>()) collider.enabled = false;
        }
        return pivot;
    }
    static Light LightAt(Transform parent, string name, Vector3 world, Color color, float range)
    {
        var go = new GameObject(name); go.transform.position = world; go.transform.SetParent(parent, true);
        var light = go.AddComponent<Light>(); light.type = LightType.Point; light.color = color; light.range = range; light.shadows = LightShadows.None; light.enabled = false; return light;
    }
    public static void Prepare()
    {
        if (File.Exists(ScenePath)) throw new Exception("Stage 4 already exists: preserve manual edits");
        FaroStageThreeTools.Open(); EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        var player = UnityEngine.Object.FindAnyObjectByType<MobilePlayerController>();
        var items = player.GetComponent<MobileInteraction>(); Number(player, "moveSpeed", .9f);
        var boat = UnityEngine.Object.FindAnyObjectByType<BoatController>();
        var boatData = new SerializedObject(boat); boatData.FindProperty("sampleHullWater").boolValue = true; boatData.ApplyModifiedPropertiesWithoutUndo();
        Physics.SyncTransforms();
        Vector3 home = boat.transform.position; home.y = boat.WaterSupportHeight(home, boat.transform.rotation); boat.transform.position = home;
        boatData.Update(); boatData.FindProperty("homePosition").vector3Value = home; boatData.ApplyModifiedPropertiesWithoutUndo();
        var root = GameObject.Find("Maquinaria_Antigua_Cuarzo").transform; Unpack(root.gameObject);
        var all = root.GetComponentsInChildren<Transform>();
        var controller = root.gameObject.AddComponent<QuartzMachineController>();
        foreach (var spec in new[] {new[] {"Flywheel", "flywheel"}, new[] {"MainGear", "mainGear"}, new[] {"SecondaryGear", "secondaryGear"}, new[] {"EnergyCore", "energyCore"}})
        {
            var mesh = Find(root, spec[0]);
            var parts = all.Where(t => t.name == spec[0] || t.name.StartsWith(spec[0] + "_")).ToArray();
            Ref(controller, spec[1], Pivot(root, "Pivot_" + spec[0], mesh.GetComponent<Renderer>().bounds.center, parts));
        }
        var indicator = Find(root, "IndicatorLamp").GetComponent<Renderer>();
        var core = Find(root, "EnergyCore").GetComponent<Renderer>();
        Ref(controller, "indicatorRenderer", indicator); Ref(controller, "coreRenderer", core);
        Ref(controller, "indicatorLight", LightAt(root, "Luz_indicador", indicator.bounds.center, Color.green, .6f));
        Ref(controller, "coreLight", LightAt(root, "Luz_nucleo", core.bounds.center, new Color(.1f, .8f, 1), .8f));
        var loop = root.gameObject.AddComponent<AudioSource>(); loop.playOnAwake = false; loop.loop = true; loop.spatialBlend = 1; loop.minDistance = .35f; loop.maxDistance = 5; loop.volume = 0;
        Ref(controller, "machineLoop", loop);
        var socketMesh = Find(root, "QuartzSocket");
        var inner = Find(root, "QuartzSocket_Inner").GetComponent<Renderer>();
        Vector3 normal = (inner.bounds.center - Find(root, "QuartzSocket_Housing").GetComponent<Renderer>().bounds.center).normalized;
        var snap = new GameObject("QuartzSnapPoint").transform; snap.SetPositionAndRotation(inner.bounds.center + normal * .025f, Quaternion.FromToRotation(Vector3.up, normal)); snap.SetParent(root, true);
        var socket = socketMesh.gameObject.AddComponent<QuartzSocketTrigger>(); Ref(socket, "machine", controller); Ref(socket, "snapPoint", snap); Ref(socket, "quartz", items.Quartz);
        var trigger = new GameObject("Zona_insercion_cuarzo"); trigger.transform.position = snap.position; trigger.transform.SetParent(root, true);
        var zone = trigger.AddComponent<SphereCollider>(); zone.isTrigger = true; zone.radius = .075f;
        // The touch action validates distance and visibility, then calls this same socket.
        var faro = GameObject.Find("El_Ultimo_Faro_LowPoly").transform; Unpack(faro.gameObject);
        var glow = Find(faro, "LampGlow").GetComponent<Renderer>(); var beam = Find(faro, "LightBeam").GetComponent<Renderer>();
        var beamPivot = Pivot(faro, "Pivot_haz_faro", glow.bounds.center, beam.transform);
        var blink = faro.gameObject.AddComponent<LighthouseBlink>(); blink.lampGlowRenderer = glow; blink.beamRenderer = beam; blink.beamPivot = beamPivot; blink.startOn = false;
        var beacon = LightAt(beamPivot, "Luz_principal_faro", glow.bounds.center, new Color(1, .65f, .25f), 24); beacon.type = LightType.Spot; beacon.spotAngle = 50; beacon.innerSpotAngle = 30;
        beacon.transform.rotation = Quaternion.LookRotation(beam.bounds.center - glow.bounds.center); blink.beaconLight = beacon;
        var leverPart = Find(root, "Lever"); var leverBase = Find(root, "Lever_Base");
        var leverPivot = Pivot(root, "Pivot_palanca", leverBase.GetComponent<Renderer>().bounds.center, leverPart, Find(root, "Lever_Handle"));
        var lever = root.gameObject.AddComponent<MachineLever>(); Ref(lever, "machine", controller); Ref(lever, "lever", leverPivot); Ref(lever, "lighthouse", blink);
        var safe = GameObject.Find("Android_Controls").transform.Find("SafeArea"); safe.Find("Title").GetComponent<Text>().text = "EL ÚLTIMO FARO · RECUPERAR LA LUZ";
        var status = UnityEngine.Object.Instantiate(safe.Find("Navegacion").gameObject, safe).GetComponent<Text>(); status.name = "Estado_maquina"; status.rectTransform.anchoredPosition = new Vector2(0, -145); status.text = "Encuentra el cuarzo en la isla secundaria";
        var interaction = player.gameObject.AddComponent<MachineInteraction>();
        Ref(interaction, "player", player); Ref(interaction, "items", items); Ref(interaction, "view", Camera.main); Ref(interaction, "machineRoot", root); Ref(interaction, "target", snap);
        Ref(interaction, "machine", controller); Ref(interaction, "socket", socket); Ref(interaction, "lever", lever); Ref(interaction, "lighthouse", blink); Ref(interaction, "status", status); Ref(items, "machinery", interaction);
        Physics.SyncTransforms();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
        File.WriteAllText(Path.GetFullPath("../stage4-preparation.txt"), "Home=" + home + "\nSocket=" + snap.position + " normal=" + normal + "\nMachine=" + root.position + "\nBeacon=" + glow.bounds.center);
        RefineTraversal();
    }
    public static void BuildAndroid()
    {
        Open(); FaroAndroidPreparation.ConfigureAndroid();
        PlayerSettings.bundleVersion = "0.4.0"; PlayerSettings.Android.bundleVersionCode = 4;
        EditorBuildSettings.scenes = new[] {new EditorBuildSettingsScene(ScenePath, true)};
        var output = Environment.GetEnvironmentVariable("FARO_APK_PATH"); if (string.IsNullOrWhiteSpace(output)) throw new Exception("FARO_APK_PATH required");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes = new[] {ScenePath}, locationPathName = output, target = BuildTarget.Android, options = BuildOptions.None});
        File.WriteAllText(Path.GetFullPath("../stage4-build-result.txt"), report.summary.result + "\nErrors=" + report.summary.totalErrors);
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Android build failed: " + report.summary.result);
    }
    [MenuItem("El Último Faro/Etapa 4/Compilar APK Android")]
    public static void BuildFromMenu()
    {
        var path = EditorUtility.SaveFilePanel("Guardar APK de la etapa 4", Path.GetFullPath(".."), "ElUltimoFaro_04_Maquina_Faro", "apk");
        if (string.IsNullOrEmpty(path)) return;
        var previous = Environment.GetEnvironmentVariable("FARO_APK_PATH");
        try { Environment.SetEnvironmentVariable("FARO_APK_PATH", path); BuildAndroid(); }
        finally { Environment.SetEnvironmentVariable("FARO_APK_PATH", previous); }
    }
    public static void AuditDock()
    {
        Open(); Physics.SyncTransforms();
        var player = UnityEngine.Object.FindAnyObjectByType<MobilePlayerController>();
        foreach (var pair in new[] {new[] {new Vector3(.5933f, -.1811f, 1.3286f), new Vector3(.7f, -.27f, 1.38f)}, new[] {new Vector3(-.3389f, .6212f, .8791f), new Vector3(-.42f, .665f, .84f)}})
        {
            var direction = pair[1] - pair[0]; direction.y = 0; direction.Normalize();
            foreach (float distance in new[] {.09f, .10f, .102f, .12f, .132f})
            {
                var point = pair[0] + direction * distance;
                Debug.Log("FARO_PROBE " + point.ToString("F4") + " supported=" + player.HasGround(point));
                foreach (var hit in Physics.RaycastAll(point + Vector3.up * .2f, Vector3.down, .5f, 1 << 8)) Debug.Log("FARO_HIT " + hit.transform.name + " dist=" + hit.distance + " angle=" + Vector3.Angle(hit.normal, Vector3.up));
            }
        }
    }
    public static void RefineTraversal()
    {
        Open();
        if (GameObject.Find("Paso_continuo_tablas")) throw new Exception("Dock refinement already applied; preserve manual edits");
        var boards = Enumerable.Range(8, 8).Select(i => GameObject.Find("pCube" + i)).ToArray();
        var bounds = boards[0].GetComponent<Renderer>().bounds;
        foreach (var board in boards) { bounds.Encapsulate(board.GetComponent<Renderer>().bounds); board.GetComponent<MeshCollider>().enabled = false; }
        float y = bounds.max.y;
        var deck = new GameObject("Paso_continuo_tablas"); deck.layer = 8;
        var deckMesh = Quad(new Vector3(bounds.min.x, y, bounds.min.z), new Vector3(bounds.max.x, y, bounds.min.z), new Vector3(bounds.min.x, y, bounds.max.z), new Vector3(bounds.max.x, y, bounds.max.z));
        AssetDatabase.CreateAsset(deckMesh, "Assets/ElUltimoFaro/Generated/PasoTablas_Etapa4.asset"); deck.AddComponent<MeshCollider>().sharedMesh = deckMesh;
        var start = new Vector3(-.42f, .665f, .84f); var end = new Vector3(.70f, -.299f, 1.38f);
        var direction = (end - start).normalized; start -= direction * .23f;
        var side = Vector3.Cross(Vector3.up, direction).normalized * .23f;
        var rampMesh = Quad(start - side, start + side, end - side, end + side);
        AssetDatabase.CreateAsset(rampMesh, "Assets/ElUltimoFaro/Generated/PasoMuelle_Etapa4.asset"); GameObject.Find("Paso_suave_muelle").GetComponent<MeshCollider>().sharedMesh = rampMesh;
        var trigger = GameObject.Find("Zona_insercion_cuarzo"); trigger.transform.SetParent(UnityEngine.Object.FindAnyObjectByType<QuartzSocketTrigger>().transform, true);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
    }
    static Mesh Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        var mesh = new Mesh(); mesh.vertices = new[] {a, b, c, d}; mesh.triangles = new[] {0, 2, 1, 1, 2, 3}; mesh.RecalculateNormals();
        if (mesh.normals[0].y < 0) { mesh.triangles = new[] {0, 1, 2, 1, 3, 2}; mesh.RecalculateNormals(); }
        mesh.RecalculateBounds(); return mesh;
    }
}
