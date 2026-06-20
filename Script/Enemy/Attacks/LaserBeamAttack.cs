using Godot;

namespace Game.Enemy
{
    public class LaserBeamAttack : EnemyAttackBase
    {
        private readonly EnemyBase _enemy;
        private readonly Node3D _target;
        private readonly Marker3D _barrel;
        private readonly PackedScene _laserBeamScene;

        private readonly float _trackingSpeed;
        private readonly float _duration;
        private readonly float _maxDistance;

        private LaserBeam _activeBeam;
        private float _timer;

        public LaserBeamAttack(
            EnemyBase enemy,
            Node3D target,
            Marker3D barrel,
            PackedScene laserBeamScene,
            float trackingSpeed,
            float duration,
            float cooldown,
            float maxDistance)
        {
            _enemy = enemy;
            _target = target;
            _barrel = barrel;
            _laserBeamScene = laserBeamScene;
            _trackingSpeed = trackingSpeed;
            _duration = duration;
            _maxDistance = maxDistance;

            Cooldown = cooldown;
        }

        public void Start()
        {
            if (!TryStart())
                return;

            GD.Print($"{_enemy.Name}: Laser started");

            if (_barrel == null || _laserBeamScene == null)
            {
                Finish();
                return;
            }

            _timer = 0f;
            _enemy.Movement.Stop();

            SpawnBeam();
        }

        public override void PhysicsUpdate(double delta)
        {
            base.PhysicsUpdate(delta);

            if (!IsRunning)
                return;

            _timer += (float)delta;

            TrackTarget(delta);

            if (_timer >= _duration)
            {
                StopBeam();
                Finish();
            }
        }

        private Node3D GetTarget()
        {
            return _enemy.GetTree()
                .GetFirstNodeInGroup("Player") as Node3D;
        }

        private void SpawnBeam()
        {
            if (_activeBeam != null)
                return;

            _activeBeam = _laserBeamScene.Instantiate<LaserBeam>();

            _activeBeam.MaxDistance = _maxDistance;
            _activeBeam.TargetPosition = new Vector3(0f, 0f, _maxDistance);

            _barrel.AddChild(_activeBeam);

            _activeBeam.Position = Vector3.Zero;
            _activeBeam.Rotation = Vector3.Zero;
        }

        private void StopBeam()
        {
            if (_activeBeam == null)
                return;

            _activeBeam.QueueFree();
            _activeBeam = null;
            GD.Print($"{_enemy.Name}: Laser finished");

        }

        private void TrackTarget(double delta)
        {
            var target = _target ?? GetTarget();

            if (_target == null || _barrel == null)
                return;

            Node3D pivot = _barrel.GetParent<Node3D>(); // BarrelPivot

            Vector3 from = pivot.GlobalPosition;
            Vector3 to = _target.GlobalPosition;

            Vector3 desiredDir = (to - from).Normalized();
            if (desiredDir == Vector3.Zero)
                return;

            pivot.LookAt(from - desiredDir, Vector3.Up);
        }
    }
}