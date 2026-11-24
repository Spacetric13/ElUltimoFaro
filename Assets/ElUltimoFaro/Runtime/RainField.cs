using UnityEngine;
namespace ElUltimoFaro
{
 [RequireComponent(typeof(ParticleSystem))]
 public sealed class RainField : MonoBehaviour
 {
  [SerializeField] Transform follow;
  [SerializeField] float heavyRate=1000,stormRate=1450,calmRate=150;
  ParticleSystem rain;
  Vector3 lastFollow;
  public int ActiveDrops=>rain?rain.particleCount:0;
  public float CurrentRate {get;private set;}
  public void Follow(Transform target){follow=target;lastFollow=target.position;if(rain)rain.Clear();}
  void Awake(){rain=GetComponent<ParticleSystem>();lastFollow=follow.position;transform.position=follow.position+Vector3.up*3;rain.Play();}
  public void SetWeather(float storm,float calm)
  {
   if(!rain)return;
   CurrentRate=Mathf.Lerp(Mathf.Lerp(heavyRate,stormRate,storm),calmRate,calm);
   var emission=rain.emission;emission.rateOverTime=CurrentRate;
   var velocity=rain.velocityOverLifetime;velocity.x=Mathf.Lerp(.6f,1.8f,storm)*(1-calm*.55f);velocity.z=.3f+storm*.4f;
  }
  void LateUpdate()
  {
   if((follow.position-lastFollow).sqrMagnitude>16)rain.Clear();
   transform.position=follow.position+Vector3.up*3;lastFollow=follow.position;
  }
 }
}
