using System;
using UnityEngine;

namespace BAA
{
    public sealed class CharacterHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float initialMaxHealth = 1f;
        [SerializeField, Min(0f)] private float invulnerabilityDuration;

        private float _nextDamageTime = float.NegativeInfinity;

        public event Action<float> Damaged;
        public event Action<CharacterHealth> Died;

        public float CurrentHealth { get; private set; }
        public float MaxHealth { get; private set; }
        public bool IsDead { get; private set; }
        public float InvulnerabilityDuration
        {
            get => invulnerabilityDuration;
            set => invulnerabilityDuration = Mathf.Max(0f, value);
        }

        private void Awake()
        {
            Initialize(initialMaxHealth);
        }

        public void Initialize(float maxHealth)
        {
            MaxHealth = Mathf.Max(1f, maxHealth);
            CurrentHealth = MaxHealth;
            IsDead = false;
            _nextDamageTime = float.NegativeInfinity;
        }

        public void TakeDamage(in DamageInfo damage)
        {
            if (IsDead || damage.Amount <= 0f || Time.time < _nextDamageTime)
            {
                return;
            }

            _nextDamageTime = Time.time + invulnerabilityDuration;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage.Amount);
            var diedFromThisDamage = CurrentHealth <= 0f;
            if (diedFromThisDamage)
            {
                IsDead = true;
            }

            Damaged?.Invoke(damage.Amount);
            if (!diedFromThisDamage)
            {
                return;
            }

            Died?.Invoke(this);
        }
    }
}
