using Game.Components;
using Game.Entity;
using Godot;

public partial class PlayerController : CharacterBody3D, IDamageable, IHealable, IKillable
{
    [Export] public Node3D CameraPivot;
    [Export] public PlayerMoveComponent Movement;
    [Export] public GroundAlignComponent GroundAlignment;

    [ExportGroup("Health")]
    [Export] public float StartingHealth = 100f;
    [Export] public HealthComponent Health;
    [Export] public HealthBar HealthBar;

    [ExportGroup("Debug")]
    [Export] public float DebugDamageAmount = 10f;


    public override void _Ready()
    {
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
        float dt = (float)delta;

        Movement.ApplyGravity(this, dt);
        Movement.HandleJump(this);
        Movement.HandleMovement(this, CameraPivot, dt);

        MoveAndSlide();

        GroundAlignment.AlignToGround(this, dt);

        if (PlayerInput.DamagePressed)
        {
            Health?.TakeDamage(DebugDamageAmount);
        }
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