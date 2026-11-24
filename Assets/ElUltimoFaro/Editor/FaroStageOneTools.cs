using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FaroStageOneTools
{
    const string ScenePath = "Assets/Scenes/SampleScene_Android_01.unity";
    public static void Materials()
    {
        EditorSceneManager.OpenScene(ScenePath);
        var text=new StringBuilder();
        foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if(r.name!="Tower_Red_Lower" && r.name!="polySurface32" && r.name!="Verde_hoja") continue;
            foreach(var m in r.sharedMaterials)
            {
                text.AppendLine(r.name+" "+m.name+" "+m.shader.name);
                for(int i=0;i<m.shader.GetPropertyCount();i++)
                    if(m.shader.GetPropertyType(i)==UnityEngine.Rendering.ShaderPropertyType.Color)
                        text.AppendLine(m.shader.GetPropertyName(i)+"="+m.GetColor(m.shader.GetPropertyName(i)));
            }
        }
        File.WriteAllText(Path.GetFullPath("../material-colors.txt"),text.ToString());
    }
    public static void Audit()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var report = new StringBuilder();
        foreach (var root in scene.GetRootGameObjects())
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            string path = t.name;
            for(var parent=t.parent; parent != null; parent=parent.parent) path=parent.name+"/"+path;
            report.AppendLine(path+" | position="+t.position.ToString("F3")+" | scale="+t.lossyScale.ToString("F3")+
                " | components="+string.Join(",", t.GetComponents<Component>().Select(c=>c?c.GetType().Name:"MISSING")));
            var r=t.GetComponent<Renderer>();
            if(r) report.AppendLine("  bounds="+r.bounds+" materials="+string.Join(";",r.sharedMaterials.Select(m=>m ? m.name+":"+(m.shader?m.shader.name:"MISSING SHADER"):"NULL")));
            var mesh=t.GetComponent<MeshFilter>();
            if(mesh && mesh.sharedMesh) report.AppendLine("  vertices="+mesh.sharedMesh.vertexCount);
        }
        File.WriteAllText(Path.GetFullPath("../scene-audit.txt"),report.ToString());
        Debug.Log("FARO_AUDIT_OK "+scene.rootCount);
    }
}
