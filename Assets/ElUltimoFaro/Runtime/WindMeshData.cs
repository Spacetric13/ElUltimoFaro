using UnityEngine;
namespace ElUltimoFaro {
 public sealed class WindMeshData:ScriptableObject {
  [System.Serializable] public struct Submesh {public int[] triangles;}
  public Vector3[] vertices,normals; public Vector2[] uv; public Submesh[] submeshes;
 }
}
