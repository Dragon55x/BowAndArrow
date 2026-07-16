using NUnit.Framework;
using UnityEngine;

namespace BAA.Tests
{
    public sealed class PlayerActionStateMachineTests
    {
        [Test]
        public void MovingInput_AlwaysProducesMoving()
        {
            var machine = new PlayerActionStateMachine(0.1f, 0.1f);
            Assert.That(machine.Tick(Vector2.right, true, 1f), Is.EqualTo(PlayerActionState.Moving));
        }

        [Test]
        public void ReleasingInput_WaitsBeforeAttacking()
        {
            var machine = new PlayerActionStateMachine(0.1f, 0.1f);
            machine.Tick(Vector2.right, true, 0.02f);
            Assert.That(machine.Tick(Vector2.zero, true, 0.05f), Is.EqualTo(PlayerActionState.Idle));
            Assert.That(machine.Tick(Vector2.zero, true, 0.05f), Is.EqualTo(PlayerActionState.Attacking));
        }

        [Test]
        public void NoTarget_ProducesIdleAfterStopping()
        {
            var machine = new PlayerActionStateMachine(0.1f, 0.1f);
            Assert.That(machine.Tick(Vector2.zero, false, 1f), Is.EqualTo(PlayerActionState.Idle));
        }

        [Test]
        public void DeadPlayer_RemainsDead()
        {
            var machine = new PlayerActionStateMachine(0.1f, 0.1f);
            machine.Kill();
            Assert.That(machine.Tick(Vector2.right, true, 1f), Is.EqualTo(PlayerActionState.Dead));
        }
    }
}
