using Godot;
using Game.Components;
using Game.Entity;

public partial class Warthog : VehicleBody3D, IDamageable, IHealable, IKillable
{
    [ExportGroup("Components")]
    [Export] public BoostComponent Boost;
    [Export] public HealthComponent Health;
    [Export] public HealthBar HealthBar;

    [ExportGroup("Health")]
    [Export] public float StartingHealth = 100f;
    
    [ExportGroup("Wheels")]
    [Export] public VehicleWheel3D FrontLeftWheel;
    [Export] public VehicleWheel3D RearLeftWheel;
    [Export] public VehicleWheel3D FrontRightWheel;
    [Export] public VehicleWheel3D RearRightWheel;

    [ExportGroup("Steering")]
    [Export] public float MaxSteerAngle = 0.6f;
    [Export] public float SteerSpeed = 10.0f;

    [ExportGroup("Suspension")]
    [Export] public float WheelFriction = 20.0f;
    [Export] public float SuspensionStiffness = 25.0f;

    [ExportGroup("Engine")]
    [Export] public float Acceleration = 1200.0f;
    [Export] public float MaxSpeed = 30.0f;
    [Export] public bool UsePositiveZForward = true;

    [ExportGroup("Grip")]
    [Export] public float WheelSideGrip = 1.0f;

    [ExportGroup("Stability")]
    [Export] public Vector3 CustomCenterOfMass = new(0, -1.0f, 0);
    [Export] public float LinearDamping = 1.5f;
    [Export] public float AngularDamping = 2.5f;

    [ExportGroup("Air Control")]
    [Export] public float GroundLinearDamping = 1.0f;
    [Export] public float AirLinearDamping = 0.05f;
    [Export] public float GroundAngularDamping = 2.0f;
    [Export] public float AirAngularDamping = 0.25f;

    private float _throttle;
    private float _steerInput;

    private VehicleWheel3D[] _wheels;

    [ExportGroup("Debug")]
    [Export] public float DebugDamageAmount = 10f;


    public override void _Ready()
    {
        _wheels =
        [
            FrontLeftWheel,
            RearLeftWheel,
            FrontRightWheel,
            RearRightWheel
        ];

        CenterOfMassMode = CenterOfMassModeEnum.Custom;
        CenterOfMass = CustomCenterOfMass;

        LinearDamp = LinearDamping;
        AngularDamp = AngularDamping;

        ApplyWheelSettings();

        if (Boost == null)
            Boost = GetNodeOrNull<BoostComponent>("BoostComponent");

        if (Health == null)
            Health = GetNodeOrNull<HealthComponent>("HealthComponent");

        if (HealthBar == null)
            HealthBar = GetNodeOrNull<HealthBar>("HealthBar");

        if (Health != null)
        {
            Health.HealthChanged += OnHealthChanged;
            Health.Died += OnDied;

            Health.Initialize(StartingHealth);
        }

        if (HealthBar != null)
        {
            HealthBar.InitHealth(StartingHealth);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float physicsDelta = (float)delta;

        _throttle =
            Input.GetAxis("drive_back", "drive_forward");

        _steerInput =
            Input.GetAxis("move_right", "move_left");

        bool hasGroundContact = HasGroundContact();

        UpdateDamping(hasGroundContact);

        Boost?.Tick(
            physicsDelta,
            _throttle,
            hasGroundContact
        );

        HandleEngineForce(hasGroundContact);
        HandleSteering(physicsDelta);
        ApplySideGrip();

        if (PlayerInput.DamagePressed)
        {
            Health?.TakeDamage(DebugDamageAmount);
        }
    }

    private void ApplyWheelSettings()
    {
        foreach (VehicleWheel3D wheel in _wheels)
        {
            if (wheel == null)
                continue;

            wheel.WheelFrictionSlip = WheelFriction;
            wheel.SuspensionStiffness = SuspensionStiffness;
        }
    }

    private void UpdateDamping(bool hasGroundContact)
    {
        if (hasGroundContact)
        {
            LinearDamp = GroundLinearDamping;
            AngularDamp = GroundAngularDamping;
        }
        else
        {
            LinearDamp = AirLinearDamping;
            AngularDamp = AirAngularDamping;
        }
    }

    private void HandleEngineForce(bool hasGroundContact)
    {
        if (!hasGroundContact)
            return;

        float currentAcceleration = Acceleration;
        float currentMaxSpeed = MaxSpeed;

        if (Boost != null)
        {
            currentAcceleration = Boost.GetAcceleration(currentAcceleration);
            currentMaxSpeed = Boost.GetMaxSpeed(currentMaxSpeed);
        }

        Vector3 forward = GetForwardDirection();

        float speed =
            Mathf.Abs(LinearVelocity.Dot(forward));

        float speedFactor =
            1.0f - Mathf.Min(speed / currentMaxSpeed, 1.0f);

        Vector3 force =
            forward *
            currentAcceleration *
            _throttle *
            speedFactor;

        ApplyCentralForce(force);
    }

    private Vector3 GetForwardDirection()
    {
        if (UsePositiveZForward)
            return GlobalTransform.Basis.Z;

        return -GlobalTransform.Basis.Z;
    }

    private void HandleSteering(float delta)
    {
        if (FrontLeftWheel == null ||
            FrontRightWheel == null)
            return;

        float targetSteer =
            _steerInput * MaxSteerAngle;

        FrontLeftWheel.Steering =
            Mathf.MoveToward(
                FrontLeftWheel.Steering,
                targetSteer,
                SteerSpeed * delta
            );

        FrontRightWheel.Steering =
            Mathf.MoveToward(
                FrontRightWheel.Steering,
                targetSteer,
                SteerSpeed * delta
            );
    }

    private void ApplySideGrip()
    {
        foreach (VehicleWheel3D wheel in _wheels)
        {
            ApplyWheelSideGrip(wheel);
        }
    }

    private void ApplyWheelSideGrip(VehicleWheel3D wheel)
    {
        if (wheel == null)
            return;

        if (!wheel.IsInContact())
            return;

        Vector3 sideDirection =
            wheel.GlobalTransform.Basis.X;

        Vector3 wheelOffset =
            wheel.GlobalPosition - GlobalPosition;

        Vector3 wheelVelocity =
            LinearVelocity +
            AngularVelocity.Cross(wheelOffset);

        float sideVelocity =
            sideDirection.Dot(wheelVelocity);

        float gravity =
            ProjectSettings
            .GetSetting("physics/3d/default_gravity")
            .AsSingle();

        Vector3 gripForce =
            -sideDirection *
            sideVelocity *
            WheelSideGrip *
            ((Mass * gravity) / 4.0f);

        ApplyForce(gripForce, wheelOffset);
    }

    private bool HasGroundContact()
    {
        return
            IsWheelGrounded(RearLeftWheel) ||
            IsWheelGrounded(RearRightWheel);
    }

    private bool IsWheelGrounded(VehicleWheel3D wheel)
    {
        return wheel != null && wheel.IsInContact();
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        if (HealthBar == null)
            return;

        HealthBar.MaxValue = maxHealth;
        HealthBar.Health = currentHealth;
    }

    private void OnDied()
    {
        GD.Print("Warthog destroyed");

        // Optional:
        // QueueFree();
        // Disable driving.
        // Play explosion.
        // Start respawn timer.
    }

    public void TakeDamage(float damage)
    {
        Health?.TakeDamage(damage);
    }

    public void Heal(float amount)
    {
        Health?.Heal(amount);
    }

    public void Kill()
    {
        Health?.Kill();
    }
}