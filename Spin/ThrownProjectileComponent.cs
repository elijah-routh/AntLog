using Godot;
using Game.Entity;
using Game.Enemy;

public partial class ThrownProjectileComponent : Area3D
{
    [ExportGroup("References")]
    [Export] public Node3D ProjectileRoot;

    [ExportGroup("Projectile")]
    [Export] public float Lifetime = 2.0f;
    [Export] public float BaseDamage = 25.0f;
    [Export] public bool StopOnHit = true;

    [ExportGroup("World Collision")]
    [Export(PropertyHint.Layers3DPhysics)]
    public uint WorldCollisionMask = 1;

    [Export] public float WallCollisionSkin = 1.0f;
    [Export] public float FloorCollisionSkin = 0.1f;
    [Export] public float FloorNormalYThreshold = 0.6f;
    [Export] public float FloorSlideFriction = 0.98f;
    [Export] public bool StopOnNonFloorHit = true;

    [ExportGroup("Launch Recovery")]
    [Export] public int LaunchRecoveryFrames = 2;
    [Export] public float FloorRecoveryStep = 0.15f;
    [Export] public int MaxFloorRecoverySteps = 8;

    [ExportGroup("Debug")]
    [Export] public bool PrintDebug = true;

    public bool IsActive { get; private set; }

    private Vector3 _velocity;
    private float _timer;
    private float _currentDamage;
    private int _launchRecoveryFramesRemaining;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        AreaEntered += OnAreaEntered;

        Monitoring = false;
        Monitorable = false;

        if (ProjectileRoot == null)
            ProjectileRoot = GetParent<Node3D>();

        _currentDamage = BaseDamage;

