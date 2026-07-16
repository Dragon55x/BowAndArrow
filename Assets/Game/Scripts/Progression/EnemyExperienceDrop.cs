using UnityEngine;

namespace BAA
{
    public sealed class EnemyExperienceDrop : MonoBehaviour
    {
        [SerializeField] private CharacterHealth health;
        [SerializeField] private ExperienceOrb orbPrefab;
        [SerializeField, Min(1)] private int experienceReward = 10;

        private void Awake()
        {
            if (health == null || orbPrefab == null)
            {
                Debug.LogError(
                    "EnemyExperienceDrop requires CharacterHealth and ExperienceOrb references.",
                    this);
                enabled = false;
                return;
            }

            health.Died += OnDied;
        }

        private void OnDied(CharacterHealth _)
        {
            var orb = Instantiate(
                orbPrefab,
                transform.position + Vector3.up * 0.6f,
                Quaternion.Euler(45f, 45f, 45f));
            orb.Initialize(experienceReward);
            health.Died -= OnDied;
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
