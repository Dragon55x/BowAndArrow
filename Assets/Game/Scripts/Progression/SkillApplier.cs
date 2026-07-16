using UnityEngine;

namespace BAA
{
    public static class SkillApplier
    {
        public static bool CanApply(SkillId skill, PlayerRuntimeStats stats)
        {
            if (stats == null)
            {
                return false;
            }

            switch (skill)
            {
                case SkillId.AttackSpeedBoost:
                    return stats.AttackInterval > 0.1501f;
                case SkillId.CriticalBoost:
                    return stats.CritChance < 0.6f;
                case SkillId.FrontArrow:
                    return stats.ForwardArrowCount < 2;
                case SkillId.DiagonalArrow:
                    return stats.DiagonalArrowCount < 2;
                case SkillId.PiercingArrow:
                    return stats.PierceCount < 3;
                default:
                    return true;
            }
        }

        public static bool Apply(SkillId skill, PlayerRuntimeStats stats)
        {
            if (!CanApply(skill, stats))
            {
                return false;
            }

            switch (skill)
            {
                case SkillId.AttackBoost:
                    stats.Attack *= 1.25f;
                    break;
                case SkillId.AttackSpeedBoost:
                    stats.AttackInterval = Mathf.Max(0.15f, stats.AttackInterval * 0.85f);
                    break;
                case SkillId.MoveSpeedBoost:
                    stats.MoveSpeed *= 1.15f;
                    break;
                case SkillId.CriticalBoost:
                    stats.CritChance = Mathf.Min(0.6f, stats.CritChance + 0.1f);
                    break;
                case SkillId.FrontArrow:
                    stats.ForwardArrowCount++;
                    break;
                case SkillId.DiagonalArrow:
                    stats.DiagonalArrowCount++;
                    break;
                case SkillId.PiercingArrow:
                    stats.PierceCount++;
                    break;
                case SkillId.HealOnKill:
                    stats.HealOnKill += 5f;
                    break;
            }

            return true;
        }
    }
}
