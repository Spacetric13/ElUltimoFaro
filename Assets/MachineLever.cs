using UnityEngine;
using ElUltimoFaro;
public class MachineLever : MonoBehaviour
{
    [SerializeField] QuartzMachineController machine;
    [SerializeField] Transform lever;
    [SerializeField] LighthouseBlink lighthouse;
    [SerializeField] float offAngle = 0, onAngle = 32, moveSpeed = 5;
    [SerializeField] Vector3 rotationAxis = Vector3.right;
    Quaternion initialRotation;
    AudioSource sound;
    AudioClip click;
    public bool IsOn { get; private set; }
    void Awake()
    {
        initialRotation = lever ? lever.localRotation : Quaternion.identity;
        sound = gameObject.AddComponent<AudioSource>(); sound.playOnAwake = false; sound.spatialBlend = 1; sound.minDistance = .25f; sound.maxDistance = 4; sound.volume = .4f;
        click = MachineEffects.Sound("Palanca", false);
    }
    void Update()
    {
        if (IsOn && (!machine || !machine.IsPowered)) { IsOn = false; if (lighthouse) lighthouse.SetPowered(false); }
        if (lever) lever.localRotation = Quaternion.Slerp(lever.localRotation, initialRotation * Quaternion.AngleAxis(IsOn ? onAngle : offAngle, rotationAxis), 1 - Mathf.Exp(-moveSpeed * Time.deltaTime));
    }
    public void PullLever()
    {
        if (!machine || !machine.IsPowered) return;
        IsOn = !IsOn; if (lighthouse) lighthouse.SetPowered(IsOn); sound.PlayOneShot(click);
    }
    void OnDestroy() { if (click) Destroy(click); }
}
