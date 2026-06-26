using Godot;
using System.Collections.Generic;

namespace Game.Targets
{
    public partial class TargetWaveSpawner3D : Node3D
    {
        [ExportGroup("References")]
        [Export] public PackedScene TargetScene;
        [Export] public Node TargetParent;
        [Export] public Rail3D[] Rails;

        [ExportGroup("Wave Settings")]
        [Export] public float WaveDuration = 8.0f;
        [Export] public int TargetsPerWave = 4;

        [ExportGroup("Target Movement")]
        [Export] public float SlowTargetRailSpeed = 0.15f;
        [Export] public float FastTargetRailSpeed = 0.35f;

        [ExportGroup("Difficulty")]
        [Export] public bool AllowSmallTargets = true;
        [Export] public bool AllowFastTargets = true;

        private readonly List<ThrowTarget3D> _currentTargets = new();

        private ScoreManager _scoreManager;
        private float _waveTimer;

        public override void _Ready()
        {
            if (TargetParent == null)
                TargetParent = this;

            _scoreManager = GetTree()
                .GetFirstNodeInGroup("score_manager") as ScoreManager;

            SpawnNewWave();
        }

        public override void _Process(double delta)
        {
            _waveTimer -= (float)delta;

            if (_waveTimer <= 0.0f)
            {
                DespawnCurrentTargets();
                SpawnNewWave();
            }
        }

        private void SpawnNewWave()
        {
            _waveTimer = WaveDuration;

            if (TargetScene == null)
            {
                GD.PushWarning("TargetWaveSpawner3D: Missing TargetScene.");
                return;
            }

            if (Rails == null || Rails.Length == 0)
            {
                GD.PushWarning("TargetWaveSpawner3D: No rails assigned.");
                return;
            }

            for (int i = 0; i < TargetsPerWave; i++)
            {
                SpawnTarget();
            }
        }

        private void SpawnTarget()
        {
            ThrowTarget3D target = TargetScene.Instantiate<ThrowTarget3D>();
            TargetParent.AddChild(target);

            Rail3D rail = PickRandomRail();

            ThrowTarget3D.TargetSizeType sizeType = PickSizeType();
            ThrowTarget3D.TargetSpeedType speedType = PickSpeedType();

            //target.Setup(sizeType, speedType, Player);
            target.TargetHit += OnTargetHit;

            RailRider3D rider = target.GetNodeOrNull<RailRider3D>("RailRider3D");

            if (rider == null)
            {
                rider = new RailRider3D();
                rider.Name = "RailRider3D";
                target.AddChild(rider);
            }

            float railSpeed = speedType == ThrowTarget3D.TargetSpeedType.Fast
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

        private ThrowTarget3D.TargetSizeType PickSizeType()
        {
            if (!AllowSmallTargets)
                return ThrowTarget3D.TargetSizeType.Large;

            return GD.Randf() > 0.5f
                ? ThrowTarget3D.TargetSizeType.Small
                : ThrowTarget3D.TargetSizeType.Large;
        }

        private ThrowTarget3D.TargetSpeedType PickSpeedType()
        {
            if (!AllowFastTargets)
                return ThrowTarget3D.TargetSpeedType.Slow;

            return GD.Randf() > 0.5f
                ? ThrowTarget3D.TargetSpeedType.Fast
                : ThrowTarget3D.TargetSpeedType.Slow;
        }

        private void OnTargetHit(ThrowTarget3D target, int points)
        {
            _scoreManager?.AddScore(points);
            _currentTargets.Remove(target);
        }

        private void DespawnCurrentTargets()
        {
            foreach (ThrowTarget3D target in _currentTargets)
            {
                if (IsInstanceValid(target))
                    target.QueueFree();
            }

            _currentTargets.Clear();
        }
    }
}