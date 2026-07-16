using NUnit.Framework;

namespace BAA.Tests
{
    public sealed class LevelProgressionTests
    {
        [Test]
        public void ExperienceBelowRequirement_StaysAtCurrentLevel()
        {
            var progression = new LevelProgression(20, 1.5f);

            var gained = progression.AddExperience(15);

            Assert.That(gained, Is.Zero);
            Assert.That(progression.Level, Is.EqualTo(1));
            Assert.That(progression.CurrentExperience, Is.EqualTo(15));
            Assert.That(progression.RequiredExperience, Is.EqualTo(20));
        }

        [Test]
        public void CrossingRequirement_CarriesRemainderAndQueuesLevel()
        {
            var progression = new LevelProgression(20, 1.5f);

            var gained = progression.AddExperience(25);

            Assert.That(gained, Is.EqualTo(1));
            Assert.That(progression.Level, Is.EqualTo(2));
            Assert.That(progression.CurrentExperience, Is.EqualTo(5));
            Assert.That(progression.RequiredExperience, Is.EqualTo(30));
            Assert.That(progression.PendingLevels, Is.EqualTo(1));
        }

        [Test]
        public void LargeExperienceGain_CanQueueMultipleLevels()
        {
            var progression = new LevelProgression(20, 1.5f);

            var gained = progression.AddExperience(80);

            Assert.That(gained, Is.EqualTo(2));
            Assert.That(progression.Level, Is.EqualTo(3));
            Assert.That(progression.CurrentExperience, Is.EqualTo(30));
            Assert.That(progression.RequiredExperience, Is.EqualTo(45));
            Assert.That(progression.PendingLevels, Is.EqualTo(2));
        }

        [Test]
        public void ConsumePendingLevel_ConsumesOnlyQueuedLevels()
        {
            var progression = new LevelProgression(20, 1.5f);
            progression.AddExperience(80);

            Assert.That(progression.ConsumePendingLevel(), Is.True);
            Assert.That(progression.ConsumePendingLevel(), Is.True);
            Assert.That(progression.ConsumePendingLevel(), Is.False);
            Assert.That(progression.PendingLevels, Is.Zero);
        }
    }
}
