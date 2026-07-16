using UnityEngine;

namespace BAA
{
    public sealed class PlayerInputReader : MonoBehaviour, IPlayerInputSource
    {
        [SerializeField] private VirtualJoystick virtualJoystick;
        [SerializeField, Range(0f, 0.9f)] private float deadZone = 0.1f;

        public float DeadZone
        {
            get => deadZone;
            set => deadZone = Mathf.Clamp(value, 0f, 0.9f);
        }

        public Vector2 MoveInput
        {
            get
            {
                var keyboard = new Vector2(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical"));

                var rawInput = keyboard.sqrMagnitude > 0f
                    ? Vector2.ClampMagnitude(keyboard, 1f)
                    : virtualJoystick == null
                        ? Vector2.zero
                        : virtualJoystick.MoveInput;

                return rawInput.sqrMagnitude <= deadZone * deadZone
                    ? Vector2.zero
                    : rawInput;
            }
        }
    }
}
