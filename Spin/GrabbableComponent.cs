using Godot;
using Game.Enemy;

public partial class GrabbableComponent : Node
{
    [Export] public Node3D GrabRoot;
    [Export] public Node3D AttachPoint;
    [Export] public ThrownProjectileComponent Projectile;
    [Export] public bool CanBeGrabbed = true;

    [ExportGroup("Grab Physics Lock")]
    [Export] public CharacterBody3D CharacterBody;
    [Export] public RigidBody3D RigidBody;
    [Export] public CollisionObject3D CollisionObject;
    [Export] public CollisionShape3D[] CollisionShapes;

    [ExportGroup("Held Spin Weapon")]
    [Export] public DamageHitboxComponent HeldSpinHitbox;
    [Export] public bool KeepSpinHitboxActiveDuringThrow = true;

    public bool IsGrabbed { get; private set; }

    private Node _originalParent;

    private uint _originalCollisionLayer;
    private uint _originalCollisionMask;

    private bool _hadCollisionObject;
    private bool[] _originalCollisionShapeDisabledStates;

    private RigidBody3D.FreezeModeEnum _originalFreezeMode;
    private bool _originalFreeze;
    private bool _originalSleeping;

    public override void _Ready()
    {
        if (GrabRoot == null)
            GrabRoot = GetOwner<Node3D>();

        if (CharacterBody == null)
            CharacterBody = GrabRoot as CharacterBody3D;

        if (RigidBody == null)
            RigidBody = GrabRoot as RigidBody3D;

        if (CollisionObject == null)
            CollisionObject = GrabRoot as CollisionObject3D;
    }

    public void Grab(Node3D holdPoint)
    {
        if (!CanBeGrabbed || IsGrabbed || GrabRoot == null || holdPoint == null)
            return;

        IsGrabbed = true;
        CanBeGrabbed = false;

        DisableHeldSpinHitbox();

        _originalParent = GrabRoot.GetParent();

        LockPhysicsForGrab();

        AlignAttachPointToHoldPoint(holdPoint);
        GrabRoot.Reparent(holdPoint, true);

        // Make sure it is exactly held after reparenting.
        AlignAttachPointToHoldPoint(holdPoint);

        GetGrabReceiver()?.OnGrabbed();

        // Keep controller active so DinoGrabState can run.
        SetEnemyActive(true);
    }

    public void Release(Node parentToReturnTo = null)
    {
        if (!IsGrabbed || GrabRoot == null)
            return;

        IsGrabbed = false;
        CanBeGrabbed = true;

        DisableHeldSpinHitbox();

        Node returnParent = parentToReturnTo ?? _originalParent;

        if (returnParent != null)
            GrabRoot.Reparent(returnParent, true);

        UnlockPhysicsAfterGrab();

        SetEnemyActive(true);
        GetGrabReceiver()?.OnReleased();
    }

    public void Throw(Vector3 velocity)
    {
        Throw(velocity, 1.0f);
    }

    public void Throw(Vector3 velocity, float damageMultiplier)
    {
        if (!IsGrabbed || GrabRoot == null)
            return;

        IsGrabbed = false;
        CanBeGrabbed = true;

        Node sceneRoot = GetTree().CurrentScene;

        if (sceneRoot != null)
            GrabRoot.Reparent(sceneRoot, true);
        else if (_originalParent != null)
            GrabRoot.Reparent(_originalParent, true);

        UnlockPhysicsAfterGrab();

        // Keep the spin hitbox active during throw if allowed.
        if (!KeepSpinHitboxActiveDuringThrow)
            DisableHeldSpinHitbox();

        GetGrabReceiver()?.OnThrown();

        SetEnemyActive(true);

        if (Projectile == null)
        {
            GD.Print("[Grabbable] Throw failed: Projectile not assigned.");
            DisableHeldSpinHitbox();
            return;
        }

        GD.Print(
            $"[Grabbable] Throwing with velocity: {velocity}, " +
            $"Damage Multiplier: {damageMultiplier}"
        );

        Projectile.Launch(velocity, damageMultiplier);
    }

