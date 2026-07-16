using UnityEngine;
using UnityEngine.EventSystems;

namespace BAA
{
    public sealed class VirtualJoystick : MonoBehaviour, IPlayerInputSource,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField, Range(0f, 0.9f)] private float deadZone = 0.1f;

        private Vector2 _rawInput;

        public Vector2 MoveInput => _rawInput.magnitude < deadZone ? Vector2.zero : _rawInput;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null || eventData == null)
            {
                ResetInput();
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var point))
            {
                ResetInput();
                return;
            }

            var radius = background.rect.size * 0.5f;
            if (Mathf.Abs(radius.x) <= Mathf.Epsilon || Mathf.Abs(radius.y) <= Mathf.Epsilon)
            {
                ResetInput();
                return;
            }

            _rawInput = new Vector2(point.x / radius.x, point.y / radius.y);
            _rawInput = Vector2.ClampMagnitude(_rawInput, 1f);

            if (handle != null)
            {
                handle.anchoredPosition = Vector2.Scale(_rawInput, radius);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetInput();
        }

        private void OnDisable()
        {
            ResetInput();
        }

        private void ResetInput()
        {
            _rawInput = Vector2.zero;

            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }
    }
}
