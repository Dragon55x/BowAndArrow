using UnityEngine;
using UnityEngine.UI;

namespace BAA
{
    public sealed class ExperienceBar : MonoBehaviour
    {
        [SerializeField] private PlayerProgression progression;
        [SerializeField] private Slider slider;
        [SerializeField] private Text levelText;

        private bool _isSubscribed;

        public void Configure(PlayerProgression source, Slider targetSlider, Text targetLevelText)
        {
            progression = source;
            slider = targetSlider;
            levelText = targetLevelText;
        }

        private void Start()
        {
            if (progression == null || slider == null || levelText == null)
            {
                Debug.LogError("ExperienceBar requires progression, Slider, and level Text.", this);
                enabled = false;
                return;
            }

            progression.ExperienceChanged += OnExperienceChanged;
            _isSubscribed = true;
            Refresh();
        }

        private void OnExperienceChanged(int _, int __, int ___)
        {
            Refresh();
        }

        private void Refresh()
        {
            slider.minValue = 0f;
            slider.maxValue = progression.RequiredExperience;
            slider.value = progression.CurrentExperience;
            levelText.text = $"LV {progression.Level}";
        }

        private void OnDestroy()
        {
            if (_isSubscribed && progression != null)
            {
                progression.ExperienceChanged -= OnExperienceChanged;
            }
        }
    }
}
