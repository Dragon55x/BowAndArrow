using System;
using UnityEngine;

namespace BAA
{
    public sealed class CharacterHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float initialMaxHealth = 1f;

        public event Action<float> Damaged;
        public event Action<CharacterHealth> Died;

        public float CurrentHealth { get; private set; }
        public float MaxHealth { get; private set; }
        public bool IsDead { get; private set; }

        private void Awake()
        {
            Initialize(initialMaxHealth);
        }

        public void Initialize(float maxHealth)
        {
            MaxHealth = Mathf.Max(1f, maxHealth);
            CurrentHealth = MaxHealth;
            IsDead = false;
        }

        public void TakeDamage(in DamageInfo damage)
        {
            if (IsDead || damage.Amount <= 0f)
            {
                return;
            }

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
