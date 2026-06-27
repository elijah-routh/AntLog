using Godot;

namespace Game.Enemy
{
    public partial class BigBlobEnemyController : EnemyControllerBase
    {
        // =========================
        // Rotation
        // =========================

        [ExportGroup("Rotation")]
        [Export] public float RotationSpeed { get; set; } = 8f;

        // =========================
        // Orbit Settings
        // =========================

        [ExportGroup("Orbit")]
        [Export] public float OrbitDistance { get; set; } = 6f;
        [Export] public float OrbitStrength { get; set; } = 1.2f;
        [Export] public float ApproachStrength { get; set; } = 1f;
        [Export] public float AttackCheckInterval { get; set; } = 1.25f;

        // =========================
        // Slam Settings
        // =========================

        [ExportGroup("Slam")]
        [Export] public float SlamAttackRange { get; set; } = 30f;
        [Export] public float SlamRadius { get; set; } = 4f;
        [Export] public float SlamCooldown { get; set; } = 4f;
        [Export] public float SlamJumpHeight { get; set; } = 10f;
        [Export] public float SlamSpeed { get; set; } = 20f;
        [Export] public float SlamJumpUpDuration { get; set; } = 0.75f;
        [Export] public float SlamHangTime { get; set; } = 0.25f;

        // =========================
        // Pathfinding
        // =========================

        [ExportGroup("Pathfinding")]
        [Export] public float PathUpdateInterval { get; set; } = 0.2f;

        // =========================
        // References
        // =========================

        [ExportGroup("References")]
        [Export] public BlobAnimationController AnimationController { get; set; }

        protected override void EnterInitialState()
        {
            ChangeState(new BigBlobChaseState(this, Enemy, Target));
        }

        protected override void OnDamaged(float damage)
        {
            if (IsInState<BigBlobChaseState>())
                return;

            if (Target == null)
                return;

            ChangeState(new BigBlobChaseState(this, Enemy, Target));
        }

        protected override void OnDied()
        {
            base.OnDied();

            ChangeState(new BigBlobDeadState(this, Enemy));
        }
    }
}