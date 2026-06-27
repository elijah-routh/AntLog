using Godot;
using System.Collections.Generic;

namespace Game.Targets
{
    public partial class PlanetWaveSpawner3D : Node3D
    {
        [ExportGroup("References")]
        [Export] public PackedScene PlanetTargetScene;
        [Export] public Node TargetParent;
        [Export] public Rail3D[] Rails;

        [ExportGroup("Wave Settings")]
        [Export] public float WaveDuration = 8.0f;
        [Export] public float WaveSpawnDelay = 2.0f;
        [Export] public int TargetsPerWave = 4;

        [ExportGroup("Target Movement")]
        [Export] public float SlowTargetRailSpeed = 0.15f;
        [Export] public float FastTargetRailSpeed = 0.35f;

        [ExportGroup("Difficulty")]
        [Export] public bool AllowSmallTargets = true;
        [Export] public bool AllowFastTargets = true;

        private readonly List<PlanetTarget> _currentTargets = new();

        private ScoreManager _scoreManager;

        private float _waveTimer;
        private float _spawnDelayTimer;

        private bool _waitingToSpawnNextWave;

        public override void _Ready()
        {
            if (TargetParent == null)
                TargetParent = this;

            _scoreManager = GetTree()
                .GetFirstNodeInGroup("score_manager") as ScoreManager;

            // First wave also waits for the delay.
            _waitingToSpawnNextWave = true;
            _spawnDelayTimer = WaveSpawnDelay;
        }

        public override void _Process(double delta)
        {
            float deltaFloat = (float)delta;

            if (_waitingToSpawnNextWave)
            {
                _spawnDelayTimer -= deltaFloat;

                if (_spawnDelayTimer <= 0.0f)
                {
                    _waitingToSpawnNextWave = false;
                    SpawnNewWave();
                }

                return;
            }

            _waveTimer -= deltaFloat;

            if (_waveTimer <= 0.0f)
            {
                DespawnCurrentTargets();

                _waitingToSpawnNextWave = true;
                _spawnDelayTimer = WaveSpawnDelay;
            }
        }

        private void SpawnNewWave()
        {
            GD.Print("PlanetWaveSpawner3D: New Wave.");

            _waveTimer = WaveDuration;

            if (PlanetTargetScene == null)
            {
                GD.PushWarning("PlanetWaveSpawner3D: Missing PlanetTargetScene.");
                return;
            }

            if (Rails == null || Rails.Length == 0)
            {
                GD.PushWarning("PlanetWaveSpawner3D: No rails assigned.");
                return;
            }

            for (int i = 0; i < TargetsPerWave; i++)
            {
                SpawnTarget();
            }
        }

        private void SpawnTarget()
        {
            Node instance = PlanetTargetScene.Instantiate();

            if (instance is not PlanetTarget target)
            {
                GD.PushWarning("PlanetWaveSpawner3D: PlanetTargetScene root must have PlanetTarget.cs.");
                instance.QueueFree();
                return;
            }

            TargetParent.AddChild(target);

            Rail3D rail = PickRandomRail();

            PlanetTarget.TargetSizeType sizeType = PickSizeType();
            PlanetTarget.TargetSpeedType speedType = PickSpeedType();

            target.Setup(sizeType, speedType);
            target.TargetHit += OnTargetHit;

            RailRider3D rider = target.GetNodeOrNull<RailRider3D>("RailRider3D");

            if (rider == null)
            {
                rider = new RailRider3D();
                rider.Name = "RailRider3D";
                target.AddChild(rider);
            }

            float railSpeed = speedType == PlanetTarget.TargetSpeedType.Fast
                ? FastTargetRailSpeed
                : SlowTargetRailSpeed;

            RailRider3D.MotionMode motionMode = rail is LineRail3D
                ? RailRider3D.MotionMode.PingPong
                : RailRider3D.MotionMode.Loop;

            rider.Setup(rail, railSpeed, motionMode);

            _currentTargets.Add(target);
        }

        private Rail3D PickRandomRail()
        {
            int index = GD.RandRange(0, Rails.Length - 1);
            return Rails[index];
        }

        private PlanetTarget.TargetSizeType PickSizeType()
        {
            if (!AllowSmallTargets)
                return PlanetTarget.TargetSizeType.Large;

            return GD.Randf() > 0.5f
                ? PlanetTarget.TargetSizeType.Small
                : PlanetTarget.TargetSizeType.Large;
        }

        private PlanetTarget.TargetSpeedType PickSpeedType()
        {
            if (!AllowFastTargets)
                return PlanetTarget.TargetSpeedType.Slow;

            return GD.Randf() > 0.5f
                ? PlanetTarget.TargetSpeedType.Fast
                : PlanetTarget.TargetSpeedType.Slow;
        }

        private void OnTargetHit(PlanetTarget target, int points)
        {
            PlanetTarget.TargetSizeType sizeType = target.SizeType;
            PlanetTarget.TargetSpeedType speedType = target.SpeedType;

            float multiplier = GetPointMultiplier(sizeType, speedType);

            GD.Print(
                $"Planet killed | Size: {sizeType} | Speed: {speedType} | " +
                $"Multiplier: x{multiplier} | Points: {points}"
            );

            _scoreManager?.AddScore(points);
            _currentTargets.Remove(target);
        }

        private void DespawnCurrentTargets()
        {
            foreach (PlanetTarget target in _currentTargets)
            {
                if (IsInstanceValid(target))
                    target.QueueFree();
            }

            _currentTargets.Clear();
        }

        private float GetPointMultiplier(
            PlanetTarget.TargetSizeType sizeType,
            PlanetTarget.TargetSpeedType speedType)
        {
            float multiplier = 1.0f;

            if (sizeType == PlanetTarget.TargetSizeType.Small)
                multiplier *= 2.0f;

            if (speedType == PlanetTarget.TargetSpeedType.Fast)
                multiplier *= 2.0f;

            return multiplier;
        }
    }
}