using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class FaroStaticAudit {
 [Serializable] public class Entry { public string scene; public int objects,activeObjects,renderers,enabledRenderers,lights,enabledLights,audioSources,particleSystems;public long meshVertices,meshTriangles; }
 [Serializable] public class Report {public string scope="Inventario estatico en editor sin ejecutar el juego; no mide FPS ni draw calls"; public Entry[] scenes;}
 public static void Run(){
  var data=new Report();data.scenes=new[]{"Hall_Quest","Isla_Quest"}.Select(name=>{
   var s=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");var roots=s.GetRootGameObjects();var ts=roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
   var rs=roots.SelectMany(r=>r.GetComponentsInChildren<Renderer>(true)).ToArray();var ls=roots.SelectMany(r=>r.GetComponentsInChildren<Light>(true)).ToArray();var meshes=roots.SelectMany(r=>r.GetComponentsInChildren<MeshFilter>(true)).Select(m=>m.sharedMesh).Where(m=>m).ToArray();
   return new Entry{scene=name,objects=ts.Length,activeObjects=ts.Count(t=>t.gameObject.activeInHierarchy),renderers=rs.Length,enabledRenderers=rs.Count(r=>r.enabled&&r.gameObject.activeInHierarchy),lights=ls.Length,enabledLights=ls.Count(l=>l.enabled&&l.gameObject.activeInHierarchy),audioSources=roots.Sum(r=>r.GetComponentsInChildren<AudioSource>(true).Length),particleSystems=roots.Sum(r=>r.GetComponentsInChildren<ParticleSystem>(true).Length),meshVertices=meshes.Sum(m=>(long)m.vertexCount),meshTriangles=meshes.Sum(m=>Enumerable.Range(0,m.subMeshCount).Sum(i=>m.GetTopology(i)==MeshTopology.Triangles?(long)m.GetIndexCount(i)/3:0))};
  }).ToArray();Directory.CreateDirectory("Documentation");File.WriteAllText("Documentation/inventario_escenas.json",JsonUtility.ToJson(data,true));
 }
}
