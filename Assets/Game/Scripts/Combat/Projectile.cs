using UnityEngine;

namespace BAA
{
    public sealed class Projectile : MonoBehaviour
    {
        private Vector3 _velocity;
        private DamageInfo _damage;
        private float _remainingLife;
        private ProjectilePool _owner;
        private bool _inFlight;
        private int _remainingPierces;
        private IDamageable _lastHitTarget;

        public void Launch(Vector3 velocity, DamageInfo damage, float lifetime, int pierceCount = 0)
        {
            if (_owner == null)
            {
                Debug.LogError("Projectile must be obtained from a ProjectilePool before launch.", this);
                return;
            }

            _velocity = velocity;
            _damage = damage;
            _remainingLife = Mathf.Max(0f, lifetime);
            _remainingPierces = Mathf.Max(0, pierceCount);
            _lastHitTarget = null;
            _inFlight = true;
            gameObject.SetActive(true);

            if (_remainingLife <= 0f)
            {
                ReturnToPool();
            }
        }

        public void Launch(
            Vector3 velocity,
            in DamageInfo damage,
            float lifetime,
            ProjectilePool owner,
            int pierceCount = 0)
        {
            BindToPool(owner);
            Launch(velocity, damage, lifetime, pierceCount);
        }

        internal void BindToPool(ProjectilePool owner)
        {
            _owner = owner;
        }

        internal void OnReleasedByPool()
        {
            _inFlight = false;
            _velocity = Vector3.zero;
            _remainingLife = 0f;
            _remainingPierces = 0;
            _lastHitTarget = null;
        }

        private void Update()
        {
            if (!_inFlight)
            {
                return;
            }

            transform.position += _velocity * Time.deltaTime;
            _remainingLife -= Time.deltaTime;
            if (_remainingLife <= 0f)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_inFlight)
            {
                return;
            }

            var target = FindDamageable(other);
            if (target == null || ReferenceEquals(target, _lastHitTarget))
            {
                return;
            }

            var targetHealth = target as CharacterHealth;
            var wasAlive = targetHealth != null && !targetHealth.IsDead;
            target.TakeDamage(_damage);
            _lastHitTarget = target;

            if (wasAlive && targetHealth.IsDead && _damage.Source is WeaponController weapon)
            {
                weapon.OnEnemyKilled();
            }

            if (_remainingPierces > 0)
            {
                _remainingPierces--;
                return;
            }

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (!_inFlight)
            {
                return;
            }

            _inFlight = false;
            _owner.Release(this);
        }

        private static IDamageable FindDamageable(Collider other)
        {
            return other.GetComponentInParent<IDamageable>();
        }
    }
}
