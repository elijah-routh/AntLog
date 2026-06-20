using Godot;

namespace Game.Enemy
{
    public class DeadState : EnemyStateBase
    {
        private float _deathTimer;
        private const float DeathDuration = 2f;

        public DeadState(
            EnemyController controller,
            EnemyBase enemy)
            : base(controller, enemy)
        {
        }

        public override void Enter()
        {
            GD.Print($"{Enemy.Name}: Dead");

            Enemy.Movement.Stop();

            Enemy.SetPhysicsProcess(false);

            // Optional:
            // disable collisions
            // play animation
            // drop loot
            // spawn VFX
        }

        public override void Update(double delta)
        {
            _deathTimer += (float)delta;

            if (_deathTimer >= DeathDuration)
            {
                Enemy.QueueFree();
            }
        }
    }
}