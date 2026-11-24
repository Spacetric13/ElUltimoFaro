using UnityEngine;
namespace ElUltimoFaro {
 // One session manager survives the hall; the destination rig contains no duplicate manager.
 [DefaultExecutionOrder(-1000)]
 public sealed class HallXRSession:MonoBehaviour {
  void Awake(){DontDestroyOnLoad(gameObject);}
 }
}
