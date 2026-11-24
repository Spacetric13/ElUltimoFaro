using UnityEngine;

namespace ElUltimoFaro
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class MobileSafeArea : MonoBehaviour
    {
        Rect lastArea;
        Vector2Int lastSize;
        void OnEnable() => Apply();
        void Update() { if (lastArea != Screen.safeArea || lastSize != new Vector2Int(Screen.width, Screen.height)) Apply(); }
        void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            lastArea = Screen.safeArea;
            lastSize = new Vector2Int(Screen.width, Screen.height);
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(lastArea.xMin / Screen.width, lastArea.yMin / Screen.height);
            rect.anchorMax = new Vector2(lastArea.xMax / Screen.width, lastArea.yMax / Screen.height);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
