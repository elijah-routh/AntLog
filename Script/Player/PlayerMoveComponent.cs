using Godot;

public partial class PlayerMoveComponent : Node
{
    [ExportGroup("Movement")]
    [Export] public float MoveSpeed = 12.0f;
    [Export] public float Acceleration = 10.0f;
    [Export] public float Deceleration = 16.0f;
    [Export] public float JumpVelocity = 8.5f;
    [Export] public float RotationSpeed = 12.0f;

    [ExportGroup("Spin")]
    [Export] public float SpinSpeed = 14.0f;
    [Export(PropertyHint.Range, "0.1,1.0,0.05")]
    public float SpinMoveSpeedMultiplier = 0.45f;
    [Export] public int SpinDirection = 1;

    [ExportGroup("Dive Grab")]
    [Export] public DiveGrabComponent DiveGrab;
    [Export] public float DiveSpeed = 24.0f;
    [Export] public float DiveDuration = 0.25f;
    [Export] public float DiveCooldown = 0.35f;
    [Export] public float DiveEndDrag = 20.0f;
    [Export] public bool CanDiveInAir = false;

    public bool IsDiving { get; private set; }
    public bool IsSpinning { get; private set; }

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
            IsSpinning = false;
            HandleDive(body, delta);
            return;
        }

        TryStartDive(body, cameraPivot);

        if (IsDiving)
            return;

        IsSpinning = PlayerInput.SpinHeld;

        Vector2 input = PlayerInput.Movement;
        Vector3 direction = GetCameraRelativeDirection(input, cameraPivot);

        float currentMoveSpeed = IsSpinning
            ? MoveSpeed * SpinMoveSpeedMultiplier
            : MoveSpeed;

        Vector3 velocity = body.Velocity;

        if (direction != Vector3.Zero)
        {
            Vector3 targetVelocity = direction * currentMoveSpeed;

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

        if (IsSpinning)
        {
            Spin(body, delta);
        }
        else if (direction != Vector3.Zero)
        {
            RotateTowards(body, direction, delta);
        }
    }

    private void Spin(Node3D player, float delta)
    {
        Vector3 rotation = player.Rotation;
        rotation.Y += SpinSpeed * SpinDirection * delta;
        player.Rotation = rotation;
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
            inputDirection = -body.GlobalBasis.Z;
            inputDirection.Y = 0;
            inputDirection = inputDirection.Normalized();
        }

        StartDive(body, inputDirection);
    }

    private void StartDive(CharacterBody3D body, Vector3 direction)
    {
        IsDiving = true;
        IsSpinning = false;

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