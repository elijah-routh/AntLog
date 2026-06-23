using Godot;

namespace Game.Enemy
{
    public partial class BossEnemyController : EnemyControllerBase
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
        // Laser Settings
        // =========================

        [ExportGroup("Laser Beam")]
        [Export] public Marker3D LaserBarrel { get; set; }
        [Export] public PackedScene LaserBeamScene { get; set; }

        [Export] public float LaserAttackRange { get; set; } = 40f;
        [Export] public float LaserTrackingSpeed { get; set; } = 2f;
        [Export] public float LaserDuration { get; set; } = 3f;
        [Export] public float LaserCooldown { get; set; } = 5f;
        [Export] public float LaserMaxDistance { get; set; } = 40f;

        [ExportGroup("Pathfinding")]
        [Export] public float PathUpdateInterval { get; set; } = 0.2f;

        protected override void EnterInitialState()
        {
            ChangeState(new IdleState(this, Enemy));
        }

        protected override void OnDamaged(float damage)
        {
            if (IsInState<ChaseState>())
                return;

            if (Target == null)
                return;

            ChangeState(new ChaseState(this, Enemy, Target));
        }

        protected override void OnDied()
        {
            base.OnDied();

            ChangeState(new DeadState(this, Enemy));
        }
    }
}