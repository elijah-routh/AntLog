using Godot;
using Game.Entity;
using Game.Items;

namespace Game.Enemy
{
    public partial class EnemyController : Node
    {
        private EnemyBase _enemy;
        private StateMachine _stateMachine;

        [Export] public Node3D Target { get; set; }

        [ExportGroup("Rotation")]
        [Export] public float RotationSpeed { get; set; } = 8f;

        // =========================
        // Orbit Settings
        // =========================

        [ExportGroup("Orbit")]

        [Export]
        public float OrbitDistance { get; set; } = 6f;

        [Export]
        public float OrbitStrength { get; set; } = 1.2f;

        [Export]
        public float ApproachStrength { get; set; } = 1f;

        [Export]
        public float AttackCheckInterval { get; set; } = 1.25f;

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


        public void Initialize(EnemyBase enemy)
        {
            _enemy = enemy;

            _stateMachine = new StateMachine();

            Target = GetTree().GetFirstNodeInGroup("player") as Node3D;

            _enemy.Health.Damaged += OnDamaged;
            _enemy.Health.Died += OnDied;

            ChangeState(new IdleState(this, _enemy));
        }

        public override void _Process(double delta)
        {
            _stateMachine?.Update(delta);
        }

        public override void _PhysicsProcess(double delta)
        {
            _stateMachine?.PhysicsUpdate(delta);
        }

        public void ChangeState(EnemyStateBase state)
        {
            _stateMachine?.ChangeState(state);
        }

        private void OnDamaged(float damage)
        {
            if (_stateMachine.IsInState<ChaseState>())
                return;

            if (Target == null)
                return;

            ChangeState(new ChaseState(this, _enemy, Target));
        }

        private void OnDied()
        {
            ChangeState(new DeadState(this, _enemy));
        }

    }
}