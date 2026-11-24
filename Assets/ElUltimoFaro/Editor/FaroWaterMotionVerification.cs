using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using ElUltimoFaro;

[InitializeOnLoad]
public static class FaroWaterMotionVerification
{
 const string Flag="Faro.WaterMotion.Verify";
 static LowPolyWater water;
 static Mesh mesh;
 static Vector3[] start,original;
 static double began;
 static int phase;
 static int frame=-1;
 static float largestChange,largestError;
 static readonly StringBuilder report=new StringBuilder();
 static FaroWaterMotionVerification(){EditorApplication.playModeStateChanged+=Changed;}
 public static void Prepare()
 {
  FaroStageFiveTools.Open();var w=UnityEngine.Object.FindAnyObjectByType<LowPolyWater>();
  var source=w.GetComponent<MeshFilter>().sharedMesh;
  if(source.subMeshCount!=1)throw new Exception("Expected one water submesh");
  w.Surface.localVertices=source.vertices;EditorUtility.SetDirty(w.Surface);AssetDatabase.SaveAssets();
  Debug.Log("WATER_SOURCE readable="+source.isReadable+" vertices="+source.vertexCount+" triangles="+source.triangles.Length/3);
 }
 public static void Run(){FaroStageFiveTools.Open();QualitySettings.SetQualityLevel(0,true);SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();}
 static void Changed(PlayModeStateChange s)
 {
  if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Flag,false)){phase=0;began=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;}
 }
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);report.AppendLine("PASS: "+message);}
 static void Tick()
 {
  try
  {
   Application.runInBackground=true;
   if(phase>0&&frame==Time.frameCount)return;frame=Time.frameCount;
   if(phase==0)
   {
    if(EditorApplication.timeSinceStartup-began<2)return;
    Application.runInBackground=true;
    water=UnityEngine.Object.FindAnyObjectByType<LowPolyWater>();mesh=water.GetComponent<MeshFilter>().sharedMesh;
    UnityEngine.Object.FindAnyObjectByType<WeatherDirector>().enabled=false;
    Check(mesh!=water.OriginalMesh,"Visible water is an independent runtime mesh");
    Check(mesh.vertexCount==7372&&mesh.triangles.SequenceEqual(water.Surface.triangles),"Original 7372 vertices and 2601 triangles retained");
    original=water.OriginalMesh.vertices;
    Check(original.SequenceEqual(water.Surface.localVertices),"Baked coordinates exactly match the imported model");
    var camera=Camera.main;camera.transform.position=new Vector3(-4,1,-5);camera.transform.LookAt(new Vector3(-4,-.2f,-9));
    water.SetWeather(0,0);start=mesh.vertices;phase=1;began=EditorApplication.timeSinceStartup;return;
   }
   var vertices=mesh.vertices;
   if(start==null){start=vertices;return;}
   for(int i=0;i<vertices.Length;i++)
   {
    var a=water.transform.TransformPoint(start[i]);var b=water.transform.TransformPoint(vertices[i]);
    largestChange=Mathf.Max(largestChange,Mathf.Abs(a.y-b.y));
    var origin=water.transform.TransformPoint(original[i]);
    if(Mathf.Abs(origin.x-b.x)>.0001f||Mathf.Abs(origin.z-b.z)>.0001f)throw new Exception("Water moved sideways");
   }
   for(int i=0;i<24;i++)
   {
    var probe=new Vector3(-12+i*.8f,5,-8);
    if(!Physics.Raycast(probe,Vector3.down,out var hit,10,1<<9))continue;
    int t=hit.triangleIndex*3;var indices=water.Surface.triangles;var b=hit.barycentricCoordinate;
    float height=water.transform.TransformPoint(vertices[indices[t]]).y*b.x+water.transform.TransformPoint(vertices[indices[t+1]]).y*b.y+water.transform.TransformPoint(vertices[indices[t+2]]).y*b.z;
    largestError=Mathf.Max(largestError,Mathf.Abs(height-water.SurfaceHeight(hit)));
   }
   double elapsed=EditorApplication.timeSinceStartup-began;
   if(phase==1&&elapsed>2){FaroNightVerification.Capture("stage5-water-motion-a.png");phase=2;}
   if(phase==2&&elapsed>4){FaroNightVerification.Capture("stage5-water-motion-b.png");phase=3;}
   if(elapsed<8)return;
   string mode=phase<=3?"rain":phase==4?"storm":"calm";
   float required=mode=="storm"?.20f:mode=="rain"?.13f:.05f;
   Check(largestChange>required,mode+" visible vertex displacement="+largestChange.ToString("F4"));
   Check(largestError<.0001f,mode+" buoyancy matches actual rendered triangles; max error="+largestError.ToString("F6"));
   Check(original.SequenceEqual(water.OriginalMesh.vertices),"Imported mesh remains unchanged in "+mode);
   if(phase==5)
   {
    water.enabled=false;Check(water.GetComponent<MeshFilter>().sharedMesh==water.OriginalMesh,"Disabling animation restores original mesh");
    water.enabled=true;Check(water.GetComponent<MeshFilter>().sharedMesh==mesh,"Re-enabling animation reuses runtime mesh");
    File.WriteAllText(Path.GetFullPath("../stage5-water-motion-verification.txt"),report.ToString());Debug.Log("FARO_WATER_MOTION_PASS");End(0);return;
   }
   phase=phase<=3?4:5;water.SetWeather(phase==4?1:0,phase==5?1:0);start=null;largestChange=0;largestError=0;began=EditorApplication.timeSinceStartup;
  }
  catch(Exception e){Debug.LogException(e);File.WriteAllText(Path.GetFullPath("../stage5-water-motion-verification.txt"),report+"\nFAIL: "+e);End(1);}
 }
 static void End(int code){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;EditorApplication.Exit(code);}
}
