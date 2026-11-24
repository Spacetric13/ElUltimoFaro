using UnityEngine;
using ElUltimoFaro;
public class LighthouseBlink : MonoBehaviour
{
    public Light beaconLight;
    public Renderer lampGlowRenderer, beamRenderer;
    public Transform beamPivot;
    [Min(.05f)] public float onTime = 2.5f, offTime = 1f;
    [Min(.05f)] public float fadeSeconds = .6f;
    public float rotationSpeed = 12, intensity = 3;
    public bool startOn = true;
    EmissionFader lampGlow, beamGlow;
    float cycle, fade;
    public bool IsPowered { get; private set; }
    public float CurrentBrightness { get; private set; }
    void Awake()
    {
        lampGlow = new EmissionFader(lampGlowRenderer); beamGlow = new EmissionFader(beamRenderer);
        IsPowered = startOn; Apply(0);
    }
    void Update()
    {
        if (IsPowered)
        {
            cycle = Mathf.Repeat(cycle + Time.deltaTime, Mathf.Max(.05f, onTime) + Mathf.Max(.05f, offTime));
            if (beamPivot) beamPivot.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
        fade = Mathf.MoveTowards(fade, IsPowered && cycle < onTime ? 1 : 0, Time.deltaTime / Mathf.Max(.05f, fadeSeconds));
        Apply(Mathf.SmoothStep(0, 1, fade));
    }
    void Apply(float brightness)
    {
        CurrentBrightness = brightness;
        if (beaconLight) { beaconLight.enabled = brightness > .001f; beaconLight.intensity = intensity * brightness; }
        if (lampGlowRenderer) { lampGlowRenderer.enabled = brightness > .001f; lampGlow.Apply(brightness); }
        if (beamRenderer) { beamRenderer.enabled = brightness > .001f; beamGlow.Apply(brightness, true); }
    }
    public void SetPowered(bool powered) { if (powered && !IsPowered) cycle = 0; IsPowered = powered; }
    public void SetState(bool powered) { SetPowered(powered); }
}
