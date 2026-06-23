using Godot;

namespace Game.Enemy
{
    public class ChaseState : EnemyStateBase
    {
        private readonly BossEnemyController _bossController;
        private readonly Node3D _target;

        private float _attackTimer;
        private int _orbitDirection = 1;

        private SlamAttack _slamAttack;
        private LaserBeamAttack _laserAttack;

        public ChaseState(
            BossEnemyController controller,
            EnemyBase enemy,
            Node3D target)
            : base(controller, enemy)
        {
            _bossController = controller;
            _target = target;
        }

        public override void Enter()
        {
            GD.Print($"{Enemy.Name}: Enter Chase");

            _orbitDirection = GD.Randf() > 0.5f ? 1 : -1;

            _slamAttack = new SlamAttack(
                Enemy,
                _target,
                _bossController.SlamRadius,
                _bossController.SlamCooldown,
                _bossController.SlamJumpHeight,
                _bossController.SlamSpeed,
                _bossController.SlamJumpUpDuration,
                _bossController.SlamHangTime
            );

            _laserAttack = new LaserBeamAttack(
                Enemy,
                _target,
                _bossController.LaserBarrel,
                _bossController.LaserBeamScene,
                _bossController.LaserTrackingSpeed,
                _bossController.LaserDuration,
                _bossController.LaserCooldown,
                _bossController.LaserMaxDistance
            );
        }

        public override void PhysicsUpdate(double delta)
        {
            if (_target == null)
            {
                Enemy.Movement.Stop();
                Controller.ChangeState(new IdleState(Controller, Enemy));
                return;
            }

            FaceTarget(delta);

            _slamAttack.PhysicsUpdate(delta);
            _laserAttack.PhysicsUpdate(delta);

            if (_slamAttack.IsRunning || _laserAttack.IsRunning)
                return;

            CircleTarget();

            _attackTimer -= (float)delta;

            if (_attackTimer <= 0f)
            {
                TryAttack();
                _attackTimer = _bossController.AttackCheckInterval;
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
                _target.GlobalPosition + radialDirection * _bossController.OrbitDistance;

            desiredPosition.Y = Enemy.GlobalPosition.Y;

            Vector3 correctionDirection = desiredPosition - Enemy.GlobalPosition;
            correctionDirection.Y = 0f;

            Vector3 finalDirection =
                tangentDirection * _bossController.OrbitStrength +
                correctionDirection * _bossController.ApproachStrength;

            Enemy.Movement.Move(finalDirection);
        }

        private void TryAttack()
        {
            float distance = Enemy.GlobalPosition.DistanceTo(_target.GlobalPosition);

            if (distance <= _bossController.SlamAttackRange && _slamAttack.CanUse)
            {
                _slamAttack.Start();
                return;
            }

            if (_laserAttack.CanUse)
            {
                _laserAttack.Start();
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
                Mathf.Clamp(_bossController.RotationSpeed * (float)delta, 0f, 1f)
            );

            Enemy.GlobalRotation = rotation;
        }
    }
}