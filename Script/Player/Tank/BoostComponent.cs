using Godot;

public partial class BoostComponent : Node
{
    [ExportGroup("Input")]
    [Export] public string BoostAction = "boost";

    [ExportGroup("Boost Power")]
    [Export] public float AccelerationMultiplier = 2.0f;
    [Export] public float MaxSpeedMultiplier = 2.0f;

    [ExportGroup("Boost Energy")]
    [Export] public float MaxEnergy = 100.0f;
    [Export] public float DrainRate = 40.0f;
    [Export] public float RegenRate = 18.0f;

    [ExportGroup("Requirements")]
    [Export] public bool RequireGrounded = true;
    [Export] public bool RequireForwardThrottle = true;

    public float Energy { get; private set; }
    public bool IsBoosting { get; private set; }

    public float EnergyPercent =>
        MaxEnergy <= 0.0f ? 0.0f : Energy / MaxEnergy;

    public override void _Ready()
    {
        Energy = MaxEnergy;
    }

    public void Tick(float delta, float throttle, bool isGrounded)
    {
        bool wantsToBoost = Input.IsActionPressed(BoostAction);

        bool hasEnergy = Energy > 0.0f;

        bool groundedEnough =
            !RequireGrounded || isGrounded;

        bool throttlingEnough =
            !RequireForwardThrottle || throttle > 0.1f;

        IsBoosting =
            wantsToBoost &&
            hasEnergy &&
            groundedEnough &&
            throttlingEnough;

        if (IsBoosting)
        {
            Energy = Mathf.Max(
                Energy - DrainRate * delta,
                0.0f
            );
        }
        else
        {
            Energy = Mathf.Min(
                Energy + RegenRate * delta,
                MaxEnergy
            );
        }
    }

    public float GetAcceleration(float baseAcceleration)
    {
        if (!IsBoosting)
            return baseAcceleration;

        return baseAcceleration * AccelerationMultiplier;
    }

    public float GetMaxSpeed(float baseMaxSpeed)
    {
        if (!IsBoosting)
            return baseMaxSpeed;

        return baseMaxSpeed * MaxSpeedMultiplier;
    }
}