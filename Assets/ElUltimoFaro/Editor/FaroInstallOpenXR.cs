using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
[InitializeOnLoad]
public static class FaroInstallOpenXR {
 static AddRequest request;
 static FaroInstallOpenXR() { EditorApplication.delayCall += Begin; }
 static void Begin() {
  if (!System.IO.File.Exists("Temp/FaroInstallXR.request")) return;
  System.IO.File.Delete("Temp/FaroInstallXR.request");
  request=Client.Add("com.unity.xr.openxr"); EditorApplication.update+=Poll;
 }
 static void Poll() {
  if(request==null || !request.IsCompleted)return;
  System.IO.File.WriteAllText("Temp/FaroInstallXR.result",request.Status==StatusCode.Success ? request.Result.name+" "+request.Result.version : request.Error.message);
  EditorApplication.update-=Poll;
 }
}
