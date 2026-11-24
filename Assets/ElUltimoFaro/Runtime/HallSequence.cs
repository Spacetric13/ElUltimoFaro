using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.Video;
namespace ElUltimoFaro {
 public sealed class HallSequence:MonoBehaviour {
  public HallPlayer player;public Transform photo;public MeshFilter photoMesh;public Material photoMaterial;
  public Text prompt;public Image fade;public AudioSource ambience,effects;public AudioClip thunder,rain;
  public string destination;public float introDuration=12;
  public VideoClip memoryVideo;
  public bool VideoCompleted{get;private set;}
  public bool IsRunning{get;private set;}public string Stage{get;private set;}="Hall";
  bool lastAction;RenderTexture movie;Camera movieCamera;GameObject photoZoom,screen;WeatherDirector weather;
  public bool CanStart=>!IsRunning&&Vector3.Distance(player.head.position,photo.position)<1.85f&&Vector3.Angle(player.head.forward,photo.position-player.head.position)<38;
  void Update(){
   if(IsRunning)return;bool action=false;
   if(player.desktop)action=Keyboard.current!=null&&Keyboard.current.eKey.isPressed;else InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton,out action);
   prompt.text=CanStart?(player.desktop?"E · RECORDAR Y PARTIR":"A · RECORDAR Y PARTIR"):"Acércate a la fotografía";
   if(action&&!lastAction&&CanStart)Begin();lastAction=action;
  }
  public void Begin(){if(IsRunning)return;IsRunning=true;player.CanMove=false;StartCoroutine(Play());}
  IEnumerator Fade(float from,float to,float duration){for(float t=0;t<duration;t+=Time.unscaledDeltaTime){fade.color=new Color(0,0,0,Mathf.Lerp(from,to,t/duration));yield return null;}fade.color=new Color(0,0,0,to);}
  IEnumerator Play(){
   Stage="PreparandoVideo";prompt.text="";
   yield return Fade(0,1,.25f);
   foreach(Transform child in transform)if(child!=player.transform)child.gameObject.SetActive(false);
   ambience.Stop();
   yield return PlayMemoryVideo();
   Stage="Trueno";effects.PlayOneShot(thunder,.8f);
   Stage="Cargando";foreach(Transform child in transform)if(child!=player.transform)child.gameObject.SetActive(false);var hallScene=gameObject.scene;
   GameObject gamePlayer=null;Camera gameCamera=null;AudioSource[] gameAudio=null;bool[] audioEnabled=null;
   UnityEngine.Events.UnityAction<Scene,LoadSceneMode> freeze=(scene,mode)=>{if(scene.name==destination)foreach(var root in scene.GetRootGameObjects())foreach(var controller in root.GetComponentsInChildren<MobilePlayerController>(true))controller.gameObject.SetActive(false);};
   SceneManager.sceneLoaded+=freeze;var loading=SceneManager.LoadSceneAsync(destination,LoadSceneMode.Additive);while(!loading.isDone)yield return null;SceneManager.sceneLoaded-=freeze;
   var gameScene=SceneManager.GetSceneByName(destination);SceneManager.SetActiveScene(gameScene);
   var mobile=FindAnyObjectByType<MobilePlayerController>(FindObjectsInactive.Include);gamePlayer=mobile.gameObject;
   var pc=gamePlayer.GetComponent<DesktopPlaytest>();var bridge=gamePlayer.GetComponent<QuestPlayerBridge>();gameCamera=pc&&pc.enabled?pc.view:bridge.head.GetComponent<Camera>();
   gamePlayer.SetActive(false);
   var all=new System.Collections.Generic.List<AudioSource>();foreach(var root in gameScene.GetRootGameObjects())all.AddRange(root.GetComponentsInChildren<AudioSource>(true));gameAudio=all.ToArray();audioEnabled=new bool[gameAudio.Length];for(int i=0;i<gameAudio.Length;i++){audioEnabled[i]=gameAudio[i].enabled;gameAudio[i].enabled=false;}
   movie=new RenderTexture(player.desktop?1920:1024,player.desktop?1080:576,16);movie.Create();
   movieCamera=new GameObject("Camara_cinematica",typeof(Camera)).GetComponent<Camera>();movieCamera.transform.position=mobile.transform.position+new Vector3(6,4,7);movieCamera.targetTexture=movie;movieCamera.nearClipPlane=.05f;movieCamera.farClipPlane=120;movieCamera.fieldOfView=52;movieCamera.cullingMask=~(1<<30);movieCamera.stereoTargetEye=StereoTargetEyeMask.None;movieCamera.allowHDR=false;
   weather=FindAnyObjectByType<WeatherDirector>();weather.PreviewStorm(true,movieCamera.transform);
   screen=GameObject.CreatePrimitive(PrimitiveType.Quad);Destroy(screen.GetComponent<Collider>());screen.name="Pantalla_cinematica";screen.layer=30;
   var screenMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit"));screenMaterial.SetTexture("_BaseMap",movie);screen.GetComponent<Renderer>().material=screenMaterial;
   screen.transform.SetPositionAndRotation(player.head.position+player.head.forward*2.1f,Quaternion.LookRotation(player.head.forward,Vector3.up));screen.transform.localScale=new Vector3(3.55f,2,1);
   if(player.desktop){var fit=screen.AddComponent<DesktopVideoFit>();fit.view=player.head.GetComponent<Camera>();fit.aspect=16f/9f;fit.Fit();}
   ambience.clip=rain;ambience.volume=.3f;ambience.Play();Stage="Isla";
   yield return Fade(1,0,.5f);Vector3 focus=mobile.transform.position+Vector3.up*.7f;
   for(float t=0;t<introDuration;t+=Time.deltaTime){float progress=t/introDuration;Vector3 offset=progress<.55f?Vector3.Lerp(new Vector3(6,4,7),new Vector3(3,2.3f,4),progress/.55f):Vector3.Lerp(new Vector3(-5,2.5f,-5),new Vector3(2,1.4f,3),(progress-.55f)/.45f);movieCamera.transform.position=focus+offset;movieCamera.transform.LookAt(focus);yield return null;}
   Stage="Entrando";yield return Fade(0,1,.6f);ambience.Stop();effects.Stop();weather.PreviewStorm(false,gameCamera.transform);
   movieCamera.enabled=false;Destroy(movieCamera.gameObject);Destroy(screen);Destroy(screenMaterial);movie.Release();Destroy(movie);
   player.head.GetComponent<Camera>().enabled=false;var listener=player.head.GetComponent<AudioListener>();if(listener)listener.enabled=false;
   foreach(var audio in GetComponentsInChildren<AudioSource>())audio.Stop();
   // Temporarily carry the black transition into the game so VR never jumps in full view.
   var fadeRoot=fade.canvas.transform;fadeRoot.SetParent(gameCamera.transform,false);fadeRoot.localPosition=new Vector3(0,0,.12f);fadeRoot.localRotation=Quaternion.identity;
   gamePlayer.SetActive(true);for(int i=0;i<gameAudio.Length;i++){if(!gameAudio[i])continue;gameAudio[i].enabled=audioEnabled[i];if(audioEnabled[i]&&gameAudio[i].loop&&gameAudio[i].clip)gameAudio[i].Play();}
   yield return null;mobile.CenterTrackedView();Stage="Juego";yield return Fade(1,0,.7f);Destroy(fadeRoot.gameObject);SceneManager.UnloadSceneAsync(hallScene);
  }
  IEnumerator PlayMemoryVideo(){
   if(!memoryVideo){Debug.LogWarning("Hall: no memory video assigned; continuing to island.");yield break;}
   photoZoom=GameObject.CreatePrimitive(PrimitiveType.Quad);Destroy(photoZoom.GetComponent<Collider>());photoZoom.name="Pantalla_recuerdo";photoZoom.layer=30;
   photoZoom.transform.SetPositionAndRotation(player.head.position+player.head.forward*2.1f,Quaternion.LookRotation(player.head.forward,Vector3.up));
   photoZoom.transform.localScale=new Vector3(3.55f,3.55f*memoryVideo.height/memoryVideo.width,1);
   if(player.desktop){var fit=photoZoom.AddComponent<DesktopVideoFit>();fit.view=player.head.GetComponent<Camera>();fit.aspect=(float)memoryVideo.width/memoryVideo.height;fit.Fit();}
   var texture=new RenderTexture((int)memoryVideo.width,(int)memoryVideo.height,0);texture.Create();
   var material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));material.SetTexture("_BaseMap",texture);photoZoom.GetComponent<Renderer>().sharedMaterial=material;
   var audio=photoZoom.AddComponent<AudioSource>();audio.playOnAwake=false;audio.spatialBlend=0;audio.volume=.8f;
   var video=photoZoom.AddComponent<VideoPlayer>();video.playOnAwake=false;video.clip=memoryVideo;video.isLooping=false;video.waitForFirstFrame=true;video.renderMode=VideoRenderMode.RenderTexture;video.targetTexture=texture;video.audioOutputMode=VideoAudioOutputMode.AudioSource;
   if(memoryVideo.audioTrackCount>0){video.controlledAudioTrackCount=1;video.EnableAudioTrack(0,true);video.SetTargetAudioSource(0,audio);}
   bool failed=false,finished=false;video.errorReceived+=(source,message)=>{failed=true;Debug.LogWarning("Hall video: "+message);};video.loopPointReached+=source=>{finished=true;VideoCompleted=true;};
   video.Prepare();float deadline=Time.realtimeSinceStartup+15;
   while(!video.isPrepared&&!failed&&Time.realtimeSinceStartup<deadline)yield return null;
   if(video.isPrepared&&!failed){
    video.Play();deadline=Time.realtimeSinceStartup+5;
    while(video.frame<0&&!failed&&Time.realtimeSinceStartup<deadline)yield return null;
    if(video.frame>=0&&!failed){Stage="Video";yield return Fade(1,0,.2f);deadline=Time.realtimeSinceStartup+(float)memoryVideo.length+5;
     while(!finished&&!failed&&Time.realtimeSinceStartup<deadline)yield return null;
     yield return Fade(0,1,.12f);
    }
   }
   if(!VideoCompleted)Debug.LogWarning("Hall video unavailable or timed out; continuing to island.");
   video.Stop();video.targetTexture=null;Destroy(photoZoom);Destroy(material);texture.Release();Destroy(texture);
  }
  void OnDestroy(){if(movie){movie.Release();Destroy(movie);}}
 }
}
