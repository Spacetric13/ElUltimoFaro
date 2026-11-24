using UnityEngine;
using UnityEngine.EventSystems;

namespace ElUltimoFaro
{
    public sealed class TouchLookArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        int pointer = int.MinValue;
        Vector2 delta;
        public void OnPointerDown(PointerEventData e) { if (pointer == int.MinValue) pointer = e.pointerId; }
        public void OnDrag(PointerEventData e)
        {
            if (pointer == e.pointerId) delta += e.delta / Mathf.Max(Screen.height, 1);
        }
        public void OnPointerUp(PointerEventData e) { if (pointer == e.pointerId) ResetInput(); }
        public Vector2 ConsumeDelta() { Vector2 result = delta; delta = Vector2.zero; return result; }
        public void ResetInput() { pointer = int.MinValue; delta = Vector2.zero; }
        void OnDisable() => ResetInput();
        void OnApplicationFocus(bool focused) { if (!focused) ResetInput(); }
        void OnApplicationPause(bool paused) { if (paused) ResetInput(); }
    }
}
