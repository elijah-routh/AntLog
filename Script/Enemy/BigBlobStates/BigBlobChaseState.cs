using Godot;

namespace Game.Enemy
{
    public class BigBlobChaseState : EnemyStateBase
    {
        private readonly BigBlobEnemyController _blobController;
        private readonly Node3D _target;

        private float _attackTimer;
        private int _orbitDirection = 1;

        private SlamAttack _slamAttack;

        public BigBlobChaseState(
            BigBlobEnemyController controller,
            EnemyBase enemy,
            Node3D target)
            : base(controller, enemy)
        {
            _blobController = controller;
            _target = target;
        }

        public override void Enter()
        {
            GD.Print($"{Enemy.Name}: Enter BigBlob Chase");

            _orbitDirection = GD.Randf() > 0.5f ? 1 : -1;
            _attackTimer = _blobController.AttackCheckInterval;

            _blobController.AnimationController?.PlayWalk();

            _slamAttack = new SlamAttack(
                Enemy,
                _target,
                _blobController.SlamRadius,
                _blobController.SlamCooldown,
                _blobController.SlamJumpHeight,
                _blobController.SlamSpeed,
                _blobController.SlamJumpUpDuration,
                _blobController.SlamHangTime
            );
        }

        public override void PhysicsUpdate(double delta)
        {
            if (_target == null)
            {
                Enemy.Movement.Stop();
                Controller.ChangeState(new BigBlobIdleState(_blobController, Enemy));
                return;
            }

            FaceTarget(delta);

            _slamAttack.PhysicsUpdate(delta);

            if (_slamAttack.IsRunning)
                return;

            CircleTarget();

            _attackTimer -= (float)delta;

            if (_attackTimer <= 0f)
            {
                TrySlam();
                _attackTimer = _blobController.AttackCheckInterval;
            }
        }

        private void CircleTarget()
        {
            Vector3 toEnemy = Enemy.GlobalPosition - _target.GlobalPosition;
            toEnemy.Y = 0f;

            if (toEnemy.Length() <= 0.01f)
                toEnemy = Enemy.GlobalTransform.Basis.Z;

            Vector3 radialDirection = toEnemy.Normalized();

            Vector3 tangentDirection = new Vector3(
                -radialDirection.Z,
                0f,
                radialDirection.X
            ) * _orbitDirection;

            Vector3 desiredPosition =
                _target.GlobalPosition + radialDirection * _blobController.OrbitDistance;

            desiredPosition.Y = Enemy.GlobalPosition.Y;

            Vector3 correctionDirection = desiredPosition - Enemy.GlobalPosition;
            correctionDirection.Y = 0f;

            Vector3 finalDirection =
                tangentDirection * _blobController.OrbitStrength +
                correctionDirection * _blobController.ApproachStrength;

            Enemy.Movement.Move(finalDirection);
        }

        private void TrySlam()
        {
            float distance = Enemy.GlobalPosition.DistanceTo(_target.GlobalPosition);

            if (distance <= _blobController.SlamAttackRange && _slamAttack.CanUse)
            {
                _slamAttack.Start();
            }
        }

        public override void Exit()
        {
            Enemy.Movement.Stop();
        }

        private void FaceTarget(double delta)
        {
            if (_target == null)
                return;

            Vector3 direction = _target.GlobalPosition - Enemy.GlobalPosition;
            direction.Y = 0f;

            if (direction.LengthSquared() <= 0.001f)
                return;

            direction = direction.Normalized();

            float targetYaw = Mathf.Atan2(direction.X, direction.Z);

            Vector3 rotation = Enemy.GlobalRotation;

            rotation.Y = Mathf.LerpAngle(
                rotation.Y,
                targetYaw,
                Mathf.Clamp(_blobController.RotationSpeed * (float)delta, 0f, 1f)
            );

            Enemy.GlobalRotation = rotation;
        }
    }
}