        DebugPrint($"Ready. Root: {ProjectileRoot?.Name}");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsActive || ProjectileRoot == null)
            return;

        float dt = (float)delta;

        _timer -= dt;

        if (_timer <= 0.0f)
        {
            StopProjectile();
            return;
        }

        if (_launchRecoveryFramesRemaining > 0)
        {
            TryRecoverFromLaunchOverlap();
            _launchRecoveryFramesRemaining--;
        }

        MoveProjectile(dt);
    }

    public void Launch(Vector3 velocity)
    {
        Launch(velocity, 1.0f);
    }

    public void Launch(Vector3 velocity, float damageMultiplier)
    {
        damageMultiplier = Mathf.Max(damageMultiplier, 0.0f);

        _velocity = velocity;
        _timer = Lifetime;
        _currentDamage = BaseDamage * damageMultiplier;
        _launchRecoveryFramesRemaining = LaunchRecoveryFrames;

        IsActive = true;
        Monitoring = true;
        Monitorable = true;

        DebugPrintLaunch(_velocity);

        DebugPrint(
            $"Launched. " +
            $"Velocity: {_velocity}, " +
            $"Timer: {_timer}, " +
            $"Damage: {_currentDamage}, " +
            $"Damage Multiplier: {damageMultiplier}"
        );
    }

    public void StopProjectile()
    {
        if (!IsActive)
            return;

        EndProjectile(notifyThrowFinished: true);
    }

    public void CancelProjectile()
    {
        if (!IsActive)
            return;

        EndProjectile(notifyThrowFinished: false);
    }

    private void EndProjectile(bool notifyThrowFinished)
    {
        IsActive = false;
        _velocity = Vector3.Zero;
        _timer = 0.0f;
        _currentDamage = BaseDamage;
        _launchRecoveryFramesRemaining = 0;

        Monitoring = false;
        Monitorable = false;

        if (notifyThrowFinished)
            NotifyThrowFinished();

        DebugPrint(
            notifyThrowFinished
                ? "Stopped."
                : "Cancelled."
        );
    }

    private void MoveProjectile(float dt)
    {
        Vector3 from = ProjectileRoot.GlobalPosition;
        Vector3 motion = _velocity * dt;
        Vector3 to = from + motion;

        if (CheckProjectileCollision(
            from,
            to,
            out Vector3 hitPosition,
            out Vector3 hitNormal,
            out Node collider))
        {
            HandleWorldCollision(hitPosition, hitNormal, collider);
            return;
        }

        ProjectileRoot.GlobalPosition = to;
    }

    private void HandleWorldCollision(
        Vector3 hitPosition,
        Vector3 hitNormal,
        Node collider)
    {
        bool hitFloor = IsFloorNormal(hitNormal);

        float skin = hitFloor
            ? FloorCollisionSkin
            : WallCollisionSkin;

        ProjectileRoot.GlobalPosition = hitPosition + hitNormal * skin;

        DebugPrint(
            $"Hit world collision: {collider.Name} | " +
            $"Floor: {hitFloor} | " +
            $"Hit Pos: {hitPosition} | " +
            $"Placed Pos: {ProjectileRoot.GlobalPosition} | " +
            $"Normal: {hitNormal}"
        );

        if (hitFloor)
        {
            GlideAlongSurface(hitNormal);
            return;
        }

        if (StopOnNonFloorHit)
        {
            StopProjectile();
            return;
        }

        GlideAlongSurface(hitNormal);
    }

    private void GlideAlongSurface(Vector3 surfaceNormal)
    {
        _velocity = _velocity.Slide(surfaceNormal);
        _velocity *= FloorSlideFriction;

        DebugPrint(
            $"Gliding along surface. " +
            $"New Velocity: {_velocity} | " +
            $"Speed: {_velocity.Length():0.00}"
        );

        if (_velocity.LengthSquared() <= 0.1f)
        {
            StopProjectile();
        }
    }

    private bool CheckProjectileCollision(
        Vector3 from,
        Vector3 to,
        out Vector3 hitPosition,
        out Vector3 hitNormal,
        out Node collider)
    {
        hitPosition = Vector3.Zero;
        hitNormal = Vector3.Zero;
        collider = null;

        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

        PhysicsRayQueryParameters3D query =
            PhysicsRayQueryParameters3D.Create(from, to);

        query.CollideWithBodies = true;
        query.CollideWithAreas = false;
        query.CollisionMask = WorldCollisionMask;

        if (ProjectileRoot is CollisionObject3D collisionObject)
        {
            query.Exclude = new Godot.Collections.Array<Rid>
            {
                collisionObject.GetRid()
            };
        }

        Godot.Collections.Dictionary result = spaceState.IntersectRay(query);

        if (result.Count == 0)
            return false;

        hitPosition = (Vector3)result["position"];
        hitNormal = (Vector3)result["normal"];
        collider = result["collider"].As<Node>();

        return true;
    }

    private bool TryRecoverFromLaunchOverlap()
    {
        if (!IsAlreadyTouchingWorld(out Node overlappingWorld))
            return false;

        DebugPrint($"Launch overlap with world: {overlappingWorld.Name}");

        for (int i = 0; i < MaxFloorRecoverySteps; i++)
        {
            ProjectileRoot.GlobalPosition += Vector3.Up * FloorRecoveryStep;

            if (!IsAlreadyTouchingWorld(out overlappingWorld))
            {
                DebugPrint(
                    $"Launch overlap recovered. " +
                    $"Steps: {i + 1} | " +
                    $"New Position: {ProjectileRoot.GlobalPosition}"
                );

                return true;
            }
        }

        DebugPrint("Launch overlap recovery incomplete.");
        return false;
    }

    private bool IsAlreadyTouchingWorld(out Node worldBody)
    {
        worldBody = null;

        foreach (Node3D body in GetOverlappingBodies())
        {
            if (ShouldIgnoreCollisionTarget(body))
                continue;

            if (body is CollisionObject3D collisionObject)
            {
                bool isOnWorldLayer =
                    ((uint)collisionObject.CollisionLayer & WorldCollisionMask) != 0;

                if (!isOnWorldLayer)
                    continue;
            }

            worldBody = body;
            return true;
        }

        return false;
    }

    private void OnBodyEntered(Node3D body)
    {
        DebugPrint($"Hit body: {body.Name}");
        Hit(body);
    }

    private void OnAreaEntered(Area3D area)
    {
        DebugPrint($"Hit area: {area.Name}");
        Hit(area);
    }

    private void Hit(Node target)
    {
        if (!IsActive)
            return;

        if (target is Node3D node3D && ShouldIgnoreCollisionTarget(node3D))
            return;

        if (target is IDamageable damageable)
        {
            damageable.TakeDamage(_currentDamage);

            DebugPrint($"Damaged {target.Name} for {_currentDamage}");

            if (StopOnHit)
                StopProjectile();
        }
    }

    private bool ShouldIgnoreCollisionTarget(Node3D target)
    {
        if (ProjectileRoot == null || target == null)
            return false;

        if (target == ProjectileRoot)
            return true;

        if (ProjectileRoot.IsAncestorOf(target))
            return true;

        if (target.IsAncestorOf(ProjectileRoot))
            return true;

        return false;
    }

    private bool IsFloorNormal(Vector3 normal)
    {
        return normal.Y >= FloorNormalYThreshold;
    }

    private void NotifyThrowFinished()
    {
        if (ProjectileRoot == null)
            return;

        Node controller = ProjectileRoot.GetNodeOrNull("EnemyController");

        if (controller is IGrabStateReceiver receiver)
        {
            receiver.OnThrowFinished();
            return;
        }

        DebugPrint(
            $"No IGrabStateReceiver found on " +
            $"{ProjectileRoot.Name}/EnemyController."
        );
    }

    private void DebugPrintLaunch(Vector3 velocity)
    {
        if (!PrintDebug)
            return;

        float speed = velocity.Length();

        Vector3 horizontalVelocity = new Vector3(
            velocity.X,
            0f,
            velocity.Z
        );

        float horizontalSpeed = horizontalVelocity.Length();

        float angleDegrees = 0f;

        if (speed > 0.001f)
        {
            angleDegrees = Mathf.RadToDeg(
                Mathf.Atan2(velocity.Y, horizontalSpeed)
            );
        }

        GD.Print(
            $"[ThrownProjectile] Launch Debug | " +
            $"Velocity: {velocity} | " +
            $"Speed: {speed:0.00} | " +
            $"Horizontal Speed: {horizontalSpeed:0.00} | " +
            $"Vertical Speed: {velocity.Y:0.00} | " +
            $"Angle: {angleDegrees:0.00} degrees"
        );
    }

    private void DebugPrint(string message)
    {
        if (!PrintDebug)
            return;

        GD.Print($"[ThrownProjectile] {message}");
    }
}