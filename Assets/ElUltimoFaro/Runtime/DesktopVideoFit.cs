using UnityEngine;
using UnityEngine.UI;
namespace ElUltimoFaro {
 public sealed class DesktopVideoFit : MonoBehaviour {
  public Camera view;public float aspect=16f/9f;
  Canvas canvas;RawImage image;AspectRatioFitter fitter;
  public void Fit(){
   if(!view)return;
   if(!canvas){
    var root=new GameObject("Video_PC_pantalla_completa",typeof(RectTransform),typeof(Canvas));root.layer=30;
    canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=view;canvas.planeDistance=1;canvas.sortingOrder=900;
    var panel=new GameObject("Imagen_completa",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));panel.layer=30;panel.transform.SetParent(root.transform,false);
    image=panel.GetComponent<RawImage>();image.raycastTarget=false;fitter=panel.GetComponent<AspectRatioFitter>();fitter.aspectMode=AspectRatioFitter.AspectMode.FitInParent;
   }
   fitter.aspectRatio=aspect;
   var renderer=GetComponent<Renderer>();renderer.enabled=false;
   if(renderer.sharedMaterial&&renderer.sharedMaterial.HasProperty("_BaseMap"))image.texture=renderer.sharedMaterial.GetTexture("_BaseMap");
   Canvas.ForceUpdateCanvases();
  }
  void LateUpdate(){Fit();}
  void OnDestroy(){if(canvas)Destroy(canvas.gameObject);}
 }
}
