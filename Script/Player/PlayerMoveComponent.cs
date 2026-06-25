using Godot;

public partial class PlayerMoveComponent : Node
{
    [ExportGroup("Movement")]
    [Export] public float MoveSpeed = 10.0f;
    [Export] public float Acceleration = 35.0f;
    [Export] public float GroundFriction = 65.0f;
    [Export] public float TurnAcceleration = 75.0f;
    [Export] public float JumpVelocity = 8.0f;
    [Export] public float RotationSpeed = 18.0f;

    [ExportGroup("Air")]
    [Export] public float AirAcceleration = 8.0f;
    [Export] public float AirFriction = 1.5f;
    [Export] public float GravityMultiplier = 1.8f;
    [Export] public float FallGravityMultiplier = 2.6f;
    [Export] public float MaxFallSpeed = 45.0f;

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
    [Export] public float DiveEndDrag = 35.0f;
    [Export] public bool CanDiveInAir = false;

    [ExportGroup("Knockback")]
    [Export] public float KnockbackFriction = 22.0f;
    [Export] public float MaxKnockbackSpeed = 35.0f;

    [ExportGroup("Debug")]
    [Export] public bool DebugHeldSpinWeapon = false;

    public bool IsDiving { get; private set; }
    public bool IsSpinning { get; private set; }

    private float _currentSpinSpeed;
    private int _spinDirection = 1;

    private Vector3 _knockbackVelocity = Vector3.Zero;

    private float _diveTimer;
    private float _diveCooldownTimer;
    private Vector3 _diveDirection = Vector3.Zero;

    public void ApplyGravity(CharacterBody3D body, float delta)
    {
        if (body == null)
            return;

        if (body.IsOnFloor())
            return;

        Vector3 velocity = body.Velocity;

        float gravityMultiplier = velocity.Y < 0.0f
            ? FallGravityMultiplier
            : GravityMultiplier;

        velocity += body.GetGravity() * gravityMultiplier * delta;

        if (velocity.Y < -MaxFallSpeed)
            velocity.Y = -MaxFallSpeed;

        body.Velocity = velocity;
    }

    public void HandleJump(CharacterBody3D body)
    {
        if (body == null)
            return;

        if (IsDiving)
            return;

        if (!PlayerInput.JumpPressed)
            return;

        if (!body.IsOnFloor())
            return;

        Vector3 velocity = body.Velocity;
        velocity.Y = JumpVelocity;
        body.Velocity = velocity;
    }

    public void HandleMovement(CharacterBody3D body, Node3D cameraPivot, float delta)
    {
        if (body == null)
            return;

        UpdateDiveCooldown(delta);

        if (IsDiving)
        {
            StopSpin(delta);
            HandleDive(body, delta);
            ApplyKnockback(body, delta);
            return;
        }

        UpdateSpinState(delta);
        UpdateHeldSpinWeapon();

        TryStartDive(body, cameraPivot);

        if (IsDiving)
            return;

        Vector2 input = PlayerInput.Movement;
        Vector3 direction = GetCameraRelativeDirection(input, cameraPivot);

        float currentMoveSpeed = GetCurrentMoveSpeed();

        Vector3 velocity = body.Velocity;

        if (body.IsOnFloor())
        {
            velocity = ApplyGroundMovement(
                velocity,
                direction,
                currentMoveSpeed,
                delta
            );
        }
        else
        {
            velocity = ApplyAirMovement(
                velocity,
                direction,
                currentMoveSpeed,
                delta
            );
        }

        body.Velocity = velocity;

        ApplyKnockback(body, delta);

        if (IsSpinning)
        {
            Spin(body, delta);
        }
        else if (direction != Vector3.Zero)
        {
            RotateTowards(body, direction, delta);
        }
    }

    private Vector3 ApplyGroundMovement(
        Vector3 velocity,
        Vector3 direction,
        float currentMoveSpeed,
        float delta)
    {
        Vector3 horizontalVelocity = new Vector3(
            velocity.X,
            0.0f,
            velocity.Z
        );

        bool hasInput = direction != Vector3.Zero;

        if (hasInput)
        {
            Vector3 targetVelocity = direction * currentMoveSpeed;

            float accelerationToUse = Acceleration;

            if (horizontalVelocity.LengthSquared() > 0.01f)
            {
                float alignment = horizontalVelocity.Normalized().Dot(direction);

                // Lower alignment means the player is trying to turn around
                // or move sharply away from their current velocity.
                if (alignment < 0.25f)
                    accelerationToUse = TurnAcceleration;
            }

            horizontalVelocity = horizontalVelocity.MoveToward(
                targetVelocity,
                accelerationToUse * delta
            );
        }
        else
        {
            horizontalVelocity = horizontalVelocity.MoveToward(
                Vector3.Zero,
                GroundFriction * delta
            );
        }

        velocity.X = horizontalVelocity.X;
        velocity.Z = horizontalVelocity.Z;

        return velocity;
    }

