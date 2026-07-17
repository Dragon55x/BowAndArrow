using System;
using UnityEngine;

namespace BAA
{
    [Serializable]
    public sealed class WaveDefinition
    {
        [SerializeField, Min(1)] private int enemyCount = 3;
        [SerializeField, Min(0f)] private float spawnInterval = 0.45f;

        public WaveDefinition(int count, float interval)
        {
            enemyCount = Mathf.Max(1, count);
            spawnInterval = Mathf.Max(0f, interval);
        }

        public int EnemyCount => Mathf.Max(1, enemyCount);
        public float SpawnInterval => Mathf.Max(0f, spawnInterval);
    }
}
