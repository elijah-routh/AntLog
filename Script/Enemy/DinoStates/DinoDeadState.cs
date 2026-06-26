using Godot;

namespace Game.Enemy
{
    public class DinoDeadState : EnemyStateBase
    {
        private readonly DinoEnemyController _dinoController;

        private float _despawnTimer = 2.0f;

        public DinoDeadState(
            DinoEnemyController controller,
            EnemyBase enemy)
            : base(controller, enemy)
        {
            _dinoController = controller;
        }

        public override void Enter()
        {
            GD.Print($"{Enemy.Name}: Enter Dino Dead");

            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();

            Enemy.GetNodeOrNull<GrabbableComponent>("GrabbableComponent")
                ?.DisableGrabbing();

            _dinoController.Animations?.PlayDeath();
        }

        public override void PhysicsUpdate(double delta)
        {
            Enemy.Movement.Stop();

            _despawnTimer -= (float)delta;

            if (_despawnTimer <= 0f)
            {
                Enemy.QueueFree();
            }
        }
    }
}