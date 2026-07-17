using System;
using System.Collections.Generic;

namespace BAA
{
    public sealed class WaveProgression
    {
        private readonly int[] _enemyCounts;
        private int _currentWaveIndex = -1;

        public WaveProgression(IReadOnlyList<int> enemyCounts)
        {
            if (enemyCounts == null || enemyCounts.Count == 0)
            {
                throw new ArgumentException("At least one wave is required.", nameof(enemyCounts));
            }

            _enemyCounts = new int[enemyCounts.Count];
            for (var i = 0; i < enemyCounts.Count; i++)
            {
                if (enemyCounts[i] <= 0)
                {
                    throw new ArgumentException(
                        "Every wave must contain at least one enemy.",
                        nameof(enemyCounts));
                }

                _enemyCounts[i] = enemyCounts[i];
            }
        }

        public int CurrentWaveNumber => _currentWaveIndex + 1;
        public int TotalWaves => _enemyCounts.Length;
        public int RemainingToSpawn { get; private set; }
        public int AliveEnemies { get; private set; }
        public int OutstandingEnemies => RemainingToSpawn + AliveEnemies;
        public bool IsCurrentWaveCleared =>
            _currentWaveIndex >= 0 && RemainingToSpawn == 0 && AliveEnemies == 0;
        public bool IsRoomCleared =>
            _currentWaveIndex == _enemyCounts.Length - 1 && IsCurrentWaveCleared;

        public void BeginNextWave()
        {
            if (IsRoomCleared)
            {
                throw new InvalidOperationException("All waves are already cleared.");
            }

            if (_currentWaveIndex >= 0 && !IsCurrentWaveCleared)
            {
                throw new InvalidOperationException(
                    "The current wave must be cleared before advancing.");
            }

            _currentWaveIndex++;
            RemainingToSpawn = _enemyCounts[_currentWaveIndex];
            AliveEnemies = 0;
        }

        public void RegisterSpawn()
        {
            if (_currentWaveIndex < 0 || RemainingToSpawn <= 0)
            {
                throw new InvalidOperationException("No enemy is waiting to spawn.");
            }

            RemainingToSpawn--;
            AliveEnemies++;
        }

        public void RegisterDeath()
        {
            if (AliveEnemies <= 0)
            {
                throw new InvalidOperationException("No living enemy can be removed.");
            }

            AliveEnemies--;
        }
    }
}
