using UnityEngine;

namespace BAA
{
    public sealed class WeaponController : MonoBehaviour
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private ProjectilePool pool;
        [SerializeField] private ProjectileConfig projectileConfig;

        public PlayerRuntimeStats Stats { get; set; }

        public void FireAt(Vector3 targetPosition)
        {
            if (Stats == null || firePoint == null || pool == null || projectileConfig == null)
            {
                return;
            }

            var baseDirection = targetPosition - firePoint.position;
            baseDirection.y = 0f;
            if (baseDirection.sqrMagnitude <= 0.001f)
            {
                return;
            }

            baseDirection.Normalize();
            var critical = Random.value < Stats.CritChance;
            var damage = Stats.Attack * (critical ? Stats.CritMultiplier : 1f);
            var snapshot = new DamageInfo(damage, this, critical);
            var angles = ShotPatternCalculator.CalculateAngles(
                Stats.ForwardArrowCount,
                Stats.DiagonalArrowCount);

            foreach (var angle in angles)
            {
                var direction = Quaternion.AngleAxis(angle, Vector3.up) * baseDirection;
                var projectile = pool.Get();
                projectile.transform.SetPositionAndRotation(
                    firePoint.position,
                    Quaternion.LookRotation(direction));
                projectile.Launch(
                    direction * projectileConfig.Speed,
                    snapshot,
                    projectileConfig.Lifetime);
            }
        }
    }
}
