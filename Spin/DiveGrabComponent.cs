using Godot;

public partial class DiveGrabComponent : Node
{
    [ExportGroup("References")]
    [Export] public PlayerMoveComponent Movement;
    [Export] public Area3D DiveGrabArea;
    [Export] public Node3D HoldPoint;

    [ExportGroup("Throw")]
    [Export] public float ThrowSpeed = 28.0f;
    [Export] public Node3D ThrowDirectionSource;

    [ExportGroup("Rules")]
    [Export] public bool GrabOnlyOncePerDive = true;

    public GrabbableComponent CurrentGrabbed { get; private set; }

    private bool _grabbedThisDive;

    public override void _Ready()
    {
        if (DiveGrabArea != null)
        {
            DiveGrabArea.AreaEntered += OnDiveGrabAreaEntered;
            DiveGrabArea.Monitoring = true;
        }
        else
        {
            GD.Print("[DiveGrab] DiveGrabArea not assigned.");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Movement == null)
            return;

        if (!Movement.IsDiving)
        {
            _grabbedThisDive = false;
        }

        if (DiveGrabArea == null)
            return;

        //foreach (Area3D area in DiveGrabArea.GetOverlappingAreas())
        //{
        //    if (area is GrabWeakSpot)
        //    {
        //        GD.Print("[DiveGrab] Currently overlapping grab point.");
        //    }
        //}
    }

    private void OnDiveGrabAreaEntered(Area3D area)
    {
        if (area is not GrabWeakSpot grabSpot)
            return;

        //GD.Print("[DiveGrab] Grab area intersected grab point.");

        if (Movement == null || !Movement.IsDiving)
            return;

        if (CurrentGrabbed != null)
            return;

        if (GrabOnlyOncePerDive && _grabbedThisDive)
            return;

        TryGrabSpot(grabSpot);
    }

    private void TryGrabSpot(GrabWeakSpot grabSpot)
    {
        if (grabSpot == null)
            return;

        Node3D grabber = GetParent<Node3D>();

        if (grabber == null)
            return;

        if (!grabSpot.IsValidGrabTarget(grabber))
            return;

        GrabbableComponent grabbable = grabSpot.Grabbable;

        if (grabbable == null)
            return;

        grabbable.Grab(HoldPoint);

        CurrentGrabbed = grabbable;
        _grabbedThisDive = true;

        OnGrabSucceeded();
    }

    private void OnGrabSucceeded()
    {
        GD.Print("Dive grab succeeded!");
    }

    public GrabbableComponent TakeGrabbedObject()
    {
        GrabbableComponent grabbed = CurrentGrabbed;
        CurrentGrabbed = null;
        return grabbed;
    }

    public void ClearGrabbedObject()
    {
        CurrentGrabbed = null;
    }

    public bool TryThrowHeld()
    {
        if (CurrentGrabbed == null)
            return false;

        Node3D player = GetParent<Node3D>();

        if (player == null)
            return false;

        Vector3 throwDirection;

        if (ThrowDirectionSource != null)
            throwDirection = ThrowDirectionSource.GlobalBasis.Z;
        else
            throwDirection = -player.GlobalBasis.Z;

        throwDirection.Y = 0.05f;
        throwDirection = throwDirection.Normalized();

        CurrentGrabbed.Throw(throwDirection * ThrowSpeed);

        CurrentGrabbed = null;
        _grabbedThisDive = false;

        GD.Print("[DiveGrab] Threw held enemy.");

        return true;
    }
}