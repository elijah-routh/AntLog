using Godot;

namespace Game.Enemy
{
    public class DinoRunState : EnemyStateBase
    {
        private readonly DinoEnemyController _dinoController;
        private readonly Node3D _target;

        private float _pathUpdateTimer;

        public DinoRunState(
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
            GD.Print($"{Enemy.Name}: Enter Dino Run");

            _pathUpdateTimer = 0f;

            Enemy.Movement.ResetSpeedMultiplier();
            _dinoController.Animations?.PlayRun();

            if (Enemy.NavigationAgent != null && _target != null)
                Enemy.NavigationAgent.TargetPosition = _target.GlobalPosition;
        }

        public override void PhysicsUpdate(double delta)
        {
            if (_target == null)
            {
                Enemy.Movement.Stop();
                _dinoController.ChangeState(new DinoIdleState(_dinoController, Enemy));
                return;
            }

            if (_dinoController.IsTargetLost())
            {
                Enemy.Movement.Stop();
                _dinoController.ChangeState(new DinoIdleState(_dinoController, Enemy));
                return;
            }

            if (_dinoController.ShouldStartCharge())
            {
                Enemy.Movement.Stop();

                _dinoController.ChangeState(
                    new DinoHopState(_dinoController, Enemy, _target)
                );

                return;
            }

            Vector3 direction = GetPathDirection((float)delta);

            if (direction == Vector3.Zero)
            {
                Enemy.Movement.Stop();
                _dinoController.Animations?.PlayIdle();
                return;
            }

            _dinoController.FaceDirection(
                direction,
                delta,
                _dinoController.RunRotationSpeed
            );

            Enemy.Movement.Move(direction);
            _dinoController.Animations?.PlayRun();
        }

        private Vector3 GetPathDirection(float delta)
        {
            if (Enemy.NavigationAgent == null)
                return _dinoController.GetDirectionToTarget();

            _pathUpdateTimer -= delta;

            if (_pathUpdateTimer <= 0f)
            {
                Enemy.NavigationAgent.TargetPosition = _target.GlobalPosition;
                _pathUpdateTimer = _dinoController.PathUpdateInterval;
            }

            if (Enemy.NavigationAgent.IsNavigationFinished())
                return Vector3.Zero;

            Vector3 nextPosition = Enemy.NavigationAgent.GetNextPathPosition();

            Vector3 direction = nextPosition - Enemy.GlobalPosition;
            direction.Y = 0f;

            if (direction.LengthSquared() <= 0.001f)
                return Vector3.Zero;

            return direction.Normalized();
        }

        public override void Exit()
        {
            Enemy.Movement.Stop();
        }
    }
}