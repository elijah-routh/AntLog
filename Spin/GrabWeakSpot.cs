using Godot;

public partial class GrabWeakSpot : Area3D
{
    [Export] public GrabbableComponent Grabbable;
    [Export] public bool CanBeGrabbed = true;

    [ExportGroup("Directional Rules")]
    [Export] public bool RequireCorrectDirection = false;

    // The local direction that counts as the exposed grab side.
    // For a tail, this is often local +Z or -Z depending on your model.
    [Export] public Vector3 LocalGrabDirection = Vector3.Back;

    [Export] public float MaxGrabAngleDegrees = 60.0f;

    public override void _Ready()
    {
        if (Grabbable == null)
        {
            Grabbable = GetParent()?.GetNodeOrNull<GrabbableComponent>("GrabbableComponent");
        }
    }

    public bool IsValidGrabTarget(Node3D grabber)
    {
        if (!CanBeGrabbed || Grabbable == null || !Grabbable.CanBeGrabbed)
            return false;

        if (!RequireCorrectDirection)
            return true;

        Vector3 weakSpotDirection = GlobalBasis * LocalGrabDirection.Normalized();
        Vector3 directionToGrabber = (grabber.GlobalPosition - GlobalPosition).Normalized();

        float dot = weakSpotDirection.Dot(directionToGrabber);
        float minDot = Mathf.Cos(Mathf.DegToRad(MaxGrabAngleDegrees));

        return dot >= minDot;
    }
}