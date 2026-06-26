using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace Game.Spawning
{
    public partial class EnemySpawner3D : Node3D
    {
        [Signal] public delegate void WaveStartedEventHandler(int waveIndex);
        [Signal] public delegate void WaveCompletedEventHandler(int waveIndex);
        [Signal] public delegate void AllWavesCompletedEventHandler();

        [ExportGroup("Spawner")]
        [Export] public bool AutoStart = true;
        [Export] public Node SpawnParent;

        [ExportGroup("Spawn Points")]
        [Export] public Array<EnemySpawnPoint3D> SpawnPoints = new();

        [ExportGroup("Waves")]
        [Export] public Array<EnemySpawnWaveData> Waves = new();

        private readonly RandomNumberGenerator _rng = new();

        private readonly List<Node> _aliveEnemies = new();
        private readonly List<RuntimeSpawnEntry> _runtimeEntries = new();

        private int _currentWaveIndex = -1;
        private float _spawnTimer;
        private bool _isRunning;
        private bool _waveActive;
        private bool _waitingForNextWave;

        private EnemySpawnWaveData CurrentWave =>
            _currentWaveIndex >= 0 && _currentWaveIndex < Waves.Count
                ? Waves[_currentWaveIndex]
                : null;

        public override void _Ready()
        {
            _rng.Randomize();

            if (SpawnParent == null)
                SpawnParent = GetTree().CurrentScene;

            if (AutoStart)
                StartSpawner();
        }

        public override void _Process(double delta)
        {
            if (!_isRunning)
                return;

            if (_waitingForNextWave)
            {
                UpdateWaveStartDelay((float)delta);
                return;
            }

            if (!_waveActive)
                return;

            UpdateCurrentWave((float)delta);
        }

        public void StartSpawner()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            StartWave(0);
        }

        public void StopSpawner()
        {
            _isRunning = false;
            _waveActive = false;
            _waitingForNextWave = false;
        }

        private void StartWave(int waveIndex)
        {
            if (waveIndex < 0 || waveIndex >= Waves.Count)
            {
                _isRunning = false;
                EmitSignal("AllWavesCompleted");
                return;
            }

            _currentWaveIndex = waveIndex;
            _runtimeEntries.Clear();

            EnemySpawnWaveData wave = CurrentWave;

            foreach (EnemySpawnEntryData entry in wave.Enemies)
            {
                if (entry == null || entry.EnemyScene == null || entry.Count <= 0)
                    continue;

                _runtimeEntries.Add(new RuntimeSpawnEntry(entry.EnemyScene, entry.Count));
            }

            _spawnTimer = wave.StartDelay;
            _waveActive = true;
            _waitingForNextWave = false;

            EmitSignal("WaveStarted", _currentWaveIndex);

            GD.Print($"Started wave {_currentWaveIndex + 1}");
        }

        private void UpdateWaveStartDelay(float delta)
        {
            _spawnTimer -= delta;

            if (_spawnTimer > 0.0f)
                return;

            EnemySpawnWaveData wave = CurrentWave;

            if (wave != null && wave.RepeatForever)
                StartWave(_currentWaveIndex);
            else
                StartWave(_currentWaveIndex + 1);
        }

        private void UpdateCurrentWave(float delta)
        {
            CleanupInvalidEnemies();

            EnemySpawnWaveData wave = CurrentWave;

            if (wave == null)
                return;

            bool hasRemainingEnemiesToSpawn = HasRemainingEnemiesToSpawn();

            if (hasRemainingEnemiesToSpawn)
            {
                _spawnTimer -= delta;

                if (_spawnTimer <= 0.0f && _aliveEnemies.Count < wave.MaxAlive)
                {
                    SpawnNextEnemy();
                    _spawnTimer = wave.DelayBetweenSpawns;
                }
            }

            bool waveFinishedSpawning = !HasRemainingEnemiesToSpawn();
            bool waveEnemiesCleared = _aliveEnemies.Count == 0;

            if (waveFinishedSpawning)
            {
                if (!wave.WaitForAllEnemiesDead || waveEnemiesCleared)
                    CompleteCurrentWave();
            }
        }

        private void SpawnNextEnemy()
        {
            RuntimeSpawnEntry entry = GetNextSpawnEntry();

            if (entry == null)
                return;

            EnemySpawnPoint3D spawnPoint = GetRandomSpawnPoint();

            if (spawnPoint == null)
            {
                GD.PushWarning($"{Name}: No valid spawn points found.");
                return;
            }

            Node enemy = entry.EnemyScene.Instantiate();

            if (enemy is not Node3D enemyNode)
            {
                GD.PushWarning($"{Name}: Enemy scene must have a Node3D root.");
                enemy.QueueFree();
                return;
            }

            SpawnParent.AddChild(enemyNode);

            enemyNode.GlobalPosition = spawnPoint.GetSpawnPosition(_rng);
            enemyNode.GlobalRotation = spawnPoint.GlobalRotation;

            _aliveEnemies.Add(enemyNode);

            enemyNode.TreeExited += () =>
            {
                _aliveEnemies.Remove(enemyNode);
            };

            entry.RemainingCount--;

            GD.Print($"Spawned enemy from wave {_currentWaveIndex + 1}. Remaining in entry: {entry.RemainingCount}");
        }

        private RuntimeSpawnEntry GetNextSpawnEntry()
        {
            foreach (RuntimeSpawnEntry entry in _runtimeEntries)
            {
                if (entry.RemainingCount > 0)
                    return entry;
            }

            return null;
        }

        private EnemySpawnPoint3D GetRandomSpawnPoint()
        {
            List<EnemySpawnPoint3D> validPoints = new();

            foreach (EnemySpawnPoint3D point in SpawnPoints)
            {
                if (point != null && point.Enabled)
                    validPoints.Add(point);
            }

            if (validPoints.Count == 0)
                return null;

            int index = _rng.RandiRange(0, validPoints.Count - 1);
            return validPoints[index];
        }

        private bool HasRemainingEnemiesToSpawn()
        {
            foreach (RuntimeSpawnEntry entry in _runtimeEntries)
            {
                if (entry.RemainingCount > 0)
                    return true;
            }

            return false;
        }

        private void CompleteCurrentWave()
        {
            EnemySpawnWaveData wave = CurrentWave;

            GD.Print($"Completed wave {_currentWaveIndex + 1}");

            EmitSignal("WaveCompleted", _currentWaveIndex);

            _waveActive = false;

            if (wave != null && wave.RepeatForever)
            {
                _waitingForNextWave = true;
                _spawnTimer = wave.StartDelay;

                GD.Print($"Repeating wave {_currentWaveIndex + 1} forever.");
                return;
            }

            int nextWaveIndex = _currentWaveIndex + 1;

            if (nextWaveIndex >= Waves.Count)
            {
                _isRunning = false;
                EmitSignal("AllWavesCompleted");
                GD.Print("All waves completed.");
                return;
            }

            _waitingForNextWave = true;
            _spawnTimer = Waves[nextWaveIndex].StartDelay;
        }

        private void CleanupInvalidEnemies()
        {
            for (int i = _aliveEnemies.Count - 1; i >= 0; i--)
            {
                if (!GodotObject.IsInstanceValid(_aliveEnemies[i]))
                    _aliveEnemies.RemoveAt(i);
            }
        }

        private class RuntimeSpawnEntry
        {
            public PackedScene EnemyScene;
            public int RemainingCount;

            public RuntimeSpawnEntry(PackedScene enemyScene, int remainingCount)
            {
                EnemyScene = enemyScene;
                RemainingCount = remainingCount;
            }
        }
    }
}