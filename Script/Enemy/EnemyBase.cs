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
        public EnemyController Controller { get; private set; }

        public override void _Ready()
        {
            Health = GetNode<HealthComponent>("HealthComponent");
            Movement = GetNode<MoveComponent>("MoveComponent");
            Controller = GetNode<EnemyController>("EnemyController");

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
            Health.TakeDamage(damage);
        }

        protected abstract void Configure();
    }
}