    private void LockPhysicsForGrab()
    {
        if (CharacterBody != null)
        {
            CharacterBody.Velocity = Vector3.Zero;
        }

        if (RigidBody != null)
        {
            _originalFreeze = RigidBody.Freeze;
            _originalFreezeMode = RigidBody.FreezeMode;
            _originalSleeping = RigidBody.Sleeping;

            RigidBody.LinearVelocity = Vector3.Zero;
            RigidBody.AngularVelocity = Vector3.Zero;
            RigidBody.Sleeping = true;
            RigidBody.FreezeMode = RigidBody3D.FreezeModeEnum.Kinematic;
            RigidBody.Freeze = true;
        }

        if (CollisionObject != null)
        {
            _hadCollisionObject = true;
            _originalCollisionLayer = CollisionObject.CollisionLayer;
            _originalCollisionMask = CollisionObject.CollisionMask;

            CollisionObject.CollisionLayer = 0;
            CollisionObject.CollisionMask = 0;
        }
        else
        {
            _hadCollisionObject = false;
        }

        if (CollisionShapes != null && CollisionShapes.Length > 0)
        {
            _originalCollisionShapeDisabledStates = new bool[CollisionShapes.Length];

            for (int i = 0; i < CollisionShapes.Length; i++)
            {
                if (CollisionShapes[i] == null)
                    continue;

                _originalCollisionShapeDisabledStates[i] = CollisionShapes[i].Disabled;
                CollisionShapes[i].Disabled = true;
            }
        }
    }

    private void UnlockPhysicsAfterGrab()
    {
        if (CollisionObject != null && _hadCollisionObject)
        {
            CollisionObject.CollisionLayer = _originalCollisionLayer;
            CollisionObject.CollisionMask = _originalCollisionMask;
        }

        if (
            CollisionShapes != null &&
            _originalCollisionShapeDisabledStates != null
        )
        {
            int count = Mathf.Min(
                CollisionShapes.Length,
                _originalCollisionShapeDisabledStates.Length
            );

            for (int i = 0; i < count; i++)
            {
                if (CollisionShapes[i] == null)
                    continue;

                CollisionShapes[i].Disabled = _originalCollisionShapeDisabledStates[i];
            }
        }

        if (CharacterBody != null)
        {
            CharacterBody.Velocity = Vector3.Zero;
        }

        if (RigidBody != null)
        {
            RigidBody.FreezeMode = _originalFreezeMode;
            RigidBody.Freeze = _originalFreeze;
            RigidBody.Sleeping = _originalSleeping;
            RigidBody.LinearVelocity = Vector3.Zero;
            RigidBody.AngularVelocity = Vector3.Zero;
        }
    }

    private void AlignAttachPointToHoldPoint(Node3D holdPoint)
    {
        Vector3 originalScale = GrabRoot.GlobalTransform.Basis.Scale;

        if (AttachPoint == null)
        {
            Transform3D targetTransform = holdPoint.GlobalTransform;

            targetTransform.Basis = targetTransform.Basis.Orthonormalized();
            targetTransform.Basis = targetTransform.Basis.Scaled(originalScale);

            GrabRoot.GlobalTransform = targetTransform;
            return;
        }

        Transform3D rootTransform = GrabRoot.GlobalTransform;
        Transform3D attachTransform = AttachPoint.GlobalTransform;
        Transform3D holdTransform = holdPoint.GlobalTransform;

        Transform3D rootToAttach = rootTransform.AffineInverse() * attachTransform;

        Transform3D finalTransform = holdTransform * rootToAttach.AffineInverse();

        finalTransform.Basis = finalTransform.Basis.Orthonormalized();
        finalTransform.Basis = finalTransform.Basis.Scaled(originalScale);

        GrabRoot.GlobalTransform = finalTransform;
    }

    private IGrabStateReceiver GetGrabReceiver()
    {
        if (GrabRoot == null)
            return null;

        Node controller = GrabRoot.GetNodeOrNull("EnemyController");

        if (controller is IGrabStateReceiver receiver)
            return receiver;

        return null;
    }

    private void SetEnemyActive(bool active)
    {
        Node enemyController = GrabRoot.GetNodeOrNull("EnemyController");

        if (enemyController != null)
        {
            enemyController.SetProcess(active);
            enemyController.SetPhysicsProcess(active);
        }
    }

    public void SetHeldSpinHitboxActive(bool active)
    {
        if (HeldSpinHitbox == null)
        {
            GD.Print("[Grabbable] No HeldSpinHitbox assigned.");
            return;
        }

        GD.Print($"[Grabbable] SetHeldSpinHitboxActive: {active}");

        if (active)
            HeldSpinHitbox.EnableHitbox();
        else
            HeldSpinHitbox.DisableHitbox();
    }

    public void DisableHeldSpinHitbox()
    {
        HeldSpinHitbox?.DisableHitbox();
    }
}