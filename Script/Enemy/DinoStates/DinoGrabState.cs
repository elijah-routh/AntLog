using Godot;

namespace Game.Enemy
{
    public class DinoGrabState : EnemyStateBase
    {
        private readonly DinoEnemyController _dinoController;

        public DinoGrabState(
            DinoEnemyController controller,
            EnemyBase enemy)
            : base(controller, enemy)
        {
            _dinoController = controller;
        }

        public override void Enter()
        {
            GD.Print($"{Enemy.Name}: Enter Dino Grabbed");

            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();

            _dinoController.ResetChargeChain();
            _dinoController.Animations?.PlayIdle();

            if (Enemy.NavigationAgent != null)
            {
                Enemy.NavigationAgent.TargetPosition = Enemy.GlobalPosition;
            }
        }

        public override void PhysicsUpdate(double delta)
        {
            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();

            // Intentionally do not face the player.
            // The player has control/opportunity while the tail is grabbed.
        }

        public override void Exit()
        {
            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();
        }
    }
}