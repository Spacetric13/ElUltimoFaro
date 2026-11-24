using UnityEngine;
namespace ElUltimoFaro
{
    // Per-renderer overrides preserve the imported materials.
    public sealed class EmissionFader
    {
        readonly Renderer renderer;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly int emission, tint;
        readonly Color originalEmission, originalTint;
        public EmissionFader(Renderer target)
        {
            renderer = target;
            if (!renderer || !renderer.sharedMaterial) return;
            var mat = renderer.sharedMaterial;
            emission = Shader.PropertyToID(mat.HasProperty("emissiveFactor") ? "emissiveFactor" : "_EmissionColor");
            tint = Shader.PropertyToID(mat.HasProperty("baseColorFactor") ? "baseColorFactor" : "_BaseColor");
            originalEmission = mat.HasProperty(emission) ? mat.GetColor(emission) : Color.black;
            originalTint = mat.HasProperty(tint) ? mat.GetColor(tint) : Color.white;
        }
        public void Apply(float amount, bool fadeAlpha = false)
        {
            if (!renderer) return;
            renderer.GetPropertyBlock(block); block.SetColor(emission, originalEmission * amount);
            if (fadeAlpha) { var color = originalTint; color.a *= amount; block.SetColor(tint, color); }
            renderer.SetPropertyBlock(block);
        }
    }
    public static class MachineEffects
    {
        public static AudioClip Sound(string name, bool loop)
        {
            const int rate = 22050;
            int length = loop ? rate : rate / 5;
            var samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / rate;
                float wave = Mathf.Sin(2 * Mathf.PI * 60 * t) * .5f + Mathf.Sin(2 * Mathf.PI * 120 * t) * .25f + Mathf.Sin(2 * Mathf.PI * 780 * t) * .08f;
                samples[i] = wave * (loop ? .7f + .3f * Mathf.Cos(2 * Mathf.PI * 4 * t) : Mathf.Sin(Mathf.PI * i / length) * Mathf.Exp(-t * 20));
            }
            var clip = AudioClip.Create(name, length, 1, rate, false); clip.SetData(samples, 0); return clip;
        }
    }
}
