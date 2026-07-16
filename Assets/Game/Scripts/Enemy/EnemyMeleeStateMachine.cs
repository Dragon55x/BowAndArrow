using UnityEngine;

namespace BAA
{
    public enum EnemyMeleeState
    {
        Spawning,
        Chasing,
        Windup,
        Recovery,
        Dead
    }

    public readonly struct EnemyMeleeTickResult
    {
        public EnemyMeleeTickResult(
            EnemyMeleeState state,
            bool enteredWindup = false,
            bool shouldStrike = false)
        {
            State = state;
            EnteredWindup = enteredWindup;
            ShouldStrike = shouldStrike;
        }

        public EnemyMeleeState State { get; }
        public bool EnteredWindup { get; }
        public bool ShouldStrike { get; }
    }

    public sealed class EnemyMeleeStateMachine
    {
        private readonly float _spawnDelay;
        private readonly float _attackRange;
        private readonly float _windupDuration;
        private readonly float _recoveryDuration;

        private float _remainingTime;

        public EnemyMeleeStateMachine(
            float spawnDelay,
            float attackRange,
            float windupDuration,
            float recoveryDuration)
        {
            _spawnDelay = Mathf.Max(0f, spawnDelay);
            _attackRange = Mathf.Max(0f, attackRange);
            _windupDuration = Mathf.Max(0f, windupDuration);
            _recoveryDuration = Mathf.Max(0f, recoveryDuration);
            Reset();
        }

        public EnemyMeleeState State { get; private set; }

        public void Reset()
        {
            State = EnemyMeleeState.Spawning;
            _remainingTime = _spawnDelay;
        }

        public EnemyMeleeTickResult Tick(float distance, float deltaTime, bool isDead)
        {
            if (isDead || State == EnemyMeleeState.Dead)
            {
                State = EnemyMeleeState.Dead;
                return new EnemyMeleeTickResult(State);
            }

            var elapsed = Mathf.Max(0f, deltaTime);
            switch (State)
            {
                case EnemyMeleeState.Spawning:
                    _remainingTime -= elapsed;
                    if (_remainingTime <= 0f)
                    {
                        State = EnemyMeleeState.Chasing;
                    }

                    break;

                case EnemyMeleeState.Chasing:
                    if (distance <= _attackRange)
                    {
                        State = EnemyMeleeState.Windup;
                        _remainingTime = _windupDuration;
                        return new EnemyMeleeTickResult(State, enteredWindup: true);
                    }

                    break;

                case EnemyMeleeState.Windup:
                    _remainingTime -= elapsed;
                    if (_remainingTime <= 0f)
                    {
                        State = EnemyMeleeState.Recovery;
                        _remainingTime = _recoveryDuration;
                        return new EnemyMeleeTickResult(State, shouldStrike: true);
                    }

                    break;

                case EnemyMeleeState.Recovery:
                    _remainingTime -= elapsed;
                    if (_remainingTime <= 0f)
                    {
                        State = EnemyMeleeState.Chasing;
                    }

                    break;
            }

            return new EnemyMeleeTickResult(State);
        }
    }
}
