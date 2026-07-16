using System;
using NUnit.Framework;

namespace BAA.Tests
{
    public sealed class SkillApplierTests
    {
        [TestCase(SkillId.AttackBoost)]
        [TestCase(SkillId.AttackSpeedBoost)]
        [TestCase(SkillId.MoveSpeedBoost)]
        [TestCase(SkillId.CriticalBoost)]
        [TestCase(SkillId.FrontArrow)]
        [TestCase(SkillId.DiagonalArrow)]
        [TestCase(SkillId.PiercingArrow)]
        [TestCase(SkillId.HealOnKill)]
        public void Apply_ChangesTheRequestedRuntimeStat(SkillId skill)
        {
            var stats = CreateStats();

            SkillApplier.Apply(skill, stats);

            switch (skill)
            {
                case SkillId.AttackBoost:
                    Assert.That(stats.Attack, Is.EqualTo(12.5f).Within(0.001f));
                    break;
                case SkillId.AttackSpeedBoost:
                    Assert.That(stats.AttackInterval, Is.EqualTo(0.51f).Within(0.001f));
                    break;
                case SkillId.MoveSpeedBoost:
                    Assert.That(stats.MoveSpeed, Is.EqualTo(5.75f).Within(0.001f));
                    break;
                case SkillId.CriticalBoost:
                    Assert.That(stats.CritChance, Is.EqualTo(0.1f).Within(0.001f));
                    break;
                case SkillId.FrontArrow:
                    Assert.That(stats.ForwardArrowCount, Is.EqualTo(2));
                    break;
                case SkillId.DiagonalArrow:
                    Assert.That(stats.DiagonalArrowCount, Is.EqualTo(1));
                    break;
                case SkillId.PiercingArrow:
                    Assert.That(stats.PierceCount, Is.EqualTo(1));
                    break;
                case SkillId.HealOnKill:
                    Assert.That(stats.HealOnKill, Is.EqualTo(5f));
                    break;
                default:
                    Assert.Fail($"Unhandled skill {skill}");
                    break;
            }
        }

        [Test]
        public void CappedSkills_AreExcludedFromChoices()
        {
            var stats = CreateStats();
            stats.ForwardArrowCount = 2;
            stats.DiagonalArrowCount = 2;
            stats.PierceCount = 3;
            stats.CritChance = 0.6f;

            Assert.That(SkillApplier.CanApply(SkillId.FrontArrow, stats), Is.False);
            Assert.That(SkillApplier.CanApply(SkillId.DiagonalArrow, stats), Is.False);
            Assert.That(SkillApplier.CanApply(SkillId.PiercingArrow, stats), Is.False);
            Assert.That(SkillApplier.CanApply(SkillId.CriticalBoost, stats), Is.False);
        }

        [Test]
        public void AttackSpeedAtMinimum_IsExcludedFromChoices()
        {
            var stats = CreateStats();
            stats.AttackInterval = 0.15f;

            Assert.That(SkillApplier.CanApply(SkillId.AttackSpeedBoost, stats), Is.False);
            Assert.That(SkillApplier.Apply(SkillId.AttackSpeedBoost, stats), Is.False);
            Assert.That(stats.AttackInterval, Is.EqualTo(0.15f));
        }

        [Test]
        public void ThreeChoices_AreUniqueAndApplicable()
        {
            var stats = CreateStats();

            var choices = SkillCatalog.CreateThreeUniqueChoices(new Random(1234), stats);

            Assert.That(choices, Has.Length.EqualTo(3));
            Assert.That(choices[0], Is.Not.EqualTo(choices[1]));
            Assert.That(choices[0], Is.Not.EqualTo(choices[2]));
            Assert.That(choices[1], Is.Not.EqualTo(choices[2]));
            Assert.That(choices, Is.All.Matches<SkillId>(skill => SkillApplier.CanApply(skill, stats)));
        }

        private static PlayerRuntimeStats CreateStats()
        {
            return new PlayerRuntimeStats
            {
                Attack = 10f,
                AttackInterval = 0.6f,
                MoveSpeed = 5f,
                ForwardArrowCount = 1
            };
        }
    }
}
