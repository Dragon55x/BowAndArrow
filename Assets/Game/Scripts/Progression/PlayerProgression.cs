using System;
using UnityEngine;

namespace BAA
{
    public sealed class PlayerProgression : MonoBehaviour
    {
        [SerializeField, Min(1)] private int baseExperienceRequirement = 20;
        [SerializeField, Min(1f)] private float requirementGrowth = 1.5f;

        private LevelProgression _progression;

        public event Action<int, int, int> ExperienceChanged;
        public event Action LevelUpQueued;

        public int Level => _progression?.Level ?? 1;
        public int CurrentExperience => _progression?.CurrentExperience ?? 0;
        public int RequiredExperience => _progression?.RequiredExperience ?? baseExperienceRequirement;
        public int PendingLevels => _progression?.PendingLevels ?? 0;

        private void Awake()
        {
            _progression = new LevelProgression(baseExperienceRequirement, requirementGrowth);
        }

        public void AddExperience(int amount)
        {
            var gainedLevels = _progression.AddExperience(amount);
            ExperienceChanged?.Invoke(Level, CurrentExperience, RequiredExperience);
            if (gainedLevels > 0)
            {
                LevelUpQueued?.Invoke();
            }
        }

        public bool ConsumePendingLevel()
        {
            return _progression.ConsumePendingLevel();
        }
    }
}
