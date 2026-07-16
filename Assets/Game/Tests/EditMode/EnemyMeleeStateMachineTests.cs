using NUnit.Framework;

namespace BAA.Tests
{
    public sealed class EnemyMeleeStateMachineTests
    {
        [Test]
        public void SpawnDelayElapsed_EntersChasing()
        {
            var machine = CreateMachine();

            var result = machine.Tick(10f, 0.5f, false);

            Assert.That(result.State, Is.EqualTo(EnemyMeleeState.Chasing));
        }

        [Test]
        public void TargetInRange_EntersWindupAndReportsEntry()
        {
            var machine = CreateChasingMachine();

            var result = machine.Tick(1.4f, 0.01f, false);

            Assert.That(result.State, Is.EqualTo(EnemyMeleeState.Windup));
            Assert.That(result.EnteredWindup, Is.True);
            Assert.That(result.ShouldStrike, Is.False);
        }

        [Test]
        public void WindupElapsed_StrikesExactlyOnceThenRecovers()
        {
            var machine = CreateChasingMachine();
            machine.Tick(1f, 0.01f, false);

            var strike = machine.Tick(1f, 0.4f, false);
            var recovery = machine.Tick(1f, 0.01f, false);

            Assert.That(strike.State, Is.EqualTo(EnemyMeleeState.Recovery));
            Assert.That(strike.ShouldStrike, Is.True);
            Assert.That(recovery.State, Is.EqualTo(EnemyMeleeState.Recovery));
            Assert.That(recovery.ShouldStrike, Is.False);
        }

        [Test]
        public void RecoveryElapsed_ReturnsToChasing()
        {
            var machine = CreateChasingMachine();
            machine.Tick(1f, 0.01f, false);
            machine.Tick(1f, 0.4f, false);

            var result = machine.Tick(1f, 1f, false);

            Assert.That(result.State, Is.EqualTo(EnemyMeleeState.Chasing));
        }

        [Test]
        public void DeathFromAnyState_EntersDeadWithoutStriking()
        {
            var machine = CreateChasingMachine();
            machine.Tick(1f, 0.01f, false);

            var result = machine.Tick(1f, 0.4f, true);

            Assert.That(result.State, Is.EqualTo(EnemyMeleeState.Dead));
            Assert.That(result.ShouldStrike, Is.False);
        }

        private static EnemyMeleeStateMachine CreateMachine()
        {
            return new EnemyMeleeStateMachine(0.5f, 1.4f, 0.4f, 1f);
        }

        private static EnemyMeleeStateMachine CreateChasingMachine()
        {
            var machine = CreateMachine();
            machine.Tick(10f, 0.5f, false);
            return machine;
        }
    }
}
