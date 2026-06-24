using Godot;

namespace Game.Enemy
{
    public class DinoChaseOrbitState : EnemyStateBase
    {
        private readonly DinoEnemyController _dinoController;
        private readonly Node3D _target;

        private float _pathUpdateTimer;
        private float _chargeDecisionTimer;
        private int _orbitDirection;

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
            _orbitDirection = GD.Randf() > 0.5f ? 1 : -1;

            _dinoController.Animations?.PlayRun();
        }

        public override void PhysicsUpdate(double delta)
        {
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

            UpdateChargeDecision((float)delta);

            if (_dinoController.CanAttemptCharge() && _chargeDecisionTimer <= 0f)
            {
                Enemy.Movement.Stop();

                _dinoController.ChangeState(
                    new DinoHopState(_dinoController, Enemy, _target)
                );

                return;
            }

            if (GD.Randf() <= _dinoController.OrbitDirectionChangeChance * (float)delta)
                _orbitDirection *= -1;

            UpdateOrbitNavigation((float)delta);

            Vector3 direction = GetPathDirection();

            if (direction == Vector3.Zero)
            {
                Enemy.Movement.Stop();
                _dinoController.Animations?.PlayIdle();
                return;
            }

            _dinoController.FaceTarget(delta, _dinoController.RunRotationSpeed);

            Enemy.Movement.Move(direction);
            _dinoController.Animations?.PlayRun();
        }

        private void UpdateChargeDecision(float delta)
        {
            _chargeDecisionTimer -= delta;

            if (_chargeDecisionTimer <= 0f && !_dinoController.CanAttemptCharge())
            {
                _chargeDecisionTimer = _dinoController.GetRandomChaseChargeDelay();
            }
        }

        private void UpdateOrbitNavigation(float delta)
        {
            if (Enemy.NavigationAgent == null)
                return;

            _pathUpdateTimer -= delta;

            if (_pathUpdateTimer > 0f)
                return;

            Vector3 orbitPoint = GetOrbitPoint();

            Enemy.NavigationAgent.TargetPosition = orbitPoint;

            _pathUpdateTimer = _dinoController.PathUpdateInterval;
        }

        private Vector3 GetOrbitPoint()
        {
            Vector3 toEnemy = Enemy.GlobalPosition - _target.GlobalPosition;
            toEnemy.Y = 0f;

            if (toEnemy.LengthSquared() <= 0.001f)
                toEnemy = -_target.GlobalTransform.Basis.Z;

            toEnemy = toEnemy.Normalized();

            Vector3 tangent = new Vector3(-toEnemy.Z, 0f, toEnemy.X) * _orbitDirection;

            float currentDistance = Enemy.GlobalPosition.DistanceTo(_target.GlobalPosition);

            Vector3 desiredRadial = toEnemy * _dinoController.OrbitIdealDistance;
            Vector3 desiredOrbitCenter = _target.GlobalPosition + desiredRadial;

            Vector3 orbitOffset = tangent * 3.0f;

            Vector3 orbitPoint = desiredOrbitCenter + orbitOffset;
            orbitPoint.Y = Enemy.GlobalPosition.Y;

            return orbitPoint;
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
            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();
        }
    }
}