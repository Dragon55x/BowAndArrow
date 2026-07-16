using UnityEngine;

namespace BAA
{
    public sealed class EnemyMeleeBrain : MonoBehaviour
    {
        private const int HitBufferSize = 8;

        [SerializeField] private EnemyConfig config;
        [SerializeField] private CharacterController controller;
        [SerializeField] private CharacterHealth health;
        [SerializeField] private EnemyTelegraphView telegraph;
        [SerializeField] private LayerMask playerMask;

        private readonly Collider[] _hitBuffer = new Collider[HitBufferSize];
        private EnemyMeleeStateMachine _stateMachine;
        private Transform _target;
        private Vector3 _attackCenter;
        private bool _isShuttingDown;

        private void Awake()
        {
            if (!ValidateConfiguration())
            {
                enabled = false;
                return;
            }

            health.Initialize(config.MaxHealth);
            health.Died += OnDied;
            _stateMachine = new EnemyMeleeStateMachine(
                config.SpawnDelay,
                config.AttackRange,
                config.WindupDuration,
                config.RecoveryDuration);
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _target = player.transform;
                return;
            }

            Debug.LogError("EnemyMeleeBrain could not find an active GameObject tagged Player.", this);
            enabled = false;
        }

        private void Update()
        {
            if (_target == null || _stateMachine == null || _isShuttingDown)
            {
                return;
            }

            var offset = _target.position - transform.position;
            offset.y = 0f;
            var distance = offset.magnitude;
            var result = _stateMachine.Tick(distance, Time.deltaTime, health.IsDead);

            if (result.EnteredWindup)
            {
                _attackCenter = _target.position;
                telegraph.SetWarning(true);
            }

            if (result.State == EnemyMeleeState.Chasing)
            {
                Chase(offset);
            }

            if (result.ShouldStrike)
            {
                telegraph.SetWarning(false);
                Strike();
            }
        }

        private void Chase(Vector3 offset)
        {
            if (offset.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            var direction = offset.normalized;
            var desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                desiredRotation,
                config.RotationSpeed * Time.deltaTime);
            controller.Move(direction * (config.MoveSpeed * Time.deltaTime));
        }

        private void Strike()
        {
            var hitCount = Physics.OverlapSphereNonAlloc(
                _attackCenter,
                config.HitRadius,
                _hitBuffer,
                playerMask,
                QueryTriggerInteraction.Collide);

            for (var i = 0; i < hitCount; i++)
            {
                var behaviours = _hitBuffer[i].GetComponentsInParent<MonoBehaviour>();
                for (var j = 0; j < behaviours.Length; j++)
                {
                    if (!(behaviours[j] is IDamageable damageable))
                    {
                        continue;
                    }

                    damageable.TakeDamage(new DamageInfo(config.Damage, this));
                    return;
                }
            }
        }

        private bool ValidateConfiguration()
        {
            if (config == null)
            {
                Debug.LogError("EnemyMeleeBrain requires an EnemyConfig.", this);
                return false;
            }

            if (controller == null)
            {
                Debug.LogError("EnemyMeleeBrain requires a CharacterController.", this);
                return false;
            }

            if (health == null)
            {
                Debug.LogError("EnemyMeleeBrain requires CharacterHealth.", this);
                return false;
            }

            if (telegraph == null)
            {
                Debug.LogError("EnemyMeleeBrain requires EnemyTelegraphView.", this);
                return false;
            }

            if (playerMask.value == 0)
            {
                Debug.LogError("EnemyMeleeBrain requires a non-empty Player LayerMask.", this);
                return false;
            }

            return true;
        }

        private void OnDied(CharacterHealth _)
        {
            _isShuttingDown = true;
            telegraph.SetWarning(false);
            controller.enabled = false;
            health.Died -= OnDied;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }
        }
    }
}
