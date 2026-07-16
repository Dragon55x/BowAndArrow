using UnityEngine;

namespace BAA
{
    [CreateAssetMenu(menuName = "BAA/Projectile Config")]
    public sealed class ProjectileConfig : ScriptableObject
    {
        [Min(0.1f)] public float Speed = 16f;
        [Min(0.1f)] public float Lifetime = 3f;
    }
}
