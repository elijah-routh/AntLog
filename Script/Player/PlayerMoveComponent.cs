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
    [Export] public SpinPowerComponent SpinPower;
    [Export] public float MinSpinSpeed = 180.0f;
    [Export] public float MaxSpinSpeed = 900.0f;
    [Export] public float SpinAcceleration = 600.0f;
    [Export] public float SpinDeceleration = 900.0f;
    [Export] public float SpinningMoveSpeedMultiplier = 0.45f;

    [ExportGroup("Dive Grab")]
    [Export] public DiveGrabComponent DiveGrab;
    [Export] public float DiveSpeed = 24.0f;
    [Export] public float DiveDuration = 0.25f;
    [Export] public float DiveCooldown = 0.35f;
    [Export] public float DiveEndDrag = 20.0f;
    [Export] public bool CanDiveInAir = false;

    public bool IsDiving { get; private set; }
    public bool IsSpinning { get; private set; }

    private float _currentSpinSpeed;
    private int _spinDirection = 1;

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
            StopSpin(delta);
            HandleDive(body, delta);
            return;
        }

        UpdateSpinState(delta);
        UpdateHeldSpinWeapon();

        TryStartDive(body, cameraPivot);

        if (IsDiving)
            return;

        Vector2 input = PlayerInput.Movement;
        Vector3 direction = GetCameraRelativeDirection(input, cameraPivot);

        float currentMoveSpeed = IsSpinning
            ? MoveSpeed * SpinningMoveSpeedMultiplier
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
                0.0f,
                Deceleration * delta
            );

            velocity.Z = Mathf.MoveToward(
                velocity.Z,
                0.0f,
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

    private void UpdateSpinState(float delta)
    {
        IsSpinning = PlayerInput.SpinHeld;

        if (IsSpinning)
        {
            if (_currentSpinSpeed <= 0.0f)
            {
                _spinDirection = GD.Randf() > 0.5f ? 1 : -1;
                _currentSpinSpeed = MinSpinSpeed;
            }

            _currentSpinSpeed = Mathf.MoveToward(
                _currentSpinSpeed,
                MaxSpinSpeed,
                SpinAcceleration * delta
            );

            float spinSpeedPercent = Mathf.InverseLerp(
                MinSpinSpeed,
                MaxSpinSpeed,
                _currentSpinSpeed
            );

            SpinPower?.BuildPower(delta, spinSpeedPercent);
        }
        else
        {
            StopSpin(delta);
        }
    }

    private void StopSpin(float delta)
    {
        IsSpinning = false;

        _currentSpinSpeed = Mathf.MoveToward(
            _currentSpinSpeed,
            0.0f,
            SpinDeceleration * delta
        );

        SpinPower?.DecayPower(delta);
    }

    private void Spin(Node3D player, float delta)
    {
        float spinRadians = Mathf.DegToRad(_currentSpinSpeed) * _spinDirection * delta;

        player.RotateY(spinRadians);
    }

    private void TryStartDive(CharacterBody3D body, Node3D cameraPivot)
    {
        if (!PlayerInput.GrabPressed)
            return;

        // Only returns true if the held object is actually thrown.
        // If not spinning, TryThrowHeld should return false,
        // then the player continues into the dive.
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
            inputDirection.Y = 0.0f;
            inputDirection = inputDirection.Normalized();
        }

        StartDive(body, inputDirection);
    }

    private void StartDive(CharacterBody3D body, Vector3 direction)
    {
        IsDiving = true;
        IsSpinning = false;

        _currentSpinSpeed = 0.0f;

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

        velocity.X = Mathf.MoveToward(velocity.X, 0.0f, DiveEndDrag);
        velocity.Z = Mathf.MoveToward(velocity.Z, 0.0f, DiveEndDrag);

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
            0.0f,
            -Mathf.Cos(yaw)
        );

        Vector3 right = new Vector3(
            Mathf.Cos(yaw),
            0.0f,
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

    private void UpdateHeldSpinWeapon()
    {
        if (DiveGrab == null || DiveGrab.CurrentGrabbed == null)
            return;

        bool hasEnoughPower =
            SpinPower != null &&
            SpinPower.HasEnoughPowerForSpinDamage();

        bool shouldBeActive =
            IsSpinning &&
            hasEnoughPower;

        DiveGrab.CurrentGrabbed.SetHeldSpinHitboxActive(shouldBeActive);

        GD.Print(
            $"[HeldSpinWeapon] Holding: {DiveGrab?.CurrentGrabbed != null}, " +
            $"Spinning: {IsSpinning}, " +
            $"EnoughPower: {SpinPower?.HasEnoughPowerForSpinDamage()}"
        );
    }
}
