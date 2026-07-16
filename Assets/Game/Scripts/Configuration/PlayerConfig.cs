using UnityEngine;

namespace BAA
{
    [CreateAssetMenu(menuName = "BAA/Player Config")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Min(1f)] public float MaxHealth = 100f;
        [Min(0.1f)] public float MoveSpeed = 5f;
        [Min(0.1f)] public float Attack = 10f;
        [Min(0.05f)] public float AttackInterval = 0.6f;
        [Min(0.1f)] public float TargetRange = 10f;
        [Range(0f, 0.9f)] public float InputDeadZone = 0.1f;
        [Min(0f)] public float SettleDuration = 0.1f;
    }
}
