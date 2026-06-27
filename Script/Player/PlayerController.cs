using Game.Components;
using Game.Enemy;
using Game.Entity;
using Godot;

public partial class PlayerController : CharacterBody3D, IDamageable, IHealable, IKillable, IKnockable
{
    [Signal]
    public delegate void PlayerDiedEventHandler();

    [Export] public Node3D CameraPivot;
    [Export] public PlayerMoveComponent Movement;
    [Export] public GroundAlignComponent GroundAlignment;

    [ExportGroup("Health")]
    [Export] public float StartingHealth = 100f;
    [Export] public HealthComponent Health;
    [Export] public HealthBar HealthBar;

    [ExportGroup("Knockback")]
    [Export] public float KnockbackResistance = 1.0f;
    [Export] public float MaxKnockbackSpeed = 35.0f;

    [ExportGroup("Debug")]
    [Export] public float DebugDamageAmount = 10f;

    [ExportGroup("Animation")]
    [Export] public PlayerAnimationController Animations { get; set; }

    [ExportGroup("Death")]
    [Export] public float DeathGameOverDelay = 3.0f;

    private bool _dead;

    public bool IsDead => _dead;

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
        if (_dead)
            return;

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

    public void ApplyKnockback(Vector3 force)
    {
        if (_dead)
            return;

        if (Movement != null)
        {
            Movement.ApplyKnockback(force / Mathf.Max(KnockbackResistance, 0.01f));
            return;
        }

        Vector3 velocity = Velocity;
        velocity += force / Mathf.Max(KnockbackResistance, 0.01f);

        Vector3 horizontal = new Vector3(velocity.X, 0f, velocity.Z);

        if (horizontal.Length() > MaxKnockbackSpeed)
        {
            horizontal = horizontal.Normalized() * MaxKnockbackSpeed;
            velocity.X = horizontal.X;
            velocity.Z = horizontal.Z;
        }

        Velocity = velocity;
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        if (HealthBar == null)
            return;

        HealthBar.MaxValue = maxHealth;
        HealthBar.Health = currentHealth;
    }

    private async void OnDied()
    {
        if (_dead)
            return;

        _dead = true;

        GD.Print("Warthog destroyed");

        Velocity = Vector3.Zero;
        SetPhysicsProcess(false);

        // Play death animation.
        Animations?.PlayDeath();
        SoundManager.Instance.PlayWinSound();

        // Wait so the death animation can be seen before game-over UI/camera.
        SceneTreeTimer timer = GetTree().CreateTimer(DeathGameOverDelay);
        await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

        if (!GodotObject.IsInstanceValid(this))
            return;

        EmitSignal(nameof(PlayerDied));
    }

    public void TakeDamage(float damage)
    {
        if (_dead)
            return;

        SoundManager.Instance.PlayHurtSound();

        Health?.TakeDamage(damage);
    }

    public void Heal(float amount)
    {
        if (_dead)
            return;

        Health?.Heal(amount);
    }

    public void Kill()
    {
        if (_dead)
            return;

        Health?.Kill();
    }
}