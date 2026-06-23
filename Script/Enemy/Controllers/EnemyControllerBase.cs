using Game.Entity;
using Godot;

namespace Game.Enemy
{
    public abstract partial class EnemyControllerBase : Node
    {
        protected EnemyBase Enemy;
        protected StateMachine StateMachine;

        [Export] public Node3D Target { get; set; }

        [ExportGroup("Health")]
        [Export] public EnemyHealthBar EnemyHealthBar { get; set; }

        public virtual void Initialize(EnemyBase enemy)
        {
            Enemy = enemy;
            StateMachine = new StateMachine();

            Target = GetTree().GetFirstNodeInGroup("player") as Node3D;

            SetupHealthBar();
            ConnectHealthSignals();

            EnterInitialState();
        }

        public override void _Process(double delta)
        {
            StateMachine?.Update(delta);
        }

        public override void _PhysicsProcess(double delta)
        {
            StateMachine?.PhysicsUpdate(delta);
        }

        public void ChangeState(EnemyStateBase state)
        {
            StateMachine?.ChangeState(state);
        }

        public bool IsInState<T>() where T : IState
        {
            return StateMachine != null && StateMachine.IsInState<T>();
        }

        protected abstract void EnterInitialState();

        protected virtual void SetupHealthBar()
        {
            if (Enemy == null)
                return;

            if (EnemyHealthBar == null)
                EnemyHealthBar = Enemy.GetNodeOrNull<EnemyHealthBar>("EnemyHealthBar");

            if (EnemyHealthBar == null)
            {
                GD.PushWarning($"{Name}: EnemyHealthBar was not assigned and could not be found.");
                return;
            }

            if (Enemy.Health == null)
            {
                GD.PushWarning($"{Name}: Enemy HealthComponent is missing.");
                return;
            }

            EnemyHealthBar.InitHealth(Enemy.Health.MaxHealth);
            EnemyHealthBar.SetHealth(Enemy.Health.CurrentHealth, Enemy.Health.MaxHealth);
        }

        protected virtual void ConnectHealthSignals()
        {
            if (Enemy == null || Enemy.Health == null)
                return;

            Enemy.Health.Damaged += OnDamaged;
            Enemy.Health.HealthChanged += OnHealthChanged;
            Enemy.Health.Died += OnDied;
        }

        protected virtual void OnHealthChanged(float currentHealth, float maxHealth)
        {
            EnemyHealthBar?.SetHealth(currentHealth, maxHealth);
        }

        protected virtual void OnDamaged(float damage)
        {
        }

        protected virtual void OnDied()
        {
            EnemyHealthBar?.HideBar();
        }
    }
}