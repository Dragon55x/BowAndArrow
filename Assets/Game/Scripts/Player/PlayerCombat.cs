using UnityEngine;

namespace BAA
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private PlayerConfig config;
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private TargetFinder targetFinder;
        [SerializeField] private WeaponController weapon;
        [SerializeField] private CharacterHealth health;
        [SerializeField] private float rotationSpeed = 720f;

        private PlayerActionStateMachine _stateMachine;
        private PlayerRuntimeStats _stats;
        private float _attackCooldown;

        public PlayerRuntimeStats RuntimeStats => _stats;

        private void Awake()
        {
            _stats = new PlayerRuntimeStats
            {
                Attack = config.Attack,
                AttackInterval = config.AttackInterval,
                MoveSpeed = config.MoveSpeed
            };
            _stateMachine = new PlayerActionStateMachine(config.InputDeadZone, config.SettleDuration);

            input.DeadZone = config.InputDeadZone;
            movement.Stats = _stats;
            weapon.Stats = _stats;
            targetFinder.Range = config.TargetRange;
            health.Initialize(config.MaxHealth);
            health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }
        }

        private void Update()
        {
            var moveInput = input.MoveInput;
            var target = targetFinder.FindNearest();
            var state = _stateMachine.Tick(moveInput, target != null, Time.deltaTime);

            _attackCooldown = Mathf.Max(0f, _attackCooldown - Time.deltaTime);
            if (state != PlayerActionState.Attacking || target == null || target.IsDead)
            {
                return;
            }

            var direction = target.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    rotationSpeed * Time.deltaTime);
            }

            if (_attackCooldown > 0f)
            {
                return;
            }

            weapon.FireAt(target.transform.position);
            _attackCooldown = _stats.AttackInterval;
        }

        private void OnDied(CharacterHealth _)
        {
            _stateMachine.Kill();
            movement.CanMove = false;
        }
    }
}
