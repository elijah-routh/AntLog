using Godot;

namespace Game.Enemy
{
    public class DinoChaseOrbitState : EnemyStateBase
    {
        private readonly DinoEnemyController _dinoController;
        private readonly Node3D _target;

        private float _pathUpdateTimer;
        private float _chargeDecisionTimer;
        private float _chargeAttemptCooldownTimer;
        private int _orbitDirection;

        private Vector3 _currentOrbitPoint;
        private Vector3 _lastMoveDirection = Vector3.Zero;

        public DinoChaseOrbitState(
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
            GD.Print($"{Enemy.Name}: Enter Dino Chase Orbit");

            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.SetSpeedMultiplier(_dinoController.OrbitSpeedMultiplier);

            _pathUpdateTimer = 0f;
            _chargeDecisionTimer = _dinoController.GetRandomChaseChargeDelay();
            _chargeAttemptCooldownTimer = _dinoController.ChargeAttemptCooldown;

            _orbitDirection = GD.Randf() > 0.5f ? 1 : -1;

            _currentOrbitPoint = Enemy.GlobalPosition;
            _lastMoveDirection = Vector3.Zero;

            _dinoController.Animations?.PlayRun();
        }

        public override void PhysicsUpdate(double delta)
        {
            float dt = (float)delta;

            if (_target == null || !GodotObject.IsInstanceValid(_target))
            {
                Enemy.Movement.Stop();
                _dinoController.ChangeState(new DinoWanderState(_dinoController, Enemy, _target));
                return;
            }

            if (_dinoController.IsTargetLost())
            {
                Enemy.Movement.Stop();
                _dinoController.ChangeState(new DinoWanderState(_dinoController, Enemy, _target));
                return;
            }

            UpdateTimers(dt);

            if (ShouldCommitToCharge())
            {
                Enemy.Movement.Stop();

                _dinoController.ChangeState(
                    new DinoHopState(_dinoController, Enemy, _target)
                );

                return;
            }

            if (GD.Randf() <= _dinoController.OrbitDirectionChangeChance * dt)
                _orbitDirection *= -1;

            UpdateOrbitNavigation(dt);

            Vector3 direction = GetMovementDirection();

            if (direction == Vector3.Zero)
            {
                // Do not instantly swap to idle. This causes run/idle flicker
                // when the nav point is very close or briefly unreachable.
                Enemy.Movement.Stop();

                if (_lastMoveDirection != Vector3.Zero)
                    _dinoController.FaceDirection(
                        _lastMoveDirection,
                        delta,
                        _dinoController.RunRotationSpeed
                    );

                _dinoController.Animations?.PlayRun();
                return;
            }

            _lastMoveDirection = direction;

            // Important change:
            // While orbiting, face where the dino is moving, not the player.
            // This exposes its side/back/tail more often.
            _dinoController.FaceDirection(
                direction,
                delta,
                _dinoController.RunRotationSpeed
            );

            Enemy.Movement.Move(direction);
            _dinoController.Animations?.PlayRun();
        }

        private void UpdateTimers(float delta)
        {
            _chargeDecisionTimer -= delta;

            if (_chargeAttemptCooldownTimer > 0f)
                _chargeAttemptCooldownTimer -= delta;

            if (_chargeDecisionTimer <= 0f && !_dinoController.CanAttemptCharge())
            {
                _chargeDecisionTimer = _dinoController.GetRandomChaseChargeDelay();
            }
        }

        private bool ShouldCommitToCharge()
        {
            if (_chargeAttemptCooldownTimer > 0f)
                return false;

            if (_chargeDecisionTimer > 0f)
                return false;

            if (!_dinoController.CanAttemptCharge())
                return false;

            return true;
        }

        private void UpdateOrbitNavigation(float delta)
        {
            _pathUpdateTimer -= delta;

            if (_pathUpdateTimer > 0f)
                return;

            _currentOrbitPoint = GetOrbitPoint();

            if (Enemy.NavigationAgent != null)
                Enemy.NavigationAgent.TargetPosition = _currentOrbitPoint;

            _pathUpdateTimer = _dinoController.PathUpdateInterval;
        }

        private Vector3 GetOrbitPoint()
        {
            Vector3 enemyPosition = Enemy.GlobalPosition;
            Vector3 targetPosition = _target.GlobalPosition;

            Vector3 awayFromTarget = enemyPosition - targetPosition;
            awayFromTarget.Y = 0f;

            if (awayFromTarget.LengthSquared() <= 0.001f)
            {
                awayFromTarget = -_target.GlobalTransform.Basis.Z;
                awayFromTarget.Y = 0f;
            }

            awayFromTarget = awayFromTarget.Normalized();

            Vector3 tangent = new Vector3(
                -awayFromTarget.Z,
                0f,
                awayFromTarget.X
            ) * _orbitDirection;

            float distanceToTarget = enemyPosition.DistanceTo(targetPosition);

            Vector3 desiredRadial = awayFromTarget * _dinoController.OrbitIdealDistance;
            Vector3 orbitPoint = targetPosition + desiredRadial;

            // Sideways orbit motion.
            orbitPoint += tangent * _dinoController.OrbitSideOffset;

            // If too close, strongly bias the point away from the player.
            // This makes the dino back up instead of hugging the player.
            if (distanceToTarget < _dinoController.BackUpDistance)
            {
                float closeness = 1f - Mathf.Clamp(
                    distanceToTarget / _dinoController.BackUpDistance,
                    0f,
                    1f
                );

                orbitPoint += awayFromTarget * _dinoController.BackUpStrength * closeness;
            }

            orbitPoint.Y = enemyPosition.Y;

            return orbitPoint;
        }

        private Vector3 GetMovementDirection()
        {
            Vector3 direction = Vector3.Zero;

            if (Enemy.NavigationAgent != null)
            {
                if (!Enemy.NavigationAgent.IsNavigationFinished())
                {
                    Vector3 nextPosition = Enemy.NavigationAgent.GetNextPathPosition();

                    direction = nextPosition - Enemy.GlobalPosition;
                    direction.Y = 0f;

                    if (direction.LengthSquared() > 0.05f)
                        return direction.Normalized();
                }
            }

            // Fallback: move toward the desired orbit point directly.
            // This prevents the dino from stopping just because the nav path
            // briefly reports finished or the next point is too close.
            direction = _currentOrbitPoint - Enemy.GlobalPosition;
            direction.Y = 0f;

            if (direction.LengthSquared() <= 0.05f)
                return Vector3.Zero;

            return direction.Normalized();
        }

        public override void Exit()
        {
            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();
        }
    }
}