using Godot;
using System.Collections.Generic;

namespace Game.Targets
{
    public partial class PlanetTarget : Node3D
    {
        public enum TargetSizeType
        {
            Small,
            Large
        }

        public enum TargetSpeedType
        {
            Slow,
            Fast
        }

        [Signal]
        public delegate void TargetHitEventHandler(PlanetTarget target, int points);

        [ExportGroup("References")]
        [Export] public Area3D HitArea;
        [Export] public Node3D HitSoundsRoot;
        [Export] public Node3D VisualSpawnPoint;

        [ExportGroup("Target Visual Scenes")]
        [Export] public PackedScene[] TargetVisualScenes;

        [ExportGroup("Target Type")]
        [Export] public TargetSizeType SizeType = TargetSizeType.Large;
        [Export] public TargetSpeedType SpeedType = TargetSpeedType.Slow;

        [ExportGroup("Size Settings")]
        [Export] public float SmallScale = 0.75f;
        [Export] public float LargeScale = 1.35f;

        [ExportGroup("Speed Settings")]
        [Export] public float SlowMoveSpeed = 0.01f;
        [Export] public float FastMoveSpeed = 0.05f;

        [ExportGroup("Score Settings")]
        [Export] public int BasePoints = 100;
        [Export] public float SmallSizeMultiplier = 2.0f;
        [Export] public float LargeSizeMultiplier = 1.0f;
        [Export] public float SlowSpeedMultiplier = 1.0f;
        [Export] public float FastSpeedMultiplier = 2.0f;

        [ExportGroup("Rules")]
        [Export] public bool DestroyOnHit = true;
        [Export] public bool PickRandomVisualOnReady = true;

        public float MoveSpeed { get; private set; }
        public int PointValue { get; private set; }
        public Node3D CurrentVisual { get; private set; }

        private bool _hasBeenHit;
        private readonly RandomNumberGenerator _rng = new();

        public override void _Ready()
        {
            _rng.Randomize();

            if (HitArea == null)
                HitArea = GetNodeOrNull<Area3D>("HitArea");

            if (VisualSpawnPoint == null)
                VisualSpawnPoint = GetNodeOrNull<Node3D>("VisualSpawnPoint");

            if (VisualSpawnPoint == null)
                VisualSpawnPoint = this;

            ApplySize();
            ApplySpeed();
            CalculatePoints();

            if (PickRandomVisualOnReady)
                PickRandomVisual();

            if (HitArea != null)
            {
                HitArea.AreaEntered += OnAreaEntered;
                HitArea.BodyEntered += OnBodyEntered;
                HitArea.Monitoring = true;
            }
            else
            {
                GD.PushWarning($"{Name}: HitArea is not assigned.");
            }
        }

        public void Setup(TargetSizeType sizeType, TargetSpeedType speedType)
        {
            SizeType = sizeType;
            SpeedType = speedType;

            ApplySize();
            ApplySpeed();
            CalculatePoints();

            PickRandomVisual();

            _hasBeenHit = false;
        }

        private void ApplySize()
        {
            float scaleValue = SizeType == TargetSizeType.Small
                ? SmallScale
                : LargeScale;

            Scale = Vector3.One * scaleValue;
        }

        private void ApplySpeed()
        {
            MoveSpeed = SpeedType == TargetSpeedType.Fast
                ? FastMoveSpeed
                : SlowMoveSpeed;
        }

        private void CalculatePoints()
        {
            float sizeMultiplier = SizeType == TargetSizeType.Small
                ? SmallSizeMultiplier
                : LargeSizeMultiplier;

            float speedMultiplier = SpeedType == TargetSpeedType.Fast
                ? FastSpeedMultiplier
                : SlowSpeedMultiplier;

            PointValue = Mathf.RoundToInt(BasePoints * sizeMultiplier * speedMultiplier);
        }

        public void PickRandomVisual()
        {
            if (TargetVisualScenes == null || TargetVisualScenes.Length == 0)
            {
                GD.PushWarning($"{Name}: No target visual scenes assigned.");
                return;
            }

            int index = _rng.RandiRange(0, TargetVisualScenes.Length - 1);
            SetVisual(index);
        }

        public void SetVisual(int index)
        {
            if (TargetVisualScenes == null || TargetVisualScenes.Length == 0)
                return;

            if (index < 0 || index >= TargetVisualScenes.Length)
            {
                GD.PushWarning($"{Name}: Target visual scene index {index} is out of range.");
                return;
            }

            ClearCurrentVisual();

            PackedScene visualScene = TargetVisualScenes[index];

            if (visualScene == null)
            {
                GD.PushWarning($"{Name}: Target visual scene at index {index} is null.");
                return;
            }

            Node instance = visualScene.Instantiate();

            if (instance is not Node3D visual)
            {
                GD.PushWarning($"{Name}: Instantiated target visual is not a Node3D.");
                instance.QueueFree();
                return;
            }

            CurrentVisual = visual;
            VisualSpawnPoint.AddChild(CurrentVisual);

            CurrentVisual.Position = Vector3.Zero;
            CurrentVisual.Rotation = Vector3.Zero;
            CurrentVisual.Scale = Vector3.One;
        }

        public void ClearCurrentVisual()
        {
            if (CurrentVisual == null)
                return;

            CurrentVisual.QueueFree();
            CurrentVisual = null;
        }

        private void OnAreaEntered(Area3D area)
        {
            TryHandleHit(area);
        }

        private void OnBodyEntered(Node3D body)
        {
            TryHandleHit(body);
        }

        private void TryHandleHit(Node node)
        {
            if (_hasBeenHit)
                return;

            if (!WasHitByThrownDino(node))
                return;

            _hasBeenHit = true;

            PlayRandomHitSound();

            EmitSignal(nameof(TargetHit), this, PointValue);

            if (DestroyOnHit)
                QueueFree();
        }

        private bool WasHitByThrownDino(Node node)
        {
            if (node == null)
                return false;

            if (node is ThrownProjectileComponent projectile && projectile.IsActive)
                return true;

            ThrownProjectileComponent childProjectile =
                node.GetNodeOrNull<ThrownProjectileComponent>("ThrownProjectileComponent");

            if (childProjectile != null && childProjectile.IsActive)
                return true;

            Node parent = node.GetParent();

            while (parent != null)
            {
                if (parent is ThrownProjectileComponent parentProjectile && parentProjectile.IsActive)
                    return true;

                parent = parent.GetParent();
            }

            return false;
        }

        private void PlayRandomHitSound()
        {
            int randomExplosion = GD.RandRange(1, 4);

            switch (randomExplosion)
            {
                case 1:
                    SoundManager.Instance.PlayExplosionSound();
                    break;

                case 2:
                    SoundManager.Instance.PlayExplosionSound2();
                    break;

                case 3:
                    SoundManager.Instance.PlayExplosionSound3();
                    break;

                case 4:
                    SoundManager.Instance.PlayExplosionSound4();
                    break;
            }
        }

        private AudioStreamPlayer3D[] GetAudioPlayers(Godot.Collections.Array<Node> children)
        {
            var players = new List<AudioStreamPlayer3D>();

            foreach (Node child in children)
            {
                if (child is AudioStreamPlayer3D audioPlayer && audioPlayer.Stream != null)
                    players.Add(audioPlayer);
            }

            return players.ToArray();
        }
    }
}