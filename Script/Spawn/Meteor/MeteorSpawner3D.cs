using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace Game.Spawning
{
    public partial class MeteorSpawner3D : Node3D
    {
        [Signal] public delegate void WaveStartedEventHandler(int waveIndex);
        [Signal] public delegate void WaveCompletedEventHandler(int waveIndex);
        [Signal] public delegate void EndlessLoopCompletedEventHandler(int loopCount);
        [Signal] public delegate void AllWavesCompletedEventHandler();

        [ExportGroup("Spawner")]
        [Export] public bool AutoStart = true;
        [Export] public Node SpawnParent;

        [ExportGroup("Spawn Points")]
        [Export] public Array<MeteorSpawnPoint3D> SpawnPoints = new();

        [ExportGroup("Target Points")]
        [Export] public Array<MeteorTargetPoint3D> TargetPoints = new();

        [ExportGroup("Waves")]
        [Export] public Array<MeteorSpawnWaveData> Waves = new();

        private readonly RandomNumberGenerator _rng = new();

        private readonly List<Node> _aliveMeteors = new();
        private readonly List<RuntimeSpawnEntry> _runtimeEntries = new();

        private int _currentWaveIndex = -1;
        private int _nextWaveIndex = 0;
        private int _endlessLoopCount = 0;

        private float _spawnTimer;

        private bool _isRunning;
        private bool _waveActive;
        private bool _waitingForNextWave;

        private MeteorSpawnWaveData CurrentWave =>
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
            _endlessLoopCount = 0;

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
                EmitSignal(SignalName.AllWavesCompleted);
                return;
            }

            _currentWaveIndex = waveIndex;
            _runtimeEntries.Clear();

            MeteorSpawnWaveData wave = CurrentWave;

            if (wave == null)
            {
                CompleteCurrentWave();
                return;
            }

            foreach (MeteorSpawnEntryData entry in wave.Meteors)
            {
                if (entry == null || entry.MeteorScene == null || entry.Count <= 0)
                    continue;

                _runtimeEntries.Add(new RuntimeSpawnEntry(entry.MeteorScene, entry.Count));
            }

            _spawnTimer = wave.StartDelay;
            _waveActive = true;
            _waitingForNextWave = false;

            EmitSignal(SignalName.WaveStarted, _currentWaveIndex);

            GD.Print($"Started meteor wave {_currentWaveIndex + 1}: {wave.DisplayName}");
        }

        private void UpdateWaveStartDelay(float delta)
        {
            _spawnTimer -= delta;

            if (_spawnTimer > 0.0f)
                return;

            StartWave(_nextWaveIndex);
        }

        private void UpdateCurrentWave(float delta)
        {
            CleanupInvalidMeteors();

            MeteorSpawnWaveData wave = CurrentWave;

            if (wave == null)
                return;

            bool hasRemainingMeteorsToSpawn = HasRemainingMeteorsToSpawn();

            if (hasRemainingMeteorsToSpawn)
            {
                _spawnTimer -= delta;

                if (_spawnTimer <= 0.0f && _aliveMeteors.Count < GetScaledMaxAlive(wave))
                {
                    SpawnNextMeteor();
                    _spawnTimer = GetScaledSpawnDelay(wave);
                }
            }

            bool waveFinishedSpawning = !HasRemainingMeteorsToSpawn();
            bool waveMeteorsCleared = _aliveMeteors.Count == 0;

            if (waveFinishedSpawning)
            {
                if (!wave.WaitForAllMeteorsGone || waveMeteorsCleared)
                    CompleteCurrentWave();
            }
        }

        private void SpawnNextMeteor()
        {
            RuntimeSpawnEntry entry = GetNextSpawnEntry();

            if (entry == null)
                return;

            MeteorSpawnPoint3D spawnPoint = GetRandomSpawnPoint();

            if (spawnPoint == null)
            {
                GD.PushWarning($"{Name}: No valid meteor spawn points found.");
                return;
            }

            MeteorTargetPoint3D targetPoint = GetRandomTargetPoint();

            if (targetPoint == null)
            {
                GD.PushWarning($"{Name}: No valid meteor target points found.");
                return;
            }

            Node meteor = entry.MeteorScene.Instantiate();

            if (meteor is not Node3D meteorNode)
            {
                GD.PushWarning($"{Name}: Meteor scene must have a Node3D root.");
                meteor.QueueFree();
                return;
            }

            SpawnParent.AddChild(meteorNode);

            meteorNode.GlobalPosition = spawnPoint.GetSpawnPosition(_rng);
            meteorNode.GlobalRotation = spawnPoint.GlobalRotation;

            Vector3 targetPosition = targetPoint.GetTargetPosition(_rng);
            float difficultyMultiplier = GetDifficultyMultiplier(CurrentWave);

            if (meteorNode is MeteorController meteorController)
            {
                meteorController.Initialize(targetPosition, difficultyMultiplier);
            }
            else
            {
                GD.PushWarning($"{Name}: Spawned meteor root is not MeteorController.");
            }

            _aliveMeteors.Add(meteorNode);

            meteorNode.TreeExited += () =>
            {
                _aliveMeteors.Remove(meteorNode);
            };

            entry.RemainingCount--;

            GD.Print(
                $"Spawned meteor. Wave: {_currentWaveIndex + 1}, " +
                $"Remaining: {entry.RemainingCount}, " +
                $"Alive: {_aliveMeteors.Count}/{GetScaledMaxAlive(CurrentWave)}, " +
                $"Difficulty: {difficultyMultiplier}"
            );
        }

        private void CompleteCurrentWave()
        {
            MeteorSpawnWaveData wave = CurrentWave;

            GD.Print($"Completed meteor wave {_currentWaveIndex + 1}");

            EmitSignal(SignalName.WaveCompleted, _currentWaveIndex);

            _waveActive = false;

            if (wave != null && wave.RepeatForever)
            {
                _endlessLoopCount++;

                EmitSignal(SignalName.EndlessLoopCompleted, _endlessLoopCount);

                _nextWaveIndex = _currentWaveIndex;
                _waitingForNextWave = true;
                _spawnTimer = wave.StartDelay;

                GD.Print(
                    $"Repeating endless meteor wave {_currentWaveIndex + 1}. " +
                    $"Loop: {_endlessLoopCount}, " +
                    $"Next Delay: {GetScaledSpawnDelay(wave)}, " +
                    $"Max Alive: {GetScaledMaxAlive(wave)}, " +
                    $"Difficulty: {GetDifficultyMultiplier(wave)}"
                );

                return;
            }

            int nextWaveIndex = _currentWaveIndex + 1;

            if (nextWaveIndex >= Waves.Count)
            {
                _isRunning = false;
                EmitSignal(SignalName.AllWavesCompleted);
                GD.Print("All meteor waves completed.");
                return;
            }

            _nextWaveIndex = nextWaveIndex;
            _waitingForNextWave = true;
            _spawnTimer = Waves[nextWaveIndex].StartDelay;
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

        private MeteorSpawnPoint3D GetRandomSpawnPoint()
        {
            List<MeteorSpawnPoint3D> validPoints = new();

            foreach (MeteorSpawnPoint3D point in SpawnPoints)
            {
                if (point != null && point.Enabled)
                    validPoints.Add(point);
            }

            if (validPoints.Count == 0)
                return null;

            int index = _rng.RandiRange(0, validPoints.Count - 1);
            return validPoints[index];
        }

        private MeteorTargetPoint3D GetRandomTargetPoint()
        {
            List<MeteorTargetPoint3D> validPoints = new();

            foreach (MeteorTargetPoint3D point in TargetPoints)
            {
                if (point != null && point.Enabled)
                    validPoints.Add(point);
            }

            if (validPoints.Count == 0)
                return null;

            int index = _rng.RandiRange(0, validPoints.Count - 1);
            return validPoints[index];
        }

        private bool HasRemainingMeteorsToSpawn()
        {
            foreach (RuntimeSpawnEntry entry in _runtimeEntries)
            {
                if (entry.RemainingCount > 0)
                    return true;
            }

            return false;
        }

        private float GetScaledSpawnDelay(MeteorSpawnWaveData wave)
        {
            if (wave == null)
                return 1.0f;

            if (!wave.ScaleDifficulty)
                return wave.DelayBetweenSpawns;

            float scaledDelay = wave.DelayBetweenSpawns -
                (_endlessLoopCount * wave.SpawnDelayReductionPerLoop);

            return Mathf.Max(wave.MinimumSpawnDelay, scaledDelay);
        }

        private int GetScaledMaxAlive(MeteorSpawnWaveData wave)
        {
            if (wave == null)
                return 1;

            if (!wave.ScaleDifficulty)
                return wave.MaxAlive;

            if (wave.MaxAliveIncreaseEveryLoops <= 0)
                return wave.MaxAlive;

            int increaseSteps = _endlessLoopCount / wave.MaxAliveIncreaseEveryLoops;
            int scaledMaxAlive = wave.MaxAlive + increaseSteps * wave.MaxAliveIncreaseAmount;

            return Mathf.Clamp(scaledMaxAlive, wave.MaxAlive, wave.MaxAliveCap);
        }

        private float GetDifficultyMultiplier(MeteorSpawnWaveData wave)
        {
            if (wave == null)
                return 1.0f;

            if (!wave.ScaleDifficulty)
                return 1.0f;

            float multiplier = 1.0f + (_endlessLoopCount * wave.DifficultyMultiplierPerLoop);

            return Mathf.Min(multiplier, wave.MaxDifficultyMultiplier);
        }

        private void CleanupInvalidMeteors()
        {
            for (int i = _aliveMeteors.Count - 1; i >= 0; i--)
            {
                if (!GodotObject.IsInstanceValid(_aliveMeteors[i]))
                    _aliveMeteors.RemoveAt(i);
            }
        }

        private class RuntimeSpawnEntry
        {
            public PackedScene MeteorScene;
            public int RemainingCount;

            public RuntimeSpawnEntry(PackedScene meteorScene, int remainingCount)
            {
                MeteorScene = meteorScene;
                RemainingCount = remainingCount;
            }
        }
    }
}