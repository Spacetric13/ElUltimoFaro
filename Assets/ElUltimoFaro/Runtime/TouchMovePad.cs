using UnityEngine;
using UnityEngine.EventSystems;

namespace ElUltimoFaro
{
    public sealed class TouchMovePad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] RectTransform handle;
        [SerializeField] float radius = 65f;
        int pointer = int.MinValue;
        public Vector2 Value { get; private set; }
        public void Configure(RectTransform thumb) => handle = thumb;
        public void OnPointerDown(PointerEventData e)
        {
            if (pointer != int.MinValue) return;
            pointer = e.pointerId;
            OnDrag(e);
        }
        public void OnDrag(PointerEventData e)
        {
            if (pointer != e.pointerId) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,
                e.position, e.pressEventCamera, out Vector2 local);
            Value = Vector2.ClampMagnitude(local / radius, 1);
            if (handle) handle.anchoredPosition = Value * radius;
        }
        public void OnPointerUp(PointerEventData e) { if (pointer == e.pointerId) ResetInput(); }
        public void ResetInput() { pointer = int.MinValue; Value = Vector2.zero; if (handle) handle.anchoredPosition = Vector2.zero; }
        void OnDisable() => ResetInput();
        void OnApplicationFocus(bool focused) { if (!focused) ResetInput(); }
        void OnApplicationPause(bool paused) { if (paused) ResetInput(); }
    }
}
