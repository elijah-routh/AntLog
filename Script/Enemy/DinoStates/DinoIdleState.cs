using Godot;

namespace Game.Enemy
{
    public class DinoIdleState : EnemyStateBase
    {
        private readonly DinoEnemyController _dinoController;

        public DinoIdleState(
            DinoEnemyController controller,
            EnemyBase enemy)
            : base(controller, enemy)
        {
            _dinoController = controller;
        }

        public override void Enter()
        {
            GD.Print($"{Enemy.Name}: Enter Dino Idle");

            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();

            _dinoController.ResetChargeChain();
            _dinoController.Animations?.PlayIdle();
        }

        public override void PhysicsUpdate(double delta)
        {
            Enemy.Movement.Stop();

            if (!_dinoController.HasValidTarget())
                return;

            if (_dinoController.IsTargetInDetectionRange())
            {
                _dinoController.ChangeState(
                    new DinoRunState(_dinoController, Enemy, _dinoController.Target)
                );
            }
        }
    }
}