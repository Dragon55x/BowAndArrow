using UnityEngine;

namespace BAA
{
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private CharacterController controller;
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private float rotationSpeed = 720f;

        public PlayerRuntimeStats Stats { get; set; }
        public bool CanMove { get; set; } = true;

        private void Update()
        {
            var moveInput = CanMove ? input.MoveInput : Vector2.zero;
            var move = new Vector3(moveInput.x, 0f, moveInput.y);
            controller.Move(move * (Stats?.MoveSpeed ?? 0f) * Time.deltaTime);

            if (move.sqrMagnitude <= 0.01f)
            {
                return;
            }

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(move),
                rotationSpeed * Time.deltaTime);
        }
    }
}
