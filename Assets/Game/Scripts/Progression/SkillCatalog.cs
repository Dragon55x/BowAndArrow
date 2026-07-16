using System;
using System.Collections.Generic;

namespace BAA
{
    public static class SkillCatalog
    {
        private static readonly SkillId[] AllSkills =
        {
            SkillId.AttackBoost,
            SkillId.AttackSpeedBoost,
            SkillId.MoveSpeedBoost,
            SkillId.CriticalBoost,
            SkillId.FrontArrow,
            SkillId.DiagonalArrow,
            SkillId.PiercingArrow,
            SkillId.HealOnKill
        };

        public static SkillId[] CreateThreeUniqueChoices(Random random, PlayerRuntimeStats stats)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var applicable = new List<SkillId>(AllSkills.Length);
            for (var i = 0; i < AllSkills.Length; i++)
            {
                if (SkillApplier.CanApply(AllSkills[i], stats))
                {
                    applicable.Add(AllSkills[i]);
                }
            }

            if (applicable.Count < 3)
            {
                throw new InvalidOperationException("At least three applicable skills are required.");
            }

            for (var i = applicable.Count - 1; i > 0; i--)
            {
                var swapIndex = random.Next(i + 1);
                var value = applicable[i];
                applicable[i] = applicable[swapIndex];
                applicable[swapIndex] = value;
            }

            return new[] { applicable[0], applicable[1], applicable[2] };
        }

        public static string GetTitle(SkillId skill)
        {
            switch (skill)
            {
                case SkillId.AttackBoost: return "攻击强化";
                case SkillId.AttackSpeedBoost: return "攻速强化";
                case SkillId.MoveSpeedBoost: return "移动强化";
                case SkillId.CriticalBoost: return "暴击强化";
                case SkillId.FrontArrow: return "正向箭 +1";
                case SkillId.DiagonalArrow: return "斜向箭 +1";
                case SkillId.PiercingArrow: return "穿透箭";
                case SkillId.HealOnKill: return "击杀回复";
                default: return skill.ToString();
            }
        }

        public static string GetDescription(SkillId skill)
        {
            switch (skill)
            {
                case SkillId.AttackBoost: return "攻击力提高 25%";
                case SkillId.AttackSpeedBoost: return "攻击间隔缩短 15%";
                case SkillId.MoveSpeedBoost: return "移动速度提高 15%";
                case SkillId.CriticalBoost: return "暴击率提高 10%";
                case SkillId.FrontArrow: return "额外发射一支正向箭";
                case SkillId.DiagonalArrow: return "两侧各增加一支斜向箭";
                case SkillId.PiercingArrow: return "箭矢可多穿透一个敌人";
                case SkillId.HealOnKill: return "每次击杀回复 5 点生命";
                default: return string.Empty;
            }
        }
    }
}
