using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BAA
{
    public sealed class WaveController : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private WaveDefinition[] waves;
        [SerializeField, Min(0f)] private float waveStartDelay = 1.5f;

        private readonly Dictionary<CharacterHealth, GameObject> _activeEnemies =
            new Dictionary<CharacterHealth, GameObject>();
        private WaveProgression _progression;
        private Coroutine _waveRoutine;
        private int _nextSpawnPoint;

        public event Action<int, int> WavePreparing;
        public event Action<int, int> WaveStarted;
        public event Action<int> RemainingEnemiesChanged;
        public event Action RoomCleared;

        public int CurrentWaveNumber => _progression?.CurrentWaveNumber ?? 0;
        public int TotalWaves => _progression?.TotalWaves ?? waves?.Length ?? 0;
        public int OutstandingEnemies => _progression?.OutstandingEnemies ?? 0;
        public bool IsRoomCleared => _progression != null && _progression.IsRoomCleared;

        public void Configure(
            GameObject prefab,
            Transform[] points,
            WaveDefinition[] definitions,
            float startDelay)
        {
            enemyPrefab = prefab;
            spawnPoints = points;
            waves = definitions;
            waveStartDelay = Mathf.Max(0f, startDelay);
        }

        private void Start()
        {
            if (!ValidateConfiguration())
            {
                enabled = false;
                return;
            }

            var enemyCounts = new int[waves.Length];
            for (var i = 0; i < waves.Length; i++)
            {
                enemyCounts[i] = waves[i].EnemyCount;
            }

            _progression = new WaveProgression(enemyCounts);
            _waveRoutine = StartCoroutine(RunWaves());
        }

        private IEnumerator RunWaves()
        {
            yield return null;
            while (!_progression.IsRoomCleared)
            {
                var nextWaveNumber = _progression.CurrentWaveNumber + 1;
                WavePreparing?.Invoke(nextWaveNumber, _progression.TotalWaves);
                if (waveStartDelay > 0f)
                {
                    yield return new WaitForSeconds(waveStartDelay);
                }

                _progression.BeginNextWave();
                var wave = waves[_progression.CurrentWaveNumber - 1];
                WaveStarted?.Invoke(
                    _progression.CurrentWaveNumber,
                    _progression.TotalWaves);

                while (_progression.RemainingToSpawn > 0)
                {
                    SpawnEnemy();
                    if (_progression.RemainingToSpawn > 0 && wave.SpawnInterval > 0f)
                    {
                        yield return new WaitForSeconds(wave.SpawnInterval);
                    }
                }

                while (!_progression.IsCurrentWaveCleared)
                {
                    yield return null;
                }
            }

            _waveRoutine = null;
            RoomCleared?.Invoke();
        }

        private void SpawnEnemy()
        {
            var point = spawnPoints[_nextSpawnPoint % spawnPoints.Length];
            _nextSpawnPoint++;
            var enemy = Instantiate(
                enemyPrefab,
                point.position,
                point.rotation);
            enemy.name =
                $"MeleeEnemy_W{_progression.CurrentWaveNumber}_{_nextSpawnPoint}";
            var health = enemy.GetComponent<CharacterHealth>();
            if (health == null)
            {
                Destroy(enemy);
                throw new InvalidOperationException(
                    "Wave enemy prefab requires CharacterHealth.");
            }

            _progression.RegisterSpawn();
            _activeEnemies.Add(health, enemy);
            health.Died += OnEnemyDied;
            RemainingEnemiesChanged?.Invoke(_progression.OutstandingEnemies);
        }

        private void OnEnemyDied(CharacterHealth health)
        {
            if (!_activeEnemies.TryGetValue(health, out var enemy))
            {
                return;
            }

            health.Died -= OnEnemyDied;
            _activeEnemies.Remove(health);
            _progression.RegisterDeath();
            RemainingEnemiesChanged?.Invoke(_progression.OutstandingEnemies);
            Destroy(enemy);
        }

        private bool ValidateConfiguration()
        {
            if (enemyPrefab == null ||
                enemyPrefab.GetComponent<CharacterHealth>() == null)
            {
                Debug.LogError(
                    "WaveController requires an enemy prefab with CharacterHealth.",
                    this);
                return false;
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("WaveController requires at least one spawn point.", this);
                return false;
            }

            for (var i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] == null)
                {
                    Debug.LogError("WaveController spawn points cannot contain null.", this);
                    return false;
                }
            }

            if (waves == null || waves.Length == 0)
            {
                Debug.LogError("WaveController requires at least one wave.", this);
                return false;
            }

            for (var i = 0; i < waves.Length; i++)
            {
                if (waves[i] == null)
                {
                    Debug.LogError("WaveController waves cannot contain null.", this);
                    return false;
                }
            }

            return true;
        }

        private void OnDestroy()
        {
            if (_waveRoutine != null)
            {
                StopCoroutine(_waveRoutine);
            }

            foreach (var pair in _activeEnemies)
            {
                if (pair.Key != null)
                {
                    pair.Key.Died -= OnEnemyDied;
                }
            }

            _activeEnemies.Clear();
        }
    }
}
