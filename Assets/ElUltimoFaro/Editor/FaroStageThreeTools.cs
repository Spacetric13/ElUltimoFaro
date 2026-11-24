using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ElUltimoFaro;

public static class FaroStageThreeTools
{
    public const string ScenePath = "Assets/Scenes/SampleScene_Android_03.unity";
    const string Generated = "Assets/ElUltimoFaro/Generated";
    static Transform[] All => SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
    [MenuItem("El Último Faro/Etapa 3/Abrir navegación Android")]
    public static void Open() => EditorSceneManager.OpenScene(ScenePath);
    public static void Prepare()
    {
        if (File.Exists(ScenePath)) throw new Exception("Stage 3 already exists; do not overwrite manual edits.");
        FaroStageTwoTools.Open();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        Directory.CreateDirectory(Generated);
        var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layer = tags.FindProperty("layers").GetArrayElementAtIndex(9);
        if (!string.IsNullOrEmpty(layer.stringValue) && layer.stringValue != "FaroWater") throw new Exception("Layer 9 already used");
        layer.stringValue = "FaroWater"; tags.ApplyModifiedPropertiesWithoutUndo();
        var player = UnityEngine.Object.FindAnyObjectByType<MobilePlayerController>();
        var interaction = player.GetComponent<MobileInteraction>();
        // FBX meshes retain names, geometry, materials and world transforms when grouped under a movable pivot.
        var environment = GameObject.Find("faro isla");
        PrefabUtility.UnpackPrefabInstance(environment, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        for (int i = 1; i <= 7; i++) GameObject.Find("pCube" + i).GetComponent<MeshCollider>().enabled = false;
        BuildDockRamp();
        var water = GameObject.Find("mar_low_poly"); water.layer = 9;
        var waterCollider = water.AddComponent<MeshCollider>(); waterCollider.sharedMesh = water.GetComponent<MeshFilter>().sharedMesh;
        var boatRoot = new GameObject("Bote_Navegable");
        var hull = GameObject.Find("pCube22"); var details = GameObject.Find("pCube23");
        Bounds bounds = hull.GetComponent<Renderer>().bounds; bounds.Encapsulate(details.GetComponent<Renderer>().bounds);
        boatRoot.transform.position = bounds.center;
        hull.transform.SetParent(boatRoot.transform, true); details.transform.SetParent(boatRoot.transform, true);
        var home = boatRoot.transform.position; home.x = -.40f; home.z = 1.95f;
        Physics.SyncTransforms();
        if (Physics.Raycast(new Vector3(home.x, 5, home.z), Vector3.down, out var sea, 10, 1 << 9)) home.y = sea.point.y + .055f;
        boatRoot.transform.position = home;
        var hullCollider = boatRoot.AddComponent<BoxCollider>(); hullCollider.size = new Vector3(.48f, .13f, 1);
        var seat = new GameObject("Puesto_jugador").transform; seat.SetParent(boatRoot.transform, false); seat.localPosition = new Vector3(0, -.10f, -.08f);
        var mainPort = new GameObject("Desembarco_Muelle").transform; mainPort.position = new Vector3(.64f, -.272f, 1.68f); mainPort.rotation = Quaternion.Euler(0, 180, 0);
        var secondary = new GameObject("Desembarco_Isla_Cuarzo").transform;
        secondary.position = FindSecondaryLanding(player.transform); secondary.rotation = Quaternion.LookRotation(interaction.Quartz.transform.position - secondary.position);
        secondary.rotation = Quaternion.Euler(0, secondary.eulerAngles.y, 0);
        GuideLight("Luz_guia_muelle", mainPort.position + new Vector3(.36f, .2f, .46f), new Color(1, .56f, .16f));
        GuideLight("Luz_guia_isla", secondary.position + Vector3.up * .045f, new Color(.15f, .85f, 1));
        var safe = GameObject.Find("Android_Controls").transform.Find("SafeArea");
        safe.Find("Title").GetComponent<Text>().text = "EL ÚLTIMO FARO · NAVEGACIÓN";
        var boardButton = Button("Embarcar", "EMBARCAR", safe, Vector2.right, new Vector2(-165, 305), new Vector2(280, 65));
        var status = Label("Navegacion", "", safe, new Vector2(.5f, 1), new Vector2(700, 38), new Vector2(0, -106), 17);
        var boat = boatRoot.AddComponent<BoatController>();
        boat.Configure(player, seat, new[] {mainPort, secondary}, boardButton, safe.Find("MoveLabel").GetComponent<Text>(), status);
        var reset = Button("PruebaBote", "PRUEBA: BOTE AL MUELLE", safe, Vector2.up, new Vector2(145, -190), new Vector2(250, 42));
        UnityEventTools.AddPersistentListener(reset.onClick, boat.ResetForTest);
        QualitySettings.SetQualityLevel(0, true);
        Physics.SyncTransforms();
        if (boat.HullBlocked(home, Quaternion.identity)) throw new Exception("Boat initial position overlaps scenery");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
        File.WriteAllText(Path.GetFullPath("../stage3-preparation.txt"), "Boat=" + home.ToString("F4") + "\nMain landing=" + mainPort.position.ToString("F4") + "\nSecondary landing=" + secondary.position.ToString("F4"));
    }
    static void BuildDockRamp()
    {
        var start = new Vector3(-.42f, .665f, .84f); var end = new Vector3(.70f, -.299f, 1.38f);
        var side = Vector3.Cross(Vector3.up, (end - start).normalized).normalized * .23f;
        var mesh = new Mesh {name = "Superficie_paso_muelle"};
        mesh.vertices = new[] {start - side, start + side, end - side, end + side};
        mesh.triangles = new[] {0, 2, 1, 1, 2, 3}; mesh.RecalculateNormals(); mesh.RecalculateBounds();
        if (mesh.normals[0].y < 0) {mesh.triangles = new[] {0, 1, 2, 1, 3, 2}; mesh.RecalculateNormals();}
        AssetDatabase.CreateAsset(mesh, Generated + "/PasoMuelle.asset");
        var ramp = new GameObject("Paso_suave_muelle"); ramp.layer = 8; ramp.AddComponent<MeshCollider>().sharedMesh = mesh;
    }
    static Vector3 FindSecondaryLanding(Transform player)
    {
        var island = GameObject.Find("polySurface32.003").GetComponent<Renderer>().bounds;
        Vector3 preferred = new Vector3(-11.25f, 0, -15.9f), result = default; float best = float.PositiveInfinity;
        for (float x = island.min.x + .2f; x < island.max.x - .2f; x += .1f)
        for (float z = island.min.z + .2f; z < island.max.z - .2f; z += .1f)
        {
            if (!Physics.Raycast(new Vector3(x, 5, z), Vector3.down, out var hit, 10, 1 << 8) || hit.transform.name != "polySurface32.003" || Vector3.Angle(hit.normal, Vector3.up) > 28) continue;
            Vector3 feet = hit.point + Vector3.up * .03f;
            if (Physics.OverlapCapsule(feet + Vector3.up * .1f, feet + Vector3.up * .48f, .07f, ~(1 << 9), QueryTriggerInteraction.Ignore).Any(c => !c.transform.IsChildOf(player))) continue;
            bool safe = true;
            foreach (var offset in new[] {Vector3.forward, Vector3.back, Vector3.left, Vector3.right})
                if (!Physics.Raycast(feet + offset * .15f + Vector3.up * .2f, Vector3.down, .4f, 1 << 8)) { safe = false; break; }
            if (!safe) continue;
            float score = Vector2.Distance(new Vector2(x, z), new Vector2(preferred.x, preferred.z));
            if (score < best) { best = score; result = feet; }
        }
        if (float.IsPositiveInfinity(best)) throw new Exception("No safe secondary landing");
        return result;
    }
    static void GuideLight(string name, Vector3 position, Color color)
    {
        var beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere); beacon.name = name; beacon.transform.position = position; beacon.transform.localScale = Vector3.one * .14f;
        UnityEngine.Object.DestroyImmediate(beacon.GetComponent<Collider>());
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")); material.SetColor("_BaseColor", color); material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", color * 5);
        AssetDatabase.CreateAsset(material, Generated + "/" + name + ".mat"); beacon.GetComponent<Renderer>().sharedMaterial = material;
        var light = beacon.AddComponent<Light>(); light.type = LightType.Point; light.color = color; light.range = 3; light.intensity = 1.5f; light.shadows = LightShadows.None;
    }
    public static void RefinePresentation()
    {
        Open();
        GameObject.Find("Puesto_jugador").transform.localPosition = new Vector3(0, -.10f, -.08f);
        GameObject.Find("Luz_guia_muelle").transform.position = GameObject.Find("Desembarco_Muelle").transform.position + new Vector3(.36f, .2f, .46f);
        GameObject.Find("Luz_guia_isla").transform.position = GameObject.Find("Desembarco_Isla_Cuarzo").transform.position + Vector3.up * .045f;
        foreach (string name in new[] {"Luz_guia_muelle", "Luz_guia_isla"}) GameObject.Find(name).GetComponent<Light>().intensity = 1.5f;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }
    static Text Label(string name, string text, Transform parent, Vector2 anchor, Vector2 size, Vector2 pos, int font)
    {
        var go = new GameObject(name, typeof(RectTransform)); var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = anchor; rect.sizeDelta = size; rect.anchoredPosition = pos;
        var label = go.AddComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.text = text; label.fontSize = font; label.color = new Color(.95f, .95f, .86f); label.alignment = TextAnchor.MiddleCenter; label.raycastTarget = false; return label;
    }
    static Button Button(string name, string text, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform)); var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = anchor; rect.sizeDelta = size; rect.anchoredPosition = pos;
        var image = go.AddComponent<Image>(); image.color = new Color(.04f, .12f, .15f, .9f); var button = go.AddComponent<Button>(); button.targetGraphic = image;
        Label("Label", text, rect, Vector2.one * .5f, size, Vector2.zero, 18); return button;
    }
    public static void BuildAndroid()
    {
        Open(); FaroAndroidPreparation.ConfigureAndroid();
        PlayerSettings.bundleVersion = "0.3.0"; PlayerSettings.Android.bundleVersionCode = 3;
        EditorBuildSettings.scenes = new[] {new EditorBuildSettingsScene(ScenePath, true)};
        var output = Environment.GetEnvironmentVariable("FARO_APK_PATH"); if (string.IsNullOrWhiteSpace(output)) throw new Exception("FARO_APK_PATH required");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes = new[] {ScenePath}, locationPathName = output, target = BuildTarget.Android, options = BuildOptions.None});
        File.WriteAllText(Path.GetFullPath("../stage3-build-result.txt"), report.summary.result + "\nErrors=" + report.summary.totalErrors);
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Android build failed: " + report.summary.result);
    }
}
