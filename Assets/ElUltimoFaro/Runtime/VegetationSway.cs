using UnityEngine;
namespace ElUltimoFaro {
 public sealed class VegetationSway:MonoBehaviour {
  [System.Serializable] public struct Part {public MeshFilter filter;public WindMeshData data;}
  [SerializeField] Part[] parts;
  [SerializeField] WeatherDirector weather;
  [SerializeField] float baseY,height=3,maxDisplacement=.09f;
  [SerializeField,Range(0,2)] float windStrength=1,windSpeed=.9f,variation=1;
  [SerializeField] float phase;
  sealed class LivePart {public Mesh original,mesh;public Vector3[] positions;public float[] weights;}
  LivePart[] live;float nextFrame;
  public float CurrentStrength {get;private set;}
  public Part[] Parts=>parts;
  public float BaseY=>baseY;
  void Awake(){
   live=new LivePart[parts.Length];
   for(int j=0;j<parts.Length;j++){
    var part=parts[j];var data=part.data;var l=new LivePart{original=part.filter.sharedMesh,positions=new Vector3[data.vertices.Length],weights=new float[data.vertices.Length]};
    l.mesh=new Mesh{name=l.original.name+" (viento)",indexFormat=l.original.indexFormat};l.mesh.MarkDynamic();
    l.mesh.vertices=data.vertices;l.mesh.normals=data.normals;l.mesh.uv=data.uv;l.mesh.subMeshCount=data.submeshes.Length;
    for(int s=0;s<data.submeshes.Length;s++)l.mesh.SetTriangles(data.submeshes[s].triangles,s);
    for(int i=0;i<l.weights.Length;i++){
     float y=part.filter.transform.TransformPoint(data.vertices[i]).y;
     float w=Mathf.Clamp01((y-baseY-height*.18f)/Mathf.Max(.01f,height*.82f));l.weights[i]=w*w;
    }
    var b=l.original.bounds;b.Expand(maxDisplacement*windStrength*2.2f/Mathf.Min(part.filter.transform.lossyScale.x,part.filter.transform.lossyScale.y,part.filter.transform.lossyScale.z));l.mesh.bounds=b;
    part.filter.GetComponent<Renderer>().localBounds=b;live[j]=l;
   }
  }
  void OnEnable(){if(live!=null)for(int j=0;j<parts.Length;j++)parts[j].filter.sharedMesh=live[j].mesh;}
  void Update(){
   float storm=weather?weather.StormStrength:0,calm=weather?weather.CalmProgress:0;
   CurrentStrength=Mathf.Lerp(Mathf.Lerp(.48f,1,storm),.2f,calm)*windStrength;
   if(Time.time<nextFrame)return;nextFrame=Time.time+1f/30;
   float t=Time.time*windSpeed,p=phase*variation;
   Vector3 shift=(new Vector3(.9f,0,.42f)*Mathf.Sin(t+p)+new Vector3(-.42f,0,.9f)*(.35f*Mathf.Sin(t*.73f+p*1.83f)))*(maxDisplacement*CurrentStrength/1.35f);
   for(int j=0;j<parts.Length;j++){
    var data=parts[j].data;var l=live[j];Vector3 local=parts[j].filter.transform.InverseTransformVector(shift);
    for(int i=0;i<l.positions.Length;i++)l.positions[i]=data.vertices[i]+local*l.weights[i];
    l.mesh.SetVertices(l.positions,0,l.positions.Length,UnityEngine.Rendering.MeshUpdateFlags.DontRecalculateBounds);
   }
  }
  void OnDisable(){if(live!=null)for(int j=0;j<parts.Length;j++)if(parts[j].filter)parts[j].filter.sharedMesh=live[j].original;}
  void OnDestroy(){if(live!=null)foreach(var l in live)if(l.mesh)Destroy(l.mesh);}
 }
}
