using Godot;

namespace Game.Enemy
{
    public class SlamAttack : EnemyAttackBase
    {
        private readonly EnemyBase _enemy;
        private readonly Node3D _target;

        private readonly float _damageRadius;
        private readonly float _jumpHeight;
        private readonly float _slamSpeed;
        private readonly float _jumpUpDuration;
        private readonly float _hangTime;

        private Vector3 _startPos;
        private Vector3 _slamPoint;
        private Vector3 _targetAbovePoint;

        private float _timer;
        private SlamPhase _phase;

        private enum SlamPhase
        {
            JumpUp,
            Hang,
            SlamDown
        }

        public SlamAttack(
            EnemyBase enemy,
            Node3D target,
            float damageRadius,
            float cooldown,
            float jumpHeight,
            float slamSpeed,
            float jumpUpDuration,
            float hangTime)
        {
            _enemy = enemy;
            _target = target;
            _damageRadius = damageRadius;
            _jumpHeight = jumpHeight;
            _slamSpeed = slamSpeed;
            _jumpUpDuration = jumpUpDuration;
            _hangTime = hangTime;

            Cooldown = cooldown;
        }

        public void Start()
        {
            if (!TryStart())
            {
                GD.Print($"{_enemy.Name}: Slam failed to start");
                return;
            }

            GD.Print($"{_enemy.Name}: Slam started");

            _timer = 0f;
            _phase = SlamPhase.JumpUp;

            _startPos = _enemy.GlobalPosition;

            _slamPoint = _target.GlobalPosition;
            _slamPoint.Y = _startPos.Y;

            _targetAbovePoint = _slamPoint;
            _targetAbovePoint.Y += _jumpHeight;

            _enemy.Movement.Stop();
        }

        public override void PhysicsUpdate(double delta)
        {
            base.PhysicsUpdate(delta);

            if (!IsRunning)
                return;

            _timer += (float)delta;

            switch (_phase)
            {
                case SlamPhase.JumpUp:
                    UpdateJumpUp();
                    break;

                case SlamPhase.Hang:
                    UpdateHang();
                    break;

                case SlamPhase.SlamDown:
                    UpdateSlamDown(delta);
                    break;
            }
        }

        private void UpdateJumpUp()
        {
            float t = Mathf.Clamp(_timer / _jumpUpDuration, 0f, 1f);

            _enemy.GlobalPosition = _startPos.Lerp(_targetAbovePoint, t);

            if (t >= 1f)
            {
                _timer = 0f;
                _phase = SlamPhase.Hang;

                GD.Print($"{_enemy.Name}: Slam reached point above target");
            }
        }

        private void UpdateHang()
        {
            if (_timer >= _hangTime)
            {
                _timer = 0f;
                _phase = SlamPhase.SlamDown;

                GD.Print($"{_enemy.Name}: Slam dropping");
            }
        }

        private void UpdateSlamDown(double delta)
        {
            Vector3 pos = _enemy.GlobalPosition;
            pos.Y -= _slamSpeed * (float)delta;
            _enemy.GlobalPosition = pos;

            if (_enemy.GlobalPosition.Y <= _slamPoint.Y)
            {
                _enemy.GlobalPosition = _slamPoint;

                DoDamage();
                Finish();

                GD.Print($"{_enemy.Name}: Slam finished");
            }
        }

        private void DoDamage()
        {
            float distance = _enemy.GlobalPosition.DistanceTo(_target.GlobalPosition);

            if (distance <= _damageRadius)
            {
                GD.Print($"{_enemy.Name}: Slam hit target!");
                // TODO: damage player here
            }
            else
            {
                GD.Print($"{_enemy.Name}: Slam missed");
            }
        }
    }
}