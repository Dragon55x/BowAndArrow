using NUnit.Framework;
using UnityEngine;

namespace BAA.Tests
{
    public sealed class GamePauseCoordinatorTests
    {
        [SetUp]
        public void SetUp()
        {
            GamePauseCoordinator.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            GamePauseCoordinator.Reset();
        }

        [Test]
        public void MultipleOwners_ResumeOnlyAfterLastOwnerReleases()
        {
            var upgradeOwner = new object();
            var gameOverOwner = new object();
            Time.timeScale = 0.75f;

            GamePauseCoordinator.Acquire(upgradeOwner);
            GamePauseCoordinator.Acquire(gameOverOwner);
            GamePauseCoordinator.Release(upgradeOwner);

            Assert.That(Time.timeScale, Is.Zero);

            GamePauseCoordinator.Release(gameOverOwner);

            Assert.That(Time.timeScale, Is.EqualTo(0.75f));
        }

        [Test]
        public void ReleasingUnknownOwner_DoesNotResumeGame()
        {
            var owner = new object();
            GamePauseCoordinator.Acquire(owner);

            GamePauseCoordinator.Release(new object());

            Assert.That(Time.timeScale, Is.Zero);
        }
    }
}