    private Vector3 ApplyAirMovement(
        Vector3 velocity,
        Vector3 direction,
        float currentMoveSpeed,
        float delta)
    {
        Vector3 horizontalVelocity = new Vector3(
            velocity.X,
            0.0f,
            velocity.Z
        );

        bool hasInput = direction != Vector3.Zero;

        if (hasInput)
        {
            Vector3 targetVelocity = direction * currentMoveSpeed;

            horizontalVelocity = horizontalVelocity.MoveToward(
                targetVelocity,
                AirAcceleration * delta
            );
        }
        else
        {
            horizontalVelocity = horizontalVelocity.MoveToward(
                Vector3.Zero,
                AirFriction * delta
            );
        }

        velocity.X = horizontalVelocity.X;
        velocity.Z = horizontalVelocity.Z;

        return velocity;
    }

    private float GetCurrentMoveSpeed()
    {
        if (IsSpinning)
            return MoveSpeed * SpinningMoveSpeedMultiplier;

        return MoveSpeed;
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
        if (player == null)
            return;

        float spinRadians =
            Mathf.DegToRad(_currentSpinSpeed) *
            _spinDirection *
            delta;

        player.RotateY(spinRadians);
    }

    private void TryStartDive(CharacterBody3D body, Node3D cameraPivot)
    {
        if (!PlayerInput.GrabPressed)
            return;

        // If holding an object and spinning, this can throw instead of diving.
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
            EndDive(body);
    }

    private void EndDive(CharacterBody3D body)
    {
        IsDiving = false;
        _diveTimer = 0.0f;

        Vector3 velocity = body.Velocity;

        velocity.X = Mathf.MoveToward(
            velocity.X,
            0.0f,
            DiveEndDrag
        );

        velocity.Z = Mathf.MoveToward(
            velocity.Z,
            0.0f,
            DiveEndDrag
        );

        body.Velocity = velocity;
    }

    private void UpdateDiveCooldown(float delta)
    {
        if (_diveCooldownTimer <= 0.0f)
            return;

        _diveCooldownTimer -= delta;

        if (_diveCooldownTimer < 0.0f)
            _diveCooldownTimer = 0.0f;
    }

    private void ApplyKnockback(CharacterBody3D body, float delta)
    {
        if (_knockbackVelocity == Vector3.Zero)
            return;

        Vector3 velocity = body.Velocity;

        velocity.X += _knockbackVelocity.X;
        velocity.Z += _knockbackVelocity.Z;
        velocity.Y += _knockbackVelocity.Y;

        body.Velocity = velocity;

        _knockbackVelocity = _knockbackVelocity.MoveToward(
            Vector3.Zero,
            KnockbackFriction * delta
        );
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
        if (player == null)
            return;

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
        if (player == null)
            return;

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

        if (DebugHeldSpinWeapon)
        {
            GD.Print(
                $"[HeldSpinWeapon] Holding: {DiveGrab.CurrentGrabbed != null}, " +
                $"Spinning: {IsSpinning}, " +
                $"EnoughPower: {hasEnoughPower}"
            );
        }
    }

    public void ApplyKnockback(Vector3 force)
    {
        _knockbackVelocity += force;

        Vector3 horizontal = new Vector3(
            _knockbackVelocity.X,
            0.0f,
            _knockbackVelocity.Z
        );

        if (horizontal.Length() > MaxKnockbackSpeed)
        {
            horizontal = horizontal.Normalized() * MaxKnockbackSpeed;

            _knockbackVelocity.X = horizontal.X;
            _knockbackVelocity.Z = horizontal.Z;
        }

        _knockbackVelocity.Y = Mathf.Clamp(
            _knockbackVelocity.Y,
            -MaxKnockbackSpeed,
            MaxKnockbackSpeed
        );
    }
}