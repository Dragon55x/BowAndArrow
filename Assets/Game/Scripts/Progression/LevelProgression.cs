using System;

namespace BAA
{
    public sealed class LevelProgression
    {
        private readonly float _requirementGrowth;

        public LevelProgression(int baseRequirement, float requirementGrowth)
        {
            RequiredExperience = Math.Max(1, baseRequirement);
            _requirementGrowth = Math.Max(1f, requirementGrowth);
            Level = 1;
        }

        public int Level { get; private set; }
        public int CurrentExperience { get; private set; }
        public int RequiredExperience { get; private set; }
        public int PendingLevels { get; private set; }

        public int AddExperience(int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            CurrentExperience += amount;
            var levelsGained = 0;
            while (CurrentExperience >= RequiredExperience)
            {
                CurrentExperience -= RequiredExperience;
                Level++;
                PendingLevels++;
                levelsGained++;
                RequiredExperience = Math.Max(
                    RequiredExperience + 1,
                    (int)Math.Ceiling(RequiredExperience * _requirementGrowth));
            }

            return levelsGained;
        }

        public bool ConsumePendingLevel()
        {
            if (PendingLevels <= 0)
            {
                return false;
            }

            PendingLevels--;
            return true;
        }
    }
}
