using UnityEngine;
namespace ElUltimoFaro
{
 [DefaultExecutionOrder(-200)]
 [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
 public sealed class LowPolyWater : MonoBehaviour
 {
  [SerializeField] LowPolyWaterSurfaceData surface;
  [SerializeField,Range(0,.25f)] float rainAmplitude=.12f,stormAmplitude=.18f,calmAmplitude=.05f;
  MeshFilter filter;
  Mesh originalMesh, animatedMesh;
  Vector3[] originalVertices, animatedVertices;
  Vector2[] worldXZ;
  Vector3 localUp;
  float waveTime;
  public float Amplitude {get;private set;}=.12f;
  public LowPolyWaterSurfaceData Surface=>surface;
  public Mesh OriginalMesh=>originalMesh;
  void Awake()
  {
   filter=GetComponent<MeshFilter>();originalMesh=filter.sharedMesh;
   // Animate a private mesh: never write to the imported asset or recook the collider.
   // Baked positions also work when Android strips CPU access to imported meshes.
   originalVertices=surface.localVertices;animatedVertices=new Vector3[originalVertices.Length];worldXZ=new Vector2[originalVertices.Length];
   animatedMesh=new Mesh{name=originalMesh.name+" (oleaje)",indexFormat=originalMesh.indexFormat};animatedMesh.MarkDynamic();
   animatedMesh.vertices=originalVertices;animatedMesh.triangles=surface.triangles;
   localUp=transform.InverseTransformVector(Vector3.up);
   for(int i=0;i<originalVertices.Length;i++)
   {
    Vector3 p=transform.TransformPoint(originalVertices[i]);worldXZ[i]=new Vector2(p.x,p.z);
   }
   var bounds=originalMesh.bounds;
   float limit=Mathf.Max(rainAmplitude,Mathf.Max(stormAmplitude,calmAmplitude));
   bounds.Expand(new Vector3(Mathf.Abs(localUp.x),Mathf.Abs(localUp.y),Mathf.Abs(localUp.z))*limit*2);
   animatedMesh.bounds=bounds;
   GetComponent<MeshRenderer>().localBounds=bounds;
   Amplitude=rainAmplitude;
  }
  void OnEnable(){if(filter&&animatedMesh)filter.sharedMesh=animatedMesh;}
  void Update()
  {
   waveTime=Time.timeSinceLevelLoad;
   for(int i=0;i<animatedVertices.Length;i++)animatedVertices[i]=originalVertices[i]+localUp*OffsetAt(worldXZ[i]);
   // Reuse arrays each frame; duplicate vertices move together, so faces stay joined.
   animatedMesh.SetVertices(animatedVertices,0,animatedVertices.Length,UnityEngine.Rendering.MeshUpdateFlags.DontRecalculateBounds);
  }
  public void SetWeather(float storm,float calm){Amplitude=Mathf.Lerp(Mathf.Lerp(rainAmplitude,stormAmplitude,Mathf.Clamp01(storm)),calmAmplitude,Mathf.Clamp01(calm));}
  public float OffsetAt(Vector2 p)
  {
   return Amplitude*(Mathf.Sin(p.x*1.35f+p.y*.75f+waveTime*1.25f)+.35f*Mathf.Sin(-p.x*.8f+p.y*1.6f-waveTime*.95f))/1.35f;
  }
  public float SurfaceHeight(RaycastHit hit)
  {
   int first=hit.triangleIndex*3;
   if(!surface||first<0||first+2>=surface.triangles.Length)return hit.point.y;
   // Same offsets and time as the visible vertices, interpolated on the original collider.
   Vector3 b=hit.barycentricCoordinate;
   return hit.point.y+OffsetAt(worldXZ[surface.triangles[first]])*b.x+OffsetAt(worldXZ[surface.triangles[first+1]])*b.y+OffsetAt(worldXZ[surface.triangles[first+2]])*b.z;
  }
  void OnDisable(){if(filter&&originalMesh)filter.sharedMesh=originalMesh;}
  void OnDestroy(){if(animatedMesh)Destroy(animatedMesh);}
 }
}
