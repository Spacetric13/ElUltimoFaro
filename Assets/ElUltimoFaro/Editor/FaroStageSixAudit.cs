using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
public static class FaroStageSixAudit
{
 public static void Run()
 {
  FaroStageFiveTools.Open();var sb=new StringBuilder();
  foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
  {
   var m=r.GetComponent<MeshFilter>().sharedMesh;
   sb.AppendLine(r.name+" | root="+r.transform.root.name+" parent="+r.transform.parent?.name+" | pos="+r.transform.position.ToString("F3")+" euler="+r.transform.eulerAngles.ToString("F1")+" scale="+r.transform.lossyScale.ToString("F4")+" | bounds="+r.bounds.center.ToString("F3")+" size="+r.bounds.size.ToString("F3")+" | verts="+m.vertexCount+" sub="+m.subMeshCount+" mats="+string.Join(",",r.sharedMaterials.Select(x=>x?x.name:"null"))+" coll="+string.Join(",",r.GetComponents<Collider>().Select(c=>c.GetType().Name+":"+c.enabled)));
   if(r.name=="Door"||r.name=="Tower_Red_Lower")sb.AppendLine("LOCAL bounds="+m.bounds+" path="+AssetDatabase.GetAssetPath(m));
  }
  File.WriteAllText(Path.GetFullPath("../stage6-audit.txt"),sb.ToString());Debug.Log("STAGE6_AUDIT_DONE");
 }
}
