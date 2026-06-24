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

            UpdateNavigationTarget();
        }

        public override void PhysicsUpdate(double delta)
        {
            if (_target == null || !GodotObject.IsInstanceValid(_target))
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

            _pathUpdateTimer -= (float)delta;

            if (_pathUpdateTimer <= 0f)
            {
                UpdateNavigationTarget();
                _pathUpdateTimer = _dinoController.PathUpdateInterval;
            }

            Vector3 direction = GetBestChaseDirection();

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

            //GD.Print($"{Enemy.Name}: Run direction = {direction}, distance = {_dinoController.GetDistanceToTarget()}");
            Enemy.Movement.Move(direction);
            _dinoController.Animations?.PlayRun();
        }

        private void UpdateNavigationTarget()
        {
            if (Enemy.NavigationAgent == null)
                return;

            if (_target == null || !GodotObject.IsInstanceValid(_target))
                return;

            Enemy.NavigationAgent.TargetPosition = _target.GlobalPosition;
        }

        private Vector3 GetBestChaseDirection()
        {
            Vector3 directDirection = _dinoController.GetDirectionToTarget();

            if (Enemy.NavigationAgent == null)
                return directDirection;

            // If navigation has finished but we are not close enough to charge,
            // keep chasing directly instead of freezing.
            if (Enemy.NavigationAgent.IsNavigationFinished())
                return directDirection;

            Vector3 nextPathPosition = Enemy.NavigationAgent.GetNextPathPosition();

            Vector3 pathDirection = nextPathPosition - Enemy.GlobalPosition;
            pathDirection.Y = 0f;

            // If the next path position is too close or bad, fall back to direct chase.
            if (pathDirection.LengthSquared() <= 0.01f)
                return directDirection;

            return pathDirection.Normalized();
        }

        public override void Exit()
        {
            Enemy.Movement.Stop();
        }
    }
}