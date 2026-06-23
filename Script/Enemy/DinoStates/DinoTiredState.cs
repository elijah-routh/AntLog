using Godot;

namespace Game.Enemy
{
    public class DinoTiredState : EnemyStateBase
    {
        private readonly DinoEnemyController _dinoController;
        private readonly Node3D _target;

        private float _timer;

        public DinoTiredState(
            DinoEnemyController controller,
            EnemyBase enemy,
            Node3D target)
            : base(controller, enemy)
        {
            _dinoController = controller;
            _target = target;
        }

        public override void Enter()
        {
            GD.Print($"{Enemy.Name}: Enter Dino Tired");

            _timer = 0f;

            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();

            _dinoController.ResetChargeChain();
            _dinoController.Animations?.PlayIdle();
        }

        public override void PhysicsUpdate(double delta)
        {
            _timer += (float)delta;

            Enemy.Movement.Stop();

            // Very slow turning while tired, so it feels aware but still vulnerable.
            if (_target != null)
            {
                _dinoController.FaceTarget(
                    delta,
                    _dinoController.TiredRotationSpeed
                );
            }

            if (_timer < _dinoController.TiredDuration)
                return;

            if (_target == null || _dinoController.IsTargetLost())
            {
                _dinoController.ChangeState(new DinoIdleState(_dinoController, Enemy));
                return;
            }

            _dinoController.ChangeState(
                new DinoRunState(_dinoController, Enemy, _target)
            );
        }

        public override void Exit()
        {
            Enemy.Movement.Stop();
        }
    }
}