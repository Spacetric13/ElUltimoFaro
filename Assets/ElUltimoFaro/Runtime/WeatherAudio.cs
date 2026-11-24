using UnityEngine;
namespace ElUltimoFaro
{
 public sealed class WeatherAudio : MonoBehaviour
 {
  [SerializeField] MobilePlayerController player;
  [SerializeField] AudioSource rain,wind,sea,thunder;
  [SerializeField] AudioClip[] thunderClips;
  bool suspended;
  float storm,calm,shelter;
  public void SetShelter(float amount){shelter=Mathf.Clamp01(amount);}
  public int ThunderCount {get;private set;}
  public float LastThunderTime {get;private set;}=-1;
  public float RainVolume=>rain.volume;
  void Awake(){rain.Play();wind.Play();sea.Play();}
  public void SetWeather(float stormAmount,float calmAmount)
  {
   storm=stormAmount;calm=calmAmount;
   rain.volume=Mathf.Lerp(Mathf.Lerp(.25f,.36f,storm),.055f,calm);
   wind.volume=Mathf.Lerp(Mathf.Lerp(.06f,.18f,storm),.025f,calm);
   float coast=player.IsOnBoat?1:1-Mathf.InverseLerp(.05f,1.4f,player.transform.position.y);
   sea.volume=Mathf.Lerp(.04f,.11f,coast)*(1+storm*.25f)*(1-calm*.25f);
   rain.volume*=Mathf.Lerp(1,.45f,shelter);wind.volume*=Mathf.Lerp(1,.3f,shelter);sea.volume*=Mathf.Lerp(1,.55f,shelter);
   thunder.volume=.7f*(1-calm*.8f);
  }
  public void PlayThunder(Vector3 position)
  {
   if(suspended||thunderClips==null||thunderClips.Length==0)return;
   thunder.transform.position=position;thunder.pitch=Random.Range(.92f,1.04f);thunder.PlayOneShot(thunderClips[Random.Range(0,thunderClips.Length)]);
   ThunderCount++;LastThunderTime=Time.time;
  }
  void OnApplicationFocus(bool focus){Suspend(!focus);}
  void OnApplicationPause(bool paused){Suspend(paused);}
  void Suspend(bool value)
  {
   if(value==suspended)return;suspended=value;
   foreach(var source in new[]{rain,wind,sea})if(source){if(value)source.Pause();else source.UnPause();}
   if(value&&thunder)thunder.Stop();
  }
 }
}
