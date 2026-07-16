using NUnit.Framework;
using UnityEngine;

namespace BAA.Tests
{
    public sealed class CharacterHealthTests
    {
        [Test]
        public void DamageAtZeroHealth_RaisesDeathOnlyOnce()
        {
            var go = new GameObject("HealthTest");
            try
            {
                var health = go.AddComponent<CharacterHealth>();
                health.Initialize(10f);
                var deaths = 0;
                health.Died += _ => deaths++;

                health.TakeDamage(new DamageInfo(10f, this));
                health.TakeDamage(new DamageInfo(10f, this));

                Assert.That(health.CurrentHealth, Is.Zero);
                Assert.That(deaths, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void LethalDamage_WhenDamagedHandlerTakesDamage_RaisesDeathOnlyOnce()
        {
            var go = new GameObject("HealthReentrantDamageTest");
            try
            {
                var health = go.AddComponent<CharacterHealth>();
                health.Initialize(10f);
                var deaths = 0;
                health.Died += _ => deaths++;

                System.Action<float> takeDamageAgain = null;
                takeDamageAgain = _ =>
                {
                    health.Damaged -= takeDamageAgain;
                    health.TakeDamage(new DamageInfo(1f, this));
                };
                health.Damaged += takeDamageAgain;

                health.TakeDamage(new DamageInfo(10f, this));

                Assert.That(health.IsDead, Is.True);
                Assert.That(health.CurrentHealth, Is.Zero);
                Assert.That(deaths, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void DamageDuringInvulnerability_IsIgnored()
        {
            var go = new GameObject("InvulnerabilityTest");
            try
            {
                var health = go.AddComponent<CharacterHealth>();
                health.Initialize(100f);
                health.InvulnerabilityDuration = 0.3f;
                var damageEvents = 0;
                health.Damaged += _ => damageEvents++;

                health.TakeDamage(new DamageInfo(10f, this));
                health.TakeDamage(new DamageInfo(10f, this));

                Assert.That(health.CurrentHealth, Is.EqualTo(90f));
                Assert.That(damageEvents, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Initialize_ResetsInvulnerabilityWindow()
        {
            var go = new GameObject("InvulnerabilityResetTest");
            try
            {
                var health = go.AddComponent<CharacterHealth>();
                health.Initialize(100f);
                health.InvulnerabilityDuration = 0.3f;
                health.TakeDamage(new DamageInfo(10f, this));

                health.Initialize(100f);
                health.TakeDamage(new DamageInfo(10f, this));

                Assert.That(health.CurrentHealth, Is.EqualTo(90f));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Heal_RestoresHealthWithoutExceedingMaximum()
        {
            var go = new GameObject("HealTest");
            try
            {
                var health = go.AddComponent<CharacterHealth>();
                health.Initialize(100f);
                health.TakeDamage(new DamageInfo(40f, this));

                health.Heal(15f);
                health.Heal(100f);

                Assert.That(health.CurrentHealth, Is.EqualTo(100f));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Heal_WhenDead_DoesNotRevive()
        {
            var go = new GameObject("DeadHealTest");
            try
            {
                var health = go.AddComponent<CharacterHealth>();
                health.Initialize(10f);
                health.TakeDamage(new DamageInfo(10f, this));

                health.Heal(10f);

                Assert.That(health.CurrentHealth, Is.Zero);
                Assert.That(health.IsDead, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Heal_WithNonPositiveAmount_DoesNotChangeHealthOrRaiseEvent()
        {
            var go = new GameObject("InvalidHealTest");
            try
            {
                var health = go.AddComponent<CharacterHealth>();
                health.Initialize(100f);
                health.TakeDamage(new DamageInfo(40f, this));
                var healedEvents = 0;
                health.Healed += _ => healedEvents++;

                health.Heal(0f);
                health.Heal(-10f);

                Assert.That(health.CurrentHealth, Is.EqualTo(60f));
                Assert.That(healedEvents, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
