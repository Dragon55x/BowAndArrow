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

        public void Launch(Vector3 velocity, DamageInfo damage, float lifetime)
        {
            if (_owner == null)
            {
                Debug.LogError("Projectile must be obtained from a ProjectilePool before launch.", this);
                return;
            }

            _velocity = velocity;
            _damage = damage;
            _remainingLife = Mathf.Max(0f, lifetime);
            _inFlight = true;
            gameObject.SetActive(true);

            if (_remainingLife <= 0f)
            {
                ReturnToPool();
            }
        }

        public void Launch(Vector3 velocity, in DamageInfo damage, float lifetime, ProjectilePool owner)
        {
            BindToPool(owner);
            Launch(velocity, damage, lifetime);
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
            if (target == null)
            {
                return;
            }

            target.TakeDamage(_damage);
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
