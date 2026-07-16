using UnityEngine;

namespace BAA
{
    [CreateAssetMenu(menuName = "BAA/Enemy Config", fileName = "EnemyConfig")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [field: SerializeField, Min(1f)] public float MaxHealth { get; set; } = 30f;
        [field: SerializeField, Min(0f)] public float MoveSpeed { get; set; } = 2.5f;
        [field: SerializeField, Min(0f)] public float RotationSpeed { get; set; } = 540f;
        [field: SerializeField, Min(0f)] public float SpawnDelay { get; set; } = 0.5f;
        [field: SerializeField, Min(0f)] public float AttackRange { get; set; } = 1.4f;
        [field: SerializeField, Min(0f)] public float HitRadius { get; set; } = 0.75f;
        [field: SerializeField, Min(0f)] public float WindupDuration { get; set; } = 0.4f;
        [field: SerializeField, Min(0f)] public float RecoveryDuration { get; set; } = 1f;
        [field: SerializeField, Min(0f)] public float Damage { get; set; } = 15f;
    }
}
