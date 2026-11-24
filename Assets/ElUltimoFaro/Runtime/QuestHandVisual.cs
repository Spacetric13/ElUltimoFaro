using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
namespace ElUltimoFaro
{
    // One low-poly mesh per glove, with finger articulation driven by controller grip.
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public sealed class QuestHandVisual:MonoBehaviour
    {
        public bool left;
        Mesh mesh;MeshRenderer visual;float shown=-1;
        readonly List<Vector3> vertices=new List<Vector3>(600);
        readonly List<int> triangles=new List<int>(900);
        readonly List<Color> colors=new List<Color>(600);
        static readonly Color Leather=new Color(.65f,.44f,.24f), Seam=new Color(.32f,.20f,.105f), Cuff=new Color(.065f,.13f,.17f);
        void Awake(){mesh=new Mesh{name="Guante low poly"};mesh.MarkDynamic();GetComponent<MeshFilter>().sharedMesh=mesh;visual=GetComponent<MeshRenderer>();Build(0);}
        void Update(){
            var device=InputDevices.GetDeviceAtXRNode(left?XRNode.LeftHand:XRNode.RightHand);
            bool tracked=device.TryGetFeatureValue(CommonUsages.isTracked,out bool t)&&t;visual.enabled=tracked;
            device.TryGetFeatureValue(CommonUsages.grip,out float curl);
            if(Mathf.Abs(curl-shown)>.025f)Build(curl);
        }
        public void Build(float curl){
            if(!mesh){mesh=new Mesh{name="Guante low poly"};GetComponent<MeshFilter>().sharedMesh=mesh;}
            shown=curl;vertices.Clear();triangles.Clear();colors.Clear();
            Box(new Vector3(0,0,.018f),new Vector3(.077f,.029f,.085f),Quaternion.identity,Leather);
            Box(new Vector3(0,0,-.042f),new Vector3(.065f,.035f,.036f),Quaternion.identity,Cuff);
            Box(new Vector3(0,.016f,.017f),new Vector3(.059f,.006f,.056f),Quaternion.identity,Seam);
            for(int finger=0;finger<4;finger++){
                float x=(finger-1.5f)*.019f;float length=finger==3?.021f:finger==0?.025f:.029f;
                Vector3 start=new Vector3(x,0,.057f);Quaternion rotation=Quaternion.identity;
                for(int joint=0;joint<3;joint++){
                    rotation*=Quaternion.Euler(curl*(joint==0?65:75),0,0);
                    Box(start+rotation*Vector3.forward*length*.5f,new Vector3(.016f,.019f,length*.96f),rotation,joint==0?Leather:Color.Lerp(Leather,Seam,.12f));
                    start+=rotation*Vector3.forward*length;length*=.8f;
                }
            }
            float sign=left?1:-1;
            Vector3 thumb=new Vector3(sign*.041f,-.004f,.005f);
            Quaternion thumbRot=Quaternion.Euler(20+curl*40,sign*(48-curl*25),sign*-15);
            for(int j=0;j<2;j++){Box(thumb+thumbRot*Vector3.forward*.016f,new Vector3(.021f,.022f,.032f),thumbRot,Leather);thumb+=thumbRot*Vector3.forward*.032f;thumbRot*=Quaternion.Euler(curl*45,0,0);}
            mesh.Clear();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetColors(colors);mesh.RecalculateNormals();mesh.RecalculateBounds();
        }
        void Box(Vector3 center,Vector3 size,Quaternion rotation,Color color){
            // Eight-corner beveled cross section avoids the pill-shaped placeholder silhouette.
            Vector2[] ring={new Vector2(-.36f,-.5f),new Vector2(.36f,-.5f),new Vector2(.5f,-.28f),new Vector2(.5f,.28f),new Vector2(.36f,.5f),new Vector2(-.36f,.5f),new Vector2(-.5f,.28f),new Vector2(-.5f,-.28f)};
            Vector3[] a=new Vector3[8],b=new Vector3[8];
            for(int i=0;i<8;i++){a[i]=center+rotation*Vector3.Scale(new Vector3(ring[i].x,ring[i].y,-.5f),size);b[i]=center+rotation*Vector3.Scale(new Vector3(ring[i].x,ring[i].y,.5f),size);}
            for(int i=0;i<8;i++){int n=(i+1)%8;Quad(a[i],a[n],b[n],b[i],color);}
            for(int i=1;i<7;i++){Tri(a[0],a[i+1],a[i],color);Tri(b[0],b[i],b[i+1],color);}
        }
        void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color){Tri(a,b,c,color);Tri(a,c,d,color);}
        void Tri(Vector3 a,Vector3 b,Vector3 c,Color color){int i=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);colors.Add(color);colors.Add(color);colors.Add(color);triangles.Add(i);triangles.Add(i+1);triangles.Add(i+2);}
        void OnDestroy(){if(mesh){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}}
    }
}

