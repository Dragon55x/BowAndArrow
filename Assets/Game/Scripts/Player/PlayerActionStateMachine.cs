using UnityEngine;

namespace BAA
{
    public enum PlayerActionState
    {
        Idle,
        Moving,
        Attacking,
        Dead
    }

    public sealed class PlayerActionStateMachine
    {
        private readonly float _inputDeadZone;
        private readonly float _settleDuration;
        private float _settledTime;
        private bool _dead;

        public PlayerActionStateMachine(float inputDeadZone, float settleDuration)
        {
            _inputDeadZone = Mathf.Max(0f, inputDeadZone);
            _settleDuration = Mathf.Max(0f, settleDuration);
        }

        public PlayerActionState Tick(Vector2 input, bool hasTarget, float deltaTime)
        {
            if (_dead)
            {
                return PlayerActionState.Dead;
            }

            if (input.sqrMagnitude > _inputDeadZone * _inputDeadZone)
            {
                _settledTime = 0f;
                return PlayerActionState.Moving;
            }

            _settledTime += Mathf.Max(0f, deltaTime);
            return hasTarget && _settledTime >= _settleDuration
                ? PlayerActionState.Attacking
                : PlayerActionState.Idle;
        }

        public void Kill()
        {
            _dead = true;
        }

        public void Reset()
        {
            _dead = false;
            _settledTime = 0f;
        }
    }
}
