using UnityEngine;
using UnityEngine.UI;

namespace BAA
{
    public sealed class PlayerHealthBar : MonoBehaviour
    {
        [SerializeField] private CharacterHealth health;
        [SerializeField] private Slider slider;

        private bool _isSubscribed;

        public void Configure(CharacterHealth source, Slider targetSlider)
        {
            health = source;
            slider = targetSlider;
        }

        private void Start()
        {
            if (health == null || slider == null)
            {
                Debug.LogError("PlayerHealthBar requires CharacterHealth and Slider references.", this);
                enabled = false;
                return;
            }

            health.Damaged += OnHealthChanged;
            health.Died += OnDied;
            _isSubscribed = true;
            Refresh();
        }

        private void OnHealthChanged(float _)
        {
            Refresh();
        }

        private void OnDied(CharacterHealth _)
        {
            Refresh();
        }

        private void Refresh()
        {
            slider.minValue = 0f;
            slider.maxValue = health.MaxHealth;
            slider.value = health.CurrentHealth;
        }

        private void OnDestroy()
        {
            if (!_isSubscribed || health == null)
            {
                return;
            }

            health.Damaged -= OnHealthChanged;
            health.Died -= OnDied;
        }
    }
}
