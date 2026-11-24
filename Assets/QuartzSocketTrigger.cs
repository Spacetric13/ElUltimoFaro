using UnityEngine;
using ElUltimoFaro;
public class QuartzSocketTrigger : MonoBehaviour
{
    [SerializeField] QuartzMachineController machine;
    [SerializeField] Transform snapPoint;
    [SerializeField] PortableItem quartz;
    [SerializeField, Min(.01f)] float crystalLength = .085f;
    public bool IsInserted { get; private set; }
    public Transform SnapPoint => snapPoint;
    public bool TryInsert(PortableItem item)
    {
        if (IsInserted || !machine || !snapPoint || !item || item != quartz || item.name != "cuarzo" || !item.IsCarried) return false;
        if (!item.Insert(snapPoint, crystalLength)) return false;
        IsInserted = true; machine.ActivateMachine(); return true;
    }
    void OnTriggerEnter(Collider other) { TryInsert(other.GetComponentInParent<PortableItem>()); }
}
