using Godot;

public partial class GrabbableComponent : Node
{
    [Export] public Node3D GrabRoot;
    [Export] public Node3D AttachPoint;
    [Export] public ThrownProjectileComponent Projectile;
    [Export] public bool CanBeGrabbed = true;

    public bool IsGrabbed { get; private set; }

    private Node _originalParent;

    public override void _Ready()
    {
        if (GrabRoot == null)
            GrabRoot = GetOwner<Node3D>();
    }

    public void Grab(Node3D holdPoint)
    {
        if (!CanBeGrabbed || IsGrabbed || GrabRoot == null || holdPoint == null)
            return;

        IsGrabbed = true;
        CanBeGrabbed = false;

        _originalParent = GrabRoot.GetParent();

        // Move the whole enemy so its AttachPoint lines up with the player's HoldPoint.
        AlignAttachPointToHoldPoint(holdPoint);

        // Then parent it to the hold point so it follows the player.
        GrabRoot.Reparent(holdPoint, true);

        SetEnemyActive(false);
    }

    private void AlignAttachPointToHoldPoint(Node3D holdPoint)
    {
        if (AttachPoint == null)
        {
            GrabRoot.GlobalTransform = holdPoint.GlobalTransform;
            return;
        }

        Transform3D rootTransform = GrabRoot.GlobalTransform;
        Transform3D attachTransform = AttachPoint.GlobalTransform;
        Transform3D holdTransform = holdPoint.GlobalTransform;

        Transform3D rootToAttach = rootTransform.AffineInverse() * attachTransform;

        GrabRoot.GlobalTransform = holdTransform * rootToAttach.AffineInverse();
    }

    public void Release(Node parentToReturnTo = null)
    {
        if (!IsGrabbed || GrabRoot == null)
            return;

        IsGrabbed = false;
        CanBeGrabbed = true;

        Node returnParent = parentToReturnTo ?? _originalParent;

        if (returnParent != null)
            GrabRoot.Reparent(returnParent, true);

        SetEnemyActive(true);
    }

    private void SetEnemyActive(bool active)
    {
        Node enemyController = GrabRoot.GetNodeOrNull("EnemyController");

        if (enemyController != null)
            enemyController.SetProcess(active);
    }

    public void Throw(Vector3 velocity)
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

        SetEnemyActive(false);

        if (Projectile == null)
        {
            GD.Print("[Grabbable] Throw failed: Projectile not assigned.");
            return;
        }

        GD.Print($"[Grabbable] Throwing with velocity: {velocity}");
        Projectile.Launch(velocity);
    }
}