using Godot;
using Game.Entity;

namespace Game.Components
{
    public partial class MoveComponent : Node, IMovable, IKnockable
    {
        private float _moveSpeed;
        private float _acceleration;
        private float _friction;
        private float _gravity;

        private float _speedMultiplier = 1f;

        private CharacterBody3D _body;
        private Vector3 _horizontalVelocity;
        private Vector3 _knockbackVelocity;
        private float _verticalVelocity;

        public bool IsPhysicsLocked { get; private set; }

        public void Initialize(
            float moveSpeed,
            float acceleration,
            float friction,
            float gravity)
        {
            _moveSpeed = moveSpeed;
            _acceleration = acceleration;
            _friction = friction;
            _gravity = gravity;
        }

        public override void _Ready()
        {
            _body = Owner as CharacterBody3D;

            if (_body == null)
                GD.PushError($"{Name}: Owner must be CharacterBody3D.");
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_body == null) return;

            if (IsPhysicsLocked)
            {
                _horizontalVelocity = Vector3.Zero;
                _knockbackVelocity = Vector3.Zero;
                _verticalVelocity = 0f;
                _body.Velocity = Vector3.Zero;

                // Important: do not call MoveAndSlide while grabbed.
                return;
            }

            float dt = (float)delta;

            if (!_body.IsOnFloor())
                _verticalVelocity -= _gravity * dt;
            else if (_verticalVelocity < 0f)
                _verticalVelocity = -0.1f;

            _knockbackVelocity = _knockbackVelocity.Lerp(Vector3.Zero, _friction * dt);

            _body.Velocity = new Vector3(
                _horizontalVelocity.X + _knockbackVelocity.X,
                _verticalVelocity + _knockbackVelocity.Y,
                _horizontalVelocity.Z + _knockbackVelocity.Z
            );

            _body.MoveAndSlide();
        }

        public void Move(Vector3 direction)
        {
            if (_body == null) return;
            if (IsPhysicsLocked) return;

            direction.Y = 0f;

            if (direction.LengthSquared() <= 0.001f)
            {
                Stop();
                return;
            }

            direction = direction.Normalized();

            float dt = (float)GetPhysicsProcessDeltaTime();

            _horizontalVelocity = _horizontalVelocity.Lerp(
                direction * _moveSpeed * _speedMultiplier,
                _acceleration * dt
            );
        }

        public void Stop()
        {
            float dt = (float)GetPhysicsProcessDeltaTime();

            _horizontalVelocity = _horizontalVelocity.Lerp(
                Vector3.Zero,
                _friction * dt
            );

            if (IsPhysicsLocked)
            {
                _horizontalVelocity = Vector3.Zero;
                _knockbackVelocity = Vector3.Zero;
                _verticalVelocity = 0f;

                if (_body != null)
                    _body.Velocity = Vector3.Zero;
            }
        }

        public void ApplyKnockback(Vector3 force)
        {
            if (IsPhysicsLocked) return;

            _knockbackVelocity += force;
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Max(multiplier, 0f);
        }

        public void ResetSpeedMultiplier()
        {
            _speedMultiplier = 1f;
        }

        public void SetPhysicsLocked(bool locked)
        {
            IsPhysicsLocked = locked;

            _horizontalVelocity = Vector3.Zero;
            _knockbackVelocity = Vector3.Zero;
            _verticalVelocity = 0f;

            if (_body != null)
                _body.Velocity = Vector3.Zero;
        }

    }
}