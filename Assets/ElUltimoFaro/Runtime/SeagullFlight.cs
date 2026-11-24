using UnityEngine;
namespace ElUltimoFaro {
 public sealed class SeagullFlight:MonoBehaviour {
  [SerializeField] Vector3 center;
  [SerializeField] Vector2 circleRadius=new Vector2(4.6f,6.2f);
  [SerializeField] float flightSpeed=1.2f,heightVariation=.18f,phase;
  [SerializeField] Transform leftWing,rightWing;
  [SerializeField] AudioSource voice;[SerializeField] AudioClip[] calls;
  [SerializeField] WeatherDirector weather;
  float clock,nextCall;bool suspended;Quaternion leftRest,rightRest;
  public int CallCount{get;private set;}
  public Vector3 Center=>center;
  public Vector2 Radius=>circleRadius;
  public float FlightClock=>clock;
  void Awake(){if(leftWing)leftRest=leftWing.localRotation;if(rightWing)rightRest=rightWing.localRotation;nextCall=Random.Range(5,9);}
  void Update(){
   if(suspended)return;clock+=Time.deltaTime;
   float a=phase+clock*flightSpeed/Mathf.Max(.5f,(circleRadius.x+circleRadius.y)*.5f);
   transform.position=center+new Vector3(Mathf.Cos(a)*circleRadius.x,Mathf.Sin(a*2+.4f)*heightVariation,Mathf.Sin(a)*circleRadius.y);
   var tangent=new Vector3(-Mathf.Sin(a)*circleRadius.x,2*heightVariation*Mathf.Cos(a*2+.4f),Mathf.Cos(a)*circleRadius.y);
   transform.rotation=Quaternion.LookRotation(tangent)*Quaternion.Euler(0,0,5*Mathf.Sin(a+.3f));
   float burst=clock%9;float flap=burst<2.2f?Mathf.Sin(burst/2.2f*Mathf.PI)*Mathf.Sin(clock*6)*8:0;
   if(leftWing)leftWing.localRotation=leftRest*Quaternion.Euler(0,0,flap);
   if(rightWing)rightWing.localRotation=rightRest*Quaternion.Euler(0,0,-flap);
   if(clock>=nextCall){
    float storm=weather?weather.StormStrength:0;voice.volume=Mathf.Lerp(.23f,.09f,storm);voice.pitch=Random.Range(.93f,1.08f);
    if(calls.Length>0){voice.PlayOneShot(calls[Random.Range(0,calls.Length)]);CallCount++;}
    nextCall=clock+Random.Range(15,26)+storm*9;
   }
  }
  void Suspend(bool value){suspended=value;if(value&&voice)voice.Stop();}
  void OnApplicationFocus(bool focus){Suspend(!focus);}
  void OnApplicationPause(bool paused){Suspend(paused);}
  void OnDisable(){if(leftWing)leftWing.localRotation=leftRest;if(rightWing)rightWing.localRotation=rightRest;if(voice)voice.Stop();}
 }
}
