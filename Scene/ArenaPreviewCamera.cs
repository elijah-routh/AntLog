using Godot;

public partial class ArenaPreviewCamera : Node3D
{
    [Export] public Node3D Pivot;
    [Export] public Camera3D Camera;

    [ExportGroup("Orbit")]
    [Export] public float OrbitSpeedDegrees = 5.0f;
    [Export] public float OrbitRadius = 35.0f;
    [Export] public float CameraHeight = 16.0f;

    [ExportGroup("Look Target")]
    [Export] public Node3D LookTarget;
    [Export] public Vector3 FallbackLookPosition = Vector3.Zero;

    [ExportGroup("Mode")]
    [Export] public bool LookAtTargetMode = true;
    [Export] public bool MakeCurrentOnReady = false;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        if (Pivot == null)
            Pivot = this;

        if (Camera != null)
        {
            Camera.Current = MakeCurrentOnReady;
            Camera.Position = new Vector3(0, CameraHeight, OrbitRadius);
        }

        UpdateLookDirection();
    }

    public override void _Process(double delta)
    {
        if (Pivot == null)
            return;

        Pivot.RotateY(Mathf.DegToRad(OrbitSpeedDegrees) * (float)delta);

        UpdateLookDirection();
    }

    public void SetTarget(Node3D target)
    {
        LookTarget = target;

        if (target != null && Pivot != null)
            Pivot.GlobalPosition = target.GlobalPosition;

        if (Camera != null)
            Camera.Current = true;

        UpdateLookDirection();
    }

    private void UpdateLookDirection()
    {
        if (LookAtTargetMode)
            LookAtTarget();
        else
            LookAwayFromTarget();
    }

    private void LookAtTarget()
    {
        if (Camera == null)
            return;

        Vector3 targetPosition = LookTarget != null
            ? LookTarget.GlobalPosition
            : FallbackLookPosition;

        Camera.LookAt(targetPosition, Vector3.Up);
    }

    private void LookAwayFromTarget()
    {
        if (Camera == null)
            return;

        Vector3 targetPosition = LookTarget != null
            ? LookTarget.GlobalPosition
            : FallbackLookPosition;

        Vector3 awayDirection = Camera.GlobalPosition - targetPosition;
        awayDirection.Y = 0;

        if (awayDirection.LengthSquared() < 0.001f)
            return;

        awayDirection = awayDirection.Normalized();

        Vector3 lookPosition = Camera.GlobalPosition + awayDirection;
        lookPosition.Y = Camera.GlobalPosition.Y;

        Camera.LookAt(lookPosition, Vector3.Up);
    }
}