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

        private CharacterBody3D _body;
        private Vector3 _horizontalVelocity;
        private Vector3 _knockbackVelocity;
        private float _verticalVelocity;

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

            direction.Y = 0f;
            direction = direction.Normalized();

            float dt = (float)GetPhysicsProcessDeltaTime();

            _horizontalVelocity = _horizontalVelocity.Lerp(
                direction * _moveSpeed,
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
        }

        public void ApplyKnockback(Vector3 force)
        {
            _knockbackVelocity += force;
        }
    }
}