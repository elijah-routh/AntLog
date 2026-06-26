using Godot;

namespace Game.Targets
{
    public partial class ThrowTarget3D : Node3D
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
        public delegate void TargetHitEventHandler(ThrowTarget3D target, int points);

        [ExportGroup("References")]
        [Export] public Sprite3D Sprite;
        [Export] public Area3D HitArea;
        [Export] public Node3D HitSoundsRoot;

        [ExportGroup("Sprites")]
        [Export] public Texture2D[] FunnySprites;

        [ExportGroup("Target Type")]
        [Export] public TargetSizeType SizeType = TargetSizeType.Large;
        [Export] public TargetSpeedType SpeedType = TargetSpeedType.Slow;

        [ExportGroup("Size Settings")]
        [Export] public float SmallScale = 0.75f;
        [Export] public float LargeScale = 1.35f;

        [ExportGroup("Speed Settings")]
        [Export] public float SlowMoveSpeed = 3.0f;
        [Export] public float FastMoveSpeed = 6.0f;

        [ExportGroup("Score Settings")]
        [Export] public int BasePoints = 100;
        [Export] public float SmallSizeMultiplier = 2.0f;
        [Export] public float LargeSizeMultiplier = 1.0f;
        [Export] public float SlowSpeedMultiplier = 1.0f;
        [Export] public float FastSpeedMultiplier = 2.0f;

        [ExportGroup("Rules")]
        [Export] public bool DestroyOnHit = true;
        [Export] public bool RandomizeSpriteOnReady = true;

        public float MoveSpeed { get; private set; }
        public int PointValue { get; private set; }

        private bool _hasBeenHit;

        public override void _Ready()
        {
            if (Sprite == null)
                Sprite = GetNodeOrNull<Sprite3D>("Sprite3D");

            if (HitArea == null)
                HitArea = GetNodeOrNull<Area3D>("HitArea");

            ApplySize();
            ApplySpeed();
            CalculatePoints();

            if (RandomizeSpriteOnReady)
                PickRandomFunnySprite();

            if (HitArea != null)
            {
                HitArea.AreaEntered += OnAreaEntered;
                HitArea.BodyEntered += OnBodyEntered;
                HitArea.Monitoring = true;
            }
        }

        public void Setup(
            TargetSizeType sizeType,
            TargetSpeedType speedType)
        {
            SizeType = sizeType;
            SpeedType = speedType;

            ApplySize();
            ApplySpeed();
            CalculatePoints();
            PickRandomFunnySprite();

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

        private void PickRandomFunnySprite()
        {
            if (Sprite == null || FunnySprites == null || FunnySprites.Length == 0)
                return;

            int index = GD.RandRange(0, FunnySprites.Length - 1);
            Sprite.Texture = FunnySprites[index];
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

            // Adjust this depending on your exact projectile setup.
            // This works if your thrown dino has ThrownProjectileComponent on the Area3D
            // or somewhere on the same root.
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
            if (HitSoundsRoot == null)
                return;

            Godot.Collections.Array<Node> children = HitSoundsRoot.GetChildren();

            if (children.Count == 0)
                return;

            AudioStreamPlayer3D[] players = GetAudioPlayers(children);

            if (players.Length == 0)
                return;

            int index = GD.RandRange(0, players.Length - 1);
            players[index].Play();
        }

        private AudioStreamPlayer3D[] GetAudioPlayers(Godot.Collections.Array<Node> children)
        {
            var players = new System.Collections.Generic.List<AudioStreamPlayer3D>();

            foreach (Node child in children)
            {
                if (child is AudioStreamPlayer3D audioPlayer && audioPlayer.Stream != null)
                    players.Add(audioPlayer);
            }

            return players.ToArray();
        }
    }
}