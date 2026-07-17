using System;
using NUnit.Framework;

namespace BAA.Tests
{
    public sealed class WaveProgressionTests
    {
        [Test]
        public void Constructor_RejectsMissingOrInvalidWaves()
        {
            Assert.That(
                () => new WaveProgression(null),
                Throws.TypeOf<ArgumentException>());
            Assert.That(
                () => new WaveProgression(Array.Empty<int>()),
                Throws.TypeOf<ArgumentException>());
            Assert.That(
                () => new WaveProgression(new[] { 3, 0, 5 }),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void BeginNextWave_InitializesFirstWave()
        {
            var progression = new WaveProgression(new[] { 3, 4, 5 });

            progression.BeginNextWave();

            Assert.That(progression.CurrentWaveNumber, Is.EqualTo(1));
            Assert.That(progression.TotalWaves, Is.EqualTo(3));
            Assert.That(progression.RemainingToSpawn, Is.EqualTo(3));
            Assert.That(progression.AliveEnemies, Is.Zero);
            Assert.That(progression.IsCurrentWaveCleared, Is.False);
            Assert.That(progression.IsRoomCleared, Is.False);
        }

        [Test]
        public void SpawnAndDeath_UpdateOutstandingEnemyCount()
        {
            var progression = new WaveProgression(new[] { 2 });
            progression.BeginNextWave();

            progression.RegisterSpawn();
            progression.RegisterSpawn();
            progression.RegisterDeath();

            Assert.That(progression.RemainingToSpawn, Is.Zero);
            Assert.That(progression.AliveEnemies, Is.EqualTo(1));
            Assert.That(progression.OutstandingEnemies, Is.EqualTo(1));
            Assert.That(progression.IsCurrentWaveCleared, Is.False);

            progression.RegisterDeath();

            Assert.That(progression.OutstandingEnemies, Is.Zero);
            Assert.That(progression.IsCurrentWaveCleared, Is.True);
            Assert.That(progression.IsRoomCleared, Is.True);
        }

        [Test]
        public void BeginNextWave_RequiresCurrentWaveToBeCleared()
        {
            var progression = new WaveProgression(new[] { 1, 2 });
            progression.BeginNextWave();

            Assert.That(
                () => progression.BeginNextWave(),
                Throws.TypeOf<InvalidOperationException>());

            progression.RegisterSpawn();
            Assert.That(
                () => progression.BeginNextWave(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void ClearedWave_CanAdvanceUntilRoomIsCleared()
        {
            var progression = new WaveProgression(new[] { 1, 2 });
            progression.BeginNextWave();
            progression.RegisterSpawn();
            progression.RegisterDeath();

            progression.BeginNextWave();

            Assert.That(progression.CurrentWaveNumber, Is.EqualTo(2));
            Assert.That(progression.RemainingToSpawn, Is.EqualTo(2));

            progression.RegisterSpawn();
            progression.RegisterSpawn();
            progression.RegisterDeath();
            progression.RegisterDeath();

            Assert.That(progression.IsRoomCleared, Is.True);
            Assert.That(
                () => progression.BeginNextWave(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void InvalidSpawnOrDeathRegistration_IsRejected()
        {
            var progression = new WaveProgression(new[] { 1 });

            Assert.That(
                () => progression.RegisterSpawn(),
                Throws.TypeOf<InvalidOperationException>());

            progression.BeginNextWave();
            Assert.That(
                () => progression.RegisterDeath(),
                Throws.TypeOf<InvalidOperationException>());

            progression.RegisterSpawn();
            progression.RegisterDeath();
            Assert.That(
                () => progression.RegisterDeath(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void ExperienceSettlementCounter_TracksPendingOrbs()
        {
            var counter = new ExperienceSettlementCounter();

            counter.Register();
            counter.Register();

            Assert.That(counter.PendingCount, Is.EqualTo(2));

            counter.Unregister();
            Assert.That(counter.PendingCount, Is.EqualTo(1));

            counter.Unregister();
            counter.Unregister();
            Assert.That(counter.PendingCount, Is.Zero);

            counter.Register();
            counter.Reset();
            Assert.That(counter.PendingCount, Is.Zero);
        }
    }
}
