using Godot;

namespace Game.Enemy
{
    public class DinoWanderState : EnemyStateBase
    {
        private readonly DinoEnemyController _dinoController;
        private readonly Node3D _target;

        private float _pauseTimer;
        private float _pathUpdateTimer;
        private Vector3 _currentWanderPoint;
        private bool _hasWanderPoint;

        public DinoWanderState(
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
            GD.Print($"{Enemy.Name}: Enter Dino Wander");

            Enemy.Movement.ResetSpeedMultiplier();
            _pauseTimer = 0f;
            _pathUpdateTimer = 0f;
            _hasWanderPoint = false;

            _dinoController.Animations?.PlayRun();
            PickNewWanderPoint();
        }

        public override void PhysicsUpdate(double delta)
        {
            if (_dinoController.ShouldEnterOrbit())
            {
                Enemy.Movement.Stop();
                _dinoController.ChangeState(
                    new DinoChaseOrbitState(_dinoController, Enemy, _target)
                );
                return;
            }

            if (_dinoController.CanAttemptCharge() && _dinoController.RollWanderChargeChance())
            {
                Enemy.Movement.Stop();
                _dinoController.ChangeState(
                    new DinoHopState(_dinoController, Enemy, _target)
                );
                return;
            }

            if (_pauseTimer > 0f)
            {
                _pauseTimer -= (float)delta;
                Enemy.Movement.Stop();
                _dinoController.Animations?.PlayIdle();
                return;
            }

            if (!_hasWanderPoint)
            {
                PickNewWanderPoint();
                return;
            }

            UpdateNavigationTarget((float)delta);

            Vector3 direction = GetPathDirection();

            if (direction == Vector3.Zero)
            {
                BeginPause();
                return;
            }

            _dinoController.FaceDirection(
                direction,
                delta,
                _dinoController.RunRotationSpeed
            );

            Enemy.Movement.Move(direction);
            _dinoController.Animations?.PlayRun();

            if (Enemy.GlobalPosition.DistanceTo(_currentWanderPoint) <= _dinoController.WanderPointReachedDistance)
                BeginPause();
        }

        private void PickNewWanderPoint()
        {
            Vector2 randomCircle = Vector2.Right.Rotated(GD.Randf() * Mathf.Tau) * GD.Randf() * _dinoController.WanderRadius;

            Vector3 rawPoint = Enemy.GlobalPosition + new Vector3(
                randomCircle.X,
                0f,
                randomCircle.Y
            );

            // This gives the nav agent a target. If the point is not valid,
            // the agent may clamp/resolve depending on the navigation map.
            _currentWanderPoint = rawPoint;
            _currentWanderPoint.Y = Enemy.GlobalPosition.Y;

            _hasWanderPoint = true;
            _pathUpdateTimer = 0f;

            if (Enemy.NavigationAgent != null)
                Enemy.NavigationAgent.TargetPosition = _currentWanderPoint;
        }

        private void BeginPause()
        {
            Enemy.Movement.Stop();
            _hasWanderPoint = false;
            _pauseTimer = (float)GD.RandRange(
                _dinoController.WanderPauseMin,
                _dinoController.WanderPauseMax
            );
        }

        private void UpdateNavigationTarget(float delta)
        {
            if (Enemy.NavigationAgent == null)
                return;

            _pathUpdateTimer -= delta;

            if (_pathUpdateTimer > 0f)
                return;

            Enemy.NavigationAgent.TargetPosition = _currentWanderPoint;
            _pathUpdateTimer = _dinoController.PathUpdateInterval;
        }

        private Vector3 GetPathDirection()
        {
            if (Enemy.NavigationAgent == null)
                return Vector3.Zero;

            if (Enemy.NavigationAgent.IsNavigationFinished())
                return Vector3.Zero;

            Vector3 nextPosition = Enemy.NavigationAgent.GetNextPathPosition();

            Vector3 direction = nextPosition - Enemy.GlobalPosition;
            direction.Y = 0f;

            if (direction.LengthSquared() <= 0.05f)
                return Vector3.Zero;

            return direction.Normalized();
        }

        public override void Exit()
        {
            Enemy.Movement.Stop();
        }
    }
}