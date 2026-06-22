using Godot;

public partial class PlayerMoveComponent : Node
{
    [ExportGroup("Movement")]
    [Export] public float MoveSpeed = 12.0f;
    [Export] public float Acceleration = 10.0f;
    [Export] public float Deceleration = 16.0f;
    [Export] public float JumpVelocity = 8.5f;
    [Export] public float RotationSpeed = 12.0f;

    [ExportGroup("Dive Grab")]
    [Export] public DiveGrabComponent DiveGrab;
    [Export] public float DiveSpeed = 24.0f;
    [Export] public float DiveDuration = 0.25f;
    [Export] public float DiveCooldown = 0.35f;
    [Export] public float DiveEndDrag = 20.0f;
    [Export] public bool CanDiveInAir = false;

    public bool IsDiving { get; private set; }

    private float _diveTimer;
    private float _diveCooldownTimer;
    private Vector3 _diveDirection = Vector3.Zero;

    public void ApplyGravity(CharacterBody3D body, float delta)
    {
        if (!body.IsOnFloor())
            body.Velocity += body.GetGravity() * delta;
    }

    public void HandleJump(CharacterBody3D body)
    {
        if (IsDiving)
            return;

        if (PlayerInput.JumpPressed && body.IsOnFloor())
        {
            Vector3 velocity = body.Velocity;
            velocity.Y = JumpVelocity;
            body.Velocity = velocity;
        }
    }

    public void HandleMovement(CharacterBody3D body, Node3D cameraPivot, float delta)
    {
        UpdateDiveCooldown(delta);

        if (IsDiving)
        {
            HandleDive(body, delta);
            return;
        }

        TryStartDive(body, cameraPivot);

        if (IsDiving)
            return;

        Vector2 input = PlayerInput.Movement;
        Vector3 direction = GetCameraRelativeDirection(input, cameraPivot);

        Vector3 velocity = body.Velocity;

        if (direction != Vector3.Zero)
        {
            Vector3 targetVelocity = direction * MoveSpeed;

            velocity.X = Mathf.MoveToward(
                velocity.X,
                targetVelocity.X,
                Acceleration * delta
            );

            velocity.Z = Mathf.MoveToward(
                velocity.Z,
                targetVelocity.Z,
                Acceleration * delta
            );

            RotateTowards(body, direction, delta);
        }
        else
        {
            velocity.X = Mathf.MoveToward(
                velocity.X,
                0,
                Deceleration * delta
            );

            velocity.Z = Mathf.MoveToward(
                velocity.Z,
                0,
                Deceleration * delta
            );
        }

        body.Velocity = velocity;
    }

    private void TryStartDive(CharacterBody3D body, Node3D cameraPivot)
    {
        if (!PlayerInput.GrabPressed)
            return;

        if (DiveGrab != null && DiveGrab.TryThrowHeld())
            return;

        if (_diveCooldownTimer > 0.0f)
            return;

        if (!CanDiveInAir && !body.IsOnFloor())
            return;

        Vector2 input = PlayerInput.Movement;
        Vector3 inputDirection = GetCameraRelativeDirection(input, cameraPivot);

        if (inputDirection == Vector3.Zero)
        {
            // Dive in the direction the player is already facing.
            inputDirection = -body.GlobalBasis.Z;
            inputDirection.Y = 0;
            inputDirection = inputDirection.Normalized();
        }

        StartDive(body, inputDirection);
    }

    private void StartDive(CharacterBody3D body, Vector3 direction)
    {
        IsDiving = true;
        _diveTimer = DiveDuration;
        _diveCooldownTimer = DiveCooldown;
        _diveDirection = direction.Normalized();

        Vector3 velocity = body.Velocity;
        velocity.X = _diveDirection.X * DiveSpeed;
        velocity.Z = _diveDirection.Z * DiveSpeed;

        body.Velocity = velocity;

        RotateInstantlyTowards(body, _diveDirection);
    }

    private void HandleDive(CharacterBody3D body, float delta)
    {
        _diveTimer -= delta;

        Vector3 velocity = body.Velocity;

        velocity.X = _diveDirection.X * DiveSpeed;
        velocity.Z = _diveDirection.Z * DiveSpeed;

        body.Velocity = velocity;

        if (_diveTimer <= 0.0f)
        {
            EndDive(body);
        }
    }

    private void EndDive(CharacterBody3D body)
    {
        IsDiving = false;
        _diveTimer = 0.0f;

        Vector3 velocity = body.Velocity;

        velocity.X = Mathf.MoveToward(velocity.X, 0, DiveEndDrag);
        velocity.Z = Mathf.MoveToward(velocity.Z, 0, DiveEndDrag);

        body.Velocity = velocity;
    }

    private void UpdateDiveCooldown(float delta)
    {
        if (_diveCooldownTimer > 0.0f)
            _diveCooldownTimer -= delta;
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

    private void RotateInstantlyTowards(Node3D player, Vector3 direction)
    {
        Vector3 rotation = player.Rotation;
        rotation.Y = Mathf.Atan2(direction.X, direction.Z);
        player.Rotation = rotation;
    }
}