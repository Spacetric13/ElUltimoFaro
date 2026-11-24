using UnityEngine;
namespace ElUltimoFaro
{
 [DefaultExecutionOrder(-300)]
 public sealed class WeatherDirector : MonoBehaviour
 {
  public enum WeatherPhase{HeavyRain,Storm,Clearing,Calm}
  [SerializeField] PortableItem quartz;
  [SerializeField] LighthouseBlink lighthouse;
  [SerializeField] MobileInteraction feedback;
  [SerializeField] Transform view;
  [SerializeField] Light moon;
  [SerializeField] RainField rain;
  [SerializeField] WeatherAudio sound;
  [SerializeField] LowPolyWater water;
  [SerializeField] LineRenderer bolt;
  [SerializeField,Min(1)] float stormTransition=8,clearingSeconds=12;
  [SerializeField] Vector2 lightningInterval=new Vector2(9,16),thunderDelay=new Vector2(1.2f,2.8f);
  [SerializeField,Range(0,2)] float flashIntensity=.95f;
  Color skyAmbient,equatorAmbient,groundAmbient,fogColor;
  float moonIntensity,fogDensity,nextFlash,flashStart=-100,thunderAt;
  bool previewStorm,lightingReady;
  bool crystalSeen,resolved,thunderPending,suspended;
  Vector3 thunderPosition;
  Material originalSky,runtimeSky;
  public WeatherPhase Phase{get;private set;}=WeatherPhase.HeavyRain;
  public float StormStrength{get;private set;}
  public float CalmProgress{get;private set;}
  public float FlashBrightness{get;private set;}
  public int FlashCount{get;private set;}
  public float LastFlashTime{get;private set;}=-1;
  public bool MissionResolved=>resolved;
  public void PreviewStorm(bool enabled,Transform cameraView)
  {
   InitializeLighting();previewStorm=enabled;view=cameraView;rain.Follow(cameraView);
   if(enabled){Phase=WeatherPhase.Storm;StormStrength=1;nextFlash=Time.time+1.5f;}
   else {Phase=crystalSeen?WeatherPhase.Storm:WeatherPhase.HeavyRain;StormStrength=0;nextFlash=float.PositiveInfinity;thunderPending=false;flashStart=-100;FlashBrightness=0;bolt.enabled=false;ApplyLight();}
  }
  void Awake(){if(gameObject.scene==UnityEngine.SceneManagement.SceneManager.GetActiveScene())InitializeLighting();}
  void InitializeLighting()
  {
   if(lightingReady||!RenderSettings.skybox)return;
   lightingReady=true;
   skyAmbient=RenderSettings.ambientSkyColor;equatorAmbient=RenderSettings.ambientEquatorColor;groundAmbient=RenderSettings.ambientGroundColor;
   fogColor=RenderSettings.fogColor;fogDensity=RenderSettings.fogDensity;moonIntensity=moon.intensity;
   originalSky=RenderSettings.skybox;runtimeSky=new Material(originalSky);RenderSettings.skybox=runtimeSky;
   bolt.enabled=false;nextFlash=float.PositiveInfinity;
  }
  void Update()
  {
   if(!lightingReady){if(gameObject.scene==UnityEngine.SceneManagement.SceneManager.GetActiveScene())InitializeLighting();if(!lightingReady)return;}
   if(!crystalSeen&&(quartz.IsCarried||quartz.IsInserted))
   {
    crystalSeen=true;Phase=WeatherPhase.Storm;nextFlash=Time.time+7;
    feedback.ShowMessage("Cuarzo recuperado. La tormenta se acerca...");
   }
   if(!resolved&&crystalSeen&&lighthouse.IsPowered)
   {
    resolved=true;Phase=WeatherPhase.Clearing;thunderPending=false;flashStart=-100;bolt.enabled=false;
   }
   StormStrength=Mathf.MoveTowards(StormStrength,(crystalSeen||previewStorm)&&!resolved?1:0,Time.deltaTime/Mathf.Max(1,stormTransition));
   CalmProgress=Mathf.MoveTowards(CalmProgress,resolved?1:0,Time.deltaTime/Mathf.Max(1,clearingSeconds));
   if(resolved&&CalmProgress>=1)Phase=WeatherPhase.Calm;
   rain.SetWeather(StormStrength,CalmProgress);sound.SetWeather(StormStrength,CalmProgress);water.SetWeather(StormStrength,CalmProgress);
   if(!suspended&&Phase==WeatherPhase.Storm&&Time.time>=nextFlash)BeginLightning();
   if(thunderPending&&!suspended&&Time.time>=thunderAt){thunderPending=false;if(!resolved)sound.PlayThunder(thunderPosition);}
   float elapsed=Time.time-flashStart;
   FlashBrightness=!suspended&&elapsed>=0&&elapsed<.65f?(elapsed<.06f?elapsed/.06f:Mathf.Exp(-(elapsed-.06f)*8)):0;
   ApplyLight();
  }
  void BeginLightning()
  {
   flashStart=Time.time;LastFlashTime=Time.time;FlashCount++;nextFlash=Time.time+Random.Range(Mathf.Max(7,lightningInterval.x),Mathf.Max(8,lightningInterval.y));
   Vector3 direction=Vector3.ProjectOnPlane(view.forward,Vector3.up).normalized;if(direction.sqrMagnitude<.1f)direction=Vector3.forward;
   direction=Quaternion.Euler(0,Random.Range(-65,65),0)*direction;
   Vector3 center=view.position+direction*Random.Range(26,38);center.y=0;
   thunderPosition=center+Vector3.up*7;
   bolt.positionCount=9;
   for(int i=0;i<9;i++)bolt.SetPosition(i,center+Vector3.up*Mathf.Lerp(15,3,i/8f)+new Vector3(Random.Range(-.6f,.6f),0,Random.Range(-.3f,.3f)));
   bolt.enabled=true;thunderPending=true;thunderAt=Time.time+Random.Range(thunderDelay.x,thunderDelay.y);
  }
  void ApplyLight()
  {
   if(!lightingReady)return;
   float flash=FlashBrightness;
   moon.intensity=moonIntensity+flash*flashIntensity;
   RenderSettings.ambientSkyColor=Color.Lerp(skyAmbient,new Color(.45f,.52f,.67f),flash);
   RenderSettings.ambientEquatorColor=Color.Lerp(equatorAmbient,new Color(.22f,.28f,.39f),flash);
   RenderSettings.ambientGroundColor=Color.Lerp(groundAmbient,new Color(.08f,.10f,.15f),flash);
   RenderSettings.fogColor=Color.Lerp(fogColor,new Color(.18f,.22f,.30f),flash*.6f);
   RenderSettings.fogDensity=fogDensity+StormStrength*.006f-CalmProgress*.004f;
   runtimeSky.SetFloat("_Flash",flash);
   bolt.startColor=new Color(.75f,.87f,1,flash);bolt.endColor=new Color(.65f,.78f,1,flash*.55f);
   bolt.enabled=flash>.001f;
  }
  void OnApplicationPause(bool paused){Suspend(paused);}
  void OnApplicationFocus(bool focus){Suspend(!focus);}
  void Suspend(bool value){suspended=value;if(value){thunderPending=false;flashStart=-100;FlashBrightness=0;if(bolt)bolt.enabled=false;if(runtimeSky&&moon)ApplyLight();}else if(crystalSeen&&!resolved)nextFlash=Time.time+Random.Range(9,16);}
  void OnDestroy()
  {
   if(RenderSettings.skybox==runtimeSky)RenderSettings.skybox=originalSky;
   if(runtimeSky)Destroy(runtimeSky);
  }
 }
}
