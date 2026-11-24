using UnityEngine;
using ElUltimoFaro;
public class QuartzMachineController : MonoBehaviour
{
    [Header("Existing model parts, centered pivots")]
    [SerializeField] Transform flywheel, mainGear, secondaryGear, energyCore;
    [SerializeField] Light indicatorLight, coreLight;
    [SerializeField] Renderer indicatorRenderer, coreRenderer;
    [Header("Gradual startup")]
    [SerializeField, Min(.1f)] float startupSeconds = 2.5f;
    [SerializeField] float flywheelSpeed = 110, gearSpeed = 85, corePulseSpeed = 2, corePulseAmount = .08f;
    [SerializeField] Vector3 rotationAxis = Vector3.up;
    [SerializeField, Min(0)] float indicatorIntensity = .08f, coreIntensity = .12f;
    [SerializeField] AudioSource machineLoop;
    Vector3 coreInitialScale;
    EmissionFader indicatorGlow, coreGlow;
    AudioClip generatedLoop;
    bool requested;
    float ramp;
    public float PowerLevel { get; private set; }
    public bool IsPowered => requested && ramp >= 1;
    public bool IsStarting => requested && !IsPowered;
    void Awake()
    {
        if (energyCore) coreInitialScale = energyCore.localScale;
        indicatorGlow = new EmissionFader(indicatorRenderer); coreGlow = new EmissionFader(coreRenderer);
        if (machineLoop && !machineLoop.clip) { generatedLoop = MachineEffects.Sound("Motor mecánico", true); machineLoop.clip = generatedLoop; }
        ApplyEffects(0);
    }
    void Update()
    {
        ramp = Mathf.MoveTowards(ramp, requested ? 1 : 0, Time.deltaTime / Mathf.Max(.1f, startupSeconds));
        PowerLevel = Mathf.SmoothStep(0, 1, ramp);
        float step = Time.deltaTime * PowerLevel;
        if (flywheel) flywheel.Rotate(rotationAxis, flywheelSpeed * step, Space.Self);
        if (mainGear) mainGear.Rotate(rotationAxis, -gearSpeed * step, Space.Self);
        if (secondaryGear) secondaryGear.Rotate(rotationAxis, gearSpeed * 1.35f * step, Space.Self);
        ApplyEffects(PowerLevel);
    }
    void ApplyEffects(float power)
    {
        float pulse = 1 + Mathf.Sin(Time.time * corePulseSpeed) * corePulseAmount * power;
        if (energyCore) energyCore.localScale = coreInitialScale * pulse;
        indicatorGlow.Apply(power); coreGlow.Apply(power * pulse);
        if (indicatorLight) { indicatorLight.enabled = power > .001f; indicatorLight.intensity = power * indicatorIntensity; }
        if (coreLight) { coreLight.enabled = power > .001f; coreLight.intensity = power * pulse * coreIntensity; }
        if (machineLoop)
        {
            machineLoop.volume = .25f * power; machineLoop.pitch = Mathf.Lerp(.55f, 1, power);
            if (power > 0 && !machineLoop.isPlaying) machineLoop.Play();
            if (power == 0 && machineLoop.isPlaying) machineLoop.Stop();
        }
    }
    public void ActivateMachine() { requested = true; }
    public void DeactivateMachine() { requested = false; }
    void OnDestroy() { if (generatedLoop) Destroy(generatedLoop); }
}
