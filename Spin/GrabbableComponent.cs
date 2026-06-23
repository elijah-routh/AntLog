using Godot;
using Game.Enemy;

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

        AlignAttachPointToHoldPoint(holdPoint);
        GrabRoot.Reparent(holdPoint, true);

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

        Node returnParent = parentToReturnTo ?? _originalParent;

        if (returnParent != null)
            GrabRoot.Reparent(returnParent, true);

        SetEnemyActive(true);
        GetGrabReceiver()?.OnReleased();
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

        GetGrabReceiver()?.OnThrown();

        // Keep the controller active so DinoThrownState can run.
        SetEnemyActive(true);

        if (Projectile == null)
        {
            GD.Print("[Grabbable] Throw failed: Projectile not assigned.");
            return;
        }

        GD.Print($"[Grabbable] Throwing with velocity: {velocity}");
        Projectile.Launch(velocity);
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
}