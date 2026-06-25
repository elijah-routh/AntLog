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

            Enemy.Movement.SetPhysicsLocked(true);
            Enemy.Movement.ResetSpeedMultiplier();

            // ThrownProjectileComponent owns movement while thrown.
            _dinoController.Animations?.PlayIdle();

            if (Enemy.NavigationAgent != null)
                Enemy.NavigationAgent.TargetPosition = Enemy.GlobalPosition;
        }

        public override void PhysicsUpdate(double delta)
        {
            // Do nothing.
            // MoveComponent is physics-locked.
            // ThrownProjectileComponent owns movement.
        }

        public override void Exit()
        {
            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();
            Enemy.Movement.SetPhysicsLocked(false);

            GetGrabbable()?.DisableHeldSpinHitbox();
            GetGrabbable()?.Projectile.CancelProjectile();
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