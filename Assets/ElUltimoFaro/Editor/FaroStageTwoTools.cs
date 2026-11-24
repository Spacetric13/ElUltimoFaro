using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ElUltimoFaro;

public static class FaroStageTwoTools
{
    public const string ScenePath = "Assets/Scenes/SampleScene_Android_02.unity";
    static Transform[] All => SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
    [MenuItem("El Último Faro/Etapa 2/Abrir cuarzo y farol Android")]
    public static void Open() => EditorSceneManager.OpenScene(ScenePath);
    public static void Prepare()
    {
        if (File.Exists(ScenePath)) throw new Exception("Stage 2 already exists; preserve manual edits.");
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene_Android_01.unity");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        // Apply the user's latest saved tree position from SampleScene to this new scene only.
        SyncSavedTree();
        var player = UnityEngine.Object.FindFirstObjectByType<MobilePlayerController>();
        var body = player.GetComponent<CharacterController>();
        body.enabled = false; body.stepOffset = .18f; body.height = 1.7f / 3;
        body.radius = .2f / 3; body.center = new Vector3(0, .85f / 3, 0); body.skinWidth = .025f / 3;
        body.enabled = true;
        var camera = Camera.main; camera.transform.localPosition = new Vector3(0, 1.55f / 3, 0); camera.nearClipPlane = .016f;
        player.ConfigureScale(1f / 3);
        // Existing dock boards and stairs become walkable. Boat meshes 22/23 remain untouched.
        for (int i = 1; i <= 15; i++)
        {
            var board = All.Single(t => t.root.name == "faro isla" && t.name == "pCube" + i);
            board.gameObject.layer = 8;
        }
        var quartzRoot = All.Single(t => t.name == "cuarzo");
        var lampRoot = All.Single(t => t.name == "Farol_de_mano");
        var quartz = quartzRoot.gameObject.AddComponent<PortableItem>(); quartz.Configure(PortableItem.ItemKind.Quartz);
        var globe = lampRoot.GetComponentsInChildren<Renderer>().Single(r => r.name == "07_Globo_luminoso");
        var lightObject = new GameObject("Luz_portatil"); lightObject.transform.SetParent(lampRoot, false);
        lightObject.transform.position = globe.bounds.center;
        var light = lightObject.AddComponent<Light>(); light.type = LightType.Point;
        light.color = new Color(1, .68f, .28f); light.intensity = 1.8f; light.range = 1.7f; light.shadows = LightShadows.None; light.enabled = false;
        var lantern = lampRoot.gameObject.AddComponent<PortableItem>(); lantern.Configure(PortableItem.ItemKind.Lantern, light, globe);
        var qHand = new GameObject("Mano_cuarzo").transform; qHand.SetParent(camera.transform, false); qHand.localPosition = new Vector3(.075f, -.11f, .3f);
        var lHand = new GameObject("Mano_farol").transform; lHand.SetParent(camera.transform, false); lHand.localPosition = new Vector3(-.12f, -.11f, .3f);
        Physics.SyncTransforms();
        var quartzSpawn = FindVantage(quartz, true, player.transform);
        var lanternSpawn = FindVantage(lantern, false, player.transform);
        var safe = GameObject.Find("Android_Controls").transform.Find("SafeArea");
        safe.Find("Title").GetComponent<Text>().text = "EL ÚLTIMO FARO · CUARZO Y FAROL";
        var take = Button("Recoger", "ACÉRCATE Y MIRA", safe, Vector2.right, new Vector2(-165, 145), new Vector2(280, 65));
        var toggle = Button("EncenderFarol", "ENCENDER FAROL", safe, Vector2.right, new Vector2(-165, 225), new Vector2(280, 65));
        var status = Label("Inventario", "", safe, new Vector2(.5f, 1), new Vector2(700, 32), new Vector2(0, -65), 18);
        var message = Label("Aviso", "", safe, new Vector2(.5f, .5f), new Vector2(640, 70), new Vector2(0, -100), 21);
        Label("Reticula", "+", safe, new Vector2(.5f, .5f), new Vector2(25, 25), Vector2.zero, 22);
        var interaction = player.gameObject.AddComponent<MobileInteraction>();
        interaction.Configure(camera, player, quartz, lantern, qHand, lHand, take, toggle, status, message, quartzSpawn, lanternSpawn);
        var quartzTest = Button("PruebaCuarzo", "PRUEBA: ISLA CUARZO", safe, Vector2.up, new Vector2(145, -140), new Vector2(250, 42));
        var lampTest = Button("PruebaFarol", "PRUEBA: IR AL FAROL", safe, Vector2.up, new Vector2(145, -90), new Vector2(250, 42));
        UnityEventTools.AddPersistentListener(quartzTest.onClick, interaction.VisitQuartzTest);
        UnityEventTools.AddPersistentListener(lampTest.onClick, interaction.VisitLanternTest);
        QualitySettings.SetQualityLevel(0, true);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
        File.WriteAllText(Path.GetFullPath("../stage2-preparation.txt"), "Eye height=" + camera.transform.localPosition.y + "\nCapsule=" + body.height + "\nQuartz test=" + quartzSpawn.ToString("F4") + "\nLantern test=" + lanternSpawn.ToString("F4"));
        Debug.Log("FARO_STAGE2_PREPARED");
    }
    static void SyncSavedTree()
    {
        var tree = All.Single(t => t.name == "Arbol_hoja_ancha 1 (4)");
        var sourceScene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Additive);
        var sourceTree = sourceScene.GetRootGameObjects().Single(t => t.name == tree.name).transform;
        tree.localPosition = sourceTree.localPosition;
        PrefabUtility.RecordPrefabInstancePropertyModifications(tree);
        EditorSceneManager.CloseScene(sourceScene, true);
    }
    static Vector3 FindVantage(PortableItem item, bool secondary, Transform player)
    {
        Vector3 target = item.VisualBounds.center;
        float best = float.PositiveInfinity; Vector3 result = default;
        for (float x = target.x - 1.5f; x < target.x + 1.5f; x += .08f)
        for (float z = target.z - 1.5f; z < target.z + 1.5f; z += .08f)
        {
            if (!Physics.Raycast(new Vector3(x, 5, z), Vector3.down, out var hit, 10, 1 << 8)) continue;
            if ((hit.transform.name == "polySurface32.003") != secondary || Vector3.Angle(hit.normal, Vector3.up) > 30) continue;
            Vector3 feet = hit.point + Vector3.up * .025f, eye = feet + Vector3.up * (1.55f / 3);
            Vector3 delta = target - eye;
            if (delta.magnitude > 1.55f || delta.magnitude < .35f || Mathf.Abs(delta.normalized.y) > .86f) continue;
            if (Physics.OverlapCapsule(feet + Vector3.up * .09f, feet + Vector3.up * .48f, .075f, ~0, QueryTriggerInteraction.Ignore)
                .Any(c => !c.transform.IsChildOf(player))) continue;
            if (Physics.RaycastAll(eye, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore)
                .Any(h => !h.transform.IsChildOf(player) && !h.transform.IsChildOf(item.transform))) continue;
            bool safe = true;
            foreach (var offset in new[] {Vector3.forward, Vector3.back, Vector3.left, Vector3.right})
                if (!Physics.Raycast(feet + offset * .12f + Vector3.up * .15f, Vector3.down, .33f, 1 << 8)) { safe = false; break; }
            if (!safe) continue;
            float score = Mathf.Abs(delta.magnitude - .7f);
            if (score < best) { best = score; result = feet; }
        }
        if (float.IsPositiveInfinity(best)) throw new Exception("No reachable safe pickup viewpoint for " + item.name);
        return result;
    }
    static Text Label(string name, string text, Transform parent, Vector2 anchor, Vector2 size, Vector2 position, int font)
    {
        var go = new GameObject(name, typeof(RectTransform)); var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor; rt.sizeDelta = size; rt.anchoredPosition = position;
        var label = go.AddComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = font; label.text = text; label.color = new Color(.95f, .95f, .86f); label.alignment = TextAnchor.MiddleCenter; label.raycastTarget = false;
        return label;
    }
    static Button Button(string name, string text, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform)); var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor; rt.sizeDelta = size; rt.anchoredPosition = pos;
        var image = go.AddComponent<Image>(); image.color = new Color(.04f, .12f, .15f, .9f);
        var button = go.AddComponent<Button>(); button.targetGraphic = image;
        Label("Label", text, rt, new Vector2(.5f, .5f), size, Vector2.zero, 18);
        return button;
    }
    public static void BuildAndroid()
    {
        Open();
        FaroAndroidPreparation.ConfigureAndroid();
        PlayerSettings.bundleVersion = "0.2.0"; PlayerSettings.Android.bundleVersionCode = 2;
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        var output = Environment.GetEnvironmentVariable("FARO_APK_PATH");
        if (string.IsNullOrWhiteSpace(output)) throw new Exception("FARO_APK_PATH required");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes = new[] {ScenePath}, locationPathName = output, target = BuildTarget.Android, options = BuildOptions.None});
        File.WriteAllText(Path.GetFullPath("../stage2-build-result.txt"), report.summary.result + "\nErrors=" + report.summary.totalErrors);
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Stage 2 Android build: " + report.summary.result);
    }
}
