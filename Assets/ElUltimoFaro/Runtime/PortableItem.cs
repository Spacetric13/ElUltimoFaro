using UnityEngine;

namespace ElUltimoFaro
{
    // The existing scene object is carried; no duplicate quartz or replacement model is created.
    public sealed class PortableItem : MonoBehaviour
    {
        public enum ItemKind { Quartz, Lantern }
        [SerializeField] ItemKind kind;
        [SerializeField] Light lanternLight;
        [SerializeField] Renderer luminousGlobe;
        Renderer[] visuals;
        Collider[] colliders;
        bool[] originalColliderStates;
        MaterialPropertyBlock glow;
        Color originalEmission;
        int emissionProperty;
        Vector3 originalWorldScale;
        float originalHeight;
        public ItemKind Kind => kind;
        public bool IsCarried { get; private set; }
        public bool IsInserted { get; private set; }
        public bool IsLit { get; private set; }
        public Light LanternLight => lanternLight;
        public string DisplayName => kind == ItemKind.Quartz ? "CUARZO" : "FAROL";

        public void Configure(ItemKind itemKind, Light light = null, Renderer globe = null)
        { kind = itemKind; lanternLight = light; luminousGlobe = globe; }
        void Awake()
        {
            visuals = GetComponentsInChildren<Renderer>();
            colliders = GetComponentsInChildren<Collider>();
            originalColliderStates = new bool[colliders.Length];
            for (int i=0;i<colliders.Length;i++) originalColliderStates[i]=colliders[i].enabled;
            originalWorldScale = transform.lossyScale;
            originalHeight = Mathf.Max(VisualBounds.size.y, .001f);
            if (luminousGlobe && luminousGlobe.sharedMaterial)
            {
                var material = luminousGlobe.sharedMaterial;
                emissionProperty = Shader.PropertyToID(material.HasProperty("emissiveFactor") ? "emissiveFactor" : "_EmissionColor");
                if (material.HasProperty(emissionProperty)) originalEmission = material.GetColor(emissionProperty);
                glow = new MaterialPropertyBlock();
            }
            SetLight(false);
        }
        public Bounds VisualBounds
        {
            get
            {
                if (visuals == null) visuals = GetComponentsInChildren<Renderer>();
                var bounds = new Bounds(transform.position, Vector3.zero);
                bool first = true;
                foreach (var r in visuals) { if (first) { bounds = r.bounds; first = false; } else bounds.Encapsulate(r.bounds); }
                return bounds;
            }
        }
        public bool Carry(Transform anchor)
        {
            if (IsCarried || IsInserted || !anchor) return false;
            IsCarried = true;
            foreach (var col in colliders) col.enabled = false;
            foreach (var rb in GetComponentsInChildren<Rigidbody>()) { rb.isKinematic = true; rb.useGravity = false; }
            // Fit the hand presentation to the smaller player without editing the imported asset.
            float height = Mathf.Max(VisualBounds.size.y, 0.001f);
            float fit = (kind == ItemKind.Quartz ? 0.13f : 0.15f) / height;
            transform.SetParent(anchor, true);
            transform.localScale *= fit;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;
            transform.position += anchor.position - VisualBounds.center;
            return true;
        }
        // VR release places the existing object on a support surface, keeping it recoverable.
        public bool ReleaseAt(Vector3 support, Quaternion rotation)
        {
            if (!IsCarried || IsInserted) return false;
            transform.SetParent(null, true);
            transform.localScale = originalWorldScale;
            transform.SetPositionAndRotation(support, rotation);
            var bounds = VisualBounds;
            transform.position += new Vector3(support.x-bounds.center.x, support.y-bounds.min.y+.005f, support.z-bounds.center.z);
            IsCarried = false;
            for (int i=0;i<colliders.Length;i++) colliders[i].enabled=originalColliderStates[i];
            return true;
        }
        public bool ToggleLight()
        {
            if (!IsCarried || kind != ItemKind.Lantern) return false;
            SetLight(!IsLit);
            return true;
        }
        public bool Insert(Transform snap, float length)
        {
            if (!snap || !IsCarried || IsInserted || kind != ItemKind.Quartz) return false;
            transform.SetParent(null, true);
            transform.localScale = originalWorldScale * (length / originalHeight);
            transform.SetPositionAndRotation(snap.position, snap.rotation);
            transform.position += snap.position - VisualBounds.center;
            transform.SetParent(snap, true);
            foreach (var col in colliders) col.enabled = false;
            IsCarried = false; IsInserted = true;
            return true;
        }
        void SetLight(bool lit)
        {
            IsLit = lit;
            if (lanternLight) lanternLight.enabled = lit;
            if (glow != null && luminousGlobe)
            {
                luminousGlobe.GetPropertyBlock(glow);
                glow.SetColor(emissionProperty, lit ? originalEmission : Color.black);
                luminousGlobe.SetPropertyBlock(glow);
            }
        }
    }
}

