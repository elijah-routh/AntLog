using Game.Components;
using Game.Entity;
using Godot;

namespace Game.Enemy
{
    public abstract partial class EnemyBase : CharacterBody3D, IDamageable
    {
        [Export] public EnemyData Data { get; set; }

        public HealthComponent Health { get; private set; }
        public MoveComponent Movement { get; private set; }
        public EdgeCheckComponent EdgeCheck { get; private set; }
        public NavigationAgent3D NavigationAgent { get; private set; }
        public EnemyControllerBase Controller { get; private set; }

        public override void _Ready()
        {
            if (Data == null)
            {
                GD.PushError($"{Name}: EnemyData is missing.");
                return;
            }

            Health = GetNodeOrNull<HealthComponent>("HealthComponent");
            Movement = GetNodeOrNull<MoveComponent>("MoveComponent");
            EdgeCheck = GetNodeOrNull<EdgeCheckComponent>("EdgeCheckComponent");
            NavigationAgent = GetNodeOrNull<NavigationAgent3D>("NavigationAgent3D");
            Controller = GetNodeOrNull<EnemyControllerBase>("EnemyController");

            if (Health == null)
            {
                GD.PushError($"{Name}: Missing HealthComponent child node.");
                return;
            }

            if (Movement == null)
            {
                GD.PushError($"{Name}: Missing MoveComponent child node.");
                return;
            }

            if (Controller == null)
            {
                GD.PushError($"{Name}: Missing EnemyController child node.");
                return;
            }

            Health.Initialize(Data.MaxHealth);

            Movement.Initialize(
                Data.MoveSpeed,
                Data.Acceleration,
                Data.Friction,
                Data.Gravity
            );

            Controller.Initialize(this);

            Configure();
        }

        public void TakeDamage(float damage)
        {
            Health?.TakeDamage(damage);
        }

        protected abstract void Configure();
    }
}