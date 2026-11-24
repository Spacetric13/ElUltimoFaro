using UnityEngine;
namespace ElUltimoFaro {
 [RequireComponent(typeof(ParticleSystem))]
 public sealed class RainShelter:MonoBehaviour {
  [SerializeField] BoxCollider interior;
  [SerializeField] Transform player;
  [SerializeField] WeatherAudio sound;
  ParticleSystem rain;ParticleSystem.Particle[] drops;float muffling;
  public bool IsSheltered{get;private set;}
  void Awake(){rain=GetComponent<ParticleSystem>();drops=new ParticleSystem.Particle[rain.main.maxParticles];}
  public bool Contains(Vector3 world){Vector3 p=interior.transform.InverseTransformPoint(world)-interior.center;Vector3 h=interior.size*.5f;return Mathf.Abs(p.x)<h.x&&Mathf.Abs(p.y)<h.y&&Mathf.Abs(p.z)<h.z;}
  void LateUpdate(){
   IsSheltered=Contains(player.position+Vector3.up*.3f);muffling=Mathf.MoveTowards(muffling,IsSheltered?1:0,Time.deltaTime*2);sound.SetShelter(muffling);
   int count=rain.GetParticles(drops);bool changed=false;
   for(int i=0;i<count;i++)if(Contains(drops[i].position)){drops[i].remainingLifetime=0;changed=true;}
   if(changed)rain.SetParticles(drops,count);
  }
  void OnDisable(){if(sound)sound.SetShelter(0);}
 }
}
