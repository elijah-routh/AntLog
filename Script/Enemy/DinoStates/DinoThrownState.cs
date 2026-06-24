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

            // ThrownProjectileComponent owns movement while thrown.
            // The spin hitbox is allowed to remain active during this state.
            _dinoController.Animations?.PlayIdle();

            if (Enemy.NavigationAgent != null)
                Enemy.NavigationAgent.TargetPosition = Enemy.GlobalPosition;
        }

        public override void PhysicsUpdate(double delta)
        {
            // Do not run normal AI movement here.
            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();
        }

        public override void Exit()
        {
            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();

            // Throw is over, so the carried-spin damage should stop.
            GetGrabbable()?.DisableHeldSpinHitbox();
        }

        private GrabbableComponent GetGrabbable()
        {
            foreach (Node child in Enemy.GetChildren())
            {
                if (child is GrabbableComponent grabbable)
                    return grabbable;
            }

            return null;
        }
    }
}