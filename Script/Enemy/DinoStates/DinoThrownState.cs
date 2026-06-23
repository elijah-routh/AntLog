using Godot;

namespace Game.Enemy
{
    public class DinoThrownState : EnemyStateBase
    {
        private readonly DinoEnemyController _dinoController;

        public DinoThrownState(
            DinoEnemyController controller,
            EnemyBase enemy)
            : base(controller, enemy)
        {
            _dinoController = controller;
        }

        public override void Enter()
        {
            GD.Print($"{Enemy.Name}: Enter Dino Thrown");

            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();

            // Optional. Use Idle unless you later add a ragdoll/flail animation.
            _dinoController.Animations?.PlayIdle();

            if (Enemy.NavigationAgent != null)
                Enemy.NavigationAgent.TargetPosition = Enemy.GlobalPosition;
        }

        public override void PhysicsUpdate(double delta)
        {
            // Do not run normal AI movement here.
            // ThrownProjectileComponent owns movement while thrown.
            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();
        }

        public override void Exit()
        {
            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();
        }
    }
}