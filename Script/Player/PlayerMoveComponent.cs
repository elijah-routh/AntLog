using Godot;

public partial class PlayerMoveComponent : Node
{
    [Export] public float MoveSpeed = 12.0f;
    [Export] public float Acceleration = 10.0f;
    [Export] public float Deceleration = 16.0f;
    [Export] public float JumpVelocity = 8.5f;
    [Export] public float RotationSpeed = 12.0f;

    public void ApplyGravity(CharacterBody3D body, float delta)
    {
        if (!body.IsOnFloor())
            body.Velocity += body.GetGravity() * delta;
    }

    public void HandleJump(CharacterBody3D body)
    {
        if (PlayerInput.JumpPressed && body.IsOnFloor())
        {
            Vector3 velocity = body.Velocity;
            velocity.Y = JumpVelocity;
            body.Velocity = velocity;
        }
    }

    public void HandleMovement(CharacterBody3D body, Node3D cameraPivot, float delta)
    {
        Vector2 input = PlayerInput.Movement;
        Vector3 direction = GetCameraRelativeDirection(input, cameraPivot);

        Vector3 velocity = body.Velocity;

        if (direction != Vector3.Zero)
        {
            Vector3 targetVelocity = direction * MoveSpeed;

            velocity.X = Mathf.MoveToward(velocity.X, targetVelocity.X, Acceleration * delta);
            velocity.Z = Mathf.MoveToward(velocity.Z, targetVelocity.Z, Acceleration * delta);

            RotateTowards(body, direction, delta);
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, Deceleration * delta);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, Deceleration * delta);
        }

        body.Velocity = velocity;
    }

    private Vector3 GetCameraRelativeDirection(Vector2 input, Node3D cameraPivot)
    {
        if (input == Vector2.Zero || cameraPivot == null)
            return Vector3.Zero;

        float yaw = cameraPivot.GlobalRotation.Y;

        Vector3 forward = new Vector3(
            -Mathf.Sin(yaw),
            0,
            -Mathf.Cos(yaw)
        );

        Vector3 right = new Vector3(
            Mathf.Cos(yaw),
            0,
            -Mathf.Sin(yaw)
        );

        return (right * -input.X + forward * input.Y).Normalized();
    }

    private void RotateTowards(Node3D player, Vector3 direction, float delta)
    {
        float targetRotation = Mathf.Atan2(direction.X, direction.Z);

        Vector3 rotation = player.Rotation;

        rotation.Y = Mathf.LerpAngle(
            rotation.Y,
            targetRotation,
            RotationSpeed * delta
        );

        player.Rotation = rotation;
    }
}