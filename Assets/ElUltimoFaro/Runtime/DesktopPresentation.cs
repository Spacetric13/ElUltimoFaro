using UnityEngine;
using UnityEngine.InputSystem;
namespace ElUltimoFaro {
 // Persists across the hall/island transition; never changes headset presentation.
 public sealed class DesktopPresentation : MonoBehaviour {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void Boot(){
   if(Application.platform!=RuntimePlatform.WindowsPlayer)return;
   var go=new GameObject("Pantalla_PC");DontDestroyOnLoad(go);go.AddComponent<DesktopPresentation>();
   var display=Screen.currentResolution;
   Screen.SetResolution(display.width,display.height,FullScreenMode.FullScreenWindow);
  }
  void Update(){var k=Keyboard.current;if(k==null)return;
   if(k.f11Key.wasPressedThisFrame)Screen.fullScreenMode=Screen.fullScreen?FullScreenMode.Windowed:FullScreenMode.FullScreenWindow;
  }
 }
}